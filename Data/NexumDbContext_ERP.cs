using Microsoft.EntityFrameworkCore;
using NexumAltivon.ERP.Models;

namespace NexumAltivon.ERP.Data
{
    /// <summary>
    /// Extensão do DbContext com as entidades do ERP/CRM GenesisGest.Net
    /// Adicione estas DbSets ao seu NexumDbContext principal da Fase 1
    /// </summary>
    public partial class NexumDbContext : DbContext
    {
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

        protected void OnModelCreatingERP(ModelBuilder modelBuilder)
        {
            // Índices para performance financeira
            modelBuilder.Entity<ContaPagar>()
                .HasIndex(c => new { c.Status, c.DataVencimento })
                .HasDatabaseName("IX_ContasPagar_Status_Vencimento");

            modelBuilder.Entity<ContaReceber>()
                .HasIndex(c => new { c.Status, c.DataVencimento })
                .HasDatabaseName("IX_ContasReceber_Status_Vencimento");

            modelBuilder.Entity<FluxoCaixa>()
                .HasIndex(f => new { f.Data, f.Tipo })
                .HasDatabaseName("IX_FluxoCaixa_Data_Tipo");

            // Índices CRM
            modelBuilder.Entity<LeadCRM>()
                .HasIndex(l => new { l.Status, l.CriadoEm })
                .HasDatabaseName("IX_Leads_Status_CriadoEm");

            modelBuilder.Entity<InteracaoCRM>()
                .HasIndex(i => i.LeadId)
                .HasDatabaseName("IX_Interacoes_LeadId");

            // Índices Estoque
            modelBuilder.Entity<MovimentacaoEstoque>()
                .HasIndex(m => new { m.ProdutoId, m.DataMovimentacao })
                .HasDatabaseName("IX_Movimentacoes_Produto_Data");

            modelBuilder.Entity<Kardex>()
                .HasIndex(k => new { k.ProdutoId, k.Data })
                .HasDatabaseName("IX_Kardex_Produto_Data");

            // Relacionamentos
            modelBuilder.Entity<ItemInventario>()
                .HasOne(i => i.Inventario)
                .WithMany()
                .HasForeignKey(i => i.InventarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ItemNotaFiscal>()
                .HasOne(i => i.NotaFiscal)
                .WithMany()
                .HasForeignKey(i => i.NotaFiscalId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AvaliacaoFornecedor>()
                .HasOne(a => a.Fornecedor)
                .WithMany()
                .HasForeignKey(a => a.FornecedorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
