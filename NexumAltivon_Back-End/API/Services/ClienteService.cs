/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NexumAltivon.API.Data;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Models;

namespace NexumAltivon.API.Services;

public class ClienteService : IClienteService
{
    private readonly NexumDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogAuditoriaService _auditoria;
    private readonly INotificacaoService _notificacao;

    public ClienteService(NexumDbContext context, IMapper mapper, ILogAuditoriaService auditoria, INotificacaoService notificacao)
    {
        _context = context;
        _mapper = mapper;
        _auditoria = auditoria;
        _notificacao = notificacao;
    }

    public async Task<ApiResponse<ClienteDto>> ObterPorIdAsync(int id)
    {
        var cliente = await _context.Clientes
            .Include(c => c.Enderecos)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (cliente == null)
            return ApiResponse<ClienteDto>.Erro("Cliente não encontrado.");
        return ApiResponse<ClienteDto>.Ok(_mapper.Map<ClienteDto>(cliente));
    }

    public async Task<ApiResponse<ClienteDto>> ObterPorEmailAsync(string email)
    {
        var cliente = await _context.Clientes
            .Include(c => c.Enderecos)
            .FirstOrDefaultAsync(c => c.Email == email);
        if (cliente == null)
            return ApiResponse<ClienteDto>.Erro("Cliente não encontrado.");
        return ApiResponse<ClienteDto>.Ok(_mapper.Map<ClienteDto>(cliente));
    }

    public async Task<ApiResponse<List<ClienteDto>>> ListarAsync(PaginacaoDto paginacao)
    {
        var query = _context.Clientes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(paginacao.Busca))
        {
            query = query.Where(c => c.Nome.Contains(paginacao.Busca) || c.Email.Contains(paginacao.Busca));
        }

        var total = await query.CountAsync();
        var clientes = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((paginacao.Pagina - 1) * paginacao.ItensPorPagina)
            .Take(paginacao.ItensPorPagina)
            .ToListAsync();

        var totalPaginas = (int)Math.Ceiling(total / (double)paginacao.ItensPorPagina);

        return ApiResponse<List<ClienteDto>>.Ok(
            _mapper.Map<List<ClienteDto>>(clientes),
            total: total, pagina: paginacao.Pagina, totalPaginas: totalPaginas);
    }

    public async Task<ApiResponse<ClienteDto>> CriarAsync(CriarClienteDto dto)
    {
        var clienteExistente = await _context.Clientes.FirstOrDefaultAsync(c =>
            c.Email == dto.Email ||
            (!string.IsNullOrWhiteSpace(dto.CpfCnpj) && c.CpfCnpj == dto.CpfCnpj));

        if (clienteExistente is not null)
        {
            if (string.IsNullOrWhiteSpace(clienteExistente.SenhaHash) && !string.IsNullOrWhiteSpace(dto.Senha))
            {
                clienteExistente.SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha, 12);
            }

            if (string.IsNullOrWhiteSpace(clienteExistente.Telefone) && !string.IsNullOrWhiteSpace(dto.Telefone))
            {
                clienteExistente.Telefone = dto.Telefone;
            }

            clienteExistente.Newsletter = dto.Newsletter;
            clienteExistente.UpdatedAt = DateTime.UtcNow;

            if (clienteExistente.Status != StatusCliente.Ativo)
            {
                clienteExistente.Status = StatusCliente.Pendente;
                clienteExistente.TokenConfirmacaoEmail ??= Guid.NewGuid().ToString("N");
                var baseUrlExistente = Environment.GetEnvironmentVariable("NEXUM_PUBLIC_APP_URL")?.Trim().TrimEnd('/') ?? "https://nexumaltivon.com.br";
                var linkExistente = $"{baseUrlExistente}/confirmar-cadastro.html?token={Uri.EscapeDataString(clienteExistente.TokenConfirmacaoEmail)}";
                await _notificacao.EnviarConfirmacaoCadastroAsync(clienteExistente, linkExistente);
            }

            await _context.SaveChangesAsync();

            var dtoExistente = _mapper.Map<ClienteDto>(clienteExistente);
            return ApiResponse<ClienteDto>.Ok(
                dtoExistente,
                clienteExistente.Status == StatusCliente.Ativo
                    ? "Cliente já cadastrado. Registro existente reutilizado."
                    : "Cliente já cadastrado. Reenviamos o link de confirmação para liberar o acesso.");
        }

        var cliente = _mapper.Map<Cliente>(dto);
        if (!string.IsNullOrEmpty(dto.Senha))
            cliente.SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha, 12);
        cliente.Status = StatusCliente.Pendente;
        cliente.TokenConfirmacaoEmail = Guid.NewGuid().ToString("N");
        cliente.ConfirmadoEm = null;
        cliente.UltimoAcesso = null;

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        var baseUrl = Environment.GetEnvironmentVariable("NEXUM_PUBLIC_APP_URL")?.Trim().TrimEnd('/') ?? "https://nexumaltivon.com.br";
        var linkConfirmacao = $"{baseUrl}/confirmar-cadastro.html?token={Uri.EscapeDataString(cliente.TokenConfirmacaoEmail)}";
        await _notificacao.EnviarConfirmacaoCadastroAsync(cliente, linkConfirmacao);

        await _auditoria.RegistrarAsync("clientes", cliente.Id, "INSERT", null, "Sistema",
            null, null, null, $"{{\"nome\":\"{dto.Nome}\",\"email\":\"{dto.Email}\"}}", "/api/clientes");

        return ApiResponse<ClienteDto>.Ok(_mapper.Map<ClienteDto>(cliente), "Cadastro criado. Verifique seu e-mail para confirmar o acesso.");
    }

    public async Task<ApiResponse<ClienteDto>> ConfirmarEmailAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return ApiResponse<ClienteDto>.Erro("Token de confirmação inválido.");

        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.TokenConfirmacaoEmail == token);

        if (cliente == null)
            return ApiResponse<ClienteDto>.Erro("Token de confirmação não encontrado.");

        cliente.Status = StatusCliente.Ativo;
        cliente.ConfirmadoEm = DateTime.UtcNow;
        cliente.TokenConfirmacaoEmail = null;
        cliente.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditoria.RegistrarAsync("clientes", cliente.Id, "CONFIRMAR_EMAIL", null, "Sistema",
            null, null, null, $"{{\"email\":\"{cliente.Email}\"}}", "/api/clientes/confirmar");

        return ApiResponse<ClienteDto>.Ok(_mapper.Map<ClienteDto>(cliente), "Cadastro confirmado com sucesso. Agora você já pode acessar a área do cliente.");
    }

    public async Task<ApiResponse<ClienteDto>> AtualizarAsync(int id, AtualizarClienteDto dto)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
            return ApiResponse<ClienteDto>.Erro("Cliente não encontrado.");

        var email = dto.Email?.Trim();
        if (!string.IsNullOrWhiteSpace(email) && !string.Equals(cliente.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            var emailJaExiste = await _context.Clientes.AnyAsync(c => c.Id != cliente.Id && c.Email == email);
            if (emailJaExiste)
                return ApiResponse<ClienteDto>.Erro("Já existe outro cliente com este e-mail.");
            cliente.Email = email;
        }

        var cpfCnpj = string.IsNullOrWhiteSpace(dto.CpfCnpj) ? null : dto.CpfCnpj.Trim();
        if (!string.IsNullOrWhiteSpace(cpfCnpj) && !string.Equals(cliente.CpfCnpj, cpfCnpj, StringComparison.OrdinalIgnoreCase))
        {
            var cpfJaExiste = await _context.Clientes.AnyAsync(c => c.Id != cliente.Id && c.CpfCnpj == cpfCnpj);
            if (cpfJaExiste)
                return ApiResponse<ClienteDto>.Erro("Já existe outro cliente com este CPF/CNPJ.");
            cliente.CpfCnpj = cpfCnpj;
        }

        cliente.Nome = dto.Nome.Trim();
        cliente.Telefone = dto.Telefone;
        cliente.Whatsapp = dto.Whatsapp;
        cliente.Newsletter = dto.Newsletter;
        cliente.Vip = dto.Vip;
        cliente.Status = Enum.TryParse<StatusCliente>(dto.Status, true, out var status) ? status : cliente.Status;
        cliente.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<ClienteDto>.Ok(_mapper.Map<ClienteDto>(cliente), "Cliente atualizado com sucesso.");
    }

    public async Task<ApiResponse<bool>> ExcluirAsync(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
            return ApiResponse<bool>.Erro("Cliente não encontrado.");

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Cliente excluído com sucesso.");
    }

    public async Task<ApiResponse<EnderecoDto>> AdicionarEnderecoAsync(int clienteId, CriarEnderecoDto dto)
    {
        var cliente = await _context.Clientes.FindAsync(clienteId);
        if (cliente == null)
            return ApiResponse<EnderecoDto>.Erro("Cliente não encontrado.");

        if (dto.Padrao)
        {
            var enderecosPadrao = await _context.Enderecos
                .Where(e => e.ClienteId == clienteId && e.Padrao)
                .ToListAsync();
            foreach (var ep in enderecosPadrao)
                ep.Padrao = false;
        }

        var endereco = _mapper.Map<Endereco>(dto);
        endereco.ClienteId = clienteId;
        _context.Enderecos.Add(endereco);
        await _context.SaveChangesAsync();

        return ApiResponse<EnderecoDto>.Ok(_mapper.Map<EnderecoDto>(endereco), "Endereço adicionado com sucesso.");
    }

    public async Task<ApiResponse<bool>> RemoverEnderecoAsync(int clienteId, int enderecoId)
    {
        var endereco = await _context.Enderecos
            .FirstOrDefaultAsync(e => e.Id == enderecoId && e.ClienteId == clienteId);
        if (endereco == null)
            return ApiResponse<bool>.Erro("Endereço não encontrado.");

        _context.Enderecos.Remove(endereco);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Endereço removido com sucesso.");
    }
}
