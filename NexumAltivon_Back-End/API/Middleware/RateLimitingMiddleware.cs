using System.Collections.Concurrent;

namespace NexumAltivon.API.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitingService _rateLimitingService;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, RateLimitingService rateLimitingService)
    {
        _next = next;
        _logger = logger;
        _rateLimitingService = rateLimitingService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var endpoint = context.Request.Path;
        var key = $"{ipAddress}:{endpoint}";

        if (!_rateLimitingService.IsAllowed(key))
        {
            _logger.LogWarning("Rate limit excedido para {Ip} em {Endpoint}", ipAddress, endpoint);
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.Append("Retry-After", "60");
            await context.Response.WriteAsJsonAsync(new { erro = "Muitas requisições. Tente novamente em 1 minuto." });
            return;
        }

        await _next(context);
    }
}

public class RateLimitingService
{
    private readonly ConcurrentDictionary<string, RateLimitInfo> _requests = new();
    private readonly int _maxRequestsPerMinute = 100;

    public bool IsAllowed(string key)
    {
        var now = DateTime.UtcNow;
        var info = _requests.GetOrAdd(key, _ => new RateLimitInfo { Count = 0, WindowStart = now });

        lock (info)
        {
            if ((now - info.WindowStart).TotalMinutes >= 1)
            {
                info.Count = 0;
                info.WindowStart = now;
            }

            if (info.Count >= _maxRequestsPerMinute)
                return false;

            info.Count++;
            return true;
        }
    }
}

public class RateLimitInfo
{
    public int Count { get; set; }
    public DateTime WindowStart { get; set; }
}
