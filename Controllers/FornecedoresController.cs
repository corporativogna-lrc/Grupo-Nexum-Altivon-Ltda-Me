using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexumAltivon.ERP.DTOs;
using NexumAltivon.ERP.Services;

namespace NexumAltivon.ERP.Controllers
{
    /// <summary>
    /// Gestão de fornecedores e avaliações
    /// </summary>
    [ApiController]
    [Route("api/erp/fornecedores")]
    [Authorize(Roles = "Gerente,Admin,SuperAdmin,Compras")]
    public class FornecedoresController : ControllerBase
    {
        private readonly IFornecedorService _fornecedorService;

        public FornecedoresController(IFornecedorService fornecedorService)
        {
            _fornecedorService = fornecedorService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FornecedorDto>>> Listar(
            [FromQuery] string? status = null,
            [FromQuery] string? segmento = null,
            [FromQuery] bool? dropshipping = null)
        {
            var fornecedores = await _fornecedorService.ListarAsync(status, segmento, dropshipping);
            return Ok(fornecedores);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FornecedorDto>> ObterPorId(int id)
        {
            var fornecedor = await _fornecedorService.ObterPorIdAsync(id);
            if (fornecedor == null) return NotFound();
            return Ok(fornecedor);
        }

        [HttpPost]
        public async Task<ActionResult<FornecedorDto>> Criar([FromBody] CriarFornecedorDto dto)
        {
            var fornecedor = await _fornecedorService.CriarAsync(dto, User.Identity?.Name ?? "Sistema");
            return CreatedAtAction(nameof(ObterPorId), new { id = fornecedor.Id }, fornecedor);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<FornecedorDto>> Atualizar(int id, [FromBody] CriarFornecedorDto dto)
        {
            var fornecedor = await _fornecedorService.AtualizarAsync(id, dto);
            return Ok(fornecedor);
        }

        [HttpPost("{id}/avaliar")]
        public async Task<IActionResult> Avaliar(int id, [FromBody] AvaliacaoFornecedorDto dto)
        {
            await _fornecedorService.AvaliarAsync(id, dto, User.Identity?.Name ?? "Sistema");
            return Ok(new { mensagem = "Avaliação registrada com sucesso" });
        }

        [HttpGet("{id}/avaliacoes")]
        public async Task<ActionResult<IEnumerable<AvaliacaoFornecedorDto>>> ListarAvaliacoes(int id)
        {
            var avaliacoes = await _fornecedorService.ListarAvaliacoesAsync(id);
            return Ok(avaliacoes);
        }
    }

    public class AvaliacaoFornecedorDto
    {
        public int Nota { get; set; }
        public string? Comentario { get; set; }
        public string? CategoriaAvaliacao { get; set; }
    }
}
