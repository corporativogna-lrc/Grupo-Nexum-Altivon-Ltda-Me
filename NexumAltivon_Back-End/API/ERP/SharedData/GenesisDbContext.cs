/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

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
