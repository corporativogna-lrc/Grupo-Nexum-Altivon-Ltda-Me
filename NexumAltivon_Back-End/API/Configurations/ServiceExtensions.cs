using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NexumAltivon.API.Services;

namespace NexumAltivon.API.Configurations
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddNexumServices(this IServiceCollection services)
        {
            // Serviços de domínio
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICarrinhoService, CarrinhoService>();
            services.AddScoped<ICheckoutService, CheckoutService>();
            services.AddScoped<IPedidoService, PedidoService>();
            services.AddScoped<IMercadoPagoService, MercadoPagoService>();
            services.AddScoped<IFreteService, FreteService>();
            services.AddScoped<INotificacaoService, NotificacaoService>();
            services.AddScoped<ILogAuditoriaService, LogAuditoriaService>();

            // HttpClients
            services.AddHttpClient("MercadoPago");
            services.AddHttpClient("MelhorEnvio");
            services.AddHttpClient("Notificacoes");

            return services;
        }
    }
}
