using Hangfire.Dashboard;

namespace NexumAltivon.ERP.Configurations
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            return httpContext.User.Identity?.IsAuthenticated == true;
        }
    }
}
