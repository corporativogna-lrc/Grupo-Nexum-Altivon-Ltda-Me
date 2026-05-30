using NexumAltivon.API.Services;

namespace NexumAltivon.API.Middleware;

public class AuditoriaMiddleware
{
    private readonly RequestDelegate _next;

    public AuditoriaMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILogAuditoriaService auditoria)
    {
        var path = context.Request.Path;
        var method = context.Request.Method;

        // Registrar apenas operações de escrita (POST, PUT, PATCH, DELETE)
        if (method is "POST" or "PUT" or "PATCH" or "DELETE")
        {
            var ip = context.Connection.RemoteIpAddress?.ToString();
            var userAgent = context.Request.Headers.UserAgent.ToString();
            var userId = context.User.Identity?.IsAuthenticated == true
                ? int.Parse(context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0")
                : (int?)null;
            var userType = userId.HasValue ? "Usuario" : "Anonimo";

            // Log será registrado pelos serviços individuais com dados específicos
            // Este middleware apenas prepara o contexto se necessário
        }

        await _next(context);
    }
}
