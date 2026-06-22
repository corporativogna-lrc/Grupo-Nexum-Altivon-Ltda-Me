using Microsoft.EntityFrameworkCore;

namespace NexumAltivon.API.ERP.SharedData;

public sealed class GenesisDbContext : DbContext
{
    public GenesisDbContext(DbContextOptions<GenesisDbContext> options) : base(options)
    {
    }

    public DbSet<GenesisContaPagar> ContasPagar => Set<GenesisContaPagar>();
    public DbSet<GenesisContaReceber> ContasReceber => Set<GenesisContaReceber>();
    public DbSet<GenesisFluxoCaixa> FluxoCaixa => Set<GenesisFluxoCaixa>();
    public DbSet<GenesisBoleto> Boletos => Set<GenesisBoleto>();
    public DbSet<GenesisFinanceReferencia> FinanceiroReferencias => Set<GenesisFinanceReferencia>();
    public DbSet<GenesisRhColaborador> RhColaboradores => Set<GenesisRhColaborador>();
    public DbSet<GenesisRhReferencia> RhReferencias => Set<GenesisRhReferencia>();
}