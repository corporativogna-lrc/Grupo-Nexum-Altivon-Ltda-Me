using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NexumAltivon.API.Models;

namespace NexumAltivon.API.Services
{
    public interface INotificacaoService
    {
        Task EnviarConfirmacaoPedidoAsync(Cliente cliente, Pedido pedido);
        Task EnviarConfirmacaoPagamentoAsync(Cliente cliente, Pedido pedido);
        Task EnviarNotificacaoWhatsAppAsync(string? telefone, string mensagem);
        Task EnviarEmailAsync(string? destinatario, string assunto, string corpoHtml);
        Task EnviarAlertaEstoqueBaixoAsync(Produto produto);
        Task EnviarStatusPedidoAsync(Cliente cliente, Pedido pedido, string mensagemPersonalizada);
    }

    public class NotificacaoService : INotificacaoService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<NotificacaoService> _logger;
        private readonly string? _sendGridKey;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string? _smtpServer;
        private readonly int _smtpPort;
        private readonly string? _smtpUsername;
        private readonly string? _smtpPassword;
        private readonly bool _smtpEnableSsl;
        private readonly string _smtpFromEmail;
        private readonly string _smtpFromName;
        private readonly bool _whatsappAtivo;
        private readonly string? _whatsappApiUrl;
        private readonly string? _whatsappApiKey;

        public NotificacaoService(IHttpClientFactory factory, IConfiguration config, ILogger<NotificacaoService> logger)
        {
            _httpClient = factory.CreateClient("Notificacoes");
            _config = config;
            _logger = logger;
            _sendGridKey = _config["Integracoes:SendGrid:ApiKey"];
            _fromEmail = _config["Integracoes:SendGrid:FromEmail"] ?? "corporativo.gna@gmail.com";
            _fromName = _config["Integracoes:SendGrid:FromName"] ?? "Grupo Nexum Altivon";
            _smtpServer = _config["EmailSettings:SmtpServer"];
            _smtpPort = int.TryParse(_config["EmailSettings:Port"], out var smtpPort) ? smtpPort : 587;
            _smtpUsername = _config["EmailSettings:Username"];
            _smtpPassword = _config["EmailSettings:Password"];
            _smtpEnableSsl = bool.TryParse(_config["EmailSettings:EnableSsl"], out var smtpEnableSsl) && smtpEnableSsl;
            _smtpFromEmail = _config["EmailSettings:FromEmail"] ?? _fromEmail;
            _smtpFromName = _config["EmailSettings:FromName"] ?? _fromName;
            _whatsappAtivo = bool.Parse(_config["Integracoes:WhatsApp:Ativo"] ?? "false");
            _whatsappApiUrl = _config["Integracoes:WhatsApp:ApiUrl"];
            _whatsappApiKey = _config["Integracoes:WhatsApp:ApiKey"];
        }

        public async Task EnviarConfirmacaoPedidoAsync(Cliente cliente, Pedido pedido)
        {
            var assunto = $"Pedido {pedido.NumeroPedido} Recebido - Nexum Altivon";
            var corpo = $@"
<!DOCTYPE html>
<html>
<head><style>
body {{ font-family: 'Montserrat', sans-serif; background: #0A0A0A; color: #F5F5F5; }}
.container {{ max-width: 600px; margin: 0 auto; background: #1A1A1A; padding: 30px; border: 1px solid #C9A227; }}
h1 {{ color: #C9A227; font-size: 24px; }}
.pedido-info {{ background: #2A2A2A; padding: 20px; border-radius: 8px; margin: 20px 0; }}
.footer {{ text-align: center; color: #A0A0A0; font-size: 12px; margin-top: 30px; }}
</style></head>
<body>
<div class='container'>
<h1>âœ… Pedido Recebido!</h1>
<p>OlÃ¡ <strong>{cliente.Nome}</strong>,</p>
<p>Seu pedido <strong style='color:#C9A227'>{pedido.NumeroPedido}</strong> foi recebido com sucesso.</p>
<div class='pedido-info'>
<p><strong>Total:</strong> R$ {pedido.Total:N2}</p>
<p><strong>Status:</strong> Aguardando pagamento</p>
<p><strong>Data:</strong> {pedido.CreatedAt:dd/MM/yyyy HH:mm}</p>
</div>
<p>Assim que o pagamento for confirmado, iniciaremos a separaÃ§Ã£o do seu pedido.</p>
<div class='footer'>
<p>Grupo Nexum Altivon<br>www.nexumaltivon.com</p>
</div>
</div>
</body>
</html>";

            await EnviarEmailAsync(cliente.Email, assunto, corpo);
            _logger.LogInformation("ConfirmaÃ§Ã£o de pedido enviada: {NumeroPedido}", pedido.NumeroPedido);
        }

        public async Task EnviarConfirmacaoPagamentoAsync(Cliente cliente, Pedido pedido)
        {
            var assunto = $"Pagamento Confirmado - Pedido {pedido.NumeroPedido}";
            var corpo = $@"
<!DOCTYPE html>
<html>
<head><style>
body {{ font-family: 'Montserrat', sans-serif; background: #0A0A0A; color: #F5F5F5; }}
.container {{ max-width: 600px; margin: 0 auto; background: #1A1A1A; padding: 30px; border: 1px solid #C9A227; }}
h1 {{ color: #C9A227; }}
</style></head>
<body>
<div class='container'>
<h1>ðŸ’³ Pagamento Confirmado!</h1>
<p>OlÃ¡ <strong>{cliente.Nome}</strong>,</p>
<p>O pagamento do pedido <strong>{pedido.NumeroPedido}</strong> foi confirmado.</p>
<p><strong>Valor pago:</strong> R$ {pedido.Total:N2}</p>
<p>Seu pedido agora estÃ¡ em <strong>separaÃ§Ã£o</strong> e em breve serÃ¡ enviado.</p>
<p>Acompanhe o status pelo site: <a href='https://www.nexumaltivon.com/pedidos/{pedido.NumeroPedido}' style='color:#C9A227'>Meus Pedidos</a></p>
</div>
</body>
</html>";

            await EnviarEmailAsync(cliente.Email, assunto, corpo);
        }

        public async Task EnviarNotificacaoWhatsAppAsync(string? telefone, string mensagem)
        {
            if (!_whatsappAtivo || string.IsNullOrEmpty(telefone)) return;

            try
            {
                telefone = telefone.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "").Trim();
                if (telefone.Length < 11) return;

                var request = new
                {
                    number = "55" + telefone,
                    text = mensagem
                };

                // Stub para API genÃ©rica de WhatsApp (ex: Evolution API, WPPConnect, etc.)
                // Em produÃ§Ã£o, substituir pela URL real do gateway WhatsApp
                var response = await _httpClient.PostAsJsonAsync(_whatsappApiUrl, request);
                if (!response.IsSuccessStatusCode)
                    _logger.LogWarning("Falha ao enviar WhatsApp: {Status}", response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar notificaÃ§Ã£o WhatsApp");
            }
        }

        public async Task EnviarEmailAsync(string? destinatario, string assunto, string corpoHtml)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(destinatario))
                {
                    _logger.LogWarning("Tentativa de envio de e-mail ignorada por destinatário vazio. Assunto: {Assunto}", assunto);
                    return;
                }

                if (!string.IsNullOrEmpty(_sendGridKey))
                {
                    var sendGridRequest = new
                    {
                        personalizations = new[]
                        {
                            new { to = new[] { new { email = destinatario } } }
                        },
                        from = new { email = _fromEmail, name = _fromName },
                        subject = assunto,
                        content = new[]
                        {
                            new { type = "text/html", value = corpoHtml }
                        }
                    };

                    var request = new HttpRequestMessage(HttpMethod.Post, "https://api.sendgrid.com/v3/mail/send")
                    {
                        Content = JsonContent.Create(sendGridRequest)
                    };
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _sendGridKey);

                    var response = await _httpClient.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        _logger.LogError("SendGrid erro: {Error}", error);
                    }

                    return;
                }

                if (!string.IsNullOrWhiteSpace(_smtpServer))
                {
                    await EnviarEmailViaSmtpAsync(destinatario, assunto, corpoHtml);
                    return;
                }

                _logger.LogWarning("Nenhum provedor de e-mail configurado. E-mail simulado para {Email}: {Assunto}", destinatario, assunto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar e-mail para {Destinatario}", destinatario);
            }
        }

        private async Task EnviarEmailViaSmtpAsync(string destinatario, string assunto, string corpoHtml)
        {
            if (string.IsNullOrWhiteSpace(_smtpServer))
            {
                _logger.LogWarning("SMTP não configurado. E-mail simulado para {Email}: {Assunto}", destinatario, assunto);
                return;
            }

            using var message = new MailMessage
            {
                From = new MailAddress(_smtpFromEmail, _smtpFromName, Encoding.UTF8),
                Subject = assunto,
                Body = corpoHtml,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };
            message.To.Add(destinatario);

            using var smtp = new SmtpClient(_smtpServer, _smtpPort)
            {
                EnableSsl = _smtpEnableSsl
            };

            if (!string.IsNullOrWhiteSpace(_smtpUsername))
            {
                smtp.Credentials = new System.Net.NetworkCredential(_smtpUsername, _smtpPassword ?? string.Empty);
            }

            await smtp.SendMailAsync(message);
            _logger.LogInformation("E-mail enviado por SMTP para {Email}: {Assunto}", destinatario, assunto);
        }

        public async Task EnviarAlertaEstoqueBaixoAsync(Produto produto)
        {
            var assunto = $"[ALERTA] Estoque Baixo - {produto.Nome}";
            var corpo = $@"<p>Produto <strong>{produto.Nome}</strong> (SKU: {produto.Sku}) atingiu estoque baixo.</p>
<p>Estoque atual: <strong>{produto.EstoqueAtual}</strong> | MÃ­nimo: {produto.EstoqueMinimo}</p>
<p>Loja: {produto.Loja?.Nome}</p>";

            var emailsAdmin = _config["Alertas:EstoqueEmailAdmin"] ?? "corporativo.gna@gmail.com";
            await EnviarEmailAsync(emailsAdmin, assunto, corpo);
        }

        public async Task EnviarStatusPedidoAsync(Cliente cliente, Pedido pedido, string mensagemPersonalizada)
        {
            var assunto = $"AtualizaÃ§Ã£o do Pedido {pedido.NumeroPedido}";
            var corpo = $@"
<!DOCTYPE html>
<html>
<body style='font-family:Montserrat,sans-serif;background:#0A0A0A;color:#F5F5F5;'>
<div style='max-width:600px;margin:0 auto;background:#1A1A1A;padding:30px;border:1px solid #C9A227;'>
<h1 style='color:#C9A227'>ðŸ“¦ AtualizaÃ§Ã£o de Pedido</h1>
<p>OlÃ¡ <strong>{cliente.Nome}</strong>,</p>
<p>{mensagemPersonalizada}</p>
<p>Pedido: <strong>{pedido.NumeroPedido}</strong></p>
<p>Status atual: <strong>{pedido.Status}</strong></p>
</div>
</body>
</html>";

            await EnviarEmailAsync(cliente.Email, assunto, corpo);
            await EnviarNotificacaoWhatsAppAsync(cliente.Telefone, $"Nexum Altivon: Pedido {pedido.NumeroPedido} - {mensagemPersonalizada}");
        }
    }
}
