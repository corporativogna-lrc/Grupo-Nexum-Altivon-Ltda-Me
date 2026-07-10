/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

namespace NexumAltivon.API.ERP.SharedData;

public sealed record GenesisContaPagarCreateRequest(
    string NumeroDocumento,
    int? FornecedorId,
    string Descricao,
    decimal ValorOriginal,
    DateTime DataEmissao,
    DateTime DataVencimento,
    string? FormaPagamento,
    string? NumeroBoleto
);

public sealed record GenesisContaReceberCreateRequest(
    string NumeroDocumento,
    int? ClienteId,
    string Descricao,
    decimal ValorOriginal,
    DateTime DataEmissao,
    DateTime DataVencimento,
    string? FormaRecebimento,
    string? NumeroPedidoReferencia
);

public sealed record GenesisBaixaPagarRequest(
    decimal ValorPago,
    DateTime? DataPagamento,
    string? FormaPagamento
);

public sealed record GenesisBaixaReceberRequest(
    decimal ValorRecebido,
    DateTime? DataRecebimento,
    string? FormaRecebimento
);

public sealed record GenesisBoletoCreateRequest(
    int ContaReceberId,
    string? NossoNumero,
    string? LinhaDigitavel,
    string? CodigoBarras,
    string? Banco,
    DateTime Vencimento,
    decimal Valor,
    string? UrlBoleto,
    string? PdfUrl
);
