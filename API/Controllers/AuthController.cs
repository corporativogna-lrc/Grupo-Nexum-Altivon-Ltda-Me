using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Services;

namespace NexumAltivon.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Autentica um usuário e retorna token JWT + refresh token
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto dto)
    {
        var resultado = await _authService.LoginAsync(dto);
        if (!resultado.Sucesso)
            return Unauthorized(resultado);
        return Ok(resultado);
    }

    /// <summary>
    /// Renova o token JWT usando o refresh token
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> RefreshToken([FromBody] RefreshTokenRequestDto dto)
    {
        var resultado = await _authService.RefreshTokenAsync(dto);
        if (!resultado.Sucesso)
            return Unauthorized(resultado);
        return Ok(resultado);
    }

    /// <summary>
    /// Registra um novo usuário administrativo (requer SuperAdmin)
    /// </summary>
    [HttpPost("registrar")]
    [Authorize(Policy = "SuperAdmin")]
    [ProducesResponseType(typeof(ApiResponse<UsuarioDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> Registrar([FromBody] RegistrarUsuarioDto dto)
    {
        var resultado = await _authService.RegistrarAsync(dto);
        if (!resultado.Sucesso)
            return BadRequest(resultado);
        return CreatedAtAction(nameof(Login), resultado);
    }

    /// <summary>
    /// Altera a senha do usuário autenticado
    /// </summary>
    [HttpPost("alterar-senha")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> AlterarSenha([FromBody] AlterarSenhaDto dto)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var resultado = await _authService.AlterarSenhaAsync(userId, dto);
        if (!resultado.Sucesso)
            return BadRequest(resultado);
        return Ok(resultado);
    }
}
