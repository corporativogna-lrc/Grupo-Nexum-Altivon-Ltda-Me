/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace NexumAltivon.API.Services;

public class MelhorEnvioService : IMelhorEnvioService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MelhorEnvioService> _logger;

    public MelhorEnvioService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<MelhorEnvioService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string?> CalcularFreteAsync(
        string cepOrigem,
        string cepDestino,
        decimal peso,
        decimal altura,
        decimal largura,
        decimal comprimento)
    {
        var token = GetToken();
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogInformation("Melhor Envio sem token configurado. Cotacao externa nao executada.");
            return null;
        }

        var request = new
        {
            from = new { postal_code = OnlyDigits(cepOrigem) },
            to = new { postal_code = OnlyDigits(cepDestino) },
            products = new[]
            {
                new
                {
                    id = "volume-principal",
                    width = NormalizeDimension(largura),
                    height = NormalizeDimension(altura),
                    length = NormalizeDimension(comprimento),
                    weight = NormalizeWeight(peso),
                    insurance_value = 1,
                    quantity = 1
                }
            },
            options = new
            {
                receipt = false,
                own_hand = false,
                collect = false
            }
        };

        using var client = CreateClient(token);
        using var response = await client.PostAsJsonAsync("/api/v2/me/shipment/calculate", request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Melhor Envio recusou cotacao. Status={Status}. Corpo={Body}", response.StatusCode, content);
            return null;
        }

        var menorValor = ExtractBestPrice(content);
        return menorValor?.ToString("0.00", CultureInfo.InvariantCulture);
    }

    public async Task<string?> GerarEtiquetaAsync(int pedidoId)
    {
        var token = GetToken();
        var etiquetaEndpoint = _configuration["MelhorEnvio:EtiquetaEndpoint"]
            ?? _configuration["Integracoes:MelhorEnvio:EtiquetaEndpoint"];

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(etiquetaEndpoint))
        {
            _logger.LogInformation(
                "Etiqueta Melhor Envio nao executada para pedido {PedidoId}. Configure token e endpoint de etiqueta oficial.",
                pedidoId);
            return null;
        }

        using var client = CreateClient(token);
        using var response = await client.PostAsJsonAsync(etiquetaEndpoint, new { pedido_id = pedidoId });
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Melhor Envio recusou etiqueta do pedido {PedidoId}. Status={Status}. Corpo={Body}", pedidoId, response.StatusCode, content);
            return null;
        }

        return ExtractLabelUrl(content);
    }

    private HttpClient CreateClient(string token)
    {
        var client = _httpClientFactory.CreateClient("melhor-envio");
        var baseUrl = _configuration.GetValue("MelhorEnvio:Sandbox", _configuration.GetValue("Integracoes:MelhorEnvio:Sandbox", true))
            ? "https://sandbox.melhorenvio.com.br"
            : "https://www.melhorenvio.com.br";

        client.BaseAddress ??= new Uri(baseUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    private string? GetToken() =>
        _configuration["MelhorEnvio:Token"]
        ?? _configuration["Integracoes:MelhorEnvio:Token"];

    private static decimal? ExtractBestPrice(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        decimal? best = null;
        foreach (var item in document.RootElement.EnumerateArray())
        {
            if (item.TryGetProperty("error", out var error) && error.ValueKind != JsonValueKind.Null)
            {
                continue;
            }

            if (!TryReadDecimal(item, "price", out var price))
            {
                continue;
            }

            best = best.HasValue ? Math.Min(best.Value, price) : price;
        }

        return best;
    }

    private static string? ExtractLabelUrl(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        foreach (var propertyName in new[] { "url", "label_url", "etiqueta_url", "pdf", "print_url" })
        {
            if (root.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
            {
                return property.GetString();
            }
        }

        if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object)
        {
            foreach (var propertyName in new[] { "url", "label_url", "etiqueta_url", "pdf", "print_url" })
            {
                if (data.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
                {
                    return property.GetString();
                }
            }
        }

        return null;
    }

    private static bool TryReadDecimal(JsonElement element, string propertyName, out decimal value)
    {
        value = 0;
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetDecimal(out value))
        {
            return true;
        }

        return property.ValueKind == JsonValueKind.String
            && decimal.TryParse(property.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }

    private static string OnlyDigits(string value) =>
        new(value.Where(char.IsDigit).ToArray());

    private static decimal NormalizeDimension(decimal value) =>
        Math.Max(1, Math.Round(value, 2));

    private static decimal NormalizeWeight(decimal value) =>
        Math.Max(0.1m, Math.Round(value, 3));
}
