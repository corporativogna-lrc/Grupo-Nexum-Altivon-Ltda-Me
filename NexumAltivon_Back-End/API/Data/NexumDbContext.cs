/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using NexumAltivon.API.Models;
using NexumAltivon.API.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace NexumAltivon.API.Data;

public class NexumDbContext : DbContext
{
    private readonly ITenantContext? _tenantContext;

    public NexumDbContext(DbContextOptions<NexumDbContext> options, ITenantContext? tenantContext = null) : base(options)
    {
        _tenantContext = tenantContext;
    }

    public Guid CurrentTenantId => _tenantContext?.TenantId ?? TenantContext.DefaultTenantId;
    public Guid? CurrentUserId => _tenantContext?.UserId;

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
    public DbSet<CrmCampanha> CrmCampanhas { get; set; } = null!;
    public DbSet<CrmSegmento> CrmSegmentos { get; set; } = null!;
    public DbSet<Financeiro> Financeiros { get; set; } = null!;
    public DbSet<Fiscal> Fiscais { get; set; } = null!;
    public DbSet<Notificacao> Notificacoes { get; set; } = null!;
    public DbSet<LogAuditoria> LogsAuditoria { get; set; } = null!;
    public DbSet<ConfiguracaoSistema> ConfiguracoesSistema { get; set; } = null!;
    public DbSet<Marketplace> Marketplaces { get; set; } = null!;
    public DbSet<DropshippingConfig> DropshippingConfigs { get; set; } = null!;
    public DbSet<EmpresaGrupo> EmpresasGrupo { get; set; } = null!;
    public DbSet<SiteMidia> SiteMidias { get; set; } = null!;

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

        modelBuilder.Entity<Cliente>()
            .Property(cliente => cliente.Tipo)
            .HasConversion<string>();

        modelBuilder.Entity<Cliente>()
            .Property(cliente => cliente.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Produto>()
            .HasIndex(p => p.Sku)
            .IsUnique();

        modelBuilder.Entity<Produto>()
            .Property(produto => produto.TipoProduto)
            .HasConversion<string>();

        modelBuilder.Entity<Fornecedor>()
            .HasIndex(f => f.Cnpj)
            .IsUnique();

        modelBuilder.Entity<Fornecedor>()
            .Property(fornecedor => fornecedor.Status)
            .HasConversion<string>();

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

        modelBuilder.Entity<SiteMidia>()
            .Property(midia => midia.Tipo)
            .HasConversion<string>()
            .HasMaxLength(30);

        modelBuilder.Entity<SiteMidia>()
            .Property(midia => midia.RowVersion)
            .IsConcurrencyToken()
            .ValueGeneratedNever();

        modelBuilder.Entity<SiteMidia>()
            .HasIndex(midia => new { midia.TenantId, midia.CreatedAt });

        modelBuilder.Entity<SiteMidia>()
            .HasIndex(midia => new { midia.TenantId, midia.CaminhoRelativo })
            .IsUnique();

        modelBuilder.Entity<EmpresaGrupo>()
            .HasIndex(empresa => empresa.Cnpj)
            .IsUnique();

        modelBuilder.Entity<EmpresaGrupo>()
            .HasIndex(empresa => empresa.CodigoEmpresa)
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

        modelBuilder.Entity<CrmCampanha>()
            .HasOne(campanha => campanha.Segmento)
            .WithMany(segmento => segmento.Campanhas)
            .HasForeignKey(campanha => campanha.SegmentoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CrmCampanha>()
            .HasIndex(campanha => new { campanha.TenantId, campanha.Nome })
            .IsUnique();

        modelBuilder.Entity<CrmSegmento>()
            .HasIndex(segmento => new { segmento.TenantId, segmento.Nome })
            .IsUnique();

        modelBuilder.Entity<CrmCampanha>()
            .Property(campanha => campanha.RowVersion)
            .IsConcurrencyToken()
            .ValueGeneratedNever();

        modelBuilder.Entity<CrmSegmento>()
            .Property(segmento => segmento.RowVersion)
            .IsConcurrencyToken()
            .ValueGeneratedNever();

        modelBuilder.Entity<Usuario>()
            .Property(usuario => usuario.Perfil)
            .HasConversion<string>();

        modelBuilder.Entity<Cupom>()
            .Property(cupom => cupom.Tipo)
            .HasConversion<string>();

        modelBuilder.Entity<Endereco>()
            .Property(endereco => endereco.Tipo)
            .HasConversion<string>();

        modelBuilder.Entity<CrmAtendimento>()
            .Property(atendimento => atendimento.Tipo)
            .HasConversion<string>();

        modelBuilder.Entity<CrmAtendimento>()
            .Property(atendimento => atendimento.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Envio>()
            .Property(envio => envio.StatusEnvio)
            .HasConversion<string>();

        modelBuilder.Entity<Financeiro>()
            .Property(lancamento => lancamento.Tipo)
            .HasConversion<string>();

        modelBuilder.Entity<Financeiro>()
            .Property(lancamento => lancamento.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Fiscal>()
            .Property(fiscal => fiscal.StatusNfe)
            .HasConversion<string>();

        modelBuilder.Entity<LogAuditoria>()
            .Property(log => log.Acao)
            .HasConversion<string>();

        modelBuilder.Entity<LogAuditoria>()
            .Property(log => log.UsuarioTipo)
            .HasConversion<string>();

        modelBuilder.Entity<Notificacao>()
            .Property(notificacao => notificacao.Tipo)
            .HasConversion<string>();

        modelBuilder.Entity<Notificacao>()
            .Property(notificacao => notificacao.DestinatarioTipo)
            .HasConversion<string>();

        modelBuilder.Entity<ConfiguracaoSistema>()
            .Property(configuracao => configuracao.Tipo)
            .HasConversion<string>();

        modelBuilder.Entity<CrmLead>()
            .Property(lead => lead.Origem)
            .HasConversion<string>();

        modelBuilder.Entity<CrmLead>()
            .Property(lead => lead.Tipo)
            .HasConversion<string>();

        modelBuilder.Entity<CrmLead>()
            .Property(lead => lead.Status)
            .HasConversion<string>();

        modelBuilder.Entity<CrmLead>()
            .Property(lead => lead.Prioridade)
            .HasConversion<string>();

        modelBuilder.Entity<CrmCampanha>()
            .Property(campanha => campanha.Tipo)
            .HasConversion<string>();

        modelBuilder.Entity<CrmCampanha>()
            .Property(campanha => campanha.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Pedido>()
            .Property(pedido => pedido.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Pedido>()
            .Property(pedido => pedido.StatusPagamento)
            .HasConversion<string>();

        modelBuilder.Entity<Pedido>()
            .Property(pedido => pedido.Origem)
            .HasConversion<string>();

        modelBuilder.Entity<PedidoItem>()
            .Property(item => item.TipoFulfillment)
            .HasConversion<string>();

        modelBuilder.Entity<PedidoItem>()
            .Property(item => item.StatusItem)
            .HasConversion<string>();

        modelBuilder.Entity<Pagamento>()
            .Property(pagamento => pagamento.Metodo)
            .HasConversion<string>();

        modelBuilder.Entity<Pagamento>()
            .Property(pagamento => pagamento.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Transportadora>()
            .Property(transportadora => transportadora.Tipo)
            .HasConversion<string>();

        modelBuilder.Entity<Marketplace>()
            .Property(marketplace => marketplace.Tipo)
            .HasConversion<string>();

        modelBuilder.Entity<DropshippingConfig>()
            .Property(config => config.Tipo)
            .HasConversion<string>();

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

        modelBuilder.Entity<EmpresaGrupo>()
            .Property(empresa => empresa.AliquotaIcmsInterna)
            .HasPrecision(10, 4);

        modelBuilder.Entity<EmpresaGrupo>()
            .Property(empresa => empresa.AliquotaIcmsInterestadual)
            .HasPrecision(10, 4);

        modelBuilder.Entity<EmpresaGrupo>()
            .Property(empresa => empresa.AliquotaPis)
            .HasPrecision(10, 4);

        modelBuilder.Entity<EmpresaGrupo>()
            .Property(empresa => empresa.AliquotaCofins)
            .HasPrecision(10, 4);

        modelBuilder.Entity<EmpresaGrupo>()
            .Property(empresa => empresa.AliquotaIss)
            .HasPrecision(10, 4);

        modelBuilder.Entity<EmpresaGrupo>()
            .Property(empresa => empresa.AliquotaIpi)
            .HasPrecision(10, 4);

        modelBuilder.Entity<EmpresaGrupo>()
            .Property(empresa => empresa.CargaTributariaPercentual)
            .HasPrecision(10, 4);

        modelBuilder.Entity<EmpresaGrupo>()
            .Property(empresa => empresa.CustoOperacionalPercentual)
            .HasPrecision(10, 4);

        modelBuilder.Entity<EmpresaGrupo>()
            .Property(empresa => empresa.MargemMinimaPercentual)
            .HasPrecision(10, 4);

        var seedCreatedAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        // Seed alinhado ao banco de produção: Chronos já é a loja 1 e possui produtos vinculados.
        modelBuilder.Entity<Loja>().HasData(
            new { Id = 1, Nome = "Chronos", Slug = "chronos", Segmento = "Relógios & Acessórios", Descricao = "Relógios que marcam estilo", CorPrimaria = "#C9A227", CorSecundaria = "#2E5A8F", Dominio = "chronos.nexumaltivon.com", Ativa = true, OrdemExibicao = 1, CreatedAt = seedCreatedAt, UpdatedAt = seedCreatedAt, TenantId = TenantContext.DefaultTenantId, RowVersion = new byte[] { 1 }, IsDeleted = false },
            new { Id = 2, Nome = "Grann-Tur", Slug = "grann-tur", Segmento = "Viagens & Turismo", Descricao = "Mochilas, malas, acessórios de viagem", CorPrimaria = "#C9A227", CorSecundaria = "#1E3A5F", Dominio = "grann-tur.nexumaltivon.com", Ativa = true, OrdemExibicao = 2, CreatedAt = seedCreatedAt, UpdatedAt = seedCreatedAt, TenantId = TenantContext.DefaultTenantId, RowVersion = new byte[] { 1 }, IsDeleted = false },
            new { Id = 3, Nome = "Moda Mim", Slug = "moda-mim", Segmento = "Moda & Vestuário", Descricao = "Tendências que vestem a sua personalidade", CorPrimaria = "#C9A227", CorSecundaria = "#8B1E3F", Dominio = "moda-mim.nexumaltivon.com", Ativa = true, OrdemExibicao = 3, CreatedAt = seedCreatedAt, UpdatedAt = seedCreatedAt, TenantId = TenantContext.DefaultTenantId, RowVersion = new byte[] { 1 }, IsDeleted = false },
            new { Id = 4, Nome = "Geração Top+", Slug = "geracao-top", Segmento = "Tecnologia & Gadgets", Descricao = "Tecnologia de ponta ao alcance de todos", CorPrimaria = "#C9A227", CorSecundaria = "#0F4C3A", Dominio = "geracao-top.nexumaltivon.com", Ativa = true, OrdemExibicao = 4, CreatedAt = seedCreatedAt, UpdatedAt = seedCreatedAt, TenantId = TenantContext.DefaultTenantId, RowVersion = new byte[] { 1 }, IsDeleted = false },
            new { Id = 5, Nome = "Estruturaline", Slug = "estruturaline", Segmento = "Construção & Estruturas", Descricao = "Ferramentas e materiais de construção", CorPrimaria = "#C9A227", CorSecundaria = "#4A3728", Dominio = "estruturaline.nexumaltivon.com", Ativa = true, OrdemExibicao = 5, CreatedAt = seedCreatedAt, UpdatedAt = seedCreatedAt, TenantId = TenantContext.DefaultTenantId, RowVersion = new byte[] { 1 }, IsDeleted = false },
            new { Id = 6, Nome = "Gran-fest-festas", Slug = "gran-fest", Segmento = "Festas & Eventos", Descricao = "Decorações e utensílios para festas", CorPrimaria = "#C9A227", CorSecundaria = "#6B2D5C", Dominio = "gran-fest.nexumaltivon.com", Ativa = true, OrdemExibicao = 6, CreatedAt = seedCreatedAt, UpdatedAt = seedCreatedAt, TenantId = TenantContext.DefaultTenantId, RowVersion = new byte[] { 1 }, IsDeleted = false }
        );

        // Seed inicial de transportadoras
        modelBuilder.Entity<Transportadora>().HasData(
            new { Id = 1, Nome = "Correios", Slug = "correios", Tipo = TipoTransportadora.Correios, ApiSandbox = true, Ativa = true, CreatedAt = seedCreatedAt, UpdatedAt = seedCreatedAt, TenantId = TenantContext.DefaultTenantId, RowVersion = new byte[] { 1 }, IsDeleted = false },
            new { Id = 2, Nome = "Melhor Envio", Slug = "melhor-envio", Tipo = TipoTransportadora.Hub, ApiSandbox = true, Ativa = true, CreatedAt = seedCreatedAt, UpdatedAt = seedCreatedAt, TenantId = TenantContext.DefaultTenantId, RowVersion = new byte[] { 1 }, IsDeleted = false },
            new { Id = 3, Nome = "Jadlog", Slug = "jadlog", Tipo = TipoTransportadora.Transportadora, ApiSandbox = true, Ativa = true, CreatedAt = seedCreatedAt, UpdatedAt = seedCreatedAt, TenantId = TenantContext.DefaultTenantId, RowVersion = new byte[] { 1 }, IsDeleted = false },
            new { Id = 4, Nome = "Loggi", Slug = "loggi", Tipo = TipoTransportadora.Logistica, ApiSandbox = true, Ativa = true, CreatedAt = seedCreatedAt, UpdatedAt = seedCreatedAt, TenantId = TenantContext.DefaultTenantId, RowVersion = new byte[] { 1 }, IsDeleted = false },
            new { Id = 5, Nome = "Kangu", Slug = "kangu", Tipo = TipoTransportadora.Hub, ApiSandbox = true, Ativa = true, CreatedAt = seedCreatedAt, UpdatedAt = seedCreatedAt, TenantId = TenantContext.DefaultTenantId, RowVersion = new byte[] { 1 }, IsDeleted = false }
        );

        // Seed inicial de marketplaces
        modelBuilder.Entity<Marketplace>().HasData(
            new { Id = 1, Nome = "Mercado Livre", Slug = "mercado-livre", Tipo = TipoMarketplace.MercadoLivre, Sandbox = true, Ativo = false, CreatedAt = seedCreatedAt, UpdatedAt = seedCreatedAt, TenantId = TenantContext.DefaultTenantId, RowVersion = new byte[] { 1 }, IsDeleted = false },
            new { Id = 2, Nome = "Shopee", Slug = "shopee", Tipo = TipoMarketplace.Shopee, Sandbox = true, Ativo = false, CreatedAt = seedCreatedAt, UpdatedAt = seedCreatedAt, TenantId = TenantContext.DefaultTenantId, RowVersion = new byte[] { 1 }, IsDeleted = false },
            new { Id = 3, Nome = "Amazon", Slug = "amazon", Tipo = TipoMarketplace.Amazon, Sandbox = true, Ativo = false, CreatedAt = seedCreatedAt, UpdatedAt = seedCreatedAt, TenantId = TenantContext.DefaultTenantId, RowVersion = new byte[] { 1 }, IsDeleted = false },
            new { Id = 4, Nome = "Magalu", Slug = "magalu", Tipo = TipoMarketplace.Magalu, Sandbox = true, Ativo = false, CreatedAt = seedCreatedAt, UpdatedAt = seedCreatedAt, TenantId = TenantContext.DefaultTenantId, RowVersion = new byte[] { 1 }, IsDeleted = false },
            new { Id = 5, Nome = "Americanas", Slug = "americanas", Tipo = TipoMarketplace.B2W, Sandbox = true, Ativo = false, CreatedAt = seedCreatedAt, UpdatedAt = seedCreatedAt, TenantId = TenantContext.DefaultTenantId, RowVersion = new byte[] { 1 }, IsDeleted = false },
            new { Id = 6, Nome = "Via Varejo", Slug = "via-varejo", Tipo = TipoMarketplace.B2W, Sandbox = true, Ativo = false, CreatedAt = seedCreatedAt, UpdatedAt = seedCreatedAt, TenantId = TenantContext.DefaultTenantId, RowVersion = new byte[] { 1 }, IsDeleted = false }
        );

        // Seed inicial de dropshipping
        modelBuilder.Entity<DropshippingConfig>().HasData(
            new { Id = 1, Nome = "AliExpress", Slug = "aliexpress", Tipo = TipoDropshipping.AliExpress, Ativo = false, CreatedAt = seedCreatedAt, UpdatedAt = seedCreatedAt, TenantId = TenantContext.DefaultTenantId, RowVersion = new byte[] { 1 }, IsDeleted = false },
            new { Id = 2, Nome = "CJ Dropshipping", Slug = "cj-dropshipping", Tipo = TipoDropshipping.CJDropshipping, Ativo = false, CreatedAt = seedCreatedAt, UpdatedAt = seedCreatedAt, TenantId = TenantContext.DefaultTenantId, RowVersion = new byte[] { 1 }, IsDeleted = false },
            new { Id = 3, Nome = "Dropi", Slug = "dropi", Tipo = TipoDropshipping.Dropi, Ativo = false, CreatedAt = seedCreatedAt, UpdatedAt = seedCreatedAt, TenantId = TenantContext.DefaultTenantId, RowVersion = new byte[] { 1 }, IsDeleted = false },
            new { Id = 4, Nome = "Cartpanda HUB", Slug = "cartpanda", Tipo = TipoDropshipping.Cartpanda, Ativo = false, CreatedAt = seedCreatedAt, UpdatedAt = seedCreatedAt, TenantId = TenantContext.DefaultTenantId, RowVersion = new byte[] { 1 }, IsDeleted = false },
            new { Id = 5, Nome = "Nuvemshop HUB", Slug = "nuvemshop", Tipo = TipoDropshipping.Nuvemshop, Ativo = false, CreatedAt = seedCreatedAt, UpdatedAt = seedCreatedAt, TenantId = TenantContext.DefaultTenantId, RowVersion = new byte[] { 1 }, IsDeleted = false }
        );

        ConfigureAuditShadowProperties(modelBuilder);
    }

    public override int SaveChanges()
    {
        ApplyAuditShadowValues();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditShadowValues();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditShadowValues()
    {
        var utcNow = DateTime.UtcNow;
        var tenantId = CurrentTenantId;
        var userId = CurrentUserId;

        foreach (var entry in ChangeTracker.Entries().Where(item => item.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            if (entry.Metadata.ClrType == typeof(Sys_AuditableEntity))
            {
                continue;
            }

            var rowVersion = Guid.NewGuid().ToByteArray();

            if (entry.State == EntityState.Added)
            {
                SetPropertyValue(entry, "TenantId", tenantId);
                SetPropertyValue(entry, "CreatedAt", utcNow);
                SetPropertyValue(entry, "CreatedByUserId", userId);
                SetPropertyValue(entry, "RowVersion", rowVersion);
            }

            if (entry.State == EntityState.Modified)
            {
                SetPropertyValue(entry, "UpdatedAt", utcNow);
                SetPropertyValue(entry, "UpdatedByUserId", userId);
                SetPropertyValue(entry, "RowVersion", rowVersion);
            }

            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                SetPropertyValue(entry, "IsDeleted", true);
                SetPropertyValue(entry, "DeletedAt", utcNow);
                SetPropertyValue(entry, "UpdatedAt", utcNow);
                SetPropertyValue(entry, "UpdatedByUserId", userId);
                SetPropertyValue(entry, "RowVersion", rowVersion);
            }
        }
    }

    private static void SetPropertyValue(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, string propertyName, object? value)
    {
        var property = entry.Metadata.FindProperty(propertyName);
        if (property is null)
        {
            return;
        }

        if (!property.IsShadowProperty())
        {
            var clrProperty = entry.Entity.GetType().GetProperty(propertyName);
            if (clrProperty is not null && clrProperty.CanWrite)
            {
                clrProperty.SetValue(entry.Entity, value);
            }

            return;
        }

        entry.Property(propertyName).CurrentValue = value;
    }

    private void ConfigureAuditShadowProperties(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (clrType is null || clrType == typeof(Sys_AuditableEntity) || entityType.IsOwned())
            {
                continue;
            }

            var entity = modelBuilder.Entity(clrType);
            entity.Property<Guid>("TenantId")
                .HasColumnName("tenant_id")
                .HasDefaultValue(TenantContext.DefaultTenantId);
            entity.Property<byte[]>("RowVersion")
                .HasColumnName("row_version")
                .HasColumnType("blob")
                .IsConcurrencyToken();
            entity.Property<Guid?>("CreatedByUserId")
                .HasColumnName("created_by_user_id");
            entity.Property<Guid?>("UpdatedByUserId")
                .HasColumnName("updated_by_user_id");
            entity.Property<bool>("IsDeleted")
                .HasColumnName("is_deleted")
                .HasDefaultValue(false);
            entity.Property<DateTime?>("DeletedAt")
                .HasColumnName("deleted_at");
            entity.HasIndex("TenantId", "IsDeleted")
                .HasDatabaseName($"ix_{entityType.GetTableName()}_tenant_deleted");
            entity.HasQueryFilter(BuildTenantSoftDeleteFilter(clrType));
        }
    }

    private LambdaExpression BuildTenantSoftDeleteFilter(Type clrType)
    {
        var parameter = Expression.Parameter(clrType, "entity");
        var isDeleted = Expression.Call(
            typeof(EF),
            nameof(EF.Property),
            new[] { typeof(bool) },
            parameter,
            Expression.Constant("IsDeleted"));
        var tenantId = Expression.Call(
            typeof(EF),
            nameof(EF.Property),
            new[] { typeof(Guid) },
            parameter,
            Expression.Constant("TenantId"));
        var currentTenantId = Expression.Property(Expression.Constant(this), nameof(CurrentTenantId));
        var notDeleted = Expression.Equal(isDeleted, Expression.Constant(false));
        var sameTenant = Expression.Equal(tenantId, currentTenantId);

        return Expression.Lambda(Expression.AndAlso(notDeleted, sameTenant), parameter);
    }
}
