using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexumAltivon.ERP.DTOs;
using NexumAltivon.ERP.Services;
using System.Security.Claims;

namespace NexumAltivon.ERP.Controllers
{
    [ApiController]
    [Route("api/erp/[controller]")]
    [Authorize(Roles = "SuperAdmin,Admin,Gerente,Logistica")]
    public class EstoqueController : ControllerBase
    {
        private readonly IEstoqueService _estoqueService;

        public EstoqueController(IEstoqueService estoqueService)
        {
            _estoqueService = estoqueService;
        }

        private string UsuarioAtual => User.FindFirst(ClaimTypes.Email)?.Value ?? "sistema";

        [HttpGet("movimentacoes")]
        public async Task<ActionResult<List<MovimentacaoEstoqueDto>>> ListarMovimentacoes(
            [FromQuery] int? produtoId,
            [FromQuery] string? tipo,
            [FromQuery] DateTime? de,
            [FromQuery] DateTime? ate)
        {
            var movs = await _estoqueService.ListarMovimentacoesAsync(produtoId, tipo, de, ate);
            return Ok(movs);
        }

        [HttpPost("movimentacoes")]
        [Authorize(Roles = "SuperAdmin,Admin,Logistica")]
        public async Task<ActionResult<MovimentacaoEstoqueDto>> RegistrarMovimentacao([FromBody] CriarMovimentacaoEstoqueDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var mov = await _estoqueService.RegistrarMovimentacaoAsync(dto, UsuarioAtual);
            return Ok(mov);
        }

        [HttpGet("kardex/{produtoId}")]
        public async Task<ActionResult<List<KardexDto>>> ObterKardex(int produtoId, [FromQuery] DateTime? de, [FromQuery] DateTime? ate)
        {
            var kardex = await _estoqueService.ObterKardexAsync(produtoId, de, ate);
            return Ok(kardex);
        }

        [HttpGet("inventarios")]
        public async Task<ActionResult<List<InventarioDto>>> ListarInventarios([FromQuery] string? status)
        {
            var invs = await _estoqueService.ListarInventariosAsync(status);
            return Ok(invs);
        }

        [HttpPost("inventarios")]
        [Authorize(Roles = "SuperAdmin,Admin,Logistica")]
        public async Task<ActionResult<InventarioDto>> CriarInventario([FromQuery] string descricao, [FromQuery] int? lojaId)
        {
            var inv = await _estoqueService.CriarInventarioAsync(descricao, lojaId, UsuarioAtual);
            return Ok(inv);
        }

        [HttpPost("inventarios/{inventarioId}/contagem")]
        [Authorize(Roles = "SuperAdmin,Admin,Logistica")]
        public async Task<IActionResult> RegistrarContagem(int inventarioId, [FromQuery] int produtoId, [FromQuery] decimal quantidadeContada, [FromQuery] string? observacoes)
        {
            var resultado = await _estoqueService.RegistrarContagemInventarioAsync(inventarioId, produtoId, quantidadeContada, observacoes);
            if (!resultado) return BadRequest("Erro ao registrar contagem");
            return Ok(new { sucesso = true });
        }

        [HttpPost("inventarios/{inventarioId}/finalizar")]
        [Authorize(Roles = "SuperAdmin,Admin,Logistica")]
        public async Task<ActionResult<InventarioDto>> FinalizarInventario(int inventarioId)
        {
            var inv = await _estoqueService.FinalizarInventarioAsync(inventarioId, UsuarioAtual);
            return Ok(inv);
        }

        [HttpPost("inventarios/{inventarioId}/ajustar")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> AjustarPorInventario(int inventarioId)
        {
            await _estoqueService.AjustarEstoquePorInventarioAsync(inventarioId, UsuarioAtual);
            return Ok(new { sucesso = true, mensagem = "Estoque ajustado com base no inventário" });
        }
    }
}
