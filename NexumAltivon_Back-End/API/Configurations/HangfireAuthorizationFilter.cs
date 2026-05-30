using Hangfire.Dashboard;

namespace NexumAltivon.API.Configurations
{
    /// <summary>
    /// Filtro de autorização para o Dashboard Hangfire
    /// Apenas SuperAdmin e Admin acessam /hangfire
    /// </summary>
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Verifica se usuário está autenticado
            if (!httpContext.User.Identity?.IsAuthenticated ?? false)
                return false;

            // Verifica roles permitidas
            var roles = httpContext.User.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            return roles.Contains("SuperAdmin") || roles.Contains("Admin");
        }
    }
}
