/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

namespace NexumAltivon.Desktop.Models;

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
    DateTime CreatedAt);
