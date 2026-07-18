/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7181
 */

using System.Data;
using Microsoft.EntityFrameworkCore;

namespace NexumAltivon.API.ERP.SharedData;

public static class GenesisFinanceService
{
    private static readonly HashSet<string> StatusPagarEmAberto = new(StringComparer.OrdinalIgnoreCase)
    {
        "ABERTO", "PENDENTE", "PARCIAL", "EM_ABERTO", "VENCIDO"
    };

    private static readonly HashSet<string> StatusReceberEmAberto = new(StringComparer.OrdinalIgnoreCase)
    {
        "PENDENTE", "PARCIAL", "EM_ABERTO", "VENCIDO"
    };

    private static readonly HashSet<string> StatusPagarRelatorio = new(StatusPagarEmAberto, StringComparer.OrdinalIgnoreCase)
    {
        "PAGO"
    };

    private static readonly HashSet<string> StatusReceberRelatorio = new(StatusReceberEmAberto, StringComparer.OrdinalIgnoreCase)
    {
        "RECEBIDO"
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

    public static Task<List<GenesisContaPagarDto>> ListarContasPagarAsync(GenesisDbContext genesisDb, CancellationToken ct)
        => ListarContasPagarAsync(genesisDb, null, null, null, ct);

    public static async Task<List<GenesisContaPagarDto>> ListarContasPagarAsync(
        GenesisDbContext genesisDb,
        DateTime? inicio,
        DateTime? fim,
        string? status,
        CancellationToken ct)
    {
        var statusNormalizado = ValidarFiltrosRelatorio(inicio, fim, status, StatusPagarRelatorio);
        var query = genesisDb.ContasPagar.AsNoTracking();
        if (inicio.HasValue)
        {
            var inicioInclusivo = inicio.Value.Date;
            query = query.Where(item => item.DataVencimento >= inicioInclusivo);
        }

        if (fim.HasValue)
        {
            var fimExclusivo = fim.Value.Date.AddDays(1);
            query = query.Where(item => item.DataVencimento < fimExclusivo);
        }

        if (statusNormalizado is not null)
        {
            query = query.Where(item => item.Status == statusNormalizado);
        }

        var entities = await query
            .OrderBy(item => item.DataVencimento)
            .ThenBy(item => item.NumeroDocumento)
            .ToListAsync(ct);

        return entities.Select(ToContaPagarDto).ToList();
    }

    public static Task<List<GenesisContaReceberDto>> ListarContasReceberAsync(GenesisDbContext genesisDb, CancellationToken ct)
        => ListarContasReceberAsync(genesisDb, null, null, null, ct);

    public static async Task<List<GenesisContaReceberDto>> ListarContasReceberAsync(
        GenesisDbContext genesisDb,
        DateTime? inicio,
        DateTime? fim,
        string? status,
        CancellationToken ct)
    {
        var statusNormalizado = ValidarFiltrosRelatorio(inicio, fim, status, StatusReceberRelatorio);
        var query = genesisDb.ContasReceber.AsNoTracking();
        if (inicio.HasValue)
        {
            var inicioInclusivo = inicio.Value.Date;
            query = query.Where(item => item.DataVencimento >= inicioInclusivo);
        }

        if (fim.HasValue)
        {
            var fimExclusivo = fim.Value.Date.AddDays(1);
            query = query.Where(item => item.DataVencimento < fimExclusivo);
        }

        if (statusNormalizado is not null)
        {
            query = query.Where(item => item.Status == statusNormalizado);
        }

        var entities = await query
            .OrderBy(item => item.DataVencimento)
            .ThenBy(item => item.NumeroDocumento)
            .ToListAsync(ct);

        return entities.Select(ToContaReceberDto).ToList();
    }

    private static string? ValidarFiltrosRelatorio(
        DateTime? inicio,
        DateTime? fim,
        string? status,
        IReadOnlySet<string> statusPermitidos)
    {
        if (inicio.HasValue && fim.HasValue)
        {
            if (fim.Value.Date < inicio.Value.Date)
            {
                throw new ArgumentException("A data final nao pode ser anterior a data inicial.");
            }

            if ((fim.Value.Date - inicio.Value.Date).TotalDays > 366)
            {
                throw new ArgumentException("O periodo do relatorio nao pode exceder 366 dias.");
            }
        }

        if (fim.HasValue && fim.Value.Date == DateTime.MaxValue.Date)
        {
            throw new ArgumentException("A data final informada esta fora do intervalo suportado.");
        }

        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        var normalized = status.Trim().ToUpperInvariant();
        if (!statusPermitidos.Contains(normalized))
        {
            throw new ArgumentException($"Status financeiro invalido: {normalized}.");
        }

        return normalized;
    }

    public static async Task<GenesisContaPagarDto?> ObterContaPagarAsync(GenesisDbContext genesisDb, int id, CancellationToken ct)
    {
        var entity = await genesisDb.ContasPagar.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, ct);
        return entity is null ? null : ToContaPagarDto(entity);
    }

    public static async Task<GenesisContaReceberDto?> ObterContaReceberAsync(GenesisDbContext genesisDb, int id, CancellationToken ct)
    {
        var entity = await genesisDb.ContasReceber.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, ct);
        return entity is null ? null : ToContaReceberDto(entity);
    }

    public static async Task<GenesisContaPagarDto> CriarContaPagarAsync(GenesisDbContext genesisDb, GenesisContaPagarCreateRequest request, CancellationToken ct)
    {
        ValidarContaPagar(request);
        var numeroDocumento = request.NumeroDocumento.Trim().ToUpperInvariant();
        var strategy = genesisDb.Database.CreateExecutionStrategy();
        var createdId = await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await genesisDb.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            if (await genesisDb.ContasPagar.AnyAsync(item => item.NumeroDocumento == numeroDocumento, ct))
            {
                throw new InvalidOperationException($"Ja existe conta a pagar com o documento {numeroDocumento}.");
            }

            var entity = new GenesisContaPagar
            {
                NumeroDocumento = numeroDocumento,
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
            await transaction.CommitAsync(ct);
            return entity.Id;
        });

        return await ObterContaPagarAsync(genesisDb, createdId, ct)
            ?? throw new InvalidOperationException("Conta a pagar foi gravada, mas nao pode ser relida do banco GenesisGest.Net.");
    }

    public static async Task<GenesisContaReceberDto> CriarContaReceberAsync(GenesisDbContext genesisDb, GenesisContaReceberCreateRequest request, CancellationToken ct)
    {
        ValidarContaReceber(request);
        var numeroDocumento = request.NumeroDocumento.Trim().ToUpperInvariant();
        var strategy = genesisDb.Database.CreateExecutionStrategy();
        var createdId = await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await genesisDb.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            if (await genesisDb.ContasReceber.AnyAsync(item => item.NumeroDocumento == numeroDocumento, ct))
            {
                throw new InvalidOperationException($"Ja existe conta a receber com o documento {numeroDocumento}.");
            }

            var entity = new GenesisContaReceber
            {
                NumeroDocumento = numeroDocumento,
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
            await transaction.CommitAsync(ct);
            return entity.Id;
        });

        return await ObterContaReceberAsync(genesisDb, createdId, ct)
            ?? throw new InvalidOperationException("Conta a receber foi gravada, mas nao pode ser relida do banco GenesisGest.Net.");
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
        if (request.ValorPago <= 0m)
        {
            throw new ArgumentException("Valor da baixa deve ser maior que zero.");
        }

        ValidarObservacoes(request.Observacoes);

        var dataPagamento = request.DataPagamento ?? DateTime.Now;
        if (dataPagamento > DateTime.Now.AddMinutes(5))
        {
            throw new ArgumentException("Data da baixa nao pode estar no futuro.");
        }

        var strategy = genesisDb.Database.CreateExecutionStrategy();
        var updatedId = await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await genesisDb.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            var entity = await genesisDb.ContasPagar.FirstOrDefaultAsync(item => item.Id == id, ct);
            if (entity is null) return (int?)null;

            var valorAberto = entity.ValorOriginal - entity.ValorPago;
            if (valorAberto <= 0m)
            {
                throw new ArgumentException("Conta a pagar ja esta integralmente baixada.");
            }

            if (request.ValorPago > valorAberto)
            {
                throw new ArgumentException($"Valor da baixa excede o saldo aberto de R$ {valorAberto:N2}.");
            }

            entity.ValorPago += request.ValorPago;
            entity.DataPagamento = dataPagamento;
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
                Observacoes = request.Observacoes?.Trim(),
                CriadoEm = DateTime.Now
            });

            await genesisDb.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return entity.Id;
        });

        return updatedId.HasValue ? await ObterContaPagarAsync(genesisDb, updatedId.Value, ct) : null;
    }

    public static async Task<GenesisContaReceberDto?> BaixarContaReceberAsync(GenesisDbContext genesisDb, int id, GenesisBaixaReceberRequest request, CancellationToken ct)
    {
        if (request.ValorRecebido <= 0m)
        {
            throw new ArgumentException("Valor do recebimento deve ser maior que zero.");
        }

        ValidarObservacoes(request.Observacoes);

        var dataRecebimento = request.DataRecebimento ?? DateTime.Now;
        if (dataRecebimento > DateTime.Now.AddMinutes(5))
        {
            throw new ArgumentException("Data do recebimento nao pode estar no futuro.");
        }

        var strategy = genesisDb.Database.CreateExecutionStrategy();
        var updatedId = await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await genesisDb.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            var entity = await genesisDb.ContasReceber.FirstOrDefaultAsync(item => item.Id == id, ct);
            if (entity is null) return (int?)null;

            var valorAberto = entity.ValorOriginal - entity.ValorRecebido;
            if (valorAberto <= 0m)
            {
                throw new ArgumentException("Conta a receber ja esta integralmente baixada.");
            }

            if (request.ValorRecebido > valorAberto)
            {
                throw new ArgumentException($"Valor do recebimento excede o saldo aberto de R$ {valorAberto:N2}.");
            }

            entity.ValorRecebido += request.ValorRecebido;
            entity.DataRecebimento = dataRecebimento;
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
                Observacoes = request.Observacoes?.Trim(),
                CriadoEm = DateTime.Now
            });

            await genesisDb.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return entity.Id;
        });

        return updatedId.HasValue ? await ObterContaReceberAsync(genesisDb, updatedId.Value, ct) : null;
    }

    private static void ValidarContaPagar(GenesisContaPagarCreateRequest request)
    {
        ValidarTitulo(request.NumeroDocumento, request.Descricao, request.ValorOriginal, request.DataEmissao, request.DataVencimento);
        if (request.FornecedorId is <= 0)
        {
            throw new ArgumentException("FornecedorId deve ser positivo quando informado.");
        }
    }

    private static void ValidarContaReceber(GenesisContaReceberCreateRequest request)
    {
        ValidarTitulo(request.NumeroDocumento, request.Descricao, request.ValorOriginal, request.DataEmissao, request.DataVencimento);
        if (request.ClienteId is <= 0)
        {
            throw new ArgumentException("ClienteId deve ser positivo quando informado.");
        }
    }

    private static void ValidarTitulo(string? numeroDocumento, string? descricao, decimal valorOriginal, DateTime dataEmissao, DateTime dataVencimento)
    {
        if (string.IsNullOrWhiteSpace(numeroDocumento))
        {
            throw new ArgumentException("Numero do documento e obrigatorio.");
        }

        if (numeroDocumento.Trim().Length > 40)
        {
            throw new ArgumentException("Numero do documento deve ter no maximo 40 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(descricao))
        {
            throw new ArgumentException("Descricao do titulo e obrigatoria.");
        }

        if (descricao.Trim().Length > 200)
        {
            throw new ArgumentException("Descricao do titulo deve ter no maximo 200 caracteres.");
        }

        if (valorOriginal <= 0m)
        {
            throw new ArgumentException("Valor original deve ser maior que zero.");
        }

        if (dataEmissao == default || dataVencimento == default)
        {
            throw new ArgumentException("Datas de emissao e vencimento sao obrigatorias.");
        }

        if (dataVencimento.Date < dataEmissao.Date)
        {
            throw new ArgumentException("Vencimento nao pode ser anterior a emissao.");
        }
    }

    private static void ValidarObservacoes(string? observacoes)
    {
        if (observacoes?.Trim().Length > 500)
        {
            throw new ArgumentException("Observacoes devem ter no maximo 500 caracteres.");
        }
    }

    private static GenesisContaPagarDto ToContaPagarDto(GenesisContaPagar entity) =>
        new(
            entity.Id,
            entity.NumeroDocumento ?? string.Empty,
            entity.FornecedorId,
            entity.Descricao ?? string.Empty,
            entity.ValorOriginal,
            entity.ValorPago,
            Math.Max(0m, entity.ValorOriginal - entity.ValorPago),
            entity.DataEmissao,
            entity.DataVencimento,
            entity.DataPagamento,
            entity.Status ?? string.Empty,
            entity.FormaPagamento,
            entity.NumeroBoleto);

    private static GenesisContaReceberDto ToContaReceberDto(GenesisContaReceber entity) =>
        new(
            entity.Id,
            entity.NumeroDocumento ?? string.Empty,
            entity.ClienteId,
            entity.Descricao ?? string.Empty,
            entity.ValorOriginal,
            entity.ValorRecebido,
            Math.Max(0m, entity.ValorOriginal - entity.ValorRecebido),
            entity.DataEmissao,
            entity.DataVencimento,
            entity.DataRecebimento,
            entity.Status ?? string.Empty,
            entity.FormaRecebimento,
            entity.NumeroPedidoReferencia);

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
