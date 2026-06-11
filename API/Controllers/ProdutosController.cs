using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Services;

namespace NexumAltivon.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoService _produtoService;

    public ProdutosController(IProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    /// <summary>
    /// Lista produtos com paginação, filtro por loja e categoria
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<ProdutoListagemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ProdutoListagemDto>>>> Listar(
        [FromQuery] PaginacaoDto paginacao,
        [FromQuery] int? lojaId,
        [FromQuery] int? categoriaId)
    {
        var resultado = await _produtoService.ListarAsync(paginacao, lojaId, categoriaId);
        Response.Headers.Append("X-Total-Count", resultado.TotalRegistros?.ToString() ?? "0");
        return Ok(resultado);
    }

    /// <summary>
    /// Lista produtos em destaque
    /// </summary>
    [HttpGet("destaques")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<ProdutoListagemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ProdutoListagemDto>>>> Destaques([FromQuery] int? lojaId)
    {
        var resultado = await _produtoService.ListarDestaquesAsync(lojaId);
        return Ok(resultado);
    }

    /// <summary>
    /// Obtém um produto pelo ID
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ProdutoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ProdutoDto>>> ObterPorId(int id)
    {
        var resultado = await _produtoService.ObterPorIdAsync(id);
        if (!resultado.Sucesso)
            return NotFound(resultado);
        return Ok(resultado);
    }

    /// <summary>
    /// Obtém um produto pelo slug
    /// </summary>
    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ProdutoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ProdutoDto>>> ObterPorSlug(string slug)
    {
        var resultado = await _produtoService.ObterPorSlugAsync(slug);
        if (!resultado.Sucesso)
            return NotFound(resultado);
        return Ok(resultado);
    }

    /// <summary>
    /// Cria um novo produto (requer Gerente)
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Gerente")]
    [ProducesResponseType(typeof(ApiResponse<ProdutoDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<ProdutoDto>>> Criar([FromBody] CriarProdutoDto dto)
    {
        var resultado = await _produtoService.CriarAsync(dto);
        if (!resultado.Sucesso)
            return BadRequest(resultado);
        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Dados?.Id }, resultado);
    }

    /// <summary>
    /// Atualiza um produto (requer Gerente)
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "Gerente")]
    [ProducesResponseType(typeof(ApiResponse<ProdutoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ProdutoDto>>> Atualizar(int id, [FromBody] CriarProdutoDto dto)
    {
        var resultado = await _produtoService.AtualizarAsync(id, dto);
        if (!resultado.Sucesso)
            return NotFound(resultado);
        return Ok(resultado);
    }

    /// <summary>
    /// Remove um produto (requer Gerente)
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "Gerente")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> Excluir(int id)
    {
        var resultado = await _produtoService.ExcluirAsync(id);
        if (!resultado.Sucesso)
            return NotFound(resultado);
        return Ok(resultado);
    }

    /// <summary>
    /// Atualiza o estoque de um produto (requer Gerente)
    /// </summary>
    [HttpPatch("{id:int}/estoque")]
    [Authorize(Policy = "Gerente")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> AtualizarEstoque(int id, [FromQuery] int quantidade)
    {
        var resultado = await _produtoService.AtualizarEstoqueAsync(id, quantidade);
        if (!resultado.Sucesso)
            return BadRequest(resultado);
        return Ok(resultado);
    }
}
