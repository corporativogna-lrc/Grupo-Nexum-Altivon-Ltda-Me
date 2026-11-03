/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Collections.Generic;

namespace NexumAltivon.Desktop.Models;

public sealed class DesktopModule
{
    public string Title { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Accent { get; init; } = "#38BDF8";
    public string ActionText { get; init; } = "Abrir";
}

public sealed class DesktopSiteConfiguracao
{
    public string Chave { get; set; } = string.Empty;
    public string? Valor { get; set; }
    public string? Tipo { get; set; }
    public string? Descricao { get; set; }
    public string? Grupo { get; set; }
    public bool? Editavel { get; set; }
}

public sealed record DesktopSiteConfiguracaoUpdate(
    string Chave,
    string? Valor,
    string? Tipo,
    string? Descricao,
    string? Grupo,
    bool? Editavel);

public sealed record DesktopSiteConfiguracaoUpdateRequest(
    List<DesktopSiteConfiguracaoUpdate> Itens);
