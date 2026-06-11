using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NexumAltivon.API.Data;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace NexumAltivon.API.Services;

public class AuthService : IAuthService
{
    private readonly NexumDbContext _context;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly ILogAuditoriaService _auditoria;

    public AuthService(NexumDbContext context, IMapper mapper, IConfiguration configuration, ILogAuditoriaService auditoria)
    {
        _context = context;
        _mapper = mapper;
        _configuration = configuration;
        _auditoria = auditoria;
    }

    public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto dto)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Ativo);

        if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash))
        {
            return ApiResponse<LoginResponseDto>.Erro("E-mail ou senha inválidos.");
        }

        usuario.UltimoLogin = DateTime.UtcNow;
        usuario.TokenRefresh = GerarRefreshToken();
        await _context.SaveChangesAsync();

        await _auditoria.RegistrarAsync("usuarios", usuario.Id, "LOGIN", usuario.Id, "Usuario",
            null, null, null, null, "/api/auth/login");

        var token = GerarTokenJWT(usuario);
        var expiracao = DateTime.UtcNow.AddHours(
            _configuration.GetValue<int>("JwtSettings:ExpirationHours", 24));

        return ApiResponse<LoginResponseDto>.Ok(new LoginResponseDto
        {
            Token = token,
            RefreshToken = usuario.TokenRefresh!,
            ExpiraEm = expiracao,
            Usuario = _mapper.Map<UsuarioDto>(usuario)
        });
    }

    public async Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto dto)
    {
        var principal = ObterPrincipalDoTokenExpirado(dto.Token);
        if (principal == null)
            return ApiResponse<LoginResponseDto>.Erro("Token inválido.");

        var email = principal.FindFirst(ClaimTypes.Email)?.Value;
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email && u.TokenRefresh == dto.RefreshToken && u.Ativo);

        if (usuario == null)
            return ApiResponse<LoginResponseDto>.Erro("Refresh token inválido.");

        usuario.TokenRefresh = GerarRefreshToken();
        await _context.SaveChangesAsync();

        var token = GerarTokenJWT(usuario);
        var expiracao = DateTime.UtcNow.AddHours(
            _configuration.GetValue<int>("JwtSettings:ExpirationHours", 24));

        return ApiResponse<LoginResponseDto>.Ok(new LoginResponseDto
        {
            Token = token,
            RefreshToken = usuario.TokenRefresh,
            ExpiraEm = expiracao,
            Usuario = _mapper.Map<UsuarioDto>(usuario)
        });
    }

    public async Task<ApiResponse<UsuarioDto>> RegistrarAsync(RegistrarUsuarioDto dto)
    {
        if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email))
            return ApiResponse<UsuarioDto>.Erro("E-mail já cadastrado.");

        if (dto.Senha != dto.ConfirmarSenha)
            return ApiResponse<UsuarioDto>.Erro("As senhas não conferem.");

        if (!Enum.TryParse<PerfilUsuario>(dto.Perfil, out var perfil))
            perfil = PerfilUsuario.Vendedor;

        var usuario = new Usuario
        {
            Nome = dto.Nome,
            Email = dto.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha, 12),
            Perfil = perfil,
            Telefone = dto.Telefone,
            Ativo = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        await _auditoria.RegistrarAsync("usuarios", usuario.Id, "INSERT", usuario.Id, "Usuario",
            null, null, null, $"{{\"nome\":\"{dto.Nome}\",\"email\":\"{dto.Email}\"}}", "/api/auth/registrar");

        return ApiResponse<UsuarioDto>.Ok(_mapper.Map<UsuarioDto>(usuario), "Usuário criado com sucesso.");
    }

    public async Task<ApiResponse<bool>> AlterarSenhaAsync(int usuarioId, AlterarSenhaDto dto)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario == null)
            return ApiResponse<bool>.Erro("Usuário não encontrado.");

        if (!BCrypt.Net.BCrypt.Verify(dto.SenhaAtual, usuario.SenhaHash))
            return ApiResponse<bool>.Erro("Senha atual incorreta.");

        if (dto.NovaSenha != dto.ConfirmarNovaSenha)
            return ApiResponse<bool>.Erro("As novas senhas não conferem.");

        usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.NovaSenha, 12);
        usuario.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Senha alterada com sucesso.");
    }

    public string GerarTokenJWT(Usuario usuario)
    {
        var secretKey = _configuration["JwtSettings:SecretKey"]!;
        var issuer = _configuration["JwtSettings:Issuer"]!;
        var audience = _configuration["JwtSettings:Audience"]!;
        var expirationHours = _configuration.GetValue<int>("JwtSettings:ExpirationHours", 24);

        var key = Encoding.ASCII.GetBytes(secretKey);
        var credenciais = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(JwtRegisteredClaimNames.Name, usuario.Nome),
            new Claim(ClaimTypes.Role, usuario.Perfil.ToString()),
            new Claim("perfil", usuario.Perfil.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expirationHours),
            signingCredentials: credenciais
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GerarRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private ClaimsPrincipal? ObterPrincipalDoTokenExpirado(string token)
    {
        var secretKey = _configuration["JwtSettings:SecretKey"]!;
        var issuer = _configuration["JwtSettings:Issuer"]!;
        var audience = _configuration["JwtSettings:Audience"]!;
        var key = Encoding.ASCII.GetBytes(secretKey);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}
