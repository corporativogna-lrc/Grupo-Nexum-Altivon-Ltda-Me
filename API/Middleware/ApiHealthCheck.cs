using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NexumAltivon.API.Helpers;

public class ApiHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthCheckResult.Healthy("API Nexum Altivon está operacional."));
    }
}
