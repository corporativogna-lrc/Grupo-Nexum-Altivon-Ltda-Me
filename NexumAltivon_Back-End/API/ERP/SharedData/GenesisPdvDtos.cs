/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

namespace NexumAltivon.API.ERP.SharedData;

public sealed record GenesisPdvVendaRequest(
    string? Numero,
    string? EmpresaCodigo,
    int? EmpresaNexumId,
    string ClienteNome,
    string? ClienteDocumento,
    string? ClienteEmail,
    int? ClienteNexumId,
    string? Terminal,
    string? CaixaCodigo,
    string? Operador,
    decimal Desconto,
    decimal Frete,
    string? Observacoes,
    List<GenesisPdvVendaItemRequest> Itens,
    List<GenesisPdvPagamentoRequest> Pagamentos
);

public sealed record GenesisPdvVendaItemRequest(
    int? ProdutoNexumId,
    string? ProdutoCodigo,
    string? Sku,
    string Descricao,
    decimal Quantidade,
    decimal PrecoUnitario,
    decimal CustoUnitario,
    decimal Desconto,
    string? OrigemAquisicao
);

public sealed record GenesisPdvPagamentoRequest(
    string Forma,
    decimal Valor,
    int Parcelas,
    string? Autorizacao,
    string? Nsu,
    string? Bandeira
);

public sealed record GenesisPdvVendaDto(
    int Id,
    string Numero,
    string? EmpresaCodigo,
    int? EmpresaNexumId,
    string ClienteNome,
    string? ClienteDocumento,
    string? ClienteEmail,
    int? ClienteNexumId,
    int? PedidoNexumId,
    string? PedidoNexumNumero,
    string? Terminal,
    string? CaixaCodigo,
    string? Operador,
    decimal Subtotal,
    decimal Desconto,
    decimal Frete,
    decimal Total,
    decimal ValorPago,
    decimal Troco,
    string Status,
    string StatusSincronizacao,
    DateTime CriadoEm,
    List<GenesisPdvVendaItemDto> Itens,
    List<GenesisPdvPagamentoDto> Pagamentos
);

public sealed record GenesisPdvVendaItemDto(
    int Id,
    int Sequencia,
    int? ProdutoNexumId,
    string? ProdutoCodigo,
    string? Sku,
    string Descricao,
    decimal Quantidade,
    decimal PrecoUnitario,
    decimal CustoUnitario,
    decimal Desconto,
    decimal Total,
    string? OrigemAquisicao
);

public sealed record GenesisPdvPagamentoDto(
    int Id,
    string Forma,
    decimal Valor,
    int Parcelas,
    string? Autorizacao,
    string? Nsu,
    string? Bandeira
);
