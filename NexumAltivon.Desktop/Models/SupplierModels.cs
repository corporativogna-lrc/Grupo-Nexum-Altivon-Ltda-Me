/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7186
 */

namespace NexumAltivon.Desktop.Models;

public sealed record DesktopFornecedor(
    int Id,
    string Nome,
    string Documento,
    string Email,
    string Telefone,
    string Categoria,
    DateTime CreatedAt,
    string? RazaoSocial,
    string? NomeFantasia,
    string? Ie,
    string? Whatsapp,
    string? Endereco,
    string? Cidade,
    string? Estado,
    string? Cep,
    int? LojaVinculadaId,
    decimal ComissaoPercentual,
    int PrazoEntregaDias,
    string? Status,
    string? Observacoes,
    DateTime? UpdatedAt,
    string RowVersion);

public sealed record DesktopFornecedorRequest(
    string Nome,
    string? Documento,
    string? Email,
    string? Telefone,
    string? Categoria,
    string? NomeFantasia,
    string? Ie,
    string? Whatsapp,
    string? Endereco,
    string? Cidade,
    string? Estado,
    string? Cep,
    int? LojaVinculadaId,
    decimal? ComissaoPercentual,
    int? PrazoEntregaDias,
    string? Status,
    string? Observacoes,
    string? RowVersion);

public sealed record DesktopFornecedorContato(
    int Id,
    int FornecedorId,
    string Nome,
    string? Cargo,
    string? Email,
    string? Telefone,
    string? Celular,
    bool Principal,
    bool Ativo,
    DateTime AtualizadoEm,
    string RowVersion);

public sealed record DesktopFornecedorContatoRequest(
    string Nome,
    string? Cargo,
    string? Email,
    string? Telefone,
    string? Celular,
    bool Principal,
    bool Ativo,
    string? RowVersion);
