/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NexumAltivon.API.Models;

namespace NexumAltivon.API.Services;

public interface INotificacaoService
{
    Task EnviarConfirmacaoPedidoAsync(Cliente cliente, Pedido pedido, CancellationToken ct = default);
    Task EnviarConfirmacaoPagamentoAsync(Cliente cliente, Pedido pedido, CancellationToken ct = default);
    Task EnviarNotaFiscalEmitidaAsync(Cliente cliente, Pedido pedido, Fiscal fiscal, CancellationToken ct = default);
    Task EnviarConfirmacaoCadastroAsync(Cliente cliente, string linkConfirmacao, CancellationToken ct = default);
    Task EnviarNotificacaoWhatsAppAsync(string? telefone, string mensagem, CancellationToken ct = default);
    Task EnviarEmailAsync(string? destinatario, string assunto, string corpoHtml, CancellationToken ct = default);
    Task EnviarAlertaEstoqueBaixoAsync(Produto produto, CancellationToken ct = default);
    Task EnviarStatusPedidoAsync(Cliente cliente, Pedido pedido, string mensagemPersonalizada, CancellationToken ct = default);
}

public sealed class NotificacaoService : INotificacaoService
{
    private const int MaxProviderErrorLength = 2_000;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificacaoService> _logger;

    public NotificacaoService(
        IHttpClientFactory factory,
        IConfiguration configuration,
        ILogger<NotificacaoService> logger)
    {
        _httpClient = factory.CreateClient("Notificacoes");
        _configuration = configuration;
        _logger = logger;
    }

    public async Task EnviarConfirmacaoPedidoAsync(Cliente cliente, Pedido pedido, CancellationToken ct = default)
    {
        var numeroPedido = Html(pedido.NumeroPedido);
        var nomeCliente = Html(cliente.Nome);
        var assunto = $"Pedido {pedido.NumeroPedido} recebido - Nexum Altivon";
        var corpo = $$"""
            <!DOCTYPE html>
            <html>
            <head><style>
            body { font-family: 'Montserrat', sans-serif; background: #0A0A0A; color: #F5F5F5; }
            .container { max-width: 600px; margin: 0 auto; background: #1A1A1A; padding: 30px; border: 1px solid #C9A227; }
            h1 { color: #C9A227; font-size: 24px; }
            .pedido-info { background: #2A2A2A; padding: 20px; border-radius: 8px; margin: 20px 0; }
            .footer { text-align: center; color: #A0A0A0; font-size: 12px; margin-top: 30px; }
            </style></head>
            <body>
            <div class='container'>
            <h1>Pedido recebido</h1>
            <p>Ola <strong>{{nomeCliente}}</strong>,</p>
            <p>Seu pedido <strong style='color:#C9A227'>{{numeroPedido}}</strong> foi recebido com sucesso.</p>
            <div class='pedido-info'>
            <p><strong>Total:</strong> R$ {{pedido.Total:N2}}</p>
            <p><strong>Status:</strong> Aguardando pagamento</p>
            <p><strong>Data:</strong> {{pedido.CreatedAt:dd/MM/yyyy HH:mm}}</p>
            </div>
            <p>Assim que o pagamento for confirmado, iniciaremos a separacao do seu pedido.</p>
            <div class='footer'><p>Grupo Nexum Altivon<br>nexumaltivon.com.br</p></div>
            </div>
            </body>
            </html>
            """;

        await EnviarEmailAsync(cliente.Email, assunto, corpo, ct);
        _logger.LogInformation("Confirmacao de pedido enviada: {NumeroPedido}", pedido.NumeroPedido);
    }

    public async Task EnviarConfirmacaoPagamentoAsync(Cliente cliente, Pedido pedido, CancellationToken ct = default)
    {
        var numeroPedido = Html(pedido.NumeroPedido);
        var nomeCliente = Html(cliente.Nome);
        var assunto = $"Pagamento confirmado - Pedido {pedido.NumeroPedido}";
        var corpo = $$"""
            <!DOCTYPE html>
            <html>
            <body style='font-family:Montserrat,sans-serif;background:#0A0A0A;color:#F5F5F5;'>
            <div style='max-width:600px;margin:0 auto;background:#1A1A1A;padding:30px;border:1px solid #C9A227;'>
            <h1 style='color:#C9A227'>Pagamento confirmado</h1>
            <p>Ola <strong>{{nomeCliente}}</strong>,</p>
            <p>O pagamento do pedido <strong>{{numeroPedido}}</strong> foi confirmado.</p>
            <p><strong>Valor pago:</strong> R$ {{pedido.Total:N2}}</p>
            <p>Seu pedido agora esta em <strong>separacao</strong> e em breve sera enviado.</p>
            <p><a href='https://nexumaltivon.com.br/pedidos/{{Uri.EscapeDataString(pedido.NumeroPedido)}}' style='color:#C9A227'>Acompanhar pedido</a></p>
            </div>
            </body>
            </html>
            """;

        await EnviarEmailAsync(cliente.Email, assunto, corpo, ct);
        await EnviarEmailAsync(ObterConfiguracaoObrigatoria("Alertas:VendaEmailAdmin"), $"[COPIA] {assunto}", corpo, ct);
    }

    public async Task EnviarConfirmacaoCadastroAsync(Cliente cliente, string linkConfirmacao, CancellationToken ct = default)
    {
        if (!Uri.TryCreate(linkConfirmacao, UriKind.Absolute, out var link) || link.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException("Link HTTPS de confirmacao obrigatorio.", nameof(linkConfirmacao));
        }

        const string assunto = "Confirme seu cadastro - Nexum Altivon";
        var nomeCliente = Html(cliente.Nome);
        var linkSeguro = Html(link.ToString());
        var corpo = $$"""
            <!DOCTYPE html>
            <html>
            <body style='font-family:Arial,sans-serif;background:#f6f3ea;color:#1f1f1f;padding:0;margin:0;'>
            <div style='max-width:640px;margin:0 auto;background:#ffffff;border:1px solid #d7c38a;padding:28px;'>
            <h1 style='margin-top:0;color:#8a6d1f;'>Confirme seu cadastro</h1>
            <p>Ola <strong>{{nomeCliente}}</strong>,</p>
            <p>Recebemos seu cadastro na Nexum Altivon e ele esta pronto para ativacao.</p>
            <p><a href='{{linkSeguro}}' style='display:inline-block;background:#c9a227;color:#000;padding:14px 22px;border-radius:8px;text-decoration:none;font-weight:bold;'>Confirmar cadastro</a></p>
            <p>Se o botao nao abrir, copie este endereco:</p>
            <p style='word-break:break-all;'><a href='{{linkSeguro}}'>{{linkSeguro}}</a></p>
            </div>
            </body>
            </html>
            """;

        await EnviarEmailAsync(cliente.Email, assunto, corpo, ct);
        await EnviarEmailAsync(ObterConfiguracaoObrigatoria("Alertas:VendaEmailAdmin"), $"[COPIA] {assunto}", corpo, ct);
    }

    public async Task EnviarNotaFiscalEmitidaAsync(
        Cliente cliente,
        Pedido pedido,
        Fiscal fiscal,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fiscal.ChaveAcesso) ||
            string.IsNullOrWhiteSpace(fiscal.NumeroNfe) ||
            string.IsNullOrWhiteSpace(fiscal.XmlUrl) ||
            string.IsNullOrWhiteSpace(fiscal.DanfeUrl))
        {
            throw new InvalidOperationException("Notificacao fiscal exige numero, chave de acesso, XML e DANFE reais.");
        }

        var assunto = $"Nota fiscal emitida - Pedido {pedido.NumeroPedido}";
        var nomeCliente = Html(cliente.Nome);
        var numeroPedido = Html(pedido.NumeroPedido);
        var corpo = $$"""
            <!DOCTYPE html>
            <html>
            <body style='font-family:Arial,sans-serif;background:#f6f3ea;color:#1f1f1f;padding:0;margin:0;'>
            <div style='max-width:640px;margin:0 auto;background:#ffffff;border:1px solid #d7c38a;padding:28px;'>
            <h1 style='margin-top:0;color:#8a6d1f;'>Nota fiscal emitida</h1>
            <p>Ola <strong>{{nomeCliente}}</strong>,</p>
            <p>A nota fiscal do pedido <strong>{{numeroPedido}}</strong> foi emitida.</p>
            <p><strong>Status:</strong> {{Html(fiscal.StatusNfe.ToString())}}</p>
            <p><strong>NFe:</strong> {{Html(fiscal.NumeroNfe)}}</p>
            <p><strong>Chave de acesso:</strong> {{Html(fiscal.ChaveAcesso)}}</p>
            <p><strong>Total:</strong> R$ {{pedido.Total:N2}}</p>
            <p><strong>XML:</strong> <a href='{{Html(fiscal.XmlUrl)}}'>Baixar XML</a></p>
            <p><strong>DANFE:</strong> <a href='{{Html(fiscal.DanfeUrl)}}'>Baixar DANFE</a></p>
            </div>
            </body>
            </html>
            """;

        await EnviarEmailAsync(cliente.Email, assunto, corpo, ct);
        await EnviarEmailAsync(ObterConfiguracaoObrigatoria("Alertas:VendaEmailAdmin"), $"[COPIA] {assunto}", corpo, ct);
    }

    public async Task EnviarNotificacaoWhatsAppAsync(
        string? telefone,
        string mensagem,
        CancellationToken ct = default)
    {
        if (!_configuration.GetValue<bool>("Integracoes:WhatsApp:Ativo"))
        {
            throw new InvalidOperationException("Integracoes:WhatsApp:Ativo deve estar habilitado para envio real.");
        }

        if (string.IsNullOrWhiteSpace(mensagem))
        {
            throw new ArgumentException("Mensagem de WhatsApp obrigatoria.", nameof(mensagem));
        }

        var apiUrl = ObterConfiguracaoObrigatoria("Integracoes:WhatsApp:ApiUrl");
        if (!Uri.TryCreate(apiUrl, UriKind.Absolute, out var endpoint) ||
            (!string.Equals(endpoint.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
             !string.Equals(endpoint.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Integracoes:WhatsApp:ApiUrl deve ser uma URL HTTP ou HTTPS absoluta.");
        }

        var numero = NormalizarTelefoneBrasil(telefone);
        var requestBody = new { number = numero, text = mensagem.Trim() };
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(requestBody)
        };

        var apiKey = _configuration["Integracoes:WhatsApp:ApiKey"]?.Trim();
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        try
        {
            using var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var error = LimitarErro(await response.Content.ReadAsStringAsync(ct));
                throw new InvalidOperationException($"Provedor WhatsApp recusou o envio. Status={(int)response.StatusCode}. Corpo={error}");
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            _logger.LogError(ex, "Falha real no envio WhatsApp para {Telefone}", numero);
            throw;
        }
    }

    public async Task EnviarEmailAsync(
        string? destinatario,
        string assunto,
        string corpoHtml,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(destinatario))
        {
            throw new ArgumentException("Destinatario obrigatorio para envio de e-mail.", nameof(destinatario));
        }

        if (string.IsNullOrWhiteSpace(assunto))
        {
            throw new ArgumentException("Assunto obrigatorio para envio de e-mail.", nameof(assunto));
        }

        if (string.IsNullOrWhiteSpace(corpoHtml))
        {
            throw new ArgumentException("Corpo HTML obrigatorio para envio de e-mail.", nameof(corpoHtml));
        }

        var apiKey = ObterConfiguracaoObrigatoria("Integracoes:SendGrid:ApiKey");
        var fromEmail = ObterConfiguracaoObrigatoria("Integracoes:SendGrid:FromEmail");
        var fromName = ObterConfiguracaoObrigatoria("Integracoes:SendGrid:FromName");
        var sendGridRequest = new
        {
            personalizations = new[] { new { to = new[] { new { email = destinatario.Trim() } } } },
            from = new { email = fromEmail, name = fromName },
            subject = assunto.Trim(),
            content = new[] { new { type = "text/html", value = corpoHtml } }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.sendgrid.com/v3/mail/send")
        {
            Content = JsonContent.Create(sendGridRequest)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        try
        {
            using var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var error = LimitarErro(await response.Content.ReadAsStringAsync(ct));
                throw new InvalidOperationException($"SendGrid recusou o envio para {destinatario}. Status={(int)response.StatusCode}. Corpo={error}");
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            _logger.LogError(ex, "Falha real no envio de e-mail para {Destinatario}", destinatario);
            throw;
        }
    }

    public Task EnviarAlertaEstoqueBaixoAsync(Produto produto, CancellationToken ct = default)
    {
        var assunto = $"[ALERTA] Estoque baixo - {produto.Nome}";
        var corpo = $"<p>Produto <strong>{produto.Nome}</strong> (SKU: {produto.Sku}) atingiu estoque baixo.</p>" +
                    $"<p>Estoque atual: <strong>{produto.EstoqueAtual}</strong> | Minimo: {produto.EstoqueMinimo}</p>" +
                    $"<p>Loja: {produto.Loja?.Nome}</p>";

        return EnviarEmailAsync(ObterConfiguracaoObrigatoria("Alertas:EstoqueEmailAdmin"), assunto, corpo, ct);
    }

    public async Task EnviarStatusPedidoAsync(
        Cliente cliente,
        Pedido pedido,
        string mensagemPersonalizada,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(mensagemPersonalizada))
        {
            throw new ArgumentException("Mensagem de status obrigatoria.", nameof(mensagemPersonalizada));
        }

        var assunto = $"Atualizacao do pedido {pedido.NumeroPedido}";
        var nomeCliente = Html(cliente.Nome);
        var numeroPedido = Html(pedido.NumeroPedido);
        var mensagemSegura = Html(mensagemPersonalizada);
        var corpo = $$"""
            <!DOCTYPE html>
            <html>
            <body style='font-family:Montserrat,sans-serif;background:#0A0A0A;color:#F5F5F5;'>
            <div style='max-width:600px;margin:0 auto;background:#1A1A1A;padding:30px;border:1px solid #C9A227;'>
            <h1 style='color:#C9A227'>Atualizacao de pedido</h1>
            <p>Ola <strong>{{nomeCliente}}</strong>,</p>
            <p>{{mensagemSegura}}</p>
            <p>Pedido: <strong>{{numeroPedido}}</strong></p>
            <p>Status atual: <strong>{{Html(pedido.Status.ToString())}}</strong></p>
            </div>
            </body>
            </html>
            """;

        await EnviarEmailAsync(cliente.Email, assunto, corpo, ct);

        if (_configuration.GetValue<bool>("Integracoes:WhatsApp:Ativo"))
        {
            await EnviarNotificacaoWhatsAppAsync(
                cliente.Telefone,
                $"Nexum Altivon: Pedido {pedido.NumeroPedido} - {mensagemPersonalizada}",
                ct);
        }
        else
        {
            _logger.LogInformation(
                "Status do pedido {NumeroPedido} entregue por e-mail; canal WhatsApp esta desativado na configuracao oficial.",
                pedido.NumeroPedido);
        }
    }

    private string ObterConfiguracaoObrigatoria(string key)
    {
        var value = _configuration[key]?.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Configuracao obrigatoria ausente: {key}.");
        }

        return value;
    }

    private static string NormalizarTelefoneBrasil(string? telefone)
    {
        var digits = new string((telefone ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.StartsWith("55", StringComparison.Ordinal) && digits.Length is 12 or 13)
        {
            return digits;
        }

        if (digits.Length is 10 or 11)
        {
            return "55" + digits;
        }

        throw new ArgumentException("Telefone brasileiro deve conter DDD e numero validos.", nameof(telefone));
    }

    private static string LimitarErro(string value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "sem corpo" : value.Trim();
        return normalized.Length <= MaxProviderErrorLength
            ? normalized
            : normalized[..MaxProviderErrorLength];
    }

    private static string Html(string? value) => HtmlEncoder.Default.Encode(value ?? string.Empty);
}
