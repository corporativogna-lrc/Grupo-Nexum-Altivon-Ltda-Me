using NexumAltivon.API.Controllers.ERP;

namespace NexumAltivon.API.Services
{
    /// <summary>
    /// Interface do serviço de Dashboard ERP
    /// </summary>
    public interface IErpDashboardService
    {
        Task<ErpDashboardCompletoDto> ObterDashboardCompletoAsync();
        Task<FinanceiroKpiDto> ObterKpiFinanceiroAsync();
        Task<CrmKpiDto> ObterKpiCrmAsync();
        Task<EstoqueKpiDto> ObterKpiEstoqueAsync();
        Task<FiscalKpiDto> ObterKpiFiscalAsync();
        Task<ResumoExecutivoDto> ObterResumoExecutivoAsync();
        Task<List<MensalFinanceiroDto>> ObterEvolucaoFinanceira12MesesAsync();
        Task<List<PipelineDistribuicaoDto>> ObterDistribuicaoPipelineAsync();
        Task<List<GiroEstoqueDto>> ObterTopGiroEstoqueAsync(int top);
        Task<List<AlertaErpDto>> ObterAlertasAtivosAsync();
    }
}
