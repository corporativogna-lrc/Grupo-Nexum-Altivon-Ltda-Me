using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexumAltivon.API.Services;

namespace NexumAltivon.API.Controllers.ERP
{
    /// <summary>
    /// Dashboard ERP GenesisGest.Net — KPIs consolidados em tempo real
    /// Grupo Nexum Altivon ME | www.nexumaltivon.com
    /// Fase 5 — ERP/CRM Completo
    /// </summary>
    [ApiController]
    [Route("api/erp/dashboard")]
    [Authorize(Policy = "Gerente")]
    public class ErpDashboardController : ControllerBase
    {
        private readonly IErpDashboardService _dashboardService;
        private readonly ILogger<ErpDashboardController> _logger;

        public ErpDashboardController(IErpDashboardService dashboardService, ILogger<ErpDashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Dashboard completo ERP — todos os KPIs consolidados
        /// </summary>
        [HttpGet("completo")]
        [ProducesResponseType(typeof(ErpDashboardCompletoDto), 200)]
        public async Task<IActionResult> GetDashboardCompleto()
        {
            try
            {
                var dashboard = await _dashboardService.ObterDashboardCompletoAsync();
                _logger.LogInformation("Dashboard ERP completo consultado por {User}", User.Identity?.Name);
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar dashboard ERP completo");
                return StatusCode(500, new { message = "Erro interno ao carregar dashboard ERP", error = ex.Message });
            }
        }

        /// <summary>
        /// KPIs Financeiros — saldo, contas atrasadas, projeção
        /// </summary>
        [HttpGet("financeiro")]
        [Authorize(Policy = "Financeiro")]
        [ProducesResponseType(typeof(FinanceiroKpiDto), 200)]
        public async Task<IActionResult> GetKpiFinanceiro()
        {
            var kpis = await _dashboardService.ObterKpiFinanceiroAsync();
            return Ok(kpis);
        }

        /// <summary>
        /// KPIs de CRM — leads, pipeline, tarefas
        /// </summary>
        [HttpGet("crm")]
        [ProducesResponseType(typeof(CrmKpiDto), 200)]
        public async Task<IActionResult> GetKpiCrm()
        {
            var kpis = await _dashboardService.ObterKpiCrmAsync();
            return Ok(kpis);
        }

        /// <summary>
        /// KPIs de Estoque — itens críticos, valor em estoque, inventários
        /// </summary>
        [HttpGet("estoque")]
        [ProducesResponseType(typeof(EstoqueKpiDto), 200)]
        public async Task<IActionResult> GetKpiEstoque()
        {
            var kpis = await _dashboardService.ObterKpiEstoqueAsync();
            return Ok(kpis);
        }

        /// <summary>
        /// KPIs Fiscais — NFe emitidas, pendentes, canceladas
        /// </summary>
        [HttpGet("fiscal")]
        [Authorize(Policy = "Fiscal")]
        [ProducesResponseType(typeof(FiscalKpiDto), 200)]
        public async Task<IActionResult> GetKpiFiscal()
        {
            var kpis = await _dashboardService.ObterKpiFiscalAsync();
            return Ok(kpis);
        }

        /// <summary>
        /// Resumo executivo para o CEO — apenas números estratégicos
        /// </summary>
        [HttpGet("executivo")]
        [Authorize(Policy = "Admin")]
        [ProducesResponseType(typeof(ResumoExecutivoDto), 200)]
        public async Task<IActionResult> GetResumoExecutivo()
        {
            var resumo = await _dashboardService.ObterResumoExecutivoAsync();
            return Ok(resumo);
        }

        /// <summary>
        /// Gráfico de evolução financeira — últimos 12 meses
        /// </summary>
        [HttpGet("grafico/financeiro-12m")]
        [Authorize(Policy = "Financeiro")]
        [ProducesResponseType(typeof(List<MensalFinanceiroDto>), 200)]
        public async Task<IActionResult> GetGraficoFinanceiro12M()
        {
            var dados = await _dashboardService.ObterEvolucaoFinanceira12MesesAsync();
            return Ok(dados);
        }

        /// <summary>
        /// Gráfico de pipeline CRM — distribuição por status
        /// </summary>
        [HttpGet("grafico/pipeline-crm")]
        [ProducesResponseType(typeof(List<PipelineDistribuicaoDto>), 200)]
        public async Task<IActionResult> GetGraficoPipelineCrm()
        {
            var dados = await _dashboardService.ObterDistribuicaoPipelineAsync();
            return Ok(dados);
        }

        /// <summary>
        /// Gráfico de giro de estoque — top 10 produtos
        /// </summary>
        [HttpGet("grafico/giro-estoque")]
        [ProducesResponseType(typeof(List<GiroEstoqueDto>), 200)]
        public async Task<IActionResult> GetGraficoGiroEstoque()
        {
            var dados = await _dashboardService.ObterTopGiroEstoqueAsync(10);
            return Ok(dados);
        }

        /// <summary>
        /// Alertas ativos do ERP — estoque baixo, contas vencidas, tarefas atrasadas
        /// </summary>
        [HttpGet("alertas")]
        [ProducesResponseType(typeof(List<AlertaErpDto>), 200)]
        public async Task<IActionResult> GetAlertasAtivos()
        {
            var alertas = await _dashboardService.ObterAlertasAtivosAsync();
            return Ok(alertas);
        }
    }

    // ============================================
    // DTOs do Dashboard ERP
    // ============================================
    public class ErpDashboardCompletoDto
    {
        public FinanceiroKpiDto Financeiro { get; set; } = new();
        public CrmKpiDto Crm { get; set; } = new();
        public EstoqueKpiDto Estoque { get; set; } = new();
        public FiscalKpiDto Fiscal { get; set; } = new();
        public List<AlertaErpDto> Alertas { get; set; } = new();
        public DateTime AtualizadoEm { get; set; } = DateTime.Now;
    }

    public class FinanceiroKpiDto
    {
        public decimal SaldoDisponivel { get; set; }
        public decimal ContasReceberTotal { get; set; }
        public decimal ContasPagarTotal { get; set; }
        public decimal ContasReceberVencidas { get; set; }
        public decimal ContasPagarVencidas { get; set; }
        public int DiasContasReceberVencidas { get; set; }
        public int DiasContasPagarVencidas { get; set; }
        public decimal Projecao7Dias { get; set; }
        public decimal FaturamentoMesAtual { get; set; }
        public decimal FaturamentoMesAnterior { get; set; }
        public decimal VariacaoFaturamento { get; set; }
        public decimal TicketMedio { get; set; }
    }

    public class CrmKpiDto
    {
        public int TotalLeads { get; set; }
        public int LeadsNovosMes { get; set; }
        public int LeadsConvertidosMes { get; set; }
        public decimal TaxaConversao { get; set; }
        public decimal ValorPipelineTotal { get; set; }
        public int TarefasPendentes { get; set; }
        public int TarefasAtrasadas { get; set; }
        public int InteracoesHoje { get; set; }
        public List<PipelineStatusDto> Pipeline { get; set; } = new();
    }

    public class PipelineStatusDto
    {
        public string Status { get; set; } = "";
        public int Quantidade { get; set; }
        public decimal ValorEstimado { get; set; }
    }

    public class EstoqueKpiDto
    {
        public int TotalProdutos { get; set; }
        public int ItensCriticos { get; set; }
        public int ItensAtencao { get; set; }
        public decimal ValorTotalEstoque { get; set; }
        public decimal ValorEstoqueCritico { get; set; }
        public int InventariosPendentes { get; set; }
        public int TransferenciasPendentes { get; set; }
        public decimal GiroMedioDias { get; set; }
    }

    public class FiscalKpiDto
    {
        public int NFeEmitidasMes { get; set; }
        public int NFePendentes { get; set; }
        public int NFeCanceladasMes { get; set; }
        public decimal ValorTotalNFeMes { get; set; }
        public decimal ValorImpostosMes { get; set; }
        public int AlertasFiscais { get; set; }
    }

    public class ResumoExecutivoDto
    {
        public decimal FaturamentoTotal { get; set; }
        public decimal LucroLiquido { get; set; }
        public decimal MargemLiquida { get; set; }
        public int PedidosMes { get; set; }
        public int ClientesAtivos { get; set; }
        public decimal CAC { get; set; }
        public decimal LTV { get; set; }
        public decimal IndiceSatisfacao { get; set; }
    }

    public class MensalFinanceiroDto
    {
        public string Mes { get; set; } = "";
        public decimal Receitas { get; set; }
        public decimal Despesas { get; set; }
        public decimal Lucro { get; set; }
    }

    public class PipelineDistribuicaoDto
    {
        public string Status { get; set; } = "";
        public int Quantidade { get; set; }
        public decimal Valor { get; set; }
        public string Cor { get; set; } = "";
    }

    public class GiroEstoqueDto
    {
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = "";
        public int QuantidadeVendida { get; set; }
        public int SaldoAtual { get; set; }
        public decimal GiroDias { get; set; }
    }

    public class AlertaErpDto
    {
        public string Tipo { get; set; } = "";
        public string Severidade { get; set; } = "";
        public string Mensagem { get; set; } = "";
        public string? LinkAcao { get; set; }
        public DateTime GeradoEm { get; set; }
    }
}
