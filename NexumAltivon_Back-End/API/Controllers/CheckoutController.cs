using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Services;

namespace NexumAltivon.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CheckoutController : ControllerBase
    {
        private readonly ICheckoutService _checkoutService;
        private readonly IAuthService _auth;

        public CheckoutController(ICheckoutService checkoutService, IAuthService auth)
        {
            _checkoutService = checkoutService;
            _auth = auth;
        }

        private int ObterClienteId()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (!int.TryParse(userId, out var id))
                throw new UnauthorizedAccessException("Cliente não identificado.");
            return id;
        }

        [HttpPost("iniciar")]
        public async Task<ActionResult<CheckoutDto>> IniciarCheckout([FromBody] IniciarCheckoutRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var clienteId = ObterClienteId();
            var checkout = await _checkoutService.IniciarCheckoutAsync(clienteId, request);
            return Ok(checkout);
        }

        [HttpPost("{checkoutId}/frete")]
        public async Task<ActionResult<CheckoutDto>> SelecionarFrete(int checkoutId, [FromBody] SelecionarFreteRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var checkout = await _checkoutService.SelecionarFreteAsync(checkoutId, request.CodigoFrete);
            return Ok(checkout);
        }

        [HttpPost("finalizar")]
        public async Task<ActionResult<CheckoutResponseDto>> FinalizarCheckout([FromBody] FinalizarCheckoutRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var clienteId = ObterClienteId();
            var resultado = await _checkoutService.FinalizarAsync(clienteId, request);
            if (!resultado.Sucesso)
                return BadRequest(resultado);
            return Ok(resultado);
        }

        [HttpGet("{checkoutId}")]
        public async Task<ActionResult<CheckoutDto>> ObterCheckout(int checkoutId)
        {
            var checkout = await _checkoutService.ObterCheckoutAsync(checkoutId);
            if (checkout == null) return NotFound();
            return Ok(checkout);
        }
    }
}
