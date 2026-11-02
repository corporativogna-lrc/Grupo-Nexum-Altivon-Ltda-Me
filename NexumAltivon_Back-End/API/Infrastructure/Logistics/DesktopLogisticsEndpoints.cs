using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using NexumAltivon.API.Data;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Models;

namespace NexumAltivon.API.Infrastructure.Logistics;

public static class DesktopLogisticsEndpoints
{
    public static IEndpointRouteBuilder MapDesktopLogisticsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/logistica/expedicoes", [Authorize(Policy = "Gerente")] async (
            int? limite, string? status, NexumDbContext db, CancellationToken ct) =>
        {
            var query = db.Pedidos.AsNoTracking()
                .Where(p => p.Status == StatusPedido.Pago || p.Status == StatusPedido.EmSeparacao || p.Status == StatusPedido.Enviado)
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (!Enum.TryParse<StatusPedido>(status, true, out var parsed) || parsed is not (StatusPedido.Pago or StatusPedido.EmSeparacao or StatusPedido.Enviado))
                    return Results.BadRequest(ApiResponse<string>.Erro("Filtro de status logístico inválido."));
                query = query.Where(p => p.Status == parsed);
            }

            var take = Math.Clamp(limite ?? 200, 1, 200);
            var pedidos = await query.Include(p => p.Cliente)
                .OrderByDescending(p => p.UpdatedAt)
                .Take(take)
                .Select(p => new
                {
                    Pedido = p,
                    RowVersion = EF.Property<byte[]>(p, "RowVersion"),
                    UltimoRastreamento = db.LogisticaRastreamentoConsultas
                        .Where(r => r.PedidoId == p.Id)
                        .OrderByDescending(r => r.ConsultadoAt)
                        .Select(r => new { r.Operacional, r.StatusExterno, r.ConsultadoAt })
                        .FirstOrDefault()
                })
                .ToListAsync(ct);

            var dtos = pedidos.Select(item => new DesktopLogisticaExpedicaoDto(
                item.Pedido.Id,
                item.Pedido.NumeroPedido,
                item.Pedido.Cliente?.Nome ?? "Cliente não identificado",
                FormatStatus(item.Pedido.Status),
                item.Pedido.FreteMetodo,
                item.Pedido.FreteTransportadora,
                item.Pedido.FretePrazoDias,
                item.Pedido.FreteCodigoRastreio,
                item.Pedido.DataEnvio,
                item.Pedido.DataEntrega,
                item.Pedido.DataEnvio?.AddDays(item.Pedido.FretePrazoDias),
                item.UltimoRastreamento?.Operacional ?? false,
                item.UltimoRastreamento?.StatusExterno,
                item.UltimoRastreamento?.ConsultadoAt,
                Convert.ToBase64String(item.RowVersion),
                item.Pedido.CreatedAt,
                item.Pedido.UpdatedAt)).ToList();

            return Results.Ok(ApiResponse<List<DesktopLogisticaExpedicaoDto>>.Ok(dtos, total: dtos.Count));
        });

        endpoints.MapPut("/api/pedidos/{id:int}/logistica", [Authorize(Policy = "Gerente")] async (
            int id, DesktopLogisticaAtualizacaoRequest request, NexumDbContext db, CancellationToken ct) =>
        {
            if (!TryRowVersion(request.RowVersion, out var rowVersion))
                return Results.BadRequest(ApiResponse<string>.Erro("Versão do pedido inválida; atualize a lista e tente novamente."));
            var metodo = request.FreteMetodo?.Trim();
            var transportadora = request.FreteTransportadora?.Trim();
            var codigo = request.FreteCodigoRastreio?.Trim();
            if (string.IsNullOrWhiteSpace(metodo) || metodo.Length > 50
                || string.IsNullOrWhiteSpace(transportadora) || transportadora.Length > 50
                || string.IsNullOrWhiteSpace(codigo) || codigo.Length > 50
                || request.FretePrazoDias is < 1 or > 120)
                return Results.BadRequest(ApiResponse<string>.Erro("Informe método, transportadora, rastreio e prazo entre 1 e 120 dias."));

            var pedido = await db.Pedidos.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (pedido is null) return Results.NotFound(ApiResponse<string>.Erro("Pedido não encontrado."));
            if (pedido.Status is not (StatusPedido.Pago or StatusPedido.EmSeparacao or StatusPedido.Enviado))
                return Results.Conflict(ApiResponse<string>.Erro("O pedido não está em um estado que permita alteração logística."));
            db.Entry(pedido).Property<byte[]>("RowVersion").OriginalValue = rowVersion;
            pedido.FreteMetodo = metodo;
            pedido.FreteTransportadora = transportadora;
            pedido.FreteCodigoRastreio = codigo;
            pedido.FretePrazoDias = request.FretePrazoDias;
            pedido.UpdatedAt = DateTime.UtcNow;
            try { await db.SaveChangesAsync(ct); }
            catch (DbUpdateConcurrencyException) { return Results.Conflict(ApiResponse<string>.Erro("O pedido foi alterado por outra sessão. Recarregue antes de salvar.")); }
            return Results.Ok(ApiResponse<DesktopLogisticaPedidoAtualizadoDto>.Ok(ToPedidoDto(pedido), "Dados logísticos gravados."));
        });

        endpoints.MapPost("/api/pedidos/{id:int}/fluxo-operacional", [Authorize(Policy = "Gerente")] async (
            int id, DesktopPedidoFluxoRequest request, NexumDbContext db, CancellationToken ct) =>
        {
            if (!TryRowVersion(request.RowVersion, out var rowVersion))
                return Results.BadRequest(ApiResponse<string>.Erro("Versão do pedido inválida; atualize a lista e tente novamente."));
            var pedido = await db.Pedidos.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (pedido is null) return Results.NotFound(ApiResponse<string>.Erro("Pedido não encontrado."));
            db.Entry(pedido).Property<byte[]>("RowVersion").OriginalValue = rowVersion;
            if (pedido.Status == StatusPedido.Pago) pedido.Status = StatusPedido.EmSeparacao;
            else if (pedido.Status == StatusPedido.EmSeparacao)
            {
                if (string.IsNullOrWhiteSpace(pedido.FreteMetodo) || string.IsNullOrWhiteSpace(pedido.FreteTransportadora)
                    || string.IsNullOrWhiteSpace(pedido.FreteCodigoRastreio) || pedido.FretePrazoDias is < 1 or > 120)
                    return Results.Conflict(ApiResponse<string>.Erro("Complete os dados de frete e rastreio antes de enviar o pedido."));
                pedido.Status = StatusPedido.Enviado;
                pedido.DataEnvio = DateTime.UtcNow;
            }
            else return Results.Conflict(ApiResponse<string>.Erro("Somente pedidos pagos ou em separação podem avançar neste fluxo."));
            pedido.UpdatedAt = DateTime.UtcNow;
            try { await db.SaveChangesAsync(ct); }
            catch (DbUpdateConcurrencyException) { return Results.Conflict(ApiResponse<string>.Erro("O pedido foi alterado por outra sessão. Recarregue antes de avançar.")); }
            return Results.Ok(ApiResponse<DesktopLogisticaPedidoAtualizadoDto>.Ok(ToPedidoDto(pedido), "Fluxo operacional gravado."));
        });

        endpoints.MapGet("/api/logistica/rastreamento/{codigo}", [Authorize(Policy = "Gerente")] async (
            string codigo, NexumDbContext db, LogisticaTrackingService tracking, CancellationToken ct) =>
        {
            var normalizedCode = codigo.Trim();
            if (normalizedCode.Length is < 1 or > 50)
                return Results.BadRequest(ApiResponse<string>.Erro("Código de rastreio inválido."));
            var pedido = await db.Pedidos.Include(p => p.Loja)
                .FirstOrDefaultAsync(p => p.FreteCodigoRastreio == normalizedCode, ct);
            if (pedido is null) return Results.NotFound(ApiResponse<string>.Erro("Nenhum pedido corresponde ao código de rastreio."));

            var result = await tracking.ConsultarAsync(normalizedCode, ct);
            var now = DateTime.UtcNow;
            var consulta = new LogisticaRastreamentoConsulta
            {
                PedidoId = pedido.Id,
                CodigoRastreio = normalizedCode,
                Provedor = result.Fonte.Length > 120 ? result.Fonte[..120] : result.Fonte,
                Configurada = result.Configurada,
                Operacional = result.Operacional,
                HttpStatusCode = result.HttpStatusCode,
                StatusExterno = result.StatusExterno is { Length: > 160 } external ? external[..160] : result.StatusExterno,
                QuantidadeEventos = result.Eventos.Count,
                EventosJson = JsonSerializer.Serialize(result.Eventos),
                PendenciasJson = JsonSerializer.Serialize(result.Pendencias),
                RespostaSha256 = result.RespostaSha256,
                ConsultadoAt = now
            };
            db.LogisticaRastreamentoConsultas.Add(consulta);
            await db.SaveChangesAsync(ct);
            var dto = new DesktopLogisticaRastreamentoDto(
                consulta.Id, normalizedCode, pedido.Id, pedido.NumeroPedido, pedido.FreteTransportadora, pedido.FreteMetodo,
                FormatStatus(pedido.Status), pedido.DataEnvio, pedido.DataEntrega,
                pedido.DataEnvio?.AddDays(pedido.FretePrazoDias), result.Configurada, result.Operacional, result.Fonte,
                result.HttpStatusCode, result.StatusExterno,
                result.Eventos.Select(e => new DesktopLogisticaRastreamentoEventoDto(e.DataHora, e.Status, e.Local, e.Descricao)).ToList(),
                result.Pendencias, result.RespostaSha256, now);
            return Results.Ok(ApiResponse<DesktopLogisticaRastreamentoDto>.Ok(dto,
                result.Operacional ? "Rastreamento atualizado." : "Consulta registrada com pendências; integração externa sem confirmação."));
        });

        return endpoints;
    }

    private static bool TryRowVersion(string? encoded, out byte[] rowVersion)
    {
        try { rowVersion = Convert.FromBase64String(encoded ?? ""); return rowVersion.Length is > 0 and <= 64; }
        catch (FormatException) { rowVersion = []; return false; }
    }

    private static string FormatStatus(StatusPedido status) => status switch
    {
        StatusPedido.EmSeparacao => "Processando",
        _ => status.ToString()
    };

    private static DesktopLogisticaPedidoAtualizadoDto ToPedidoDto(Pedido pedido) => new(
        pedido.Id, pedido.NumeroPedido, pedido.Total, FormatStatus(pedido.Status), pedido.CreatedAt,
        pedido.StatusPagamento.ToString(), pedido.MeioPagamento, pedido.GatewayPagamento, pedido.GatewayTransacaoId,
        pedido.FreteValor, pedido.FreteMetodo, pedido.FreteTransportadora, pedido.FretePrazoDias, pedido.FreteCodigoRastreio);
}
