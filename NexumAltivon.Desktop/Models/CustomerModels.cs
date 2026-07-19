/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7187
 */

namespace NexumAltivon.Desktop.Models;

public sealed record DesktopCliente(
    int Id,
    string Nome,
    string Email,
    string? Telefone,
    string? Cpf,
    string? Tipo,
    string? RgIe,
    DateTime? DataNascimento,
    string? Whatsapp,
    string? Avatar,
    bool Newsletter,
    bool Vip,
    int PontosFidelidade,
    string? Status,
    DateTime? UltimoAcesso,
    DateTime? ConfirmadoEm,
    DateTime? CreatedAt,
    DateTime? UpdatedAt,
    string? RowVersion);

public sealed record DesktopClienteRequest(
    string Nome,
    string Email,
    string? Cpf,
    string? Telefone,
    string? Senha = null,
    bool? Newsletter = null,
    string? CpfCnpj = null,
    string? RgIe = null,
    DateTime? DataNascimento = null,
    string? Whatsapp = null,
    string? Avatar = null,
    bool? Vip = null,
    int? PontosFidelidade = null,
    string? Status = null,
    string? Tipo = null,
    string? RowVersion = null);

public sealed record DesktopClienteEndereco(
    int Id,
    string Apelido,
    string Tipo,
    string Cep,
    string Logradouro,
    string Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Estado,
    string Pais,
    bool Padrao,
    string? RowVersion);

public sealed record DesktopClienteEnderecoRequest(
    string? Apelido,
    string? Tipo,
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Estado,
    string? Pais,
    bool Padrao,
    string? RowVersion = null);
