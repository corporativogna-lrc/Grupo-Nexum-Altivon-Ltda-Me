using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Services;

namespace NexumAltivon.API.Controllers
{
    [ApiController]
    [Route("api/webhooks")]
    public class WebhookController : ControllerBase
    {
        private readonly IMercadoPagoService _mp;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(IMercadoPagoService mp, ILogger<WebhookController> logger)
        {
            _mp = mp;
            _logger = logger;
        }

        [HttpPost("mercadopago")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> MercadoPago()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                _logger.LogInformation("Webhook MP recebido: {Body}", body);

                var payload = JsonSerializer.Deserialize<WebhookMercadoPagoDto>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (payload == null)
                {
                    _logger.LogWarning("Payload MP inválido");
                    return BadRequest("Payload inválido");
                }

                var resultado = await _mp.ProcessarWebhookAsync(payload);
                return resultado ? Ok() : StatusCode(500, "Erro ao processar webhook");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Erro no webhook MP");
                return StatusCode(500, "Erro interno");
            }
        }

        [HttpPost("melhorenvio")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> MelhorEnvio()
        {
            // Implementar conforme necessidade futura
            _logger.LogInformation("Webhook Melhor Envio recebido");
            return Ok();
        }

        [HttpPost("teste")]
        public IActionResult Teste()
        {
            _logger.LogInformation("Webhook de teste acionado");
            return Ok(new { status = "Webhook ativo", timestamp = System.DateTime.UtcNow });
        }
    }
}
