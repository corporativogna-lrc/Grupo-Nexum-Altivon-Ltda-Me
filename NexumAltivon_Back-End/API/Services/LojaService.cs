using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NexumAltivon.API.Data;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Models;

namespace NexumAltivon.API.Services;

public class LojaService : ILojaService
{
    private readonly NexumDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogAuditoriaService _auditoria;

    public LojaService(NexumDbContext context, IMapper mapper, ILogAuditoriaService auditoria)
    {
        _context = context;
        _mapper = mapper;
        _auditoria = auditoria;
    }

    public async Task<ApiResponse<LojaDto>> ObterPorIdAsync(int id)
    {
        var loja = await _context.Lojas.FindAsync(id);
        if (loja == null)
            return ApiResponse<LojaDto>.Erro("Loja não encontrada.");
        return ApiResponse<LojaDto>.Ok(_mapper.Map<LojaDto>(loja));
    }

    public async Task<ApiResponse<LojaDto>> ObterPorSlugAsync(string slug)
    {
        var loja = await _context.Lojas.FirstOrDefaultAsync(l => l.Slug == slug);
        if (loja == null)
            return ApiResponse<LojaDto>.Erro("Loja não encontrada.");
        return ApiResponse<LojaDto>.Ok(_mapper.Map<LojaDto>(loja));
    }

    public async Task<ApiResponse<List<LojaDto>>> ListarTodasAsync()
    {
        var lojas = await _context.Lojas
            .OrderBy(l => l.OrdemExibicao)
            .ToListAsync();
        return ApiResponse<List<LojaDto>>.Ok(_mapper.Map<List<LojaDto>>(lojas));
    }

    public async Task<ApiResponse<LojaDto>> CriarAsync(CriarLojaDto dto)
    {
        if (await _context.Lojas.AnyAsync(l => l.Slug == dto.Slug))
            return ApiResponse<LojaDto>.Erro("Slug já existe.");

        var loja = _mapper.Map<Loja>(dto);
        _context.Lojas.Add(loja);
        await _context.SaveChangesAsync();

        await _auditoria.RegistrarAsync("lojas", loja.Id, "INSERT", null, "Usuario",
            null, null, null, $"{{\"nome\":\"{dto.Nome}\",\"slug\":\"{dto.Slug}\"}}", "/api/lojas");

        return ApiResponse<LojaDto>.Ok(_mapper.Map<LojaDto>(loja), "Loja criada com sucesso.");
    }

    public async Task<ApiResponse<LojaDto>> AtualizarAsync(int id, CriarLojaDto dto)
    {
        var loja = await _context.Lojas.FindAsync(id);
        if (loja == null)
            return ApiResponse<LojaDto>.Erro("Loja não encontrada.");

        var dadosAnteriores = $"{{\"nome\":\"{loja.Nome}\",\"ativa\":{loja.Ativa.ToString().ToLower()}}}";
        _mapper.Map(dto, loja);
        loja.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditoria.RegistrarAsync("lojas", loja.Id, "UPDATE", null, "Usuario",
            null, null, dadosAnteriores, $"{{\"nome\":\"{dto.Nome}\"}}", $"/api/lojas/{id}");

        return ApiResponse<LojaDto>.Ok(_mapper.Map<LojaDto>(loja), "Loja atualizada com sucesso.");
    }

    public async Task<ApiResponse<bool>> ExcluirAsync(int id)
    {
        var loja = await _context.Lojas.FindAsync(id);
        if (loja == null)
            return ApiResponse<bool>.Erro("Loja não encontrada.");

        _context.Lojas.Remove(loja);
        await _context.SaveChangesAsync();

        await _auditoria.RegistrarAsync("lojas", id, "DELETE", null, "Usuario",
            null, null, $"{{\"nome\":\"{loja.Nome}\"}}", null, $"/api/lojas/{id}");

        return ApiResponse<bool>.Ok(true, "Loja excluída com sucesso.");
    }
}
