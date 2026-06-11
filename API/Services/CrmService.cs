using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NexumAltivon.API.Data;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Models;

namespace NexumAltivon.API.Services;

public class CrmService : ICrmService
{
    private readonly NexumDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogAuditoriaService _auditoria;

    public CrmService(NexumDbContext context, IMapper mapper, ILogAuditoriaService auditoria)
    {
        _context = context;
        _mapper = mapper;
        _auditoria = auditoria;
    }

    public async Task<ApiResponse<CrmLeadDto>> ObterPorIdAsync(int id)
    {
        var lead = await _context.CrmLeads.FindAsync(id);
        if (lead == null)
            return ApiResponse<CrmLeadDto>.Erro("Lead não encontrado.");
        return ApiResponse<CrmLeadDto>.Ok(_mapper.Map<CrmLeadDto>(lead));
    }

    public async Task<ApiResponse<List<CrmLeadDto>>> ListarAsync(PaginacaoDto paginacao, string? tipo = null, string? status = null)
    {
        var query = _context.CrmLeads.AsQueryable();

        if (!string.IsNullOrEmpty(tipo))
            query = query.Where(l => l.Tipo.ToString() == tipo);
        if (!string.IsNullOrEmpty(status))
            query = query.Where(l => l.Status.ToString() == status);
        if (!string.IsNullOrWhiteSpace(paginacao.Busca))
            query = query.Where(l => l.Nome.Contains(paginacao.Busca) || (l.Email != null && l.Email.Contains(paginacao.Busca)));

        var total = await query.CountAsync();
        var leads = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((paginacao.Pagina - 1) * paginacao.ItensPorPagina)
            .Take(paginacao.ItensPorPagina)
            .ToListAsync();

        var totalPaginas = (int)Math.Ceiling(total / (double)paginacao.ItensPorPagina);

        return ApiResponse<List<CrmLeadDto>>.Ok(
            _mapper.Map<List<CrmLeadDto>>(leads),
            total: total, pagina: paginacao.Pagina, totalPaginas: totalPaginas);
    }

    public async Task<ApiResponse<CrmLeadDto>> CriarAsync(CriarLeadDto dto)
    {
        var lead = _mapper.Map<CrmLead>(dto);
        lead.Status = StatusLead.Novo;
        lead.CreatedAt = DateTime.UtcNow;

        _context.CrmLeads.Add(lead);
        await _context.SaveChangesAsync();

        await _auditoria.RegistrarAsync("crm_leads", lead.Id, "INSERT", null, "Sistema",
            null, null, null, $"{{\"nome\":\"{dto.Nome}\",\"tipo\":\"{dto.Tipo}\"}}", "/api/crm/leads");

        return ApiResponse<CrmLeadDto>.Ok(_mapper.Map<CrmLeadDto>(lead), "Lead criado com sucesso.");
    }

    public async Task<ApiResponse<CrmLeadDto>> AtualizarAsync(int id, AtualizarLeadDto dto)
    {
        var lead = await _context.CrmLeads.FindAsync(id);
        if (lead == null)
            return ApiResponse<CrmLeadDto>.Erro("Lead não encontrado.");

        if (!string.IsNullOrEmpty(dto.Status) && Enum.TryParse<StatusLead>(dto.Status, out var novoStatus))
            lead.Status = novoStatus;
        if (!string.IsNullOrEmpty(dto.Prioridade) && Enum.TryParse<PrioridadeLead>(dto.Prioridade, out var novaPrioridade))
            lead.Prioridade = novaPrioridade;
        if (dto.ResponsavelId.HasValue)
            lead.ResponsavelId = dto.ResponsavelId.Value;
        if (dto.Anotacoes != null)
            lead.Anotacoes = dto.Anotacoes;

        lead.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<CrmLeadDto>.Ok(_mapper.Map<CrmLeadDto>(lead), "Lead atualizado com sucesso.");
    }

    public async Task<ApiResponse<bool>> ExcluirAsync(int id)
    {
        var lead = await _context.CrmLeads.FindAsync(id);
        if (lead == null)
            return ApiResponse<bool>.Erro("Lead não encontrado.");

        _context.CrmLeads.Remove(lead);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Lead excluído com sucesso.");
    }
}
