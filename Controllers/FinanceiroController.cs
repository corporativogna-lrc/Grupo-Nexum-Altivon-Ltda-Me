using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexumAltivon.ERP.DTOs;
using NexumAltivon.ERP.Services;
using System.Security.Claims;

namespace NexumAltivon.ERP.Controllers
{
    [ApiController]
    [Route("api/erp/[controller]")]
    [Authorize(Roles = "SuperAdmin,Admin,Gerente,Financeiro")]
    public class FinanceiroController : ControllerBase
    {
        private readonly IFinanceiroService _financeiroService;

        public FinanceiroController(IFinanceiroService financeiroService)
        {
            _financeiroService = financeiroService;
        }

        private string UsuarioAtual => User.FindFirst(ClaimTypes.Email)?.Value ?? "sistema";

        // ==================== CONTAS A PAGAR ====================

        [HttpGet("contas-pagar")]
        public async Task<ActionResult<List<ContaPagarDto>>> ListarContasPagar(
            [FromQuery] string? status,
            [FromQuery] int? fornecedorId,
            [FromQuery] DateTime? vencimentoDe,
            [FromQuery] DateTime? vencimentoAte)
        {
            var contas = await _financeiroService.ListarContasPagarAsync(status, fornecedorId, vencimentoDe, vencimentoAte);
            return Ok(contas);
        }

        [HttpGet("contas-pagar/{id}")]
        public async Task<ActionResult<ContaPagarDto>> ObterContaPagar(int id)
        {
            var conta = await _financeiroService.ObterContaPagarPorIdAsync(id);
            if (conta == null) return NotFound();
            return Ok(conta);
        }

        [HttpPost("contas-pagar")]
        [Authorize(Roles = "SuperAdmin,Admin,Financeiro")]
        public async Task<ActionResult<ContaPagarDto>> CriarContaPagar([FromBody] CriarContaPagarDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var conta = await _financeiroService.CriarContaPagarAsync(dto, UsuarioAtual);
            return CreatedAtAction(nameof(ObterContaPagar), new { id = conta.Id }, conta);
        }

        [HttpPost("contas-pagar/baixar")]
        [Authorize(Roles = "SuperAdmin,Admin,Financeiro")]
        public async Task<ActionResult<ContaPagarDto>> BaixarContaPagar([FromBody] BaixarContaPagarDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var conta = await _financeiroService.BaixarContaPagarAsync(dto, UsuarioAtual);
            return Ok(conta);
        }

        [HttpDelete("contas-pagar/{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,Financeiro")]
        public async Task<IActionResult> CancelarContaPagar(int id)
        {
            var resultado = await _financeiroService.CancelarContaPagarAsync(id, UsuarioAtual);
            if (!resultado) return NotFound();
            return NoContent();
        }

        // ==================== CONTAS A RECEBER ====================

        [HttpGet("contas-receber")]
        public async Task<ActionResult<List<ContaReceberDto>>> ListarContasReceber(
            [FromQuery] string? status,
            [FromQuery] int? clienteId,
            [FromQuery] DateTime? vencimentoDe,
            [FromQuery] DateTime? vencimentoAte)
        {
            var contas = await _financeiroService.ListarContasReceberAsync(status, clienteId, vencimentoDe, vencimentoAte);
            return Ok(contas);
        }

        [HttpGet("contas-receber/{id}")]
        public async Task<ActionResult<ContaReceberDto>> ObterContaReceber(int id)
        {
            var conta = await _financeiroService.ObterContaReceberPorIdAsync(id);
            if (conta == null) return NotFound();
            return Ok(conta);
        }

        [HttpPost("contas-receber")]
        [Authorize(Roles = "SuperAdmin,Admin,Financeiro")]
        public async Task<ActionResult<ContaReceberDto>> CriarContaReceber([FromBody] CriarContaReceberDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var conta = await _financeiroService.CriarContaReceberAsync(dto, UsuarioAtual);
            return CreatedAtAction(nameof(ObterContaReceber), new { id = conta.Id }, conta);
        }

        [HttpPost("contas-receber/baixar")]
        [Authorize(Roles = "SuperAdmin,Admin,Financeiro")]
        public async Task<ActionResult<ContaReceberDto>> BaixarContaReceber([FromBody] BaixarContaReceberDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var conta = await _financeiroService.BaixarContaReceberAsync(dto, UsuarioAtual);
            return Ok(conta);
        }

        [HttpDelete("contas-receber/{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,Financeiro")]
        public async Task<IActionResult> CancelarContaReceber(int id)
        {
            var resultado = await _financeiroService.CancelarContaReceberAsync(id, UsuarioAtual);
            if (!resultado) return NotFound();
            return NoContent();
        }

        // ==================== RESUMO E FLUXO ====================

        [HttpGet("resumo")]
        public async Task<ActionResult<ResumoFinanceiroDto>> ObterResumo()
        {
            var resumo = await _financeiroService.ObterResumoFinanceiroAsync();
            return Ok(resumo);
        }

        [HttpGet("fluxo-caixa")]
        public async Task<ActionResult<List<FluxoCaixaDto>>> ObterFluxoCaixa(
            [FromQuery] DateTime de,
            [FromQuery] DateTime ate)
        {
            var fluxo = await _financeiroService.ObterFluxoCaixaAsync(de, ate);
            return Ok(fluxo);
        }
    }
}
