using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.DTOs.Admin;
using NexumAltivon.API.Services.Admin;

namespace NexumAltivon.API.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Policy = "Gerente")]
public class DashboardController : ControllerBase
{
    private readonly IAdminDashboardService _dashboardService;

    public DashboardController(IAdminDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Obtém o dashboard completo com todos os KPIs, gráficos e listas
    /// </summary>
    [HttpGet("completo")]
    [ProducesResponseType(typeof(ApiResponse<DashboardCompletoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DashboardCompletoDto>>> ObterDashboardCompleto()
    {
        var resultado = await _dashboardService.ObterDashboardCompletoAsync();
        return Ok(ApiResponse<DashboardCompletoDto>.Ok(resultado));
    }

    /// <summary>
    /// Obtém apenas os KPIs principais
    /// </summary>
    [HttpGet("kpis")]
    [ProducesResponseType(typeof(ApiResponse<DashboardKpiDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DashboardKpiDto>>> ObterKpis()
    {
        var resultado = await _dashboardService.ObterKpisAsync();
        return Ok(ApiResponse<DashboardKpiDto>.Ok(resultado));
    }

    /// <summary>
    /// Obtém faturamento dos últimos 7 dias
    /// </summary>
    [HttpGet("faturamento/semanal")]
    [ProducesResponseType(typeof(ApiResponse<List<FaturamentoPorPeriodoDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<FaturamentoPorPeriodoDto>>>> FaturamentoSemanal()
    {
        var resultado = await _dashboardService.ObterFaturamentoSemanalAsync();
        return Ok(ApiResponse<List<FaturamentoPorPeriodoDto>>.Ok(resultado));
    }

    /// <summary>
    /// Obtém faturamento dos últimos 12 meses
    /// </summary>
    [HttpGet("faturamento/mensal")]
    [ProducesResponseType(typeof(ApiResponse<List<FaturamentoPorPeriodoDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<FaturamentoPorPeriodoDto>>>> FaturamentoMensal([FromQuery] int meses = 12)
    {
        var resultado = await _dashboardService.ObterFaturamentoMensalAsync(meses);
        return Ok(ApiResponse<List<FaturamentoPorPeriodoDto>>.Ok(resultado));
    }

    /// <summary>
    /// Obtém vendas por loja no período
    /// </summary>
    [HttpGet("vendas/lojas")]
    [ProducesResponseType(typeof(ApiResponse<List<VendasPorLojaDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<VendasPorLojaDto>>>> VendasPorLoja(
        [FromQuery] DateTime? inicio,
        [FromQuery] DateTime? fim)
    {
        var resultado = await _dashboardService.ObterVendasPorLojaAsync(inicio, fim);
        return Ok(ApiResponse<List<VendasPorLojaDto>>.Ok(resultado));
    }

    /// <summary>
    /// Obtém os produtos mais vendidos
    /// </summary>
    [HttpGet("produtos/mais-vendidos")]
    [ProducesResponseType(typeof(ApiResponse<List<ProdutosMaisVendidosDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ProdutosMaisVendidosDto>>>> ProdutosMaisVendidos([FromQuery] int top = 10)
    {
        var resultado = await _dashboardService.ObterProdutosMaisVendidosAsync(top);
        return Ok(ApiResponse<List<ProdutosMaisVendidosDto>>.Ok(resultado));
    }

    /// <summary>
    /// Obtém clientes recentes
    /// </summary>
    [HttpGet("clientes/recentes")]
    [ProducesResponseType(typeof(ApiResponse<List<ClientesRecentesDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ClientesRecentesDto>>>> ClientesRecentes([FromQuery] int quantidade = 10)
    {
        var resultado = await _dashboardService.ObterClientesRecentesAsync(quantidade);
        return Ok(ApiResponse<List<ClientesRecentesDto>>.Ok(resultado));
    }

    /// <summary>
    /// Obtém pedidos recentes
    /// </summary>
    [HttpGet("pedidos/recentes")]
    [ProducesResponseType(typeof(ApiResponse<List<PedidosRecentesDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<PedidosRecentesDto>>>> PedidosRecentes([FromQuery] int quantidade = 10)
    {
        var resultado = await _dashboardService.ObterPedidosRecentesAsync(quantidade);
        return Ok(ApiResponse<List<PedidosRecentesDto>>.Ok(resultado));
    }

    /// <summary>
    /// Obtém leads recentes
    /// </summary>
    [HttpGet("leads/recentes")]
    [ProducesResponseType(typeof(ApiResponse<List<LeadsRecentesDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<LeadsRecentesDto>>>> LeadsRecentes([FromQuery] int quantidade = 10)
    {
        var resultado = await _dashboardService.ObterLeadsRecentesAsync(quantidade);
        return Ok(ApiResponse<List<LeadsRecentesDto>>.Ok(resultado));
    }
}
