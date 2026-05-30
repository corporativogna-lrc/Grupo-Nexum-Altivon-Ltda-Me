using Microsoft.EntityFrameworkCore;
using NexumAltivon_ERP.Models.Financeiro;
using NexumAltivon_ERP.Models.Fiscal;
using NexumAltivon_ERP.Models.Estoque;
using NexumAltivon_ERP.Models.CRM;

namespace NexumAltivon_ERP.Data
{
    public class GenesisDbContext : DbContext
    {
        public GenesisDbContext(DbContextOptions<GenesisDbContext> options) : base(options) { }

        // === FINANCEIRO ===
        public DbSet<ContaPagar> ContasPagar { get; set; }
        public DbSet<ContaReceber> ContasReceber { get; set; }
        public DbSet<FluxoCaixa> FluxosCaixa { get; set; }
        public DbSet<Banco> Bancos { get; set; }
        public DbSet<ContaBancaria> ContasBancarias { get; set; }
        public DbSet<MovimentacaoBancaria> MovimentacoesBancarias { get; set; }
        public DbSet<CentroCusto> CentrosCusto { get; set; }
        public DbSet<PlanoContas> PlanosContas { get; set; }
        public DbSet<ConciliacaoBancaria> ConciliacoesBancarias { get; set; }
        public DbSet<DRE> DREs { get; set; }

        // === FISCAL ===
        public DbSet<NFe> NFes { get; set; }
        public DbSet<NFCe> NFCes { get; set; }
        public DbSet<ItemNFe> ItensNFe { get; set; }
        public DbSet<ConfiguracaoFiscal> ConfiguracoesFiscais { get; set; }
        public DbSet<SPED> SPEDs { get; set; }
        public DbSet<Sintegra> Sintegras { get; set; }
        public DbSet<Imposto> Impostos { get; set; }
        public DbSet<CFOP> CFOPs { get; set; }

        // === ESTOQUE AVANÇADO ===
        public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }
        public DbSet<ItemInventario> ItensInventario { get; set; }
        public DbSet<Kardex> Kardex { get; set; }
        public DbSet<LocalEstoque> LocaisEstoque { get; set; }
        public DbSet<TransferenciaEstoque> TransferenciasEstoque { get; set; }
        public DbSet<ProdutoFornecedor> ProdutosFornecedores { get; set; }
        public DbSet<AlertaEstoque> AlertasEstoque { get; set; }

        // === CRM ===
        public DbSet<Pipeline> Pipelines { get; set; }
        public DbSet<Oportunidade> Oportunidades { get; set; }
        public DbSet<Atividade> Atividades { get; set; }
        public DbSet<Campanha> Campanhas { get; set; }
        public DbSet<LeadCRM> LeadsCRM { get; set; }
        public DbSet<SegmentoCliente> SegmentosClientes { get; set; }
        public DbSet<TicketSuporte> TicketsSuporte { get; set; }
        public DbSet<InteracaoTicket> InteracoesTickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Centros de Custo
            modelBuilder.Entity<CentroCusto>().HasData(
                new CentroCusto { Id = 1, Codigo = "1000", Nome = "Vendas", Tipo = "Receita", Ativo = true },
                new CentroCusto { Id = 2, Codigo = "2000", Nome = "Marketing", Tipo = "Despesa", Ativo = true },
                new CentroCusto { Id = 3, Codigo = "3000", Nome = "Operacional", Tipo = "Despesa", Ativo = true },
                new CentroCusto { Id = 4, Codigo = "4000", Nome = "Administrativo", Tipo = "Despesa", Ativo = true },
                new CentroCusto { Id = 5, Codigo = "5000", Nome = "Logística", Tipo = "Despesa", Ativo = true }
            );

            // Seed Plano de Contas
            modelBuilder.Entity<PlanoContas>().HasData(
                new PlanoContas { Id = 1, Codigo = "1.1.1", Nome = "Caixa", Tipo = "Ativo", Natureza = "Debito", Ativo = true },
                new PlanoContas { Id = 2, Codigo = "1.1.2", Nome = "Bancos", Tipo = "Ativo", Natureza = "Debito", Ativo = true },
                new PlanoContas { Id = 3, Codigo = "1.2.1", Nome = "Clientes", Tipo = "Ativo", Natureza = "Debito", Ativo = true },
                new PlanoContas { Id = 4, Codigo = "2.1.1", Nome = "Fornecedores", Tipo = "Passivo", Natureza = "Credito", Ativo = true },
                new PlanoContas { Id = 5, Codigo = "3.1.1", Nome = "Capital Social", Tipo = "Patrimonio", Natureza = "Credito", Ativo = true },
                new PlanoContas { Id = 6, Codigo = "4.1.1", Nome = "Receita de Vendas", Tipo = "Receita", Natureza = "Credito", Ativo = true },
                new PlanoContas { Id = 7, Codigo = "5.1.1", Nome = "CMV", Tipo = "Custo", Natureza = "Debito", Ativo = true },
                new PlanoContas { Id = 8, Codigo = "5.2.1", Nome = "Despesas Administrativas", Tipo = "Despesa", Natureza = "Debito", Ativo = true }
            );

            // Seed CFOPs principais
            modelBuilder.Entity<CFOP>().HasData(
                new CFOP { Id = 1, Codigo = "5101", Descricao = "Venda de producao do estabelecimento", Tipo = "Saida", Aplicacao = "Venda" },
                new CFOP { Id = 2, Codigo = "5102", Descricao = "Venda de mercadoria adquirida ou recebida de terceiros", Tipo = "Saida", Aplicacao = "Venda" },
                new CFOP { Id = 3, Codigo = "6101", Descricao = "Venda de producao do estabelecimento", Tipo = "Saida", Aplicacao = "Venda" },
                new CFOP { Id = 4, Codigo = "1102", Descricao = "Entrada de mercadoria com compra para industrializacao", Tipo = "Entrada", Aplicacao = "Compra" },
                new CFOP { Id = 5, Codigo = "2102", Descricao = "Entrada de mercadoria com compra para comercializacao", Tipo = "Entrada", Aplicacao = "Compra" }
            );

            // Seed Locais de Estoque
            modelBuilder.Entity<LocalEstoque>().HasData(
                new LocalEstoque { Id = 1, Codigo = "CD-01", Nome = "Centro de Distribuicao Principal", Tipo = "Fisico", Endereco = "Av. Principal, 1000", Ativo = true },
                new LocalEstoque { Id = 2, Codigo = "LOJA-01", Nome = "Loja Fisica Grann-Tur", Tipo = "Fisico", Endereco = "Rua das Viagens, 200", Ativo = true },
                new LocalEstoque { Id = 3, Codigo = "LOJA-02", Nome = "Loja Fisica Chronos", Tipo = "Fisico", Endereco = "Rua do Tempo, 300", Ativo = true },
                new LocalEstoque { Id = 4, Codigo = "VIRTUAL", Nome = "Estoque Virtual Dropshipping", Tipo = "Virtual", Endereco = "N/A", Ativo = true }
            );

            // Seed Pipelines CRM
            modelBuilder.Entity<Pipeline>().HasData(
                new Pipeline { Id = 1, Nome = "Vendas E-commerce", Ordem = 1, Cor = "#C9A227", Ativo = true },
                new Pipeline { Id = 2, Nome = "Parcerias B2B", Ordem = 2, Cor = "#1E3A5F", Ativo = true },
                new Pipeline { Id = 3, Nome = "Fornecedores", Ordem = 3, Cor = "#2E5A8F", Ativo = true }
            );

            // Seed Segmentos
            modelBuilder.Entity<SegmentoCliente>().HasData(
                new SegmentoCliente { Id = 1, Nome = "VIP", Descricao = "Clientes de alto ticket", Cor = "#C9A227", Prioridade = 1 },
                new SegmentoCliente { Id = 2, Nome = "Recorrente", Descricao = "Compram regularmente", Cor = "#1E3A5F", Prioridade = 2 },
                new SegmentoCliente { Id = 3, Nome = "Novo", Descricao = "Primeira compra", Cor = "#2E5A8F", Prioridade = 3 },
                new SegmentoCliente { Id = 4, Nome = "Inativo", Descricao = "Sem compra > 90 dias", Cor = "#666666", Prioridade = 4 }
            );

            // Configuracoes de precisao decimal
            modelBuilder.Entity<ContaPagar>().Property(c => c.Valor).HasPrecision(18, 2);
            modelBuilder.Entity<ContaReceber>().Property(c => c.Valor).HasPrecision(18, 2);
            modelBuilder.Entity<FluxoCaixa>().Property(f => f.Valor).HasPrecision(18, 2);
            modelBuilder.Entity<MovimentacaoBancaria>().Property(m => m.Valor).HasPrecision(18, 2);
            modelBuilder.Entity<MovimentacaoEstoque>().Property(m => m.Quantidade).HasPrecision(18, 3);
            modelBuilder.Entity<Kardex>().Property(k => k.Quantidade).HasPrecision(18, 3);
            modelBuilder.Entity<Kardex>().Property(k => k.ValorUnitario).HasPrecision(18, 2);
            modelBuilder.Entity<Kardex>().Property(k => k.ValorTotal).HasPrecision(18, 2);
            modelBuilder.Entity<ItemNFe>().Property(i => i.ValorUnitario).HasPrecision(18, 2);
            modelBuilder.Entity<ItemNFe>().Property(i => i.ValorTotal).HasPrecision(18, 2);
            modelBuilder.Entity<ItemNFe>().Property(i => i.AliquotaICMS).HasPrecision(5, 2);
            modelBuilder.Entity<ItemNFe>().Property(i => i.AliquotaIPI).HasPrecision(5, 2);
            modelBuilder.Entity<Oportunidade>().Property(o => o.ValorEstimado).HasPrecision(18, 2);

            // Indices
            modelBuilder.Entity<ContaPagar>().HasIndex(c => new { c.DataVencimento, c.Status });
            modelBuilder.Entity<ContaReceber>().HasIndex(c => new { c.DataVencimento, c.Status });
            modelBuilder.Entity<MovimentacaoEstoque>().HasIndex(m => new { m.ProdutoId, m.DataMovimentacao });
            modelBuilder.Entity<Kardex>().HasIndex(k => new { k.ProdutoId, k.Data });
            modelBuilder.Entity<NFe>().HasIndex(n => n.Numero);
            modelBuilder.Entity<Oportunidade>().HasIndex(o => new { o.PipelineId, o.Etapa });
        }
    }
}
