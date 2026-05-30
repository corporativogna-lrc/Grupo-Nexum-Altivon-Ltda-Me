using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Services;

namespace NexumAltivon.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LojasController : ControllerBase
{
    private readonly ILojaService _lojaService;

    public LojasController(ILojaService lojaService)
    {
        _lojaService = lojaService;
    }

    /// <summary>
    /// Lista todas as lojas do Grupo Nexum Altivon
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<LojaDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<LojaDto>>>> Listar()
    {
        var resultado = await _lojaService.ListarTodasAsync();
        return Ok(resultado);
    }

    /// <summary>
    /// Obtém uma loja pelo ID
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LojaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LojaDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LojaDto>>> ObterPorId(int id)
    {
        var resultado = await _lojaService.ObterPorIdAsync(id);
        if (!resultado.Sucesso)
            return NotFound(resultado);
        return Ok(resultado);
    }

    /// <summary>
    /// Obtém uma loja pelo slug (ex: grann-tur, chronos)
    /// </summary>
    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LojaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LojaDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LojaDto>>> ObterPorSlug(string slug)
    {
        var resultado = await _lojaService.ObterPorSlugAsync(slug);
        if (!resultado.Sucesso)
            return NotFound(resultado);
        return Ok(resultado);
    }

    /// <summary>
    /// Cria uma nova loja (requer Admin)
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<LojaDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<LojaDto>>> Criar([FromBody] CriarLojaDto dto)
    {
        var resultado = await _lojaService.CriarAsync(dto);
        if (!resultado.Sucesso)
            return BadRequest(resultado);
        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Dados?.Id }, resultado);
    }

    /// <summary>
    /// Atualiza uma loja (requer Admin)
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<LojaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LojaDto>>> Atualizar(int id, [FromBody] CriarLojaDto dto)
    {
        var resultado = await _lojaService.AtualizarAsync(id, dto);
        if (!resultado.Sucesso)
            return NotFound(resultado);
        return Ok(resultado);
    }

    /// <summary>
    /// Remove uma loja (requer SuperAdmin)
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "SuperAdmin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> Excluir(int id)
    {
        var resultado = await _lojaService.ExcluirAsync(id);
        if (!resultado.Sucesso)
            return NotFound(resultado);
        return Ok(resultado);
    }
}
