/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

namespace NexumAltivon.API.ERP.SharedData;

public sealed record GenesisFinanceSummaryDto(
    decimal ContasReceberPendentes,
    decimal ContasPagarPendentes,
    decimal FluxoEntradasMes,
    decimal FluxoSaidasMes,
    decimal SaldoProjetadoMes,
    int TitulosReceberVencidos,
    int TitulosPagarVencidos,
    DateTime AtualizadoEm
);

public sealed record GenesisContaPagarDto(
    int Id,
    string NumeroDocumento,
    int? FornecedorId,
    string Descricao,
    decimal ValorOriginal,
    decimal ValorPago,
    decimal ValorAberto,
    DateTime DataEmissao,
    DateTime DataVencimento,
    DateTime? DataPagamento,
    string Status,
    string? FormaPagamento,
    string? NumeroBoleto
);

public sealed record GenesisContaReceberDto(
    int Id,
    string NumeroDocumento,
    int? ClienteId,
    string Descricao,
    decimal ValorOriginal,
    decimal ValorRecebido,
    decimal ValorAberto,
    DateTime DataEmissao,
    DateTime DataVencimento,
    DateTime? DataRecebimento,
    string Status,
    string? FormaRecebimento,
    string? NumeroPedidoReferencia
);

public sealed record GenesisBoletoDto(
    int Id,
    int ContaReceberId,
    string? NossoNumero,
    string? LinhaDigitavel,
    string? CodigoBarras,
    string? Banco,
    DateTime Vencimento,
    decimal Valor,
    string Status,
    string? UrlBoleto,
    string? PdfUrl,
    DateTime CriadoEm
);

public sealed record GenesisFinanceReferenciaDto(
    int Id,
    string Tipo,
    string Codigo,
    string Descricao,
    int Ordem
);
