/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7182
 */

using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;

namespace NexumAltivon.API.Infrastructure.Logistics;

public sealed class LogisticaTrackingService
{
    private const int MaxResponseBytes = 1_048_576;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public LogisticaTrackingService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<LogisticaTrackingResult> ConsultarAsync(string codigoRastreio, CancellationToken cancellationToken)
    {
        var codigo = codigoRastreio.Trim();
        if (codigo.Length == 0)
        {
            throw new ArgumentException("Codigo de rastreio obrigatorio.", nameof(codigoRastreio));
        }

        var customEndpoint = ReadConfiguration(
            "Logistica:RastreamentoEndpointTemplate",
            "Integracoes:Logistica:RastreamentoEndpointTemplate");
        if (!string.IsNullOrWhiteSpace(customEndpoint))
        {
            return await ConsultarEndpointCustomizadoAsync(codigo, customEndpoint, cancellationToken);
        }

        return await ConsultarMelhorEnvioAsync(codigo, cancellationToken);
    }

    private async Task<LogisticaTrackingResult> ConsultarEndpointCustomizadoAsync(
        string codigo,
        string endpointTemplate,
        CancellationToken cancellationToken)
    {
        var token = ReadConfiguration(
            "Logistica:RastreamentoToken",
            "Integracoes:Logistica:RastreamentoToken");
        var pendencias = new List<string>();
        if (!endpointTemplate.Contains("{codigo}", StringComparison.OrdinalIgnoreCase))
        {
            pendencias.Add("Logistica__RastreamentoEndpointTemplate deve conter o marcador {codigo}.");
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            pendencias.Add("Logistica__RastreamentoToken nao configurado.");
        }

        if (pendencias.Count > 0)
        {
            return LogisticaTrackingResult.NotConfigured("Endpoint logistico customizado", pendencias);
        }

        var endpoint = endpointTemplate.Replace("{codigo}", Uri.EscapeDataString(codigo), StringComparison.OrdinalIgnoreCase);
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri) || endpointUri.Scheme != Uri.UriSchemeHttps)
        {
            return LogisticaTrackingResult.NotConfigured(
                "Endpoint logistico customizado",
                ["Logistica__RastreamentoEndpointTemplate deve ser uma URL HTTPS absoluta."]);
        }

        try
        {
            var client = _httpClientFactory.CreateClient("logistica-tracking");
            using var request = CreateAuthorizedRequest(HttpMethod.Get, endpointUri, token!);
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            var body = await ReadLimitedBodyAsync(response, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return LogisticaTrackingResult.Failed(
                    endpointUri.Host,
                    (int)response.StatusCode,
                    ComputeSha256(body),
                    [$"Provedor de rastreamento retornou HTTP {(int)response.StatusCode}."]);
            }

            var parsed = ParseTrackingPayload(body, null);
            return parsed.HasOperationalData
                ? LogisticaTrackingResult.Success(endpointUri.Host, (int)response.StatusCode, parsed.Status, parsed.Events, ComputeSha256(body))
                : LogisticaTrackingResult.Failed(
                    endpointUri.Host,
                    (int)response.StatusCode,
                    ComputeSha256(body),
                    ["Provedor respondeu sem status ou eventos de rastreamento reconheciveis."]);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or InvalidDataException)
        {
            return LogisticaTrackingResult.Failed(
                endpointUri.Host,
                null,
                null,
                [$"Falha real ao consultar o provedor: {ex.Message}"]);
        }
    }

    private async Task<LogisticaTrackingResult> ConsultarMelhorEnvioAsync(
        string codigo,
        CancellationToken cancellationToken)
    {
        var token = ReadConfiguration("MelhorEnvio:Token", "Integracoes:MelhorEnvio:Token");
        var contatoTecnico = ReadConfiguration(
            "MelhorEnvio:ContatoTecnico",
            "Integracoes:MelhorEnvio:ContatoTecnico",
            "Logistica:ContatoTecnico");
        var pendencias = new List<string>();
        if (string.IsNullOrWhiteSpace(token))
        {
            pendencias.Add("MelhorEnvio__Token nao configurado.");
        }

        if (string.IsNullOrWhiteSpace(contatoTecnico) || !contatoTecnico.Contains('@', StringComparison.Ordinal))
        {
            pendencias.Add("MelhorEnvio__ContatoTecnico deve conter o e-mail tecnico exigido pelo provedor.");
        }

        var sandbox = _configuration.GetValue(
            "MelhorEnvio:Sandbox",
            _configuration.GetValue("Integracoes:MelhorEnvio:Sandbox", true));
        var fonte = sandbox ? "Melhor Envio Sandbox" : "Melhor Envio Producao";
        if (pendencias.Count > 0)
        {
            return LogisticaTrackingResult.NotConfigured(fonte, pendencias);
        }

        var baseUri = new Uri(sandbox
            ? "https://sandbox.melhorenvio.com.br/"
            : "https://www.melhorenvio.com.br/");

        try
        {
            var client = _httpClientFactory.CreateClient("melhor-envio");
            var searchUri = new Uri(baseUri, $"api/v2/me/orders/search?q={Uri.EscapeDataString(codigo)}");
            using var searchRequest = CreateAuthorizedRequest(HttpMethod.Get, searchUri, token!, contatoTecnico);
            using var searchResponse = await client.SendAsync(searchRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            var searchBody = await ReadLimitedBodyAsync(searchResponse, cancellationToken);
            if (!searchResponse.IsSuccessStatusCode)
            {
                return LogisticaTrackingResult.Failed(
                    fonte,
                    (int)searchResponse.StatusCode,
                    ComputeSha256(searchBody),
                    [$"Pesquisa da etiqueta no Melhor Envio retornou HTTP {(int)searchResponse.StatusCode}."]);
            }

            var etiquetaId = FindExactLabelId(searchBody, codigo);
            if (string.IsNullOrWhiteSpace(etiquetaId))
            {
                return LogisticaTrackingResult.Failed(
                    fonte,
                    (int)searchResponse.StatusCode,
                    ComputeSha256(searchBody),
                    ["Nenhuma etiqueta do Melhor Envio corresponde exatamente ao codigo de rastreio informado."]);
            }

            var trackingUri = new Uri(baseUri, "api/v2/me/shipment/tracking");
            using var trackingRequest = CreateAuthorizedRequest(HttpMethod.Post, trackingUri, token!, contatoTecnico);
            trackingRequest.Content = JsonContent.Create(new { orders = new[] { etiquetaId } });
            using var trackingResponse = await client.SendAsync(trackingRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            var trackingBody = await ReadLimitedBodyAsync(trackingResponse, cancellationToken);
            if (!trackingResponse.IsSuccessStatusCode)
            {
                return LogisticaTrackingResult.Failed(
                    fonte,
                    (int)trackingResponse.StatusCode,
                    ComputeSha256(trackingBody),
                    [$"Consulta de status da etiqueta no Melhor Envio retornou HTTP {(int)trackingResponse.StatusCode}."]);
            }

            var parsed = ParseTrackingPayload(trackingBody, etiquetaId);
            return parsed.HasOperationalData
                ? LogisticaTrackingResult.Success(
                    fonte,
                    (int)trackingResponse.StatusCode,
                    parsed.Status,
                    parsed.Events,
                    ComputeSha256(trackingBody))
                : LogisticaTrackingResult.Failed(
                    fonte,
                    (int)trackingResponse.StatusCode,
                    ComputeSha256(trackingBody),
                    ["Melhor Envio respondeu sem status ou eventos reconheciveis para a etiqueta localizada."]);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or InvalidDataException)
        {
            return LogisticaTrackingResult.Failed(
                fonte,
                null,
                null,
                [$"Falha real ao consultar o Melhor Envio: {ex.Message}"]);
        }
    }

    private static HttpRequestMessage CreateAuthorizedRequest(
        HttpMethod method,
        Uri uri,
        string token,
        string? contatoTecnico = null)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (!string.IsNullOrWhiteSpace(contatoTecnico))
        {
            request.Headers.UserAgent.Clear();
            request.Headers.UserAgent.ParseAdd($"GenesisGest.Net ({contatoTecnico.Trim()})");
        }

        return request;
    }

    private static string FindExactLabelId(byte[] json, string codigo)
    {
        using var document = JsonDocument.Parse(json);
        return FindExactLabelId(document.RootElement, codigo) ?? string.Empty;
    }

    private static string? FindExactLabelId(JsonElement element, string codigo)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var tracking = ReadFirstString(element, "tracking", "tracking_code", "trackingCode");
            var id = ReadFirstString(element, "id", "order_id", "orderId");
            if (string.Equals(tracking, codigo, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(id))
            {
                return id;
            }

            foreach (var property in element.EnumerateObject())
            {
                var nested = FindExactLabelId(property.Value, codigo);
                if (!string.IsNullOrWhiteSpace(nested))
                {
                    return nested;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var nested = FindExactLabelId(item, codigo);
                if (!string.IsNullOrWhiteSpace(nested))
                {
                    return nested;
                }
            }
        }

        return null;
    }

    private static ParsedTrackingPayload ParseTrackingPayload(byte[] json, string? etiquetaId)
    {
        using var document = JsonDocument.Parse(json);
        var node = FindTrackingNode(document.RootElement, etiquetaId) ?? document.RootElement;
        var status = ReadFirstString(node, "status", "status_name", "statusName", "situacao", "situation", "state");
        var events = new List<LogisticaTrackingEvent>();
        CollectTrackingEvents(node, events);
        var distinct = events
            .GroupBy(item => new { item.DataHora, item.Status, item.Local, item.Descricao })
            .Select(group => group.First())
            .OrderByDescending(item => item.DataHora ?? DateTime.MinValue)
            .ToList();
        return new ParsedTrackingPayload(status, distinct);
    }

    private static JsonElement? FindTrackingNode(JsonElement element, string? etiquetaId)
    {
        if (string.IsNullOrWhiteSpace(etiquetaId))
        {
            return null;
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, etiquetaId, StringComparison.OrdinalIgnoreCase))
                {
                    return property.Value;
                }
            }

            var id = ReadFirstString(element, "id", "order_id", "orderId");
            if (string.Equals(id, etiquetaId, StringComparison.OrdinalIgnoreCase))
            {
                return element;
            }

            foreach (var property in element.EnumerateObject())
            {
                var nested = FindTrackingNode(property.Value, etiquetaId);
                if (nested.HasValue)
                {
                    return nested;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var nested = FindTrackingNode(item, etiquetaId);
                if (nested.HasValue)
                {
                    return nested;
                }
            }
        }

        return null;
    }

    private static void CollectTrackingEvents(JsonElement element, List<LogisticaTrackingEvent> events)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object)
                {
                    var status = ReadFirstString(item, "status", "status_name", "statusName", "name", "titulo", "title");
                    var description = ReadFirstString(item, "description", "descricao", "message", "detail", "detalhe");
                    var location = ReadFirstString(item, "location", "local", "city", "cidade", "unit", "unidade");
                    var dateTime = ReadFirstDateTime(item, "date", "datetime", "data", "data_hora", "created_at", "updated_at", "timestamp");
                    if (!string.IsNullOrWhiteSpace(status) || !string.IsNullOrWhiteSpace(description))
                    {
                        events.Add(new LogisticaTrackingEvent(dateTime, status ?? "Evento", location, description));
                    }
                }

                CollectTrackingEvents(item, events);
            }

            return;
        }

        if (element.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (property.Value.ValueKind is JsonValueKind.Array or JsonValueKind.Object)
            {
                CollectTrackingEvents(property.Value, events);
            }
        }
    }

    private static string? ReadFirstString(JsonElement element, params string[] names)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (!names.Any(name => string.Equals(name, property.Name, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (property.Value.ValueKind is JsonValueKind.String or JsonValueKind.Number)
            {
                var value = property.Value.ToString().Trim();
                if (value.Length > 0)
                {
                    return value;
                }
            }
        }

        return null;
    }

    private static DateTime? ReadFirstDateTime(JsonElement element, params string[] names)
    {
        var value = ReadFirstString(element, names);
        return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed)
            ? parsed.UtcDateTime
            : null;
    }

    private string? ReadConfiguration(params string[] keys)
    {
        foreach (var key in keys)
        {
            var value = _configuration[key];
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }

    private static async Task<byte[]> ReadLimitedBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content.Headers.ContentLength > MaxResponseBytes)
        {
            throw new InvalidDataException($"Resposta externa excedeu o limite de {MaxResponseBytes} bytes.");
        }

        await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var target = new MemoryStream();
        var buffer = new byte[16_384];
        while (true)
        {
            var read = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
            if (read == 0)
            {
                break;
            }

            target.Write(buffer, 0, read);
            if (target.Length > MaxResponseBytes)
            {
                throw new InvalidDataException($"Resposta externa excedeu o limite de {MaxResponseBytes} bytes.");
            }
        }

        return target.ToArray();
    }

    private static string ComputeSha256(byte[] body) => Convert.ToHexString(SHA256.HashData(body)).ToLowerInvariant();

    private sealed record ParsedTrackingPayload(string? Status, List<LogisticaTrackingEvent> Events)
    {
        public bool HasOperationalData => !string.IsNullOrWhiteSpace(Status) || Events.Count > 0;
    }
}

public sealed record LogisticaTrackingEvent(
    DateTime? DataHora,
    string Status,
    string? Local,
    string? Descricao);

public sealed record LogisticaTrackingResult(
    bool Configurada,
    bool Operacional,
    string Fonte,
    int? HttpStatusCode,
    string? StatusExterno,
    List<LogisticaTrackingEvent> Eventos,
    List<string> Pendencias,
    string? RespostaSha256)
{
    public static LogisticaTrackingResult NotConfigured(string fonte, List<string> pendencias) =>
        new(false, false, fonte, null, null, [], pendencias, null);

    public static LogisticaTrackingResult Failed(
        string fonte,
        int? httpStatusCode,
        string? respostaSha256,
        List<string> pendencias) =>
        new(true, false, fonte, httpStatusCode, null, [], pendencias, respostaSha256);

    public static LogisticaTrackingResult Success(
        string fonte,
        int httpStatusCode,
        string? status,
        List<LogisticaTrackingEvent> eventos,
        string respostaSha256) =>
        new(true, true, fonte, httpStatusCode, status, eventos, [], respostaSha256);
}
