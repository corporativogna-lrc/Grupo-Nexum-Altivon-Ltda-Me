using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Services;

namespace NexumAltivon.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Gerente,Financeiro")]
    public class PagamentoController : ControllerBase
    {
        private readonly IMercadoPagoService _mp;

        public PagamentoController(IMercadoPagoService mp)
        {
            _mp = mp;
        }

        [HttpGet("consulta/{transacaoId}")]
        public async Task<ActionResult<ConsultaPagamentoDto>> ConsultarPagamento(string transacaoId)
        {
            var resultado = await _mp.ConsultarPagamentoAsync(transacaoId);
            if (resultado == null) return NotFound();
            return Ok(resultado);
        }

        [HttpPost("reembolso")]
        [Authorize(Roles = "Admin,Financeiro")]
        public async Task<ActionResult<ReembolsoDto>> SolicitarReembolso([FromBody] ReembolsoRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            // Buscar transacaoId pelo pedido
            // Simplificado: assumindo que o frontend envia transacaoId futuramente ou buscamos no service
            return Ok(new ReembolsoDto { Sucesso = true, Mensagem = "Use o endpoint de reembolso via transacaoId diretamente." });
        }

        [HttpPost("reembolso/{transacaoId}")]
        [Authorize(Roles = "Admin,Financeiro")]
        public async Task<ActionResult<ReembolsoDto>> ReembolsarTransacao(string transacaoId, [FromBody] ReembolsoRequest request)
        {
            var resultado = await _mp.SolicitarReembolsoAsync(transacaoId, request.ValorParcial);
            if (!resultado.Sucesso) return BadRequest(resultado);
            return Ok(resultado);
        }
    }
}
