/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using Microsoft.EntityFrameworkCore;

namespace NexumAltivon.API.ERP.SharedData;

public static class GenesisFinanceService
{
    private static readonly HashSet<string> StatusPagarEmAberto = new(StringComparer.OrdinalIgnoreCase)
    {
        "PENDENTE", "PARCIAL", "EM_ABERTO", "VENCIDO"
    };

    private static readonly HashSet<string> StatusReceberEmAberto = new(StringComparer.OrdinalIgnoreCase)
    {
        "PENDENTE", "PARCIAL", "EM_ABERTO", "VENCIDO"
    };

    private static string NormalizarStatusPagar(decimal valorOriginal, decimal valorPago)
    {
        if (valorPago <= 0m) return "PENDENTE";
        if (valorPago >= valorOriginal) return "PAGO";
        return "PARCIAL";
    }

    private static string NormalizarStatusReceber(decimal valorOriginal, decimal valorRecebido)
    {
        if (valorRecebido <= 0m) return "PENDENTE";
        if (valorRecebido >= valorOriginal) return "RECEBIDO";
        return "PARCIAL";
    }

    private static string NormalizarTipoReferencia(string? tipo) =>
        string.IsNullOrWhiteSpace(tipo) ? string.Empty : tipo.Trim().ToUpperInvariant();

    public static async Task<GenesisFinanceSummaryDto> GetResumoAsync(GenesisDbContext genesisDb, CancellationToken ct)
    {
        var hoje = DateTime.Today;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
        var fimMes = inicioMes.AddMonths(1);

        var contasReceberPendentes = await genesisDb.ContasReceber
            .Where(item => item.Status != null && StatusReceberEmAberto.Contains(item.Status))
            .SumAsync(item => (decimal?)(item.ValorOriginal - item.ValorRecebido), ct) ?? 0m;

        var contasPagarPendentes = await genesisDb.ContasPagar
            .Where(item => item.Status != null && StatusPagarEmAberto.Contains(item.Status))
            .SumAsync(item => (decimal?)(item.ValorOriginal - item.ValorPago), ct) ?? 0m;

        var entradasMes = await genesisDb.FluxoCaixa
            .Where(item => item.Data >= inicioMes && item.Data < fimMes && item.Tipo == "ENTRADA")
            .SumAsync(item => (decimal?)item.Valor, ct) ?? 0m;

        var saidasMes = await genesisDb.FluxoCaixa
            .Where(item => item.Data >= inicioMes && item.Data < fimMes && item.Tipo == "SAIDA")
            .SumAsync(item => (decimal?)item.Valor, ct) ?? 0m;

        var titulosReceberVencidos = await genesisDb.ContasReceber
            .CountAsync(item => item.DataVencimento < hoje && item.Status != null && StatusReceberEmAberto.Contains(item.Status), ct);

        var titulosPagarVencidos = await genesisDb.ContasPagar
            .CountAsync(item => item.DataVencimento < hoje && item.Status != null && StatusPagarEmAberto.Contains(item.Status), ct);

        return new GenesisFinanceSummaryDto(
            contasReceberPendentes,
            contasPagarPendentes,
            entradasMes,
            saidasMes,
            entradasMes - saidasMes,
            titulosReceberVencidos,
            titulosPagarVencidos,
            DateTime.UtcNow);
    }

    public static async Task<List<GenesisContaPagarDto>> ListarContasPagarAsync(GenesisDbContext genesisDb, CancellationToken ct)
    {
        return await genesisDb.ContasPagar
            .AsNoTracking()
            .OrderBy(item => item.DataVencimento)
            .Select(item => new GenesisContaPagarDto(
                item.Id,
                item.NumeroDocumento ?? string.Empty,
                item.FornecedorId,
                item.Descricao ?? "Conta a pagar sem descricao",
                item.ValorOriginal,
                item.ValorPago,
                item.ValorOriginal - item.ValorPago,
                item.DataEmissao,
                item.DataVencimento,
                item.DataPagamento,
                item.Status ?? "PENDENTE",
                item.FormaPagamento,
                item.NumeroBoleto))
            .ToListAsync(ct);
    }

    public static async Task<List<GenesisContaReceberDto>> ListarContasReceberAsync(GenesisDbContext genesisDb, CancellationToken ct)
    {
        return await genesisDb.ContasReceber
            .AsNoTracking()
            .OrderBy(item => item.DataVencimento)
            .Select(item => new GenesisContaReceberDto(
                item.Id,
                item.NumeroDocumento ?? string.Empty,
                item.ClienteId,
                item.Descricao ?? "Conta a receber sem descricao",
                item.ValorOriginal,
                item.ValorRecebido,
                item.ValorOriginal - item.ValorRecebido,
                item.DataEmissao,
                item.DataVencimento,
                item.DataRecebimento,
                item.Status ?? "PENDENTE",
                item.FormaRecebimento,
                item.NumeroPedidoReferencia))
            .ToListAsync(ct);
    }

    public static async Task<GenesisContaPagarDto> CriarContaPagarAsync(GenesisDbContext genesisDb, GenesisContaPagarCreateRequest request, CancellationToken ct)
    {
        var entity = new GenesisContaPagar
        {
            NumeroDocumento = request.NumeroDocumento.Trim(),
            FornecedorId = request.FornecedorId,
            Descricao = request.Descricao.Trim(),
            ValorOriginal = request.ValorOriginal,
            ValorPago = 0m,
            ValorMulta = 0m,
            ValorJuros = 0m,
            ValorDesconto = 0m,
            DataEmissao = request.DataEmissao,
            DataVencimento = request.DataVencimento,
            DataPagamento = null,
            Status = "PENDENTE",
            FormaPagamento = request.FormaPagamento?.Trim(),
            NumeroBoleto = request.NumeroBoleto?.Trim()
        };

        genesisDb.ContasPagar.Add(entity);
        await genesisDb.SaveChangesAsync(ct);

        return new GenesisContaPagarDto(
            entity.Id, entity.NumeroDocumento, entity.FornecedorId, entity.Descricao, entity.ValorOriginal, entity.ValorPago,
            entity.ValorOriginal - entity.ValorPago, entity.DataEmissao, entity.DataVencimento, entity.DataPagamento, entity.Status ?? "PENDENTE",
            entity.FormaPagamento, entity.NumeroBoleto);
    }

    public static async Task<GenesisContaReceberDto> CriarContaReceberAsync(GenesisDbContext genesisDb, GenesisContaReceberCreateRequest request, CancellationToken ct)
    {
        var entity = new GenesisContaReceber
        {
            NumeroDocumento = request.NumeroDocumento.Trim(),
            ClienteId = request.ClienteId,
            Descricao = request.Descricao.Trim(),
            ValorOriginal = request.ValorOriginal,
            ValorRecebido = 0m,
            ValorMulta = 0m,
            ValorJuros = 0m,
            ValorDesconto = 0m,
            DataEmissao = request.DataEmissao,
            DataVencimento = request.DataVencimento,
            DataRecebimento = null,
            Status = "PENDENTE",
            FormaRecebimento = request.FormaRecebimento?.Trim(),
            NumeroPedidoReferencia = request.NumeroPedidoReferencia?.Trim()
        };

        genesisDb.ContasReceber.Add(entity);
        await genesisDb.SaveChangesAsync(ct);

        return new GenesisContaReceberDto(
            entity.Id, entity.NumeroDocumento, entity.ClienteId, entity.Descricao, entity.ValorOriginal, entity.ValorRecebido,
            entity.ValorOriginal - entity.ValorRecebido, entity.DataEmissao, entity.DataVencimento, entity.DataRecebimento, entity.Status ?? "PENDENTE",
            entity.FormaRecebimento, entity.NumeroPedidoReferencia);
    }

    public static async Task<List<GenesisFinanceReferenciaDto>> ListarReferenciasAsync(GenesisDbContext genesisDb, string? tipo, CancellationToken ct)
    {
        var tipoNormalizado = NormalizarTipoReferencia(tipo);

        var query = genesisDb.FinanceiroReferencias
            .AsNoTracking()
            .Where(item => item.Ativo);

        if (!string.IsNullOrWhiteSpace(tipoNormalizado))
        {
            query = query.Where(item => item.Tipo == tipoNormalizado);
        }

        return await query
            .OrderBy(item => item.Tipo)
            .ThenBy(item => item.Ordem)
            .ThenBy(item => item.Descricao)
            .Select(item => new GenesisFinanceReferenciaDto(
                item.Id,
                item.Tipo,
                item.Codigo,
                item.Descricao,
                item.Ordem))
            .ToListAsync(ct);
    }

    public static async Task<GenesisFinanceReferenciaDto> CriarReferenciaAsync(GenesisDbContext genesisDb, GenesisFinanceReferenciaCreateRequest request, CancellationToken ct)
    {
        var tipo = NormalizarTipoReferencia(request.Tipo);
        var codigo = request.Codigo?.Trim().ToUpperInvariant() ?? string.Empty;
        var descricao = request.Descricao?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(tipo))
        {
            throw new ArgumentException("Tipo de referencia obrigatorio.");
        }

        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("Codigo de referencia obrigatorio.");
        }

        if (string.IsNullOrWhiteSpace(descricao))
        {
            throw new ArgumentException("Descricao de referencia obrigatoria.");
        }

        var entity = await genesisDb.FinanceiroReferencias
            .FirstOrDefaultAsync(item => item.Tipo == tipo && item.Codigo == codigo, ct);

        if (entity is null)
        {
            entity = new GenesisFinanceReferencia
            {
                Tipo = tipo,
                Codigo = codigo,
                Descricao = descricao,
                Ordem = request.Ordem,
                Ativo = true
            };
            genesisDb.FinanceiroReferencias.Add(entity);
        }
        else
        {
            entity.Descricao = descricao;
            entity.Ordem = request.Ordem;
            entity.Ativo = true;
        }

        await genesisDb.SaveChangesAsync(ct);

        return new GenesisFinanceReferenciaDto(entity.Id, entity.Tipo, entity.Codigo, entity.Descricao, entity.Ordem);
    }

    public static async Task<GenesisContaPagarDto?> BaixarContaPagarAsync(GenesisDbContext genesisDb, int id, GenesisBaixaPagarRequest request, CancellationToken ct)
    {
        var entity = await genesisDb.ContasPagar.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (entity is null) return null;

        entity.ValorPago += request.ValorPago;
        entity.DataPagamento = request.DataPagamento ?? DateTime.Now;
        entity.FormaPagamento = request.FormaPagamento?.Trim() ?? entity.FormaPagamento;
        entity.Status = NormalizarStatusPagar(entity.ValorOriginal, entity.ValorPago);

        genesisDb.FluxoCaixa.Add(new GenesisFluxoCaixa
        {
            Data = entity.DataPagamento.Value,
            Tipo = "SAIDA",
            Descricao = $"Baixa CP {entity.NumeroDocumento}",
            Valor = request.ValorPago,
            Categoria = "CONTAS_PAGAR",
            ContaPagarId = entity.Id,
            FormaPagamento = entity.FormaPagamento,
            ContaBancaria = null,
            Observacoes = "Baixa automática via ERP local",
            CriadoEm = DateTime.Now
        });

        await genesisDb.SaveChangesAsync(ct);

        return new GenesisContaPagarDto(
            entity.Id, entity.NumeroDocumento, entity.FornecedorId, entity.Descricao, entity.ValorOriginal, entity.ValorPago,
            entity.ValorOriginal - entity.ValorPago, entity.DataEmissao, entity.DataVencimento, entity.DataPagamento, entity.Status ?? "PENDENTE",
            entity.FormaPagamento, entity.NumeroBoleto);
    }

    public static async Task<GenesisContaReceberDto?> BaixarContaReceberAsync(GenesisDbContext genesisDb, int id, GenesisBaixaReceberRequest request, CancellationToken ct)
    {
        var entity = await genesisDb.ContasReceber.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (entity is null) return null;

        entity.ValorRecebido += request.ValorRecebido;
        entity.DataRecebimento = request.DataRecebimento ?? DateTime.Now;
        entity.FormaRecebimento = request.FormaRecebimento?.Trim() ?? entity.FormaRecebimento;
        entity.Status = NormalizarStatusReceber(entity.ValorOriginal, entity.ValorRecebido);

        genesisDb.FluxoCaixa.Add(new GenesisFluxoCaixa
        {
            Data = entity.DataRecebimento.Value,
            Tipo = "ENTRADA",
            Descricao = $"Baixa CR {entity.NumeroDocumento}",
            Valor = request.ValorRecebido,
            Categoria = "CONTAS_RECEBER",
            ContaReceberId = entity.Id,
            FormaPagamento = entity.FormaRecebimento,
            ContaBancaria = null,
            Observacoes = "Baixa automática via ERP local",
            CriadoEm = DateTime.Now
        });

        await genesisDb.SaveChangesAsync(ct);

        return new GenesisContaReceberDto(
            entity.Id, entity.NumeroDocumento, entity.ClienteId, entity.Descricao, entity.ValorOriginal, entity.ValorRecebido,
            entity.ValorOriginal - entity.ValorRecebido, entity.DataEmissao, entity.DataVencimento, entity.DataRecebimento, entity.Status ?? "PENDENTE",
            entity.FormaRecebimento, entity.NumeroPedidoReferencia);
    }

    public static async Task<List<GenesisBoletoDto>> ListarBoletosAsync(GenesisDbContext genesisDb, CancellationToken ct)
    {
        return await genesisDb.Boletos
            .AsNoTracking()
            .OrderByDescending(item => item.CriadoEm)
            .Select(item => new GenesisBoletoDto(
                item.Id,
                item.ContaReceberId,
                item.NossoNumero,
                item.LinhaDigitavel,
                item.CodigoBarras,
                item.Banco,
                item.Vencimento,
                item.Valor,
                item.Status ?? "EM_ABERTO",
                item.UrlBoleto,
                item.PdfUrl,
                item.CriadoEm))
            .ToListAsync(ct);
    }

    public static async Task<GenesisBoletoDto> CriarBoletoAsync(GenesisDbContext genesisDb, GenesisBoletoCreateRequest request, CancellationToken ct)
    {
        var entity = new GenesisBoleto
        {
            ContaReceberId = request.ContaReceberId,
            NossoNumero = request.NossoNumero?.Trim(),
            LinhaDigitavel = request.LinhaDigitavel?.Trim(),
            CodigoBarras = request.CodigoBarras?.Trim(),
            Banco = request.Banco?.Trim(),
            Vencimento = request.Vencimento,
            Valor = request.Valor,
            Status = "EM_ABERTO",
            UrlBoleto = request.UrlBoleto?.Trim(),
            PdfUrl = request.PdfUrl?.Trim(),
            CriadoEm = DateTime.Now
        };

        genesisDb.Boletos.Add(entity);
        await genesisDb.SaveChangesAsync(ct);

        return new GenesisBoletoDto(
            entity.Id,
            entity.ContaReceberId,
            entity.NossoNumero,
            entity.LinhaDigitavel,
            entity.CodigoBarras,
            entity.Banco,
            entity.Vencimento,
            entity.Valor,
            entity.Status ?? "EM_ABERTO",
            entity.UrlBoleto,
            entity.PdfUrl,
            entity.CriadoEm);
    }
}
