using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Services;

namespace NexumAltivon.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FinanceiroController : ControllerBase
{
    private readonly IFinanceiroService _financeiroService;

    public FinanceiroController(IFinanceiroService financeiroService)
    {
        _financeiroService = financeiroService;
    }

    /// <summary>
    /// Obtém o faturamento em um período (requer Financeiro)
    /// </summary>
    [HttpGet("faturamento")]
    [Authorize(Policy = "Financeiro")]
    [ProducesResponseType(typeof(ApiResponse<decimal>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<decimal>>> Faturamento(
        [FromQuery] DateTime inicio,
        [FromQuery] DateTime fim,
        [FromQuery] int? lojaId)
    {
        var resultado = await _financeiroService.ObterFaturamentoAsync(inicio, fim, lojaId);
        return Ok(resultado);
    }

    /// <summary>
    /// Obtém faturamento por loja em um período (requer Financeiro)
    /// </summary>
    [HttpGet("faturamento/lojas")]
    [Authorize(Policy = "Financeiro")]
    [ProducesResponseType(typeof(ApiResponse<Dictionary<string, decimal>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<Dictionary<string, decimal>>>> FaturamentoPorLoja(
        [FromQuery] DateTime inicio,
        [FromQuery] DateTime fim)
    {
        var resultado = await _financeiroService.ObterFaturamentoPorLojaAsync(inicio, fim);
        return Ok(resultado);
    }
}
