using Microsoft.EntityFrameworkCore;
using NexumAltivon.ERP.Data;
using NexumAltivon.ERP.Services;

namespace NexumAltivon.ERP.Configurations
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Registra todos os serviços do ERP GenesisGest.Net
        /// </summary>
        public static IServiceCollection AddERPServices(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContext — compartilha a mesma string de conexão da API
            services.AddDbContext<NexumDbContext>(options =>
                options.UseMySql(
                    configuration.GetConnectionString("NexumDb"),
                    ServerVersion.AutoDetect(configuration.GetConnectionString("NexumDb")),
                    b => b.MigrationsAssembly("NexumAltivon.ERP")));

            // Services de Financeiro
            services.AddScoped<IFinanceiroService, FinanceiroService>();

            // Services de Estoque
            services.AddScoped<IEstoqueService, EstoqueService>();

            // Services de CRM
            services.AddScoped<ICrmService, CrmService>();

            // Services de Fornecedores
            services.AddScoped<IFornecedorService, FornecedorService>();

            // TODO: Fase 5 Parte 2 — RelatóriosService, DashboardERPService, FiscalService
            // services.AddScoped<IRelatorioService, RelatorioService>();
            // services.AddScoped<IFiscalService, FiscalService>();
            // services.AddScoped<IDashboardERPService, DashboardERPService>();

            return services;
        }
    }
}
