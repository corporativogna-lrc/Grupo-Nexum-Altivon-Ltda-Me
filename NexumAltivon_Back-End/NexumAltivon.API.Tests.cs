/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NexumAltivon.API.Models;
using NexumAltivon.API.Services;
using Xunit;

namespace NexumAltivon.API.Tests;

public sealed class AssistenteIaServiceTests
{
    [Fact]
    public async Task ResponderAsync_DeveEnviarContratoOficialEExtrairOutputText()
    {
        var transport = new RecordingHttpMessageHandler((_, _) => JsonResponse(HttpStatusCode.OK, "{\"output_text\":\"Resposta operacional\"}"));
        var service = CreateService(transport, OpenAiConfiguration());
        var request = new AssistenteIaRequest(
            "sophia",
            new string('a', 1_300),
            "sessao-operacional",
            Enumerable.Range(1, 10).Select(i => new AssistenteIaMensagem(i % 2 == 0 ? "assistente" : "usuario", $"historico-{i}")).ToList());

        var result = await service.ResponderAsync(request, CancellationToken.None);

        result.Assistente.Should().Be("Sophia");
        result.Mensagem.Should().Be("Resposta operacional");
        result.Modelo.Should().Be("gpt-4.1-mini");
        transport.Requests.Should().ContainSingle();
        var sent = transport.Requests.Single();
        sent.Method.Should().Be(HttpMethod.Post);
        sent.Uri.Should().Be("https://api.openai.com/v1/responses");
        sent.Authorization.Should().Be("Bearer chave-operacional");

        using var body = JsonDocument.Parse(sent.Body);
        body.RootElement.GetProperty("model").GetString().Should().Be("gpt-4.1-mini");
        body.RootElement.GetProperty("store").GetBoolean().Should().BeFalse();
        var input = body.RootElement.GetProperty("input");
        input.GetArrayLength().Should().Be(10);
        input[input.GetArrayLength() - 1].GetProperty("content").GetString().Should().HaveLength(1_200);
    }

    [Fact]
    public async Task ResponderAsync_DeveExtrairTextoDaEstruturaOutput()
    {
        const string payload = "{\"output\":[{\"content\":[{\"type\":\"output_text\",\"text\":\"Conteudo aninhado\"}]}]}";
        var service = CreateService(
            new RecordingHttpMessageHandler((_, _) => JsonResponse(HttpStatusCode.OK, payload)),
            OpenAiConfiguration());

        var result = await service.ResponderAsync(
            new AssistenteIaRequest("yara", "Qual o proximo passo?", "sessao", null),
            CancellationToken.None);

        result.Assistente.Should().Be("Yara");
        result.Mensagem.Should().Be("Conteudo aninhado");
    }

    [Fact]
    public async Task ResponderAsync_DeveRecusarAssistenteDesconhecida()
    {
        var service = CreateService(new RecordingHttpMessageHandler(), OpenAiConfiguration());

        var action = () => service.ResponderAsync(
            new AssistenteIaRequest("operador", "mensagem", "sessao", null),
            CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>().WithMessage("*yara ou sophia*");
    }

    [Fact]
    public async Task ResponderAsync_DeveRecusarMensagemVazia()
    {
        var service = CreateService(new RecordingHttpMessageHandler(), OpenAiConfiguration());

        var action = () => service.ResponderAsync(
            new AssistenteIaRequest("yara", "  ", "sessao", null),
            CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>().WithMessage("*Mensagem obrigatoria*");
    }

    [Fact]
    public async Task ResponderAsync_DeveRecusarIntegracaoDesativada()
    {
        var service = CreateService(
            new RecordingHttpMessageHandler(),
            OpenAiConfiguration(("OpenAI:Enabled", "false")));

        var action = () => service.ResponderAsync(
            new AssistenteIaRequest("yara", "mensagem", "sessao", null),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*desativado*");
    }

    [Fact]
    public async Task ResponderAsync_DeveRecusarChaveAusente()
    {
        var service = CreateService(
            new RecordingHttpMessageHandler(),
            OpenAiConfiguration(("OpenAI:ApiKey", null)));

        var action = () => service.ResponderAsync(
            new AssistenteIaRequest("yara", "mensagem", "sessao", null),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*ApiKey*");
    }

    [Fact]
    public async Task ResponderAsync_DeveRecusarModeloAusente()
    {
        var originalModel = Environment.GetEnvironmentVariable("OPENAI_MODEL");
        var originalSpecificModel = Environment.GetEnvironmentVariable("OPENAI_MODEL_YARA");
        Environment.SetEnvironmentVariable("OPENAI_MODEL", null);
        Environment.SetEnvironmentVariable("OPENAI_MODEL_YARA", null);

        try
        {
            var service = CreateService(
                new RecordingHttpMessageHandler(),
                OpenAiConfiguration(("OpenAI:Model", null)));

            var action = () => service.ResponderAsync(
                new AssistenteIaRequest("yara", "mensagem", "sessao", null),
                CancellationToken.None);

            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*modelo OpenAI*");
        }
        finally
        {
            Environment.SetEnvironmentVariable("OPENAI_MODEL", originalModel);
            Environment.SetEnvironmentVariable("OPENAI_MODEL_YARA", originalSpecificModel);
        }
    }

    [Fact]
    public async Task ResponderAsync_DevePropagarRecusaDoProvedorSemRespostaFalsa()
    {
        var service = CreateService(
            new RecordingHttpMessageHandler((_, _) => JsonResponse(HttpStatusCode.TooManyRequests, "{\"error\":\"limite\"}")),
            OpenAiConfiguration());

        var action = () => service.ResponderAsync(
            new AssistenteIaRequest("sophia", "mensagem", "sessao", null),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*429*");
    }

    [Fact]
    public async Task ResponderAsync_DevePropagarRespostaSemTexto()
    {
        var service = CreateService(
            new RecordingHttpMessageHandler((_, _) => JsonResponse(HttpStatusCode.OK, "{\"output\":[]}")),
            OpenAiConfiguration());

        var action = () => service.ResponderAsync(
            new AssistenteIaRequest("sophia", "mensagem", null, null),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*nao retornou texto*");
    }

    [Fact]
    public async Task ResponderAsync_DeveEncapsularJsonInvalido()
    {
        var service = CreateService(
            new RecordingHttpMessageHandler((_, _) => JsonResponse(HttpStatusCode.OK, "nao-json")),
            OpenAiConfiguration());

        var action = () => service.ResponderAsync(
            new AssistenteIaRequest("sophia", "mensagem", "sessao", null),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<InvalidOperationException>();
        exception.WithMessage("*falha ao consultar OpenAI*");
        exception.Which.InnerException.Should().BeAssignableTo<JsonException>();
    }

    private static AssistenteIaService CreateService(RecordingHttpMessageHandler transport, IConfiguration configuration)
    {
        var client = new HttpClient(transport) { BaseAddress = new Uri("https://api.openai.com/v1/") };
        return new AssistenteIaService(
            new StaticHttpClientFactory(client),
            configuration,
            NullLogger<AssistenteIaService>.Instance);
    }

    private static IConfiguration OpenAiConfiguration(params (string Key, string? Value)[] overrides)
    {
        var values = new Dictionary<string, string?>
        {
            ["OpenAI:Enabled"] = "true",
            ["OpenAI:ApiKey"] = "chave-operacional",
            ["OpenAI:Model"] = "gpt-4.1-mini"
        };

        foreach (var (key, value) in overrides)
        {
            values[key] = value;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    private static Task<HttpResponseMessage> JsonResponse(HttpStatusCode status, string body)
    {
        return Task.FromResult(new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        });
    }
}

public sealed class NotificacaoServiceTests
{
    [Fact]
    public async Task EnviarEmailAsync_DeveEnviarContratoSendGridReal()
    {
        var transport = new RecordingHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted)));
        var service = CreateService(transport, NotificationConfiguration());

        await service.EnviarEmailAsync("cliente@nexumaltivon.com", "Assunto operacional", "<p>Conteudo</p>");

        transport.Requests.Should().ContainSingle();
        var sent = transport.Requests.Single();
        sent.Uri.Should().Be("https://api.sendgrid.com/v3/mail/send");
        sent.Authorization.Should().Be("Bearer sg-chave-operacional");
        using var body = JsonDocument.Parse(sent.Body);
        body.RootElement.GetProperty("personalizations")[0].GetProperty("to")[0].GetProperty("email").GetString()
            .Should().Be("cliente@nexumaltivon.com");
        body.RootElement.GetProperty("from").GetProperty("email").GetString().Should().Be("sistema@nexumaltivon.com");
    }

    [Theory]
    [InlineData("Integracoes:SendGrid:ApiKey")]
    [InlineData("Integracoes:SendGrid:FromEmail")]
    [InlineData("Integracoes:SendGrid:FromName")]
    public async Task EnviarEmailAsync_DeveRecusarConfiguracaoObrigatoriaAusente(string key)
    {
        var service = CreateService(
            new RecordingHttpMessageHandler(),
            NotificationConfiguration((key, null)));

        var action = () => service.EnviarEmailAsync("cliente@nexumaltivon.com", "Assunto", "<p>Corpo</p>");

        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage($"*{key}*");
    }

    [Theory]
    [InlineData(null, "Assunto", "<p>Corpo</p>", "*Destinatario*")]
    [InlineData("cliente@nexumaltivon.com", "", "<p>Corpo</p>", "*Assunto*")]
    [InlineData("cliente@nexumaltivon.com", "Assunto", "", "*Corpo HTML*")]
    public async Task EnviarEmailAsync_DeveRecusarContratoIncompleto(
        string? destinatario,
        string assunto,
        string corpo,
        string mensagem)
    {
        var service = CreateService(new RecordingHttpMessageHandler(), NotificationConfiguration());

        var action = () => service.EnviarEmailAsync(destinatario, assunto, corpo);

        await action.Should().ThrowAsync<ArgumentException>().WithMessage(mensagem);
    }

    [Fact]
    public async Task EnviarEmailAsync_DevePropagarRecusaSendGrid()
    {
        var transport = new RecordingHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("destinatario recusado")
        }));
        var service = CreateService(transport, NotificationConfiguration());

        var action = () => service.EnviarEmailAsync("cliente@nexumaltivon.com", "Assunto", "<p>Corpo</p>");

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Status=400*destinatario recusado*");
    }

    [Fact]
    public async Task EnviarWhatsAppAsync_DeveNormalizarNumeroEEnviarContratoReal()
    {
        var transport = new RecordingHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        var service = CreateService(transport, NotificationConfiguration());

        await service.EnviarNotificacaoWhatsAppAsync("(11) 99876-5432", "Pedido enviado");

        transport.Requests.Should().ContainSingle();
        var sent = transport.Requests.Single();
        sent.Uri.Should().Be("https://whatsapp.nexumaltivon.com/messages");
        sent.Authorization.Should().Be("Bearer wa-chave-operacional");
        using var body = JsonDocument.Parse(sent.Body);
        body.RootElement.GetProperty("number").GetString().Should().Be("5511998765432");
        body.RootElement.GetProperty("text").GetString().Should().Be("Pedido enviado");
    }

    [Fact]
    public async Task EnviarWhatsAppAsync_DeveRecusarIntegracaoDesativada()
    {
        var service = CreateService(
            new RecordingHttpMessageHandler(),
            NotificationConfiguration(("Integracoes:WhatsApp:Ativo", "false")));

        var action = () => service.EnviarNotificacaoWhatsAppAsync("11998765432", "Pedido enviado");

        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*deve estar habilitado*");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("")]
    public async Task EnviarWhatsAppAsync_DeveRecusarTelefoneInvalido(string telefone)
    {
        var service = CreateService(new RecordingHttpMessageHandler(), NotificationConfiguration());

        var action = () => service.EnviarNotificacaoWhatsAppAsync(telefone, "Pedido enviado");

        await action.Should().ThrowAsync<ArgumentException>().WithMessage("*DDD e numero validos*");
    }

    [Fact]
    public async Task EnviarWhatsAppAsync_DevePropagarRecusaDoProvedor()
    {
        var transport = new RecordingHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
        {
            Content = new StringContent("canal indisponivel")
        }));
        var service = CreateService(transport, NotificationConfiguration());

        var action = () => service.EnviarNotificacaoWhatsAppAsync("5511998765432", "Pedido enviado");

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Status=503*canal indisponivel*");
    }

    [Fact]
    public async Task NotificacoesDeDominio_DevemExecutarTodosOsEnviosConfigurados()
    {
        var transport = new RecordingHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted)));
        var service = CreateService(
            transport,
            NotificationConfiguration(("Integracoes:WhatsApp:Ativo", "false")));
        var cliente = new Cliente { Nome = "Cliente Operacional", Email = "cliente@nexumaltivon.com", Telefone = "11998765432" };
        var pedido = new Pedido
        {
            NumeroPedido = "NX-20260717-1",
            Total = 250.75m,
            Status = StatusPedido.EmSeparacao,
            CreatedAt = new DateTime(2026, 7, 17, 10, 0, 0, DateTimeKind.Utc)
        };
        var fiscal = new Fiscal
        {
            NumeroNfe = "12345",
            ChaveAcesso = new string('1', 44),
            XmlUrl = "https://api.nexumaltivon.com.br/api/anexos/download/fiscal/12345.xml",
            DanfeUrl = "https://api.nexumaltivon.com.br/api/anexos/download/fiscal/12345.pdf",
            StatusNfe = StatusNfe.Autorizada
        };
        var produto = new Produto { Nome = "Produto operacional", Sku = "SKU-001", EstoqueAtual = 1, EstoqueMinimo = 5 };

        await service.EnviarConfirmacaoPedidoAsync(cliente, pedido);
        await service.EnviarConfirmacaoPagamentoAsync(cliente, pedido);
        await service.EnviarConfirmacaoCadastroAsync(cliente, "https://nexumaltivon.com.br/confirmar-cadastro?token=real");
        await service.EnviarNotaFiscalEmitidaAsync(cliente, pedido, fiscal);
        await service.EnviarAlertaEstoqueBaixoAsync(produto);
        await service.EnviarStatusPedidoAsync(cliente, pedido, "Pedido em separacao");

        transport.Requests.Should().HaveCount(9);
        transport.Requests.Should().OnlyContain(request => request.Uri == "https://api.sendgrid.com/v3/mail/send");
    }

    [Fact]
    public async Task EnviarStatusPedidoAsync_DeveEnviarEmailEWhatsAppQuandoHabilitado()
    {
        var transport = new RecordingHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted)));
        var service = CreateService(transport, NotificationConfiguration());
        var cliente = new Cliente { Nome = "Cliente", Email = "cliente@nexumaltivon.com", Telefone = "11998765432" };
        var pedido = new Pedido { NumeroPedido = "NX-1", Status = StatusPedido.Enviado };

        await service.EnviarStatusPedidoAsync(cliente, pedido, "Pedido enviado");

        transport.Requests.Should().HaveCount(2);
        transport.Requests.Select(request => request.Uri).Should().Contain(new[]
        {
            "https://api.sendgrid.com/v3/mail/send",
            "https://whatsapp.nexumaltivon.com/messages"
        });
    }

    [Fact]
    public async Task EnviarNotaFiscalEmitidaAsync_DeveRecusarDocumentoIncompleto()
    {
        var service = CreateService(new RecordingHttpMessageHandler(), NotificationConfiguration());

        var action = () => service.EnviarNotaFiscalEmitidaAsync(
            new Cliente { Email = "cliente@nexumaltivon.com" },
            new Pedido { NumeroPedido = "NX-1" },
            new Fiscal { NumeroNfe = "123" });

        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*XML e DANFE reais*");
    }

    [Fact]
    public async Task EnviarConfirmacaoCadastroAsync_DeveRecusarLinkNaoSeguro()
    {
        var service = CreateService(new RecordingHttpMessageHandler(), NotificationConfiguration());

        var action = () => service.EnviarConfirmacaoCadastroAsync(
            new Cliente { Email = "cliente@nexumaltivon.com" },
            "http://nexumaltivon.com.br/confirmar");

        await action.Should().ThrowAsync<ArgumentException>().WithMessage("*HTTPS*");
    }

    private static NotificacaoService CreateService(RecordingHttpMessageHandler transport, IConfiguration configuration)
    {
        return new NotificacaoService(
            new StaticHttpClientFactory(new HttpClient(transport)),
            configuration,
            NullLogger<NotificacaoService>.Instance);
    }

    private static IConfiguration NotificationConfiguration(params (string Key, string? Value)[] overrides)
    {
        var values = new Dictionary<string, string?>
        {
            ["Integracoes:SendGrid:ApiKey"] = "sg-chave-operacional",
            ["Integracoes:SendGrid:FromEmail"] = "sistema@nexumaltivon.com",
            ["Integracoes:SendGrid:FromName"] = "GenesisGest.Net",
            ["Integracoes:WhatsApp:Ativo"] = "true",
            ["Integracoes:WhatsApp:ApiUrl"] = "https://whatsapp.nexumaltivon.com/messages",
            ["Integracoes:WhatsApp:ApiKey"] = "wa-chave-operacional",
            ["Alertas:VendaEmailAdmin"] = "administracao@nexumaltivon.com",
            ["Alertas:EstoqueEmailAdmin"] = "estoque@nexumaltivon.com"
        };

        foreach (var (key, value) in overrides)
        {
            values[key] = value;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }
}

internal sealed class StaticHttpClientFactory : IHttpClientFactory
{
    private readonly HttpClient _client;

    public StaticHttpClientFactory(HttpClient client)
    {
        _client = client;
    }

    public HttpClient CreateClient(string name) => _client;
}

internal sealed class RecordingHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _responseFactory;

    public RecordingHttpMessageHandler()
        : this((_, _) => throw new InvalidOperationException("Nenhuma requisicao HTTP era esperada neste teste."))
    {
    }

    public RecordingHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFactory)
    {
        _responseFactory = responseFactory;
    }

    public List<RecordedHttpRequest> Requests { get; } = [];

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
        Requests.Add(new RecordedHttpRequest(
            request.Method,
            request.RequestUri?.ToString() ?? string.Empty,
            request.Headers.Authorization?.ToString(),
            body));

        return await _responseFactory(request, cancellationToken);
    }
}

internal sealed record RecordedHttpRequest(
    HttpMethod Method,
    string Uri,
    string? Authorization,
    string Body);
