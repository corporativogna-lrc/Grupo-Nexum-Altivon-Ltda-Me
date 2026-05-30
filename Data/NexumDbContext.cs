using Microsoft.EntityFrameworkCore;
using NexumAltivon.ERP.Models;

namespace NexumAltivon.ERP.Data
{
    /// <summary>
    /// DbContext do ERP GenesisGest.Net — extensão do banco nexum_altivon
    /// Este contexto herda/compartilha as tabelas da API de e-commerce
    /// </summary>
    public class NexumDbContext : DbContext
    {
        public NexumDbContext(DbContextOptions<NexumDbContext> options) : base(options) { }

        // ==================== TABELAS E-COMMERCE (herdadas/compartilhadas) ====================
        public DbSet<Cliente> Clientes { get; set; } = null!;
        public DbSet<Produto> Produtos { get; set; } = null!;
        public DbSet<Pedido> Pedidos { get; set; } = null!;
        public DbSet<Loja> Lojas { get; set; } = null!;

        // ==================== FINANCEIRO ====================
        public DbSet<ContaPagar> ContasPagar { get; set; } = null!;
        public DbSet<ContaReceber> ContasReceber { get; set; } = null!;
        public DbSet<CentroCusto> CentrosCusto { get; set; } = null!;
        public DbSet<FluxoCaixa> FluxoCaixa { get; set; } = null!;
        public DbSet<ContaBancaria> ContasBancarias { get; set; } = null!;

        // ==================== FISCAL ====================
        public DbSet<NotaFiscal> NotasFiscais { get; set; } = null!;
        public DbSet<ItemNotaFiscal> ItensNotaFiscal { get; set; } = null!;
        public DbSet<ConfiguracaoImposto> ConfiguracoesImposto { get; set; } = null!;

        // ==================== ESTOQUE AVANÇADO ====================
        public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; } = null!;
        public DbSet<Inventario> Inventarios { get; set; } = null!;
        public DbSet<ItemInventario> ItensInventario { get; set; } = null!;
        public DbSet<Kardex> Kardex { get; set; } = null!;
        public DbSet<LocalEstoque> LocaisEstoque { get; set; } = null!;

        // ==================== CRM ====================
        public DbSet<LeadCRM> LeadsCRM { get; set; } = null!;
        public DbSet<InteracaoCRM> InteracoesCRM { get; set; } = null!;
        public DbSet<TarefaCRM> TarefasCRM { get; set; } = null!;

        // ==================== FORNECEDORES ====================
        public DbSet<Fornecedor> Fornecedores { get; set; } = null!;
        public DbSet<AvaliacaoFornecedor> AvaliacoesFornecedor { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Índices para performance
            modelBuilder.Entity<ContaPagar>()
                .HasIndex(c => new { c.Status, c.DataVencimento })
                .HasDatabaseName("IX_ContasPagar_Status_Vencimento");

            modelBuilder.Entity<ContaReceber>()
                .HasIndex(c => new { c.Status, c.DataVencimento })
                .HasDatabaseName("IX_ContasReceber_Status_Vencimento");

            modelBuilder.Entity<FluxoCaixa>()
                .HasIndex(f => f.Data)
                .HasDatabaseName("IX_FluxoCaixa_Data");

            modelBuilder.Entity<MovimentacaoEstoque>()
                .HasIndex(m => new { m.ProdutoId, m.DataMovimentacao })
                .HasDatabaseName("IX_MovEstoque_Produto_Data");

            modelBuilder.Entity<Kardex>()
                .HasIndex(k => new { k.ProdutoId, k.Data })
                .HasDatabaseName("IX_Kardex_Produto_Data");

            modelBuilder.Entity<LeadCRM>()
                .HasIndex(l => new { l.Status, l.CriadoEm })
                .HasDatabaseName("IX_Leads_Status_Criado");

            modelBuilder.Entity<NotaFiscal>()
                .HasIndex(n => n.ChaveAcesso)
                .HasDatabaseName("IX_NF_ChaveAcesso");

            // Seed de centros de custo
            modelBuilder.Entity<CentroCusto>().HasData(
                new CentroCusto { Id = 1, Codigo = "1.1.01", Nome = "Vendas — Grann-Tur", Tipo = "Analitico" },
                new CentroCusto { Id = 2, Codigo = "1.1.02", Nome = "Vendas — Chronos", Tipo = "Analitico" },
                new CentroCusto { Id = 3, Codigo = "1.1.03", Nome = "Vendas — Moda Mim", Tipo = "Analitico" },
                new CentroCusto { Id = 4, Codigo = "1.1.04", Nome = "Vendas — Geração Top+", Tipo = "Analitico" },
                new CentroCusto { Id = 5, Codigo = "1.1.05", Nome = "Vendas — Estruturaline", Tipo = "Analitico" },
                new CentroCusto { Id = 6, Codigo = "1.1.06", Nome = "Vendas — Gran-fest", Tipo = "Analitico" },
                new CentroCusto { Id = 7, Codigo = "2.1.01", Nome = "Marketing Digital", Tipo = "Analitico" },
                new CentroCusto { Id = 8, Codigo = "2.1.02", Nome = "Logística e Transporte", Tipo = "Analitico" },
                new CentroCusto { Id = 9, Codigo = "2.1.03", Nome = "Tecnologia e Infraestrutura", Tipo = "Analitico" },
                new CentroCusto { Id = 10, Codigo = "2.1.04", Nome = "Administrativo", Tipo = "Analitico" },
                new CentroCusto { Id = 11, Codigo = "3.1.01", Nome = "Compras e Fornecedores", Tipo = "Analitico" },
                new CentroCusto { Id = 12, Codigo = "3.1.02", Nome = "Dropshipping", Tipo = "Analitico" }
            );

            // Seed de contas bancárias
            modelBuilder.Entity<ContaBancaria>().HasData(
                new ContaBancaria { Id = 1, Nome = "Conta Principal", Banco = "Itaú", TipoConta = "Corrente", SaldoInicial = 0, SaldoAtual = 0 },
                new ContaBancaria { Id = 2, Nome = "Conta Secundária", Banco = "Bradesco", TipoConta = "Corrente", SaldoInicial = 0, SaldoAtual = 0 },
                new ContaBancaria { Id = 3, Nome = "Reserva", Banco = "Nubank", TipoConta = "Investimento", SaldoInicial = 0, SaldoAtual = 0 }
            );
        }
    }
}
