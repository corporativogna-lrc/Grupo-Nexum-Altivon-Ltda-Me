using Microsoft.EntityFrameworkCore;
using NexumAltivon.API.Data;
using NexumAltivon.API.DTOs;

namespace NexumAltivon.API.Services;

public class FinanceiroService : IFinanceiroService
{
    private readonly NexumDbContext _context;

    public FinanceiroService(NexumDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<decimal>> ObterFaturamentoAsync(DateTime inicio, DateTime fim, int? lojaId = null)
    {
        var query = _context.Pedidos
            .Where(p => p.CreatedAt >= inicio && p.CreatedAt <= fim)
            .Where(p => p.Status != StatusPedido.Cancelado && p.Status != StatusPedido.Reembolsado)
            .AsQueryable();

        if (lojaId.HasValue)
            query = query.Where(p => p.LojaId == lojaId.Value);

        var faturamento = await query.SumAsync(p => p.Total);
        return ApiResponse<decimal>.Ok(faturamento, $"Faturamento de {inicio:dd/MM/yyyy} a {fim:dd/MM/yyyy}");
    }

    public async Task<ApiResponse<Dictionary<string, decimal>>> ObterFaturamentoPorLojaAsync(DateTime inicio, DateTime fim)
    {
        var resultado = await _context.Pedidos
            .Where(p => p.CreatedAt >= inicio && p.CreatedAt <= fim)
            .Where(p => p.Status != StatusPedido.Cancelado && p.Status != StatusPedido.Reembolsado)
            .Include(p => p.Loja)
            .GroupBy(p => p.Loja != null ? p.Loja.Nome : "Sem Loja")
            .Select(g => new { Loja = g.Key, Total = g.Sum(p => p.Total) })
            .ToDictionaryAsync(x => x.Loja, x => x.Total);

        return ApiResponse<Dictionary<string, decimal>>.Ok(resultado);
    }
}
