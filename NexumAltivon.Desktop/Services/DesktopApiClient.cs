/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Net.Http;
using NexumAltivon.Desktop.Models;

namespace NexumAltivon.Desktop.Services;

public sealed class DesktopApiClient
{
    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(8)
    };

    public async Task<DesktopApiHealthResult> CheckHealthAsync(TerminalProfile profile, CancellationToken cancellationToken = default)
    {
        var local = await TryHealthAsync(profile.ApiBaseUrl, cancellationToken);
        var publicApi = await TryHealthAsync(profile.PublicApiUrl, cancellationToken);

        var status = local.Success
            ? "Servidor principal operacional"
            : publicApi.Success
                ? "API pública operacional; origem local indisponível"
                : "API indisponível";

        var detail = local.Success
            ? $"Origem {profile.ApiBaseUrl} respondeu Healthy. Terminal pronto para ERP/PDV."
            : publicApi.Success
                ? $"Cloudflare respondeu em {profile.PublicApiUrl}, mas o terminal não alcançou {profile.ApiBaseUrl}."
                : $"Sem resposta local nem pública. Contingência local: {(profile.OfflineContingencyEnabled ? "ativa" : "inativa")}.";

        return new DesktopApiHealthResult(status, detail, local.Success, publicApi.Success, DateTime.Now);
    }

    private static async Task<(bool Success, string Detail)> TryHealthAsync(string baseUrl, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await Http.GetAsync($"{baseUrl.TrimEnd('/')}/health", cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var success = response.IsSuccessStatusCode && content.Contains("Healthy", StringComparison.OrdinalIgnoreCase);
            return (success, $"{(int)response.StatusCode} {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}

public sealed record DesktopApiHealthResult(
    string Status,
    string Detail,
    bool LocalHealthy,
    bool PublicHealthy,
    DateTime CheckedAt);
