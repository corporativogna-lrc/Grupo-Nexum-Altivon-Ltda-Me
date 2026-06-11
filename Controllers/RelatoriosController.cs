using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexumAltivon.ERP.DTOs;
using NexumAltivon.ERP.Services;
using System.Security.Claims;

namespace NexumAltivon.ERP.Controllers;

[ApiController]
[Route("api/erp/relatorios")]
[Authorize(Policy = "ERP.Admin")]
public class RelatoriosController : ControllerBase
{
    private readonly IRelatorioService _service;
    public RelatoriosController(IRelatorioService service) => _service = service;

    [HttpPost("gerar")]
    public async Task<ActionResult> GerarRelatorio(FiltroRelatorioDto filtro)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _service.GerarRelatorioAsync(filtro, userId);
        return Ok(result);
    }

    [HttpPost("download")]
    public async Task<ActionResult> DownloadRelatorio(FiltroRelatorioDto filtro)
    {
        var dados = await _service.ObterDadosRelatorioAsync(filtro);

        if (filtro.Formato?.ToUpper() == "EXCEL")
        {
            var bytes = await _service.GerarExcelAsync(dados, filtro.TipoRelatorio);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"REL_{filtro.TipoRelatorio}_{DateTime.Now:yyyyMMdd}.xlsx");
        }
        else
        {
            var html = $"<html><body><h1>{filtro.TipoRelatorio}</h1><pre>{System.Text.Json.JsonSerializer.Serialize(dados)}</pre></body></html>";
            var bytes = await _service.GerarPdfAsync(html);
            return File(bytes, "application/pdf",
                $"REL_{filtro.TipoRelatorio}_{DateTime.Now:yyyyMMdd}.pdf");
        }
    }
}
