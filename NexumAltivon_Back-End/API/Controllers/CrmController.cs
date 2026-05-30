using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Services;

namespace NexumAltivon.API.Controllers;

[ApiController]
[Route("api/crm")]
public class CrmController : ControllerBase
{
    private readonly ICrmService _crmService;

    public CrmController(ICrmService crmService)
    {
        _crmService = crmService;
    }

    /// <summary>
    /// Lista leads com paginação e filtros (requer Vendedor)
    /// </summary>
    [HttpGet("leads")]
    [Authorize(Policy = "Vendedor")]
    [ProducesResponseType(typeof(ApiResponse<List<CrmLeadDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<CrmLeadDto>>>> ListarLeads(
        [FromQuery] PaginacaoDto paginacao,
        [FromQuery] string? tipo,
        [FromQuery] string? status)
    {
        var resultado = await _crmService.ListarAsync(paginacao, tipo, status);
        Response.Headers.Append("X-Total-Count", resultado.TotalRegistros?.ToString() ?? "0");
        return Ok(resultado);
    }

    /// <summary>
    /// Obtém um lead pelo ID
    /// </summary>
    [HttpGet("leads/{id:int}")]
    [Authorize(Policy = "Vendedor")]
    [ProducesResponseType(typeof(ApiResponse<CrmLeadDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CrmLeadDto>>> ObterLead(int id)
    {
        var resultado = await _crmService.ObterPorIdAsync(id);
        if (!resultado.Sucesso)
            return NotFound(resultado);
        return Ok(resultado);
    }

    /// <summary>
    /// Cria um novo lead (público - formulário do site)
    /// </summary>
    [HttpPost("leads")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<CrmLeadDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<CrmLeadDto>>> CriarLead([FromBody] CriarLeadDto dto)
    {
        var resultado = await _crmService.CriarAsync(dto);
        if (!resultado.Sucesso)
            return BadRequest(resultado);
        return CreatedAtAction(nameof(ObterLead), new { id = resultado.Dados?.Id }, resultado);
    }

    /// <summary>
    /// Atualiza um lead (requer Vendedor)
    /// </summary>
    [HttpPut("leads/{id:int}")]
    [Authorize(Policy = "Vendedor")]
    [ProducesResponseType(typeof(ApiResponse<CrmLeadDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CrmLeadDto>>> AtualizarLead(int id, [FromBody] AtualizarLeadDto dto)
    {
        var resultado = await _crmService.AtualizarAsync(id, dto);
        if (!resultado.Sucesso)
            return NotFound(resultado);
        return Ok(resultado);
    }

    /// <summary>
    /// Remove um lead (requer Admin)
    /// </summary>
    [HttpDelete("leads/{id:int}")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> ExcluirLead(int id)
    {
        var resultado = await _crmService.ExcluirAsync(id);
        if (!resultado.Sucesso)
            return NotFound(resultado);
        return Ok(resultado);
    }
}
