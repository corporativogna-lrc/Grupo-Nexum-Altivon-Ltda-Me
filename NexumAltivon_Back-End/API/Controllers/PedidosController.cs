using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Services;

namespace NexumAltivon.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly IPedidoService _pedidoService;

    public PedidosController(IPedidoService pedidoService)
    {
        _pedidoService = pedidoService;
    }

    /// <summary>
    /// Lista pedidos com paginação (requer Vendedor)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "Vendedor")]
    [ProducesResponseType(typeof(ApiResponse<List<PedidoDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<PedidoDto>>>> Listar(
        [FromQuery] PaginacaoDto paginacao,
        [FromQuery] int? clienteId,
        [FromQuery] string? status)
    {
        var resultado = await _pedidoService.ListarAsync(paginacao, clienteId, status);
        Response.Headers.Append("X-Total-Count", resultado.TotalRegistros?.ToString() ?? "0");
        return Ok(resultado);
    }

    /// <summary>
    /// Obtém um pedido pelo ID
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PedidoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PedidoDto>>> ObterPorId(int id)
    {
        var resultado = await _pedidoService.ObterPorIdAsync(id);
        if (!resultado.Sucesso)
            return NotFound(resultado);
        return Ok(resultado);
    }

    /// <summary>
    /// Obtém um pedido pelo número
    /// </summary>
    [HttpGet("numero/{numero}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PedidoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PedidoDto>>> ObterPorNumero(string numero)
    {
        var resultado = await _pedidoService.ObterPorNumeroAsync(numero);
        if (!resultado.Sucesso)
            return NotFound(resultado);
        return Ok(resultado);
    }

    /// <summary>
    /// Cria um novo pedido
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PedidoDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<PedidoDto>>> Criar([FromBody] CriarPedidoDto dto)
    {
        var resultado = await _pedidoService.CriarAsync(dto);
        if (!resultado.Sucesso)
            return BadRequest(resultado);
        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Dados?.Id }, resultado);
    }

    /// <summary>
    /// Atualiza o status de um pedido (requer Gerente)
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Policy = "Gerente")]
    [ProducesResponseType(typeof(ApiResponse<PedidoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PedidoDto>>> AtualizarStatus(int id, [FromBody] AtualizarStatusPedidoDto dto)
    {
        var resultado = await _pedidoService.AtualizarStatusAsync(id, dto);
        if (!resultado.Sucesso)
            return NotFound(resultado);
        return Ok(resultado);
    }
}
