/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7181
 */

using Microsoft.EntityFrameworkCore;
using NexumAltivon.API.Infrastructure.Tenancy;

namespace NexumAltivon.API.ERP.SharedData;

public sealed class GenesisDbContext : DbContext
{
    private readonly ITenantContext? _tenantContext;

    public GenesisDbContext(DbContextOptions<GenesisDbContext> options, ITenantContext? tenantContext = null) : base(options)
    {
        _tenantContext = tenantContext;
    }

    public Guid CurrentTenantId => _tenantContext?.TenantId ?? TenantContext.DefaultTenantId;
    public Guid? CurrentUserId => _tenantContext?.UserId;

    public DbSet<GenesisContaPagar> ContasPagar => Set<GenesisContaPagar>();
    public DbSet<GenesisContaReceber> ContasReceber => Set<GenesisContaReceber>();
    public DbSet<GenesisFluxoCaixa> FluxoCaixa => Set<GenesisFluxoCaixa>();
    public DbSet<GenesisBoleto> Boletos => Set<GenesisBoleto>();
    public DbSet<GenesisFinanceReferencia> FinanceiroReferencias => Set<GenesisFinanceReferencia>();
    public DbSet<GenesisRhColaborador> RhColaboradores => Set<GenesisRhColaborador>();
    public DbSet<GenesisRhReferencia> RhReferencias => Set<GenesisRhReferencia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureAuditableEntity(modelBuilder.Entity<GenesisContaPagar>());
        ConfigureAuditableEntity(modelBuilder.Entity<GenesisContaReceber>());
        ConfigureAuditableEntity(modelBuilder.Entity<GenesisFluxoCaixa>());
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        PrepareAuditableEntities();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        PrepareAuditableEntities();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void PrepareAuditableEntities()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<GenesisLegacyAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.TenantId = CurrentTenantId;
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedByUserId = CurrentUserId;
                    entry.Entity.UpdatedAt = null;
                    entry.Entity.UpdatedByUserId = null;
                    entry.Entity.IsDeleted = false;
                    entry.Entity.DeletedAt = null;
                    entry.Entity.RowVersion = Guid.NewGuid().ToByteArray();
                    break;
                case EntityState.Modified:
                    entry.Property(entity => entity.TenantId).IsModified = false;
                    entry.Property(entity => entity.CreatedAt).IsModified = false;
                    entry.Property(entity => entity.CreatedByUserId).IsModified = false;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedByUserId = CurrentUserId;
                    entry.Entity.RowVersion = Guid.NewGuid().ToByteArray();
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Property(entity => entity.TenantId).IsModified = false;
                    entry.Property(entity => entity.CreatedAt).IsModified = false;
                    entry.Property(entity => entity.CreatedByUserId).IsModified = false;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = now;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedByUserId = CurrentUserId;
                    entry.Entity.RowVersion = Guid.NewGuid().ToByteArray();
                    break;
            }
        }
    }

    private void ConfigureAuditableEntity<TEntity>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> entity)
        where TEntity : GenesisLegacyAuditableEntity
    {
        entity.Property(item => item.TenantId).HasColumnType("char(36)");
        entity.Property(item => item.RowVersion).HasColumnType("binary(16)").IsConcurrencyToken().ValueGeneratedNever();
        entity.HasIndex(item => new { item.TenantId, item.IsDeleted });
        entity.HasQueryFilter(item => item.TenantId == CurrentTenantId && !item.IsDeleted);
    }
}
