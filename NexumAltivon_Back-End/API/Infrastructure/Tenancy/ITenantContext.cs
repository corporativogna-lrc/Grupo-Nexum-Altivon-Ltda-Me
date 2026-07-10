/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

namespace NexumAltivon.API.Infrastructure.Tenancy;

public interface ITenantContext
{
    Guid TenantId { get; }
    Guid? UserId { get; }
    string? UserEmail { get; }
    void SetTenant(Guid tenantId);
    void SetUser(Guid? userId, string? userEmail);
}
