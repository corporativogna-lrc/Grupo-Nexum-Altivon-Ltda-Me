using Microsoft.Extensions.DependencyInjection;
using NexumAltivon.API.Services;

namespace NexumAltivon.API.Configurations
{
    public static class IntegrationExtensions
    {
        public static IServiceCollection AddIntegrationServices(this IServiceCollection services)
        {
            // Marketplaces
            services.AddScoped<IMercadoLivreService, MercadoLivreService>();
            services.AddScoped<IMarketplaceHubService, MarketplaceHubService>();
            services.AddScoped<IMarketplaceSyncService, MarketplaceSyncService>();

            // Dropshipping
            services.AddScoped<IDropshippingService, DropshippingService>();

            // Logística
            services.AddScoped<ILogisticaService, LogisticaService>();

            // ERP
            services.AddScoped<IErpSyncService, ErpSyncService>();

            // HttpClients adicionais
            services.AddHttpClient("MercadoLivre");
            services.AddHttpClient("Shopee");
            services.AddHttpClient("Amazon");
            services.AddHttpClient("GenesisGest");

            return services;
        }
    }
}
