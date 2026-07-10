/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Collections.ObjectModel;

namespace NexumAltivon.Desktop.Models;

public sealed class OrganizationNode
{
    public string Name { get; init; } = string.Empty;
    public string Domain { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string TaxSituation { get; init; } = string.Empty;
    public string EmailPattern { get; init; } = string.Empty;
    public ObservableCollection<OrganizationNode> Children { get; } = new();
}
