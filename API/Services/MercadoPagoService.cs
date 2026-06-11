using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Models;

namespace NexumAltivon.API.Services
{
    public interface IMercadoPagoService
    {
        Task<PagamentoResultado> GerarPagamentoPixAsync(Pedido pedido);
        Task<PagamentoResultado> GerarPagamentoCartaoAsync(Pedido pedido, DadosCartaoDto cartao, int parcelas);
        Task<PagamentoResultado> GerarPagamentoBoletoAsync(Pedido pedido);
        Task<bool> ProcessarWebhookAsync(WebhookMercadoPagoDto payload);
        Task<ConsultaPagamentoDto> ConsultarPagamentoAsync(string transacaoId);
        Task<ReembolsoDto> SolicitarReembolsoAsync(string transacaoId, decimal? valorParcial = null);
    }

    public class MercadoPagoService : IMercadoPagoService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<MercadoPagoService> _logger;
        private readonly string _accessToken;
        private readonly string _webhookSecret;
        private readonly bool _sandbox;

        public MercadoPagoService(IHttpClientFactory factory, IConfiguration config, ILogger<MercadoPagoService> logger)
        {
            _httpClient = factory.CreateClient("MercadoPago");
            _config = config;
            _logger = logger;
            _accessToken = _config["Integracoes:MercadoPago:AccessToken"];
            _webhookSecret = _config["Integracoes:MercadoPago:WebhookSecret"];
            _sandbox = bool.Parse(_config["Integracoes:MercadoPago:Sandbox"] ?? "true");
            _httpClient.BaseAddress = new Uri("https://api.mercadopago.com");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        public async Task<PagamentoResultado> GerarPagamentoPixAsync(Pedido pedido)
        {
            try
            {
                var request = new
                {
                    transaction_amount = (double)pedido.Total,
                    description = $"Pedido {pedido.NumeroPedido} - Nexum Altivon",
                    payment_method_id = "pix",
                    payer = new
                    {
                        email = pedido.Cliente?.Email ?? "cliente@nexumaltivon.com",
                        first_name = pedido.Cliente?.Nome?.Split(' ').FirstOrDefault(),
                        last_name = pedido.Cliente?.Nome?.Split(' ').Skip(1).FirstOrDefault(),
                        identification = new
                        {
                            type = "CPF",
                            number = pedido.Cliente?.Cpf?.Replace(".", "").Replace("-", "")
                        }
                    },
                    external_reference = pedido.NumeroPedido,
                    notification_url = "https://api.nexumaltivon.com/api/webhooks/mercadopago"
                };

                var response = await _httpClient.PostAsJsonAsync("/v1/payments", request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Erro PIX MP: {Content}", content);
                    return new PagamentoResultado { Sucesso = false, Mensagem = $"Erro Mercado Pago: {content}" };
                }

                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                var point = root.GetProperty("point_of_interaction").GetProperty("transaction_data");

                return new PagamentoResultado
                {
                    Sucesso = true,
                    TransacaoId = root.GetProperty("id").GetInt64().ToString(),
                    QrCodeBase64 = point.GetProperty("qr_code_base64").GetString(),
                    QrCodeTexto = point.GetProperty("qr_code").GetString(),
                    UrlPagamento = point.GetProperty("ticket_url").GetString(),
                    Mensagem = "PIX gerado com sucesso. Escaneie o QR Code para pagar."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao gerar PIX");
                return new PagamentoResultado { Sucesso = false, Mensagem = ex.Message };
            }
        }

        public async Task<PagamentoResultado> GerarPagamentoCartaoAsync(Pedido pedido, DadosCartaoDto cartao, int parcelas)
        {
            try
            {
                // Criar token do cartão (simplificado — em produção usar MercadoPago.js para tokenização PCI-compliant)
                var tokenRequest = new
                {
                    card_number = cartao.Numero.Replace(" ", ""),
                    expiration_month = int.Parse(cartao.Validade.Split('/')[0]),
                    expiration_year = int.Parse("20" + cartao.Validade.Split('/')[1]),
                    security_code = cartao.Cvv,
                    cardholder = new
                    {
                        name = cartao.NomeTitular,
                        identification = new
                        {
                            type = "CPF",
                            number = cartao.CpfTitular?.Replace(".", "").Replace("-", "")
                        }
                    }
                };

                var tokenResponse = await _httpClient.PostAsJsonAsync("/v1/card_tokens", tokenRequest);
                var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
                if (!tokenResponse.IsSuccessStatusCode)
                    return new PagamentoResultado { Sucesso = false, Mensagem = $"Tokenização falhou: {tokenContent}" };

                using var tokenDoc = JsonDocument.Parse(tokenContent);
                var cardToken = tokenDoc.RootElement.GetProperty("id").GetString();

                var paymentRequest = new
                {
                    transaction_amount = (double)pedido.Total,
                    token = cardToken,
                    description = $"Pedido {pedido.NumeroPedido} - Nexum Altivon",
                    installments = parcelas,
                    payment_method_id = "master", // detectar bandeira via BIN em produção
                    payer = new
                    {
                        email = pedido.Cliente?.Email,
                        identification = new { type = "CPF", number = pedido.Cliente?.Cpf }
                    },
                    external_reference = pedido.NumeroPedido,
                    notification_url = "https://api.nexumaltivon.com/api/webhooks/mercadopago"
                };

                var response = await _httpClient.PostAsJsonAsync("/v1/payments", paymentRequest);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Erro Cartão MP: {Content}", content);
                    return new PagamentoResultado { Sucesso = false, Mensagem = $"Erro: {content}" };
                }

                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                var status = root.GetProperty("status").GetString();

                return new PagamentoResultado
                {
                    Sucesso = status == "approved" || status == "authorized" || status == "in_process",
                    TransacaoId = root.GetProperty("id").GetInt64().ToString(),
                    Status = status,
                    UrlPagamento = root.TryGetProperty("transaction_details", out var td) && td.TryGetProperty("external_resource_url", out var url)
                        ? url.GetString() : null,
                    Mensagem = status == "approved" ? "Pagamento aprovado!" : "Pagamento em processamento."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao processar cartão");
                return new PagamentoResultado { Sucesso = false, Mensagem = ex.Message };
            }
        }

        public async Task<PagamentoResultado> GerarPagamentoBoletoAsync(Pedido pedido)
        {
            try
            {
                var request = new
                {
                    transaction_amount = (double)pedido.Total,
                    description = $"Pedido {pedido.NumeroPedido} - Nexum Altivon",
                    payment_method_id = "bolbradesco",
                    payer = new
                    {
                        email = pedido.Cliente?.Email,
                        first_name = pedido.Cliente?.Nome?.Split(' ').FirstOrDefault(),
                        last_name = pedido.Cliente?.Nome?.Split(' ').Skip(1).FirstOrDefault(),
                        identification = new { type = "CPF", number = pedido.Cliente?.Cpf }
                    },
                    external_reference = pedido.NumeroPedido,
                    notification_url = "https://api.nexumaltivon.com/api/webhooks/mercadopago"
                };

                var response = await _httpClient.PostAsJsonAsync("/v1/payments", request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Erro Boleto MP: {Content}", content);
                    return new PagamentoResultado { Sucesso = false, Mensagem = $"Erro: {content}" };
                }

                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                var td = root.GetProperty("transaction_details");

                return new PagamentoResultado
                {
                    Sucesso = true,
                    TransacaoId = root.GetProperty("id").GetInt64().ToString(),
                    LinhaDigitavel = td.GetProperty("payment_method_reference_id").GetString(),
                    UrlPagamento = td.GetProperty("external_resource_url").GetString(),
                    Mensagem = "Boleto gerado com sucesso."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao gerar boleto");
                return new PagamentoResultado { Sucesso = false, Mensagem = ex.Message };
            }
        }

        public async Task<bool> ProcessarWebhookAsync(WebhookMercadoPagoDto payload)
        {
            try
            {
                if (payload.Action != "payment.updated" && payload.Action != "payment.created")
                    return true; // Ignorar outros eventos

                var paymentId = payload.Data?.Id;
                if (string.IsNullOrEmpty(paymentId)) return false;

                // Consultar pagamento na API do MP para validar
                var response = await _httpClient.GetAsync($"/v1/payments/{paymentId}");
                if (!response.IsSuccessStatusCode) return false;

                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                var externalRef = root.GetProperty("external_reference").GetString();
                var status = root.GetProperty("status").GetString();
                var statusDetail = root.GetProperty("status_detail").GetString();

                var pedido = await _context.Pedidos.FirstOrDefaultAsync(p => p.NumeroPedido == externalRef);
                if (pedido == null) return false;

                // Atualizar status do pedido conforme pagamento
                switch (status)
                {
                    case "approved":
                        pedido.Status = "PAGO";
                        pedido.PagoEm = DateTime.UtcNow;
                        break;
                    case "in_process":
                    case "pending":
                        pedido.Status = "AGUARDANDO_PAGAMENTO";
                        break;
                    case "rejected":
                        pedido.Status = "PAGAMENTO_RECUSADO";
                        break;
                    case "cancelled":
                        pedido.Status = "CANCELADO";
                        break;
                }

                // Atualizar ou criar registro de pagamento
                var pagamento = await _context.Pagamentos.FirstOrDefaultAsync(p => p.TransacaoGatewayId == paymentId);
                if (pagamento == null)
                {
                    pagamento = new Pagamento
                    {
                        PedidoId = pedido.PedidoId,
                        Metodo = root.GetProperty("payment_method_id").GetString(),
                        Status = status,
                        Valor = (decimal)root.GetProperty("transaction_amount").GetDouble(),
                        TransacaoId = paymentId,
                        GatewayReferencia = "mercadopago",
                        CriadoEm = DateTime.UtcNow
                    };
                    _context.Pagamentos.Add(pagamento);
                }
                else
                {
                    pagamento.Status = status;
                    if (status == "approved") pagamento.PagoEm = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Webhook MP processado: Pedido {Numero} -> {Status}", externalRef, status);

                // Notificar cliente
                if (status == "approved")
                {
                    var cliente = await _context.Clientes.FindAsync(pedido.ClienteId);
                    await _notificacao.EnviarConfirmacaoPagamentoAsync(cliente, pedido);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar webhook MP");
                return false;
            }
        }

        public async Task<ConsultaPagamentoDto> ConsultarPagamentoAsync(string transacaoId)
        {
            var pagamento = await _context.Pagamentos
                .Include(p => p.Pedido)
                .FirstOrDefaultAsync(p => p.TransacaoId == transacaoId);

            if (pagamento == null) return null;

            return new ConsultaPagamentoDto
            {
                PedidoId = pagamento.PedidoId,
                NumeroPedido = pagamento.Pedido?.NumeroPedido,
                StatusPagamento = pagamento.Status,
                StatusPedido = pagamento.Pedido?.Status,
                ValorPago = pagamento.Valor,
                DataPagamento = pagamento.PagoEm,
                MetodoPagamento = pagamento.Metodo
            };
        }

        public async Task<ReembolsoDto> SolicitarReembolsoAsync(string transacaoId, decimal? valorParcial = null)
        {
            try
            {
                var pagamento = await _context.Pagamentos.FirstOrDefaultAsync(p => p.TransacaoId == transacaoId);
                if (pagamento == null) return new ReembolsoDto { Sucesso = false, Mensagem = "Pagamento não encontrado." };

                var url = $"/v1/payments/{transacaoId}/refunds";
                HttpResponseMessage response;
                if (valorParcial.HasValue && valorParcial.Value > 0)
                {
                    var body = new { amount = (double)valorParcial.Value };
                    response = await _httpClient.PostAsJsonAsync(url, body);
                }
                else
                {
                    response = await _httpClient.PostAsync(url, null);
                }

                var content = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    return new ReembolsoDto { Sucesso = false, Mensagem = content };

                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                pagamento.Status = "refunded";
                if (pagamento.Pedido != null) pagamento.Pedido.Status = "REEMBOLSADO";
                await _context.SaveChangesAsync();

                return new ReembolsoDto
                {
                    Sucesso = true,
                    TransacaoId = root.GetProperty("id").GetInt64().ToString(),
                    ValorReembolsado = (decimal)root.GetProperty("amount").GetDouble(),
                    Status = root.GetProperty("status").GetString(),
                    Mensagem = "Reembolso processado com sucesso."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha no reembolso");
                return new ReembolsoDto { Sucesso = false, Mensagem = ex.Message };
            }
        }
    }

    public class PagamentoResultado
    {
        public bool Sucesso { get; set; }
        public string TransacaoId { get; set; }
        public string Status { get; set; }
        public string UrlPagamento { get; set; }
        public string QrCodeBase64 { get; set; }
        public string QrCodeTexto { get; set; }
        public string LinhaDigitavel { get; set; }
        public string Mensagem { get; set; }
    }
}
