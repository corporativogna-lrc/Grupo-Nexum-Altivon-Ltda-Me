/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

namespace NexumAltivon.Desktop.Models;

public sealed class NetworkEndpointSetting
{
    public string Name { get; set; } = string.Empty;
    public string EnvironmentVariable { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Source { get; set; } = "Sistema";
    public string Status { get; set; } = "Aguardando teste";
}
