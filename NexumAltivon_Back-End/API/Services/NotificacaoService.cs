/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */
using System;
using System.Net.Http;
using System.Net.Http.Json;
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
        Task EnviarNotaFiscalEmitidaAsync(Cliente cliente, Pedido pedido, Fiscal fiscal);
        Task EnviarConfirmacaoCadastroAsync(Cliente cliente, string linkConfirmacao);
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
        private readonly string _salesAdminEmail;
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
            _salesAdminEmail = _config["Alertas:VendaEmailAdmin"] ?? "corporativo.gna@gmail.com";
            _whatsappAtivo = bool.TryParse(_config["Integracoes:WhatsApp:Ativo"], out var whatsappAtivo) && whatsappAtivo;
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
<p>Grupo Nexum Altivon<br>nexumaltivon.com.br</p>
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
<p>Acompanhe o status pelo site: <a href='https://nexumaltivon.com.br/pedidos/{pedido.NumeroPedido}' style='color:#C9A227'>Meus Pedidos</a></p>
</div>
</body>
</html>";

            await EnviarEmailAsync(cliente.Email, assunto, corpo);
            await EnviarEmailAsync(_salesAdminEmail, $"[COPIA] {assunto}", corpo);
        }

        public async Task EnviarConfirmacaoCadastroAsync(Cliente cliente, string linkConfirmacao)
        {
            var assunto = $"Confirme seu cadastro - Nexum Altivon";
            var corpo = $@"
<!DOCTYPE html>
<html>
<body style='font-family:Arial,sans-serif;background:#f6f3ea;color:#1f1f1f;padding:0;margin:0;'>
<div style='max-width:640px;margin:0 auto;background:#ffffff;border:1px solid #d7c38a;padding:28px;'>
<h1 style='margin-top:0;color:#8a6d1f;'>Confirme seu cadastro</h1>
<p>Olá <strong>{cliente.Nome}</strong>,</p>
<p>Recebemos seu cadastro na Nexum Altivon e ele está pronto para ativação.</p>
<p>Para liberar o acesso da sua área do cliente, clique no botão abaixo:</p>
<p><a href='{linkConfirmacao}' style='display:inline-block;background:#c9a227;color:#000;padding:14px 22px;border-radius:8px;text-decoration:none;font-weight:bold;'>Confirmar cadastro</a></p>
<p>Se o botão não abrir, copie e cole este endereço no navegador:</p>
<p style='word-break:break-all;'><a href='{linkConfirmacao}'>{linkConfirmacao}</a></p>
<p>Depois da confirmação, você poderá entrar normalmente com seu e-mail e senha.</p>
</div>
</body>
</html>";

            await EnviarEmailAsync(cliente.Email, assunto, corpo);
            await EnviarEmailAsync(_salesAdminEmail, $"[COPIA] {assunto}", corpo);
        }

        public async Task EnviarNotaFiscalEmitidaAsync(Cliente cliente, Pedido pedido, Fiscal fiscal)
        {
            var assunto = $"Nota Fiscal emitida - Pedido {pedido.NumeroPedido}";
            var arquivoXml = string.IsNullOrWhiteSpace(fiscal.XmlUrl) ? "Indisponivel" : fiscal.XmlUrl;
            var arquivoDanfe = string.IsNullOrWhiteSpace(fiscal.DanfeUrl) ? "Indisponivel" : fiscal.DanfeUrl;
            var chaveAcesso = string.IsNullOrWhiteSpace(fiscal.ChaveAcesso) ? "Nao informada" : fiscal.ChaveAcesso;
            var numeroNfe = string.IsNullOrWhiteSpace(fiscal.NumeroNfe) ? "Nao informado" : fiscal.NumeroNfe;
            var statusFiscal = fiscal.StatusNfe.ToString();

            var corpo = $@"
<!DOCTYPE html>
<html>
<body style='font-family:Arial,sans-serif;background:#f6f3ea;color:#1f1f1f;padding:0;margin:0;'>
<div style='max-width:640px;margin:0 auto;background:#ffffff;border:1px solid #d7c38a;padding:28px;'>
<h1 style='margin-top:0;color:#8a6d1f;'>Nota Fiscal emitida</h1>
<p>Ola <strong>{cliente.Nome}</strong>,</p>
<p>A nota fiscal do seu pedido <strong>{pedido.NumeroPedido}</strong> foi emitida no sistema.</p>
<div style='background:#faf7ef;border:1px solid #e4d8b7;padding:18px;margin:20px 0;'>
<p><strong>Status:</strong> {statusFiscal}</p>
<p><strong>NFe:</strong> {numeroNfe}</p>
<p><strong>Chave de acesso:</strong> {chaveAcesso}</p>
<p><strong>Total:</strong> R$ {pedido.Total:N2}</p>
<p><strong>XML:</strong> {arquivoXml}</p>
<p><strong>DANFE:</strong> {arquivoDanfe}</p>
</div>
<p>Voce pode acompanhar o pedido pela area do cliente no site.</p>
<p>Se quiser, responda este e-mail ou fale com nosso atendimento.</p>
            </div>
</body>
</html>";

            await EnviarEmailAsync(cliente.Email, assunto, corpo);
            await EnviarEmailAsync(_salesAdminEmail, $"[COPIA] {assunto}", corpo);
        }

        public async Task EnviarNotificacaoWhatsAppAsync(string? telefone, string mensagem)
        {
            if (!_whatsappAtivo || string.IsNullOrEmpty(telefone)) return;
            if (string.IsNullOrWhiteSpace(_whatsappApiUrl))
            {
                throw new InvalidOperationException("Integracoes:WhatsApp:Ativo está habilitado, mas Integracoes:WhatsApp:ApiUrl nao foi configurada.");
            }

            try
            {
                telefone = telefone.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "").Trim();
                if (telefone.Length < 11) return;

                var request = new
                {
                    number = "55" + telefone,
                    text = mensagem
                };

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, _whatsappApiUrl)
                {
                    Content = JsonContent.Create(request)
                };
                if (!string.IsNullOrWhiteSpace(_whatsappApiKey))
                {
                    httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _whatsappApiKey);
                }

                var response = await _httpClient.SendAsync(httpRequest);
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
                    throw new InvalidOperationException($"Destinatario obrigatorio para envio de e-mail. Assunto: {assunto}");
                }

                if (string.IsNullOrEmpty(_sendGridKey))
                {
                    throw new InvalidOperationException("Integracoes:SendGrid:ApiKey nao configurada. Envio real de e-mail bloqueado para evitar sucesso falso.");
                }

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
                    throw new InvalidOperationException($"SendGrid recusou o envio para {destinatario}. Status={(int)response.StatusCode}. Corpo={error}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar e-mail para {Destinatario}", destinatario);
                throw;
            }
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

