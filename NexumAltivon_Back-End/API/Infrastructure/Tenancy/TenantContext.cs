/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

namespace NexumAltivon.API.Infrastructure.Tenancy;

public sealed class TenantContext : ITenantContext
{
    public static readonly Guid DefaultTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public Guid TenantId { get; private set; } = DefaultTenantId;
    public Guid? UserId { get; private set; }
    public string? UserEmail { get; private set; }

    public void SetTenant(Guid tenantId)
    {
        TenantId = tenantId == Guid.Empty ? DefaultTenantId : tenantId;
    }

    public void SetUser(Guid? userId, string? userEmail)
    {
        UserId = userId == Guid.Empty ? null : userId;
        UserEmail = string.IsNullOrWhiteSpace(userEmail) ? null : userEmail.Trim();
    }
}
