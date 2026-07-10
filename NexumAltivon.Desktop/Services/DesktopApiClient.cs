/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using NexumAltivon.Desktop.Models;

namespace NexumAltivon.Desktop.Services;

public sealed class DesktopApiClient
{
    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(12)
    };

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

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

    public async Task<DesktopApiSubmitResult> SubmitSaleAsync(
        TerminalProfile profile,
        PdvSaleDraft sale,
        CancellationToken cancellationToken = default)
    {
        var payload = new GenesisPdvVendaApiRequest(
            sale.CodigoVenda,
            sale.EmpresaEmissora,
            null,
            sale.ClienteNome,
            sale.ClienteDocumento,
            null,
            null,
            sale.Terminal,
            profile.TerminalCode,
            sale.Operador,
            sale.Desconto,
            0m,
            $"Canal={sale.Canal}; entrega={sale.TipoEntrega}; fiscal={sale.StatusFiscal}; financeiro={sale.StatusFinanceiro}; logistica={sale.StatusLogistico}; decisao={sale.DecisaoEmpresaEmissora}",
            sale.Itens.Select(item => new GenesisPdvVendaItemApiRequest(
                null,
                item.Codigo,
                item.Codigo,
                item.Descricao,
                item.Quantidade,
                item.ValorUnitario,
                item.CustoEstimado,
                item.Desconto,
                item.OrigemAquisicao)).ToList(),
            sale.Pagamentos.Select(payment => new GenesisPdvPagamentoApiRequest(
                payment.Forma,
                payment.Valor,
                1,
                payment.Autorizacao,
                null,
                null)).ToList());

        return await SubmitJsonWithFallbackAsync(
            profile,
            "/api/desktop/genesis/pdv/vendas",
            payload,
            "venda PDV",
            cancellationToken);
    }

    public async Task<DesktopApiSubmitResult> SubmitOperationAsync<TPayload>(
        TerminalProfile profile,
        string module,
        string operationCode,
        TPayload payload,
        CancellationToken cancellationToken = default)
    {
        var request = new GenesisDesktopOperationApiRequest(
            operationCode,
            profile.TerminalCode,
            profile.StoreCode,
            "GenesisGest.Net Desktop",
            "Recebido",
            JsonSerializer.SerializeToElement(payload, JsonOptions),
            $"Operação {module} enviada pelo terminal {profile.TerminalCode}.");

        return await SubmitJsonWithFallbackAsync(
            profile,
            $"/api/desktop/genesis/operacoes/{Uri.EscapeDataString(module)}",
            request,
            $"operação {module}",
            cancellationToken);
    }

    private static async Task<DesktopApiSubmitResult> SubmitJsonWithFallbackAsync<TPayload>(
        TerminalProfile profile,
        string path,
        TPayload payload,
        string operationLabel,
        CancellationToken cancellationToken)
    {
        var endpoints = new[]
        {
            profile.ApiBaseUrl,
            profile.PublicApiUrl
        }
        .Where(url => !string.IsNullOrWhiteSpace(url))
        .Select(url => url.TrimEnd('/'))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

        var failures = new List<string>();

        foreach (var endpoint in endpoints)
        {
            var result = await TrySubmitAsync(profile, endpoint, path, payload, operationLabel, cancellationToken);
            if (result.Success)
            {
                return result;
            }

            failures.Add($"{endpoint}: {result.Detail}");
        }

        return new DesktopApiSubmitResult(
            false,
            false,
            string.Empty,
            string.Join(" | ", failures),
            null);
    }

    private static async Task<DesktopApiSubmitResult> TrySubmitAsync<TPayload>(
        TerminalProfile profile,
        string endpoint,
        string path,
        TPayload payload,
        string operationLabel,
        CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{endpoint}{path}")
            {
                Content = JsonContent.Create(payload, options: JsonOptions)
            };

            request.Headers.TryAddWithoutValidation("X-Nexum-Terminal", profile.TerminalCode);
            request.Headers.TryAddWithoutValidation("X-Nexum-Store", profile.StoreCode);
            if (!string.IsNullOrWhiteSpace(profile.DesktopAccessToken))
            {
                request.Headers.TryAddWithoutValidation("X-Nexum-Desktop-Token", profile.DesktopAccessToken.Trim());
            }

            using var response = await Http.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new DesktopApiSubmitResult(
                    false,
                    false,
                    endpoint,
                    $"{(int)response.StatusCode} {response.ReasonPhrase}: {TrimContent(content)}",
                    null);
            }

            return new DesktopApiSubmitResult(
                true,
                true,
                endpoint,
                $"{operationLabel} gravada no servidor",
                ExtractServerReference(content));
        }
        catch (Exception ex)
        {
            return new DesktopApiSubmitResult(false, false, endpoint, ex.Message, null);
        }
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

    private static string? ExtractServerReference(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("id", out var id))
            {
                return $"Id={id}";
            }

            if (doc.RootElement.TryGetProperty("venda", out var venda) && venda.TryGetProperty("id", out var vendaId))
            {
                return $"VendaId={vendaId}";
            }
        }
        catch (JsonException)
        {
            return TrimContent(content);
        }

        return TrimContent(content);
    }

    private static string TrimContent(string content)
    {
        var value = content.ReplaceLineEndings(" ").Trim();
        return value.Length <= 180 ? value : value[..180];
    }
}

public sealed record DesktopApiHealthResult(
    string Status,
    string Detail,
    bool LocalHealthy,
    bool PublicHealthy,
    DateTime CheckedAt);

public sealed record DesktopApiSubmitResult(
    bool Success,
    bool PersistedOnServer,
    string Endpoint,
    string Detail,
    string? ServerReference);

internal sealed record GenesisPdvVendaApiRequest(
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
    List<GenesisPdvVendaItemApiRequest> Itens,
    List<GenesisPdvPagamentoApiRequest> Pagamentos);

internal sealed record GenesisPdvVendaItemApiRequest(
    int? ProdutoNexumId,
    string? ProdutoCodigo,
    string? Sku,
    string Descricao,
    decimal Quantidade,
    decimal PrecoUnitario,
    decimal CustoUnitario,
    decimal Desconto,
    string? OrigemAquisicao);

internal sealed record GenesisPdvPagamentoApiRequest(
    string Forma,
    decimal Valor,
    int Parcelas,
    string? Autorizacao,
    string? Nsu,
    string? Bandeira);

internal sealed record GenesisDesktopOperationApiRequest(
    string Codigo,
    string? Terminal,
    string? Loja,
    string? Origem,
    string? Status,
    JsonElement Payload,
    string? Observacoes);
