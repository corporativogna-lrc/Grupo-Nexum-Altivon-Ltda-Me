using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexumAltivon.ERP.DTOs;
using NexumAltivon.ERP.Services;

namespace NexumAltivon.ERP.Controllers;

[ApiController]
[Route("api/erp/fiscal")]
[Authorize(Policy = "ERP.Fiscal")]
public class FiscalController : ControllerBase
{
    private readonly IFiscalService _service;
    public FiscalController(IFiscalService service) => _service = service;

    [HttpGet("nfe")]
    public async Task<ActionResult> ListarNFes(
        [FromQuery] string? status, [FromQuery] int? lojaId,
        [FromQuery] DateTime? de, [FromQuery] DateTime? ate)
        => Ok(await _service.ListarNFesAsync(status, lojaId, de, ate));

    [HttpGet("nfe/{id}")]
    public async Task<ActionResult> ObterNFe(int id)
    {
        var result = await _service.ObterNFePorIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("nfe/emitir")]
    public async Task<ActionResult> EmitirNFe(EmitirNFeDto dto)
        => Ok(await _service.EmitirNFeAsync(dto));

    [HttpPost("nfe/cancelar")]
    public async Task<ActionResult> CancelarNFe(CancelarNFeDto dto)
        => Ok(await _service.CancelarNFeAsync(dto));

    [HttpGet("configuracao/{lojaId}")]
    public async Task<ActionResult> ObterConfiguracao(int lojaId)
    {
        var result = await _service.ObterConfiguracaoFiscalAsync(lojaId);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("configuracao")]
    public async Task<ActionResult> SalvarConfiguracao(ConfiguracaoFiscalDto dto)
        => Ok(await _service.SalvarConfiguracaoFiscalAsync(dto));

    [HttpGet("manifestos")]
    public async Task<ActionResult> ListarManifestos()
        => Ok(await _service.ListarManifestosPendentesAsync());

    [HttpPost("manifestos/{id}/confirmar")]
    public async Task<ActionResult> ConfirmarManifesto(int id, [FromBody] string operacao)
        => Ok(await _service.ConfirmarManifestoAsync(id, operacao));
}
