/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7225 Data: 16/08/2026
 */

using System.Text.Json.Serialization;

namespace NexumAltivon.Desktop.Models;

public sealed record DesktopFileDownloadResult(bool Success, byte[]? Content, string? FileName, string? Detail);

public sealed record DesktopContaPagar(
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
    string? NumeroBoleto);

public sealed record DesktopContaReceber(
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
    string? NumeroPedidoReferencia);

public sealed record DesktopContaPagarCreateRequest(
    string NumeroDocumento,
    int? FornecedorId,
    string Descricao,
    decimal ValorOriginal,
    DateTime DataEmissao,
    DateTime DataVencimento,
    string? FormaPagamento,
    string? NumeroBoleto);

public sealed record DesktopContaReceberCreateRequest(
    string NumeroDocumento,
    int? ClienteId,
    string Descricao,
    decimal ValorOriginal,
    DateTime DataEmissao,
    DateTime DataVencimento,
    string? FormaRecebimento,
    string? NumeroPedidoReferencia);

public sealed record DesktopBaixaPagarRequest(
    decimal ValorPago,
    DateTime? DataPagamento,
    string? FormaPagamento,
    string? Observacoes);

public sealed record DesktopBaixaReceberRequest(
    decimal ValorRecebido,
    DateTime? DataRecebimento,
    string? FormaRecebimento,
    string? Observacoes);

public sealed record DesktopAuditoriaOperacional(
    long Id,
    string Tabela,
    int RegistroId,
    string Acao,
    int? UsuarioId,
    string UsuarioTipo,
    string? IpAddress,
    string? Endpoint,
    [property: JsonPropertyName("criadoEm")] DateTime CreatedAt);

public sealed record DesktopConciliacaoFinanceira(
    int LancamentoFinanceiroId,
    string? Descricao,
    decimal Valor,
    DateTime? DataPagamento,
    string? MeioPagamento,
    string? ContaBancaria,
    string Status,
    int? ConciliacaoId,
    string? ReferenciaBancaria,
    string? Observacoes,
    DateTime? DataConciliacao,
    string? RowVersion);

public sealed record DesktopConciliacaoFinanceiraRequest(
    int LancamentoFinanceiroId,
    string Status,
    string? ReferenciaBancaria,
    string? Observacoes,
    string? RowVersion);

public sealed record DesktopDreGerencial(
    int EmpresaId,
    string? Competencia,
    string? ClasseConta,
    string? ContaCodigo,
    string? ContaNome,
    decimal? ValorDebito,
    decimal? ValorCredito,
    decimal? SaldoConta);

public sealed record DesktopContabilLancamento(
    int Id,
    int EmpresaId,
    string Lote,
    string? Sublote,
    DateTime Data,
    string? HistoricoPadrao,
    string? Complemento,
    decimal Valor,
    string Tipo,
    string? OrigemModulo,
    int? OrigemId,
    bool Estornado,
    int? LancamentoEstornoId,
    int UsuarioCadastroId,
    DateTime CriadoEm);

public sealed record DesktopContabilPartidaRequest(
    string Tipo,
    int PlanoContaId,
    int? CentroCustoId,
    decimal Valor,
    string? Historico);

public sealed record DesktopContabilLancamentoRequest(
    int EmpresaId,
    string? Lote,
    string? Sublote,
    DateTime Data,
    string? HistoricoPadrao,
    string? Complemento,
    decimal Valor,
    string? Tipo,
    string? OrigemModulo,
    int? OrigemId,
    List<DesktopContabilPartidaRequest> Partidas);

public sealed record DesktopFechamentoContabil(
    int Id,
    int EmpresaId,
    DateTime Periodo,
    DateTime DataFechamento,
    int UsuarioResponsavelId,
    bool Bloqueado,
    string? Observacoes);

public sealed record DesktopFechamentoContabilRequest(
    int EmpresaId,
    DateTime Periodo,
    bool Bloqueado,
    string? Observacoes);
