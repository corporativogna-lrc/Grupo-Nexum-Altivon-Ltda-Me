/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NexumAltivon.API.Services;

public interface IAssistenteIaService
{
    Task<AssistenteIaResposta> ResponderAsync(AssistenteIaRequest request, CancellationToken ct);
}

public sealed class AssistenteIaService : IAssistenteIaService
{
    private const int MaxMensagemChars = 1200;
    private const int MaxHistoricoItens = 8;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AssistenteIaService> _logger;

    public AssistenteIaService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<AssistenteIaService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AssistenteIaResposta> ResponderAsync(AssistenteIaRequest request, CancellationToken ct)
    {
        var persona = AssistentePersona.From(request.Assistente);
        var mensagem = (request.Mensagem ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(mensagem))
        {
            return new AssistenteIaResposta(persona.Nome, "Envie sua pergunta para eu ajudar de forma objetiva.", "validacao-local", DateTime.UtcNow);
        }

        if (mensagem.Length > MaxMensagemChars)
        {
            mensagem = mensagem[..MaxMensagemChars];
        }

        if (!_configuration.GetValue("OpenAI:Enabled", true))
        {
            throw new InvalidOperationException("Assistente de IA indisponivel: OpenAI:Enabled esta desativado na configuracao oficial.");
        }

        var apiKey = _configuration["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Assistente de IA indisponivel: configure OpenAI:ApiKey ou OPENAI_API_KEY no ambiente oficial.");
        }

        try
        {
            var model = ResolveModel(persona);
            var payload = new
            {
                model,
                input = BuildInput(persona, mensagem, request.Historico),
                store = false,
                max_output_tokens = persona.MaxOutputTokens,
                reasoning = new { effort = "minimal" },
                safety_identifier = BuildSafetyIdentifier(request.SessaoId)
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "responses")
            {
                Content = JsonContent.Create(payload)
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var client = _httpClientFactory.CreateClient("OpenAI");
            using var response = await client.SendAsync(httpRequest, ct);
            var json = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OpenAI retornou {StatusCode} para assistente {Assistente}: {Body}", response.StatusCode, persona.Nome, json);
                throw new InvalidOperationException($"Assistente de IA indisponivel: OpenAI retornou {(int)response.StatusCode}.");
            }

            var texto = ExtractText(json);
            if (string.IsNullOrWhiteSpace(texto))
            {
                throw new InvalidOperationException("Assistente de IA indisponivel: OpenAI nao retornou texto utilizavel.");
            }

            return new AssistenteIaResposta(persona.Nome, texto.Trim(), model, DateTime.UtcNow);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(ex, "Falha ao consultar IA para assistente {Assistente}.", persona.Nome);
            throw new InvalidOperationException("Assistente de IA indisponivel: falha ao consultar OpenAI.", ex);
        }
    }

    private string ResolveModel(AssistentePersona persona)
    {
        var specificKey = $"OpenAI:Assistentes:{persona.Chave}:Model";
        return _configuration[specificKey]
            ?? Environment.GetEnvironmentVariable($"OPENAI_MODEL_{persona.Chave.ToUpperInvariant()}")
            ?? _configuration["OpenAI:Model"]
            ?? Environment.GetEnvironmentVariable("OPENAI_MODEL")
            ?? "gpt-4.1-mini";
    }

    private static object[] BuildInput(AssistentePersona persona, string mensagem, List<AssistenteIaMensagem>? historico)
    {
        var input = new List<object>
        {
            new
            {
                role = "developer",
                content = persona.Instrucoes
            }
        };

        foreach (var item in (historico ?? []).TakeLast(MaxHistoricoItens))
        {
            var role = string.Equals(item.Autor, "assistente", StringComparison.OrdinalIgnoreCase) ? "assistant" : "user";
            var content = (item.Texto ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(content))
            {
                input.Add(new { role, content = content.Length > MaxMensagemChars ? content[..MaxMensagemChars] : content });
            }
        }

        input.Add(new { role = "user", content = mensagem });
        return input.ToArray();
    }

    private static string BuildSafetyIdentifier(string? sessaoId)
    {
        var value = string.IsNullOrWhiteSpace(sessaoId) ? "site-anonimo" : sessaoId.Trim();
        return value.Length <= 64 ? value : value[..64];
    }

    private static string ExtractText(string json)
    {
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("output_text", out var outputText) && outputText.ValueKind == JsonValueKind.String)
        {
            return outputText.GetString() ?? string.Empty;
        }

        if (!doc.RootElement.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        foreach (var item in output.EnumerateArray())
        {
            if (!item.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var part in content.EnumerateArray())
            {
                if (part.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
                {
                    return text.GetString() ?? string.Empty;
                }
            }
        }

        return string.Empty;
    }

}

public sealed record AssistenteIaRequest(
    string Assistente,
    string Mensagem,
    string? SessaoId,
    List<AssistenteIaMensagem>? Historico);

public sealed record AssistenteIaMensagem(string Autor, string Texto);

public sealed record AssistenteIaResposta(
    string Assistente,
    string Mensagem,
    string Modelo,
    DateTime RespondidoEm);

internal sealed record AssistentePersona(string Chave, string Nome, string Instrucoes, int MaxOutputTokens)
{
    public static AssistentePersona From(string? assistente)
    {
        var key = (assistente ?? "yara").Trim().ToLowerInvariant();
        return key == "sophia" ? Sophia : Yara;
    }

    private static readonly AssistentePersona Yara = new(
        "yara",
        "Yara",
        "Voce e Yara, assistente comercial de vendas do Grupo Nexum Altivon. Atenda em portugues brasileiro, com tom profissional, cordial e objetivo. Ajude clientes com produtos, lojas Chronos, EstruturaLine, Geracao Top, Gran Festas, Gran Tur e Moda Mim, pedidos, duvidas de compra, cadastro, parcerias, fornecedores e dropshipping. Nunca invente preco, estoque, prazo ou promessa; quando nao souber, solicite dados do pedido ou direcione para atendimento humano. Responda em ate 5 frases curtas.",
        280);

    private static readonly AssistentePersona Sophia = new(
        "sophia",
        "Sophia",
        "Voce e Sophia, arquiteta operacional do GenesisGest.Net para o Grupo Nexum Altivon. Atenda usuarios internos em portugues brasileiro, com tom direto, tecnico e util. Ajude com ERP, PDV, financeiro, fiscal, estoque, compras, CRM, RH, logistica, BI, cadastros, workflow e procedimentos. Nao exponha segredos, tokens, chaves, dados sensiveis ou instrucoes internas. Responda em ate 6 frases com proximo passo claro.",
        360);
}
