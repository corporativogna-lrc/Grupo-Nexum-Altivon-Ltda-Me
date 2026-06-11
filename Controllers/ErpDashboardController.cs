using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexumAltivon.ERP.Services;

namespace NexumAltivon.ERP.Controllers
{
    /// <summary>
    /// Dashboard ERP — KPIs consolidados de todas as áreas
    /// </summary>
    [ApiController]
    [Route("api/erp/dashboard")]
    [Authorize(Roles = "Gerente,Admin,SuperAdmin,Financeiro")]
    public class ErpDashboardController : ControllerBase
    {
        private readonly IFinanceiroService _financeiroService;
        private readonly ICrmService _crmService;
        private readonly IEstoqueService _estoqueService;
        private readonly IFiscalService _fiscalService;
        private readonly IRelatorioService _relatorioService;

        public ErpDashboardController(
            IFinanceiroService financeiroService,
            ICrmService crmService,
            IEstoqueService estoqueService,
            IFiscalService fiscalService,
            IRelatorioService relatorioService)
        {
            _financeiroService = financeiroService;
            _crmService = crmService;
            _estoqueService = estoqueService;
            _fiscalService = fiscalService;
            _relatorioService = relatorioService;
        }

        [HttpGet("completo")]
        public async Task<IActionResult> ObterDashboardCompleto()
        {
            var hoje = DateTime.Now;
            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);

            var financeiro = await _financeiroService.ObterResumoFinanceiroAsync();
            var pipeline = await _crmService.ObterPipelineAsync();
            var posicaoEstoque = await _relatorioService.GerarPosicaoEstoqueAsync(apenasBaixo: true);
            var tarefas = await _crmService.ListarTarefasAsync(status: "Pendente");
            var dre = await _financeiroService.ObterDreAsync(inicioMes, hoje);

            return Ok(new
            {
                Financeiro = financeiro,
                CRM = new
                {
                    Pipeline = pipeline,
                    TarefasPendentes = tarefas,
                    TotalLeadsNovos = pipeline.Sum(p => p.Quantidade)
                },
                Estoque = new
                {
                    ItensCriticos = posicaoEstoque,
                    TotalCritico = posicaoEstoque.Count()
                },
                Fiscal = new
                {
                    Periodo = $"{inicioMes:MM/yyyy}"
                },
                DRE = dre
            });
        }

        [HttpGet("financeiro")]
        public async Task<IActionResult> ObterKPIsFinanceiro()
        {
            var resumo = await _financeiroService.ObterResumoFinanceiroAsync();
            return Ok(resumo);
        }

        [HttpGet("crm")]
        public async Task<IActionResult> ObterKPIsCRM()
        {
            var pipeline = await _crmService.ObterPipelineAsync();
            var tarefas = await _crmService.ListarTarefasAsync();
            var tarefasAtrasadas = tarefas.Where(t => t.Atrasada).ToList();

            return Ok(new
            {
                Pipeline = pipeline,
                TarefasPendentes = tarefas.Count(t => t.Status == "Pendente"),
                TarefasAtrasadas = tarefasAtrasadas.Count,
                ValorPipeline = pipeline.Sum(p => p.ValorTotal)
            });
        }

        [HttpGet("estoque")]
        public async Task<IActionResult> ObterKPIsEstoque()
        {
            var critico = await _relatorioService.GerarPosicaoEstoqueAsync(apenasBaixo: true);
            var inventarios = await _estoqueService.ListarInventariosAsync(status: "Aberto");

            return Ok(new
            {
                ItensCriticos = critico.Count(),
                InventariosAbertos = inventarios.Count(),
                ValorEstoqueTotal = 0
            });
        }
    }
}
