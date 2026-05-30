using NexumAltivon.API.Models;
using Microsoft.EntityFrameworkCore;

namespace NexumAltivon.API.Data;

public class NexumDbContext : DbContext
{
    public NexumDbContext(DbContextOptions<NexumDbContext> options) : base(options) { }

    // DbSets
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Loja> Lojas { get; set; } = null!;
    public DbSet<Categoria> Categorias { get; set; } = null!;
    public DbSet<Produto> Produtos { get; set; } = null!;
    public DbSet<Fornecedor> Fornecedores { get; set; } = null!;
    public DbSet<Cliente> Clientes { get; set; } = null!;
    public DbSet<Endereco> Enderecos { get; set; } = null!;
    public DbSet<Pedido> Pedidos { get; set; } = null!;
    public DbSet<PedidoItem> PedidoItens { get; set; } = null!;
    public DbSet<Carrinho> Carrinhos { get; set; } = null!;
    public DbSet<Cupom> Cupons { get; set; } = null!;
    public DbSet<Pagamento> Pagamentos { get; set; } = null!;
    public DbSet<Transportadora> Transportadoras { get; set; } = null!;
    public DbSet<Envio> Envios { get; set; } = null!;
    public DbSet<CrmLead> CrmLeads { get; set; } = null!;
    public DbSet<CrmAtendimento> CrmAtendimentos { get; set; } = null!;
    public DbSet<Financeiro> Financeiros { get; set; } = null!;
    public DbSet<Fiscal> Fiscais { get; set; } = null!;
    public DbSet<Notificacao> Notificacoes { get; set; } = null!;
    public DbSet<LogAuditoria> LogsAuditoria { get; set; } = null!;
    public DbSet<ConfiguracaoSistema> ConfiguracoesSistema { get; set; } = null!;
    public DbSet<Marketplace> Marketplaces { get; set; } = null!;
    public DbSet<DropshippingConfig> DropshippingConfigs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurações de índices e constraints adicionais
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Cliente>()
            .HasIndex(c => c.Email)
            .IsUnique();

        modelBuilder.Entity<Cliente>()
            .HasIndex(c => c.CpfCnpj)
            .IsUnique();

        modelBuilder.Entity<Produto>()
            .HasIndex(p => p.Sku)
            .IsUnique();

        modelBuilder.Entity<Fornecedor>()
            .HasIndex(f => f.Cnpj)
            .IsUnique();

        modelBuilder.Entity<Cupom>()
            .HasIndex(c => c.Codigo)
            .IsUnique();

        modelBuilder.Entity<Loja>()
            .HasIndex(l => l.Slug)
            .IsUnique();

        modelBuilder.Entity<Marketplace>()
            .HasIndex(m => m.Slug)
            .IsUnique();

        modelBuilder.Entity<DropshippingConfig>()
            .HasIndex(d => d.Slug)
            .IsUnique();

        modelBuilder.Entity<Transportadora>()
            .HasIndex(t => t.Slug)
            .IsUnique();

        // Relacionamentos
        modelBuilder.Entity<Pedido>()
            .HasOne(p => p.Cliente)
            .WithMany(c => c.Pedidos)
            .HasForeignKey(p => p.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PedidoItem>()
            .HasOne(pi => pi.Pedido)
            .WithMany(p => p.Itens)
            .HasForeignKey(pi => pi.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Carrinho>()
            .HasOne(c => c.Cliente)
            .WithMany(cl => cl.Carrinhos)
            .HasForeignKey(c => c.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Endereco>()
            .HasOne(e => e.Cliente)
            .WithMany(c => c.Enderecos)
            .HasForeignKey(e => e.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CrmAtendimento>()
            .HasOne(a => a.Lead)
            .WithMany(l => l.Atendimentos)
            .HasForeignKey(a => a.LeadId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configuração de precisão para decimais
        modelBuilder.Entity<Produto>()
            .Property(p => p.Preco)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Pedido>()
            .Property(p => p.Total)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Pagamento>()
            .Property(p => p.Valor)
            .HasPrecision(10, 2);

        // Seed inicial das 6 lojas
        modelBuilder.Entity<Loja>().HasData(
            new Loja { Id = 1, Nome = "Grann-Tur", Slug = "grann-tur", Segmento = "Viagens & Turismo", Descricao = "Mochilas, malas, acessórios de viagem", CorPrimaria = "#C9A227", CorSecundaria = "#1E3A5F", Dominio = "grann-tur.nexumaltivon.com", Ativa = true, OrdemExibicao = 1 },
            new Loja { Id = 2, Nome = "Chronos", Slug = "chronos", Segmento = "Relógios & Acessórios", Descricao = "Relógios que marcam estilo", CorPrimaria = "#C9A227", CorSecundaria = "#2E5A8F", Dominio = "chronos.nexumaltivon.com", Ativa = true, OrdemExibicao = 2 },
            new Loja { Id = 3, Nome = "Moda Mim", Slug = "moda-mim", Segmento = "Moda & Vestuário", Descricao = "Tendências que vestem a sua personalidade", CorPrimaria = "#C9A227", CorSecundaria = "#8B1E3F", Dominio = "moda-mim.nexumaltivon.com", Ativa = true, OrdemExibicao = 3 },
            new Loja { Id = 4, Nome = "Geração Top+", Slug = "geracao-top", Segmento = "Tecnologia & Gadgets", Descricao = "Tecnologia de ponta ao alcance de todos", CorPrimaria = "#C9A227", CorSecundaria = "#0F4C3A", Dominio = "geracao-top.nexumaltivon.com", Ativa = true, OrdemExibicao = 4 },
            new Loja { Id = 5, Nome = "Estruturaline", Slug = "estruturaline", Segmento = "Construção & Estruturas", Descricao = "Ferramentas e materiais de construção", CorPrimaria = "#C9A227", CorSecundaria = "#4A3728", Dominio = "estruturaline.nexumaltivon.com", Ativa = true, OrdemExibicao = 5 },
            new Loja { Id = 6, Nome = "Gran-fest-festas", Slug = "gran-fest", Segmento = "Festas & Eventos", Descricao = "Decorações e utensílios para festas", CorPrimaria = "#C9A227", CorSecundaria = "#6B2D5C", Dominio = "gran-fest.nexumaltivon.com", Ativa = true, OrdemExibicao = 6 }
        );

        // Seed inicial de transportadoras
        modelBuilder.Entity<Transportadora>().HasData(
            new Transportadora { Id = 1, Nome = "Correios", Slug = "correios", Tipo = TipoTransportadora.Correios, Ativa = true },
            new Transportadora { Id = 2, Nome = "Melhor Envio", Slug = "melhor-envio", Tipo = TipoTransportadora.Hub, Ativa = true },
            new Transportadora { Id = 3, Nome = "Jadlog", Slug = "jadlog", Tipo = TipoTransportadora.Transportadora, Ativa = true },
            new Transportadora { Id = 4, Nome = "Loggi", Slug = "loggi", Tipo = TipoTransportadora.Logistica, Ativa = true },
            new Transportadora { Id = 5, Nome = "Kangu", Slug = "kangu", Tipo = TipoTransportadora.Hub, Ativa = true }
        );

        // Seed inicial de marketplaces
        modelBuilder.Entity<Marketplace>().HasData(
            new Marketplace { Id = 1, Nome = "Mercado Livre", Slug = "mercado-livre", Tipo = TipoMarketplace.MercadoLivre, Ativo = false },
            new Marketplace { Id = 2, Nome = "Shopee", Slug = "shopee", Tipo = TipoMarketplace.Shopee, Ativo = false },
            new Marketplace { Id = 3, Nome = "Amazon", Slug = "amazon", Tipo = TipoMarketplace.Amazon, Ativo = false },
            new Marketplace { Id = 4, Nome = "Magalu", Slug = "magalu", Tipo = TipoMarketplace.Magalu, Ativo = false },
            new Marketplace { Id = 5, Nome = "Americanas", Slug = "americanas", Tipo = TipoMarketplace.B2W, Ativo = false },
            new Marketplace { Id = 6, Nome = "Via Varejo", Slug = "via-varejo", Tipo = TipoMarketplace.B2W, Ativo = false }
        );

        // Seed inicial de dropshipping
        modelBuilder.Entity<DropshippingConfig>().HasData(
            new DropshippingConfig { Id = 1, Nome = "AliExpress", Slug = "aliexpress", Tipo = TipoDropshipping.AliExpress, Ativo = false },
            new DropshippingConfig { Id = 2, Nome = "CJ Dropshipping", Slug = "cj-dropshipping", Tipo = TipoDropshipping.CJDropshipping, Ativo = false },
            new DropshippingConfig { Id = 3, Nome = "Dropi", Slug = "dropi", Tipo = TipoDropshipping.Dropi, Ativo = false },
            new DropshippingConfig { Id = 4, Nome = "Cartpanda HUB", Slug = "cartpanda", Tipo = TipoDropshipping.Cartpanda, Ativo = false },
            new DropshippingConfig { Id = 5, Nome = "Nuvemshop HUB", Slug = "nuvemshop", Tipo = TipoDropshipping.Nuvemshop, Ativo = false }
        );
    }
}
