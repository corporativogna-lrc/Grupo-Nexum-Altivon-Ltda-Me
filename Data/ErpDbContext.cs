using Microsoft.EntityFrameworkCore;
using NexumAltivon.ERP.Models;

namespace NexumAltivon.ERP.Data;

public class ErpDbContext : DbContext
{
    public ErpDbContext(DbContextOptions<ErpDbContext> options) : base(options) { }

    // === Tabelas Compartilhadas com E-Commerce (readonly / sync) ===
    public DbSet<Loja> Lojas { get; set; } = null!;
    public DbSet<Produto> Produtos { get; set; } = null!;
    public DbSet<Cliente> Clientes { get; set; } = null!;
    public DbSet<Pedido> Pedidos { get; set; } = null!;
    public DbSet<PedidoItem> PedidoItens { get; set; } = null!;
    public DbSet<Usuario> Usuarios { get; set; } = null!;

    // === Módulo Financeiro ===
    public DbSet<ContaPagar> ContasPagar { get; set; } = null!;
    public DbSet<ContaReceber> ContasReceber { get; set; } = null!;
    public DbSet<FluxoCaixa> FluxoCaixa { get; set; } = null!;
    public DbSet<CentroCusto> CentrosCusto { get; set; } = null!;
    public DbSet<PlanoConta> PlanosConta { get; set; } = null!;
    public DbSet<ConciliacaoBancaria> ConciliacoesBancarias { get; set; } = null!;
    public DbSet<DrePeriodo> DrePeriodos { get; set; } = null!;

    // === Módulo Fiscal ===
    public DbSet<NotaFiscal> NotasFiscais { get; set; } = null!;
    public DbSet<NotaFiscalItem> NotaFiscalItens { get; set; } = null!;
    public DbSet<ConfiguracaoFiscal> ConfiguracoesFiscais { get; set; } = null!;
    public DbSet<ManifestoDestinatario> ManifestosDestinatario { get; set; } = null!;

    // === Módulo Estoque Avançado ===
    public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; } = null!;
    public DbSet<Inventario> Inventarios { get; set; } = null!;
    public DbSet<InventarioItem> InventarioItens { get; set; } = null!;
    public DbSet<LocalEstoque> LocaisEstoque { get; set; } = null!;
    public DbSet<Fornecedor> Fornecedores { get; set; } = null!;
    public DbSet<Compra> Compras { get; set; } = null!;
    public DbSet<CompraItem> CompraItens { get; set; } = null!;

    // === Módulo CRM Avançado ===
    public DbSet<PipelineEtapa> PipelineEtapas { get; set; } = null!;
    public DbSet<Oportunidade> Oportunidades { get; set; } = null!;
    public DbSet<Atendimento> Atendimentos { get; set; } = null!;
    public DbSet<CampanhaMarketing> CampanhasMarketing { get; set; } = null!;
    public DbSet<LeadScore> LeadScores { get; set; } = null!;

    // === Módulo Relatórios & Auditoria ERP ===
    public DbSet<RelatorioGerado> RelatoriosGerados { get; set; } = null!;
    public DbSet<AuditoriaErp> AuditoriasErp { get; set; } = null!;
    public DbSet<ErpLog> ErpLogs { get; set; } = null!;

    // === Sincronização ===
    public DbSet<SyncQueue> SyncQueues { get; set; } = null!;
    public DbSet<SyncLog> SyncLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica configurações automaticamente deste assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ErpDbContext).Assembly);

        // Seed inicial ERP
        modelBuilder.Entity<CentroCusto>().HasData(
            new CentroCusto { Id = 1, Codigo = "CC-001", Nome = "Administrativo", Tipo = "Fixo", Ativo = true },
            new CentroCusto { Id = 2, Codigo = "CC-002", Nome = "Marketing", Tipo = "Variavel", Ativo = true },
            new CentroCusto { Id = 3, Codigo = "CC-003", Nome = "Logistica", Tipo = "Variavel", Ativo = true },
            new CentroCusto { Id = 4, Codigo = "CC-004", Nome = "TI", Tipo = "Fixo", Ativo = true }
        );

        modelBuilder.Entity<PlanoConta>().HasData(
            new PlanoConta { Id = 1, Codigo = "1.1.01", Nome = "Caixa", Tipo = "Ativo", Natureza = "Debito", Nivel = 1 },
            new PlanoConta { Id = 2, Codigo = "1.1.02", Nome = "Bancos", Tipo = "Ativo", Natureza = "Debito", Nivel = 1 },
            new PlanoConta { Id = 3, Codigo = "2.1.01", Nome = "Fornecedores", Tipo = "Passivo", Natureza = "Credito", Nivel = 1 },
            new PlanoConta { Id = 4, Codigo = "3.1.01", Nome = "Capital Social", Tipo = "Patrimonio", Natureza = "Credito", Nivel = 1 },
            new PlanoConta { Id = 5, Codigo = "4.1.01", Nome = "Vendas de Mercadorias", Tipo = "Receita", Natureza = "Credito", Nivel = 1 },
            new PlanoConta { Id = 6, Codigo = "5.1.01", Nome = "CMV", Tipo = "Custo", Natureza = "Debito", Nivel = 1 }
        );

        modelBuilder.Entity<LocalEstoque>().HasData(
            new LocalEstoque { Id = 1, Codigo = "MATRIZ-CD", Nome = "Centro de Distribuição Matriz", Tipo = "CD", Endereco = "Rua Principal, 1000", Cidade = "Bauru", UF = "SP", Ativo = true },
            new LocalEstoque { Id = 2, Codigo = "LOJA-01", Nome = "Depósito Loja 01", Tipo = "Loja", Endereco = "Av. Central, 500", Cidade = "Bauru", UF = "SP", Ativo = true }
        );

        modelBuilder.Entity<PipelineEtapa>().HasData(
            new PipelineEtapa { Id = 1, Nome = "Prospect", Ordem = 1, Cor = "#A0A0A0", Probabilidade = 10 },
            new PipelineEtapa { Id = 2, Nome = "Qualificado", Ordem = 2, Cor = "#C9A227", Probabilidade = 25 },
            new PipelineEtapa { Id = 3, Nome = "Proposta", Ordem = 3, Cor = "#1E3A5F", Probabilidade = 50 },
            new PipelineEtapa { Id = 4, Nome = "Negociacao", Ordem = 4, Cor = "#E67E22", Probabilidade = 75 },
            new PipelineEtapa { Id = 5, Nome = "Fechamento", Ordem = 5, Cor = "#27AE60", Probabilidade = 100 },
            new PipelineEtapa { Id = 6, Nome = "Perdido", Ordem = 6, Cor = "#E74C3C", Probabilidade = 0 }
        );
    }
}
