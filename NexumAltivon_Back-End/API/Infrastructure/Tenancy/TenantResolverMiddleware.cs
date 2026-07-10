/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace NexumAltivon.API.Infrastructure.Tenancy;

public sealed class TenantResolverMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Guid _defaultTenantId;

    public TenantResolverMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _defaultTenantId = ResolveConfiguredDefaultTenantId(configuration);
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        tenantContext.SetTenant(ResolveTenantId(context.User, context.Request));
        tenantContext.SetUser(ResolveUserId(context.User), ResolveUserEmail(context.User));

        await _next(context);
    }

    private Guid ResolveTenantId(ClaimsPrincipal user, HttpRequest request)
    {
        var candidates = new[]
        {
            user.FindFirstValue("tenant_id"),
            user.FindFirstValue("tenantId"),
            request.Headers["X-Tenant-Id"].FirstOrDefault(),
            request.Query["tenant_id"].FirstOrDefault(),
            request.Query["tenantId"].FirstOrDefault()
        };

        foreach (var candidate in candidates)
        {
            if (Guid.TryParse(candidate, out var tenantId) && tenantId != Guid.Empty)
            {
                return tenantId;
            }
        }

        return _defaultTenantId;
    }

    private static Guid ResolveConfiguredDefaultTenantId(IConfiguration configuration)
    {
        var configured = configuration["TenantSettings:DefaultTenantId"];
        if (string.IsNullOrWhiteSpace(configured))
        {
            return TenantContext.DefaultTenantId;
        }

        if (Guid.TryParse(configured, out var tenantId) && tenantId != Guid.Empty)
        {
            return tenantId;
        }

        throw new InvalidOperationException("TenantSettings:DefaultTenantId invalido. Configure um UUID valido ou remova a chave para usar o tenant operacional padrao.");
    }

    private static Guid? ResolveUserId(ClaimsPrincipal user)
    {
        var rawId = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(rawId, out var userId) && userId != Guid.Empty)
        {
            return userId;
        }

        return null;
    }

    private static string? ResolveUserEmail(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Email)
            ?? user.FindFirstValue(JwtRegisteredClaimNames.Email)
            ?? user.Identity?.Name;
    }
}
