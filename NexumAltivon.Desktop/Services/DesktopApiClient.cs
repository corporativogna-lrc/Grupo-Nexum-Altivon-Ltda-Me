/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Net.Http;
using System.Net.Http.Headers;
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

    public async Task<DesktopLoginResult> AuthenticateAsync(
        TerminalProfile profile,
        string email,
        string password,
        string? mfaCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return new DesktopLoginResult(false, null, null, "E-mail e senha sao obrigatorios.");
        }

        foreach (var endpoint in GetEndpoints(profile))
        {
            try
            {
                using var response = await Http.PostAsJsonAsync(
                    $"{endpoint}/api/auth/login",
                    new DesktopLoginRequest(email.Trim(), password, string.IsNullOrWhiteSpace(mfaCode) ? null : mfaCode.Trim()),
                    JsonOptions,
                    cancellationToken);
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode is 400 or 401 or 403)
                    {
                        return new DesktopLoginResult(false, null, null, ExtractErrorMessage(content, response.ReasonPhrase));
                    }

                    continue;
                }

                var envelope = JsonSerializer.Deserialize<DesktopApiEnvelope<DesktopLoginResponse>>(content, JsonOptions);
                var login = envelope?.Dados;
                if (envelope is null || !envelope.Sucesso || login is null || string.IsNullOrWhiteSpace(login.Token) || login.ExpiraEm <= DateTime.UtcNow)
                {
                    return new DesktopLoginResult(false, null, null, envelope?.Mensagem ?? "A API nao confirmou uma sessao JWT valida.");
                }

                return new DesktopLoginResult(true, login.Token, login.Usuario, $"Sessao autenticada em {endpoint}.");
            }
            catch (HttpRequestException)
            {
                continue;
            }
            catch (TaskCanceledException)
            {
                continue;
            }
        }

        return new DesktopLoginResult(false, null, null, "API local e publica indisponiveis para autenticacao.");
    }

    public Task<DesktopApiDataResult<List<DesktopContaPagar>>> GetContasPagarAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<List<DesktopContaPagar>>(
            profile, token, HttpMethod.Get, "/api/erp/genesis/financeiro/contas-pagar", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopContaPagar>> CreateContaPagarAsync(
        TerminalProfile profile,
        string token,
        DesktopContaPagarCreateRequest payload,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopContaPagar>(
            profile, token, HttpMethod.Post, "/api/erp/genesis/financeiro/contas-pagar", payload, cancellationToken);

    public Task<DesktopApiDataResult<DesktopContaPagar>> BaixarContaPagarAsync(
        TerminalProfile profile,
        string token,
        int id,
        DesktopBaixaPagarRequest payload,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopContaPagar>(
            profile, token, HttpMethod.Post, $"/api/erp/genesis/financeiro/contas-pagar/{id}/baixa", payload, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopContaReceber>>> GetContasReceberAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<List<DesktopContaReceber>>(
            profile, token, HttpMethod.Get, "/api/erp/genesis/financeiro/contas-receber", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopContaReceber>> CreateContaReceberAsync(
        TerminalProfile profile,
        string token,
        DesktopContaReceberCreateRequest payload,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopContaReceber>(
            profile, token, HttpMethod.Post, "/api/erp/genesis/financeiro/contas-receber", payload, cancellationToken);

    public Task<DesktopApiDataResult<DesktopContaReceber>> BaixarContaReceberAsync(
        TerminalProfile profile,
        string token,
        int id,
        DesktopBaixaReceberRequest payload,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopContaReceber>(
            profile, token, HttpMethod.Post, $"/api/erp/genesis/financeiro/contas-receber/{id}/baixa", payload, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopAuditoriaOperacional>>> GetAuditoriaAsync(
        TerminalProfile profile,
        string token,
        string tabela,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<List<DesktopAuditoriaOperacional>>(
            profile,
            token,
            HttpMethod.Get,
            $"/api/auditoria?tabela={Uri.EscapeDataString(tabela)}",
            null,
            cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopCrmSegmento>>> GetMarketingSegmentsAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<List<DesktopCrmSegmento>>(profile, token, HttpMethod.Get, "/api/crm/segmentos", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopOpenAiAssistentesStatus>> GetOpenAiAssistantsStatusAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopOpenAiAssistentesStatus>(
            profile,
            token,
            HttpMethod.Get,
            "/api/admin/integracoes/openai",
            null,
            cancellationToken);

    public Task<DesktopApiDataResult<DesktopOpenAiAssistentesStatus>> SaveOpenAiAssistantsAsync(
        TerminalProfile profile,
        string token,
        DesktopOpenAiAssistentesConfiguracaoRequest payload,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopOpenAiAssistentesStatus>(
            profile,
            token,
            HttpMethod.Put,
            "/api/admin/integracoes/openai",
            payload,
            cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopUsuarioAcesso>>> GetAccessUsersAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<List<DesktopUsuarioAcesso>>(
            profile, token, HttpMethod.Get, "/api/admin/usuarios", null, cancellationToken);

    public Task<DesktopApiDataResult<List<string>>> GetAdministrativeRolesAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<List<string>>(
            profile, token, HttpMethod.Get, "/api/admin/usuarios/perfis", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopUsuarioAcesso>> SaveAccessUserAsync(
        TerminalProfile profile,
        string token,
        DesktopUsuarioAcessoRequest payload,
        int? id,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopUsuarioAcesso>(
            profile,
            token,
            id.HasValue ? HttpMethod.Put : HttpMethod.Post,
            id.HasValue ? $"/api/admin/usuarios/{id.Value}" : "/api/admin/usuarios",
            payload,
            cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopPerfilAcesso>>> GetAccessProfilesAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<List<DesktopPerfilAcesso>>(
            profile, token, HttpMethod.Get, "/api/perfis", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopPerfilAcesso>> SaveAccessProfileAsync(
        TerminalProfile profile,
        string token,
        DesktopPerfilAcessoRequest payload,
        int? id,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopPerfilAcesso>(
            profile,
            token,
            id.HasValue ? HttpMethod.Put : HttpMethod.Post,
            id.HasValue ? $"/api/perfis/{id.Value}" : "/api/perfis",
            payload,
            cancellationToken);

    public Task<DesktopApiDataResult<DesktopPerfilAcesso>> DeactivateAccessProfileAsync(
        TerminalProfile profile,
        string token,
        int id,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopPerfilAcesso>(
            profile, token, HttpMethod.Delete, $"/api/perfis/{id}", null, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopPermissaoAcesso>>> GetPermissionsAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<List<DesktopPermissaoAcesso>>(
            profile, token, HttpMethod.Get, "/api/permissoes", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopPermissaoAcesso>> SavePermissionAsync(
        TerminalProfile profile,
        string token,
        DesktopPermissaoAcessoRequest payload,
        int? id,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopPermissaoAcesso>(
            profile,
            token,
            id.HasValue ? HttpMethod.Put : HttpMethod.Post,
            id.HasValue ? $"/api/permissoes/{id.Value}" : "/api/permissoes",
            payload,
            cancellationToken);

    public Task<DesktopApiDataResult<DesktopPermissaoAcesso>> DeactivatePermissionAsync(
        TerminalProfile profile,
        string token,
        int id,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopPermissaoAcesso>(
            profile, token, HttpMethod.Delete, $"/api/permissoes/{id}", null, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopPerfilPermissao>>> GetProfilePermissionsAsync(
        TerminalProfile profile,
        string token,
        int profileId,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<List<DesktopPerfilPermissao>>(
            profile, token, HttpMethod.Get, $"/api/perfis/{profileId}/permissoes", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopPerfilPermissao>> SaveProfilePermissionAsync(
        TerminalProfile profile,
        string token,
        int profileId,
        DesktopPerfilPermissaoRequest payload,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopPerfilPermissao>(
            profile, token, HttpMethod.Post, $"/api/perfis/{profileId}/permissoes", payload, cancellationToken);

    public Task<DesktopApiDataResult<JsonElement>> RemoveProfilePermissionAsync(
        TerminalProfile profile,
        string token,
        int profileId,
        int permissionId,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<JsonElement>(
            profile,
            token,
            HttpMethod.Delete,
            $"/api/perfis/{profileId}/permissoes/{permissionId}",
            null,
            cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopCrmCampanha>>> GetMarketingCampaignsAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<List<DesktopCrmCampanha>>(profile, token, HttpMethod.Get, "/api/crm/campanhas", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopCrmSegmento>> SaveMarketingSegmentAsync(
        TerminalProfile profile,
        string token,
        DesktopCrmSegmentoRequest payload,
        Guid? id,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopCrmSegmento>(
            profile,
            token,
            id.HasValue ? HttpMethod.Put : HttpMethod.Post,
            id.HasValue ? $"/api/crm/segmentos/{id.Value}" : "/api/crm/segmentos",
            payload,
            cancellationToken);

    public Task<DesktopApiDataResult<DesktopCrmCampanha>> SaveMarketingCampaignAsync(
        TerminalProfile profile,
        string token,
        DesktopCrmCampanhaRequest payload,
        Guid? id,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopCrmCampanha>(
            profile,
            token,
            id.HasValue ? HttpMethod.Put : HttpMethod.Post,
            id.HasValue ? $"/api/crm/campanhas/{id.Value}" : "/api/crm/campanhas",
            payload,
            cancellationToken);

    public Task<DesktopApiDataResult<bool>> DeleteMarketingSegmentAsync(
        TerminalProfile profile,
        string token,
        Guid id,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<bool>(profile, token, HttpMethod.Delete, $"/api/crm/segmentos/{id}", null, cancellationToken);

    public Task<DesktopApiDataResult<bool>> DeleteMarketingCampaignAsync(
        TerminalProfile profile,
        string token,
        Guid id,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<bool>(profile, token, HttpMethod.Delete, $"/api/crm/campanhas/{id}", null, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopDropshippingCanal>>> GetDropshippingChannelsAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<List<DesktopDropshippingCanal>>(profile, token, HttpMethod.Get, "/api/dropshipping/canais", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopDropshippingCanal>> SaveDropshippingChannelAsync(
        TerminalProfile profile,
        string token,
        DesktopDropshippingCanalRequest payload,
        int? id,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopDropshippingCanal>(
            profile,
            token,
            id.HasValue ? HttpMethod.Put : HttpMethod.Post,
            id.HasValue ? $"/api/dropshipping/canais/{id.Value}" : "/api/dropshipping/canais",
            payload,
            cancellationToken);

    public Task<DesktopApiDataResult<bool>> DeleteDropshippingChannelAsync(
        TerminalProfile profile,
        string token,
        int id,
        string rowVersion,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<bool>(
            profile,
            token,
            HttpMethod.Delete,
            $"/api/dropshipping/canais/{id}?rowVersion={Uri.EscapeDataString(rowVersion)}",
            null,
            cancellationToken);

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

    private static async Task<DesktopApiDataResult<T>> SendAuthorizedAsync<T>(
        TerminalProfile profile,
        string token,
        HttpMethod method,
        string path,
        object? payload,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return new DesktopApiDataResult<T>(false, default, "Sessao administrativa ausente.");
        }

        var failures = new List<string>();
        foreach (var endpoint in GetEndpoints(profile))
        {
            try
            {
                using var request = new HttpRequestMessage(method, $"{endpoint}{path}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                if (payload is not null)
                {
                    request.Content = JsonContent.Create(payload, options: JsonOptions);
                }

                using var response = await Http.SendAsync(request, cancellationToken);
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var detail = $"{(int)response.StatusCode} {ExtractErrorMessage(content, response.ReasonPhrase)}";
                    return new DesktopApiDataResult<T>(false, default, $"{endpoint}: {detail}");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    object deleted = true;
                    return new DesktopApiDataResult<T>(true, (T)deleted, $"Exclusao confirmada em {endpoint}.");
                }

                var envelope = JsonSerializer.Deserialize<DesktopApiEnvelope<T>>(content, JsonOptions);
                if (envelope is null || !envelope.Sucesso || envelope.Dados is null)
                {
                    return new DesktopApiDataResult<T>(false, default, envelope?.Mensagem ?? "A API nao confirmou os dados persistidos.");
                }

                return new DesktopApiDataResult<T>(true, envelope.Dados, envelope.Mensagem);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                failures.Add($"{endpoint}: {ex.Message}");
            }
        }

        return new DesktopApiDataResult<T>(false, default, string.Join(" | ", failures));
    }

    private static string[] GetEndpoints(TerminalProfile profile)
        => new[] { profile.ApiBaseUrl, profile.PublicApiUrl }
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Select(url => url.TrimEnd('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static string ExtractErrorMessage(string content, string? reasonPhrase)
    {
        if (!string.IsNullOrWhiteSpace(content))
        {
            try
            {
                using var document = JsonDocument.Parse(content);
                foreach (var property in new[] { "mensagem", "detail", "erro", "message" })
                {
                    if (document.RootElement.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String)
                    {
                        return value.GetString() ?? reasonPhrase ?? "Falha na API.";
                    }
                }
            }
            catch (JsonException)
            {
                return TrimContent(content);
            }
        }

        return reasonPhrase ?? "Falha na API.";
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

public sealed record DesktopLoginResult(bool Success, string? Token, DesktopUser? User, string Detail);
public sealed record DesktopLoginRequest(string Email, string Senha, string? MfaCode);
public sealed record DesktopLoginResponse(string Token, string RefreshToken, DateTime ExpiraEm, DesktopUser Usuario);
public sealed record DesktopUser(int Id, string Nome, string Email, string Perfil);
public sealed record DesktopApiEnvelope<T>(bool Sucesso, string Mensagem, T? Dados, List<string>? Erros);
public sealed record DesktopApiDataResult<T>(bool Success, T? Data, string Detail);

public sealed record DesktopUsuarioAcesso(
    int Id,
    string Nome,
    string Email,
    string Perfil,
    bool Ativo,
    string? Telefone,
    DateTime? UltimoLogin,
    DateTime UpdatedAt);

public sealed record DesktopUsuarioAcessoRequest(
    string Nome,
    string Email,
    string Perfil,
    bool Ativo,
    string? Telefone,
    string? Senha);

public sealed record DesktopPerfilAcesso(
    int Id,
    string Nome,
    string? Descricao,
    decimal AlcadaMaxima,
    int NivelHierarquico,
    bool Ativo,
    DateTime CriadoEm);

public sealed record DesktopPerfilAcessoRequest(
    string Nome,
    string? Descricao,
    decimal AlcadaMaxima,
    int NivelHierarquico,
    bool Ativo);

public sealed record DesktopPermissaoAcesso(
    int Id,
    string Modulo,
    string Funcionalidade,
    string Chave,
    string? Descricao,
    bool Ativo);

public sealed record DesktopPermissaoAcessoRequest(
    string Modulo,
    string Funcionalidade,
    string Chave,
    string? Descricao,
    bool Ativo);

public sealed record DesktopPerfilPermissao(
    int Id,
    int PerfilId,
    int PermissaoId,
    string Modulo,
    string Funcionalidade,
    string Chave,
    bool Leitura,
    bool Escrita,
    bool Exclusao,
    bool Impressao);

public sealed record DesktopPerfilPermissaoRequest(
    int PermissaoId,
    bool Leitura,
    bool Escrita,
    bool Exclusao,
    bool Impressao);

public sealed record DesktopCrmSegmento(
    Guid Id,
    string Nome,
    string? Descricao,
    string Cor,
    int Prioridade,
    decimal? TicketMedioMinimo,
    decimal? TicketMedioMaximo,
    int? FrequenciaMinimaDias,
    int? FrequenciaMaximaDias,
    bool Ativo,
    string RowVersion,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record DesktopCrmSegmentoRequest(
    string Nome,
    string? Descricao,
    string Cor,
    int Prioridade,
    decimal? TicketMedioMinimo,
    decimal? TicketMedioMaximo,
    int? FrequenciaMinimaDias,
    int? FrequenciaMaximaDias,
    bool Ativo,
    string? RowVersion);

public sealed record DesktopCrmCampanha(
    Guid Id,
    string Nome,
    string? Descricao,
    string Tipo,
    string Status,
    Guid? SegmentoId,
    string? SegmentoNome,
    DateTime DataInicio,
    DateTime? DataFim,
    decimal Orcamento,
    decimal CustoAtual,
    int Alcance,
    int Cliques,
    int LeadsGerados,
    int OportunidadesGeradas,
    int VendasGeradas,
    decimal ReceitaGerada,
    decimal Roas,
    decimal Cpc,
    decimal Cpl,
    decimal Cpa,
    string? PublicoAlvo,
    string? Conteudo,
    string RowVersion,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record DesktopCrmCampanhaRequest(
    string Nome,
    string? Descricao,
    string Tipo,
    string Status,
    Guid? SegmentoId,
    DateTime DataInicio,
    DateTime? DataFim,
    decimal Orcamento,
    decimal CustoAtual,
    int Alcance,
    int Cliques,
    int LeadsGerados,
    int OportunidadesGeradas,
    int VendasGeradas,
    decimal ReceitaGerada,
    string? PublicoAlvo,
    string? Conteudo,
    string? RowVersion);

public sealed record DesktopDropshippingCanal(
    int Id,
    string Nome,
    string Slug,
    string Tipo,
    string? ApiEndpoint,
    bool Ativo,
    bool CredenciaisConfiguradas,
    string StatusCredenciais,
    string DetalheCredenciais,
    string RowVersion,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record DesktopDropshippingCanalRequest(
    string Nome,
    string Slug,
    string Tipo,
    string? ApiEndpoint,
    bool Ativo,
    string? RowVersion);

public sealed record DesktopOpenAiAssistentesConfiguracaoRequest(
    string? YaraApiKey,
    string YaraModel,
    string? SophiaApiKey,
    string SophiaModel);

public sealed record DesktopOpenAiAssistentesStatus(
    DesktopOpenAiAssistenteStatus Yara,
    DesktopOpenAiAssistenteStatus Sophia);

public sealed record DesktopOpenAiAssistenteStatus(
    string Assistente,
    bool Configurada,
    string Modelo,
    string Origem,
    DateTime? AtualizadoEm);
