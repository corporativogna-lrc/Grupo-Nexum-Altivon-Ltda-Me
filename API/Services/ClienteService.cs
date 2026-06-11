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

    public ClienteService(NexumDbContext context, IMapper mapper, ILogAuditoriaService auditoria)
    {
        _context = context;
        _mapper = mapper;
        _auditoria = auditoria;
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
        if (await _context.Clientes.AnyAsync(c => c.Email == dto.Email))
            return ApiResponse<ClienteDto>.Erro("E-mail já cadastrado.");

        if (!string.IsNullOrEmpty(dto.CpfCnpj) && await _context.Clientes.AnyAsync(c => c.CpfCnpj == dto.CpfCnpj))
            return ApiResponse<ClienteDto>.Erro("CPF/CNPJ já cadastrado.");

        var cliente = _mapper.Map<Cliente>(dto);
        if (!string.IsNullOrEmpty(dto.Senha))
            cliente.SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha, 12);

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        await _auditoria.RegistrarAsync("clientes", cliente.Id, "INSERT", null, "Sistema",
            null, null, null, $"{{\"nome\":\"{dto.Nome}\",\"email\":\"{dto.Email}\"}}", "/api/clientes");

        return ApiResponse<ClienteDto>.Ok(_mapper.Map<ClienteDto>(cliente), "Cliente criado com sucesso.");
    }

    public async Task<ApiResponse<ClienteDto>> AtualizarAsync(int id, AtualizarClienteDto dto)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
            return ApiResponse<ClienteDto>.Erro("Cliente não encontrado.");

        _mapper.Map(dto, cliente);
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
