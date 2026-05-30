using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

public enum TipoMarketplace
{
    B2W,
    Magalu,
    MercadoLivre,
    Shopee,
    Amazon,
    AliExpress,
    Outro
}

[Table("marketplaces")]
public class Marketplace
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("nome")]
    [MaxLength(50)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [Column("slug")]
    [MaxLength(50)]
    public string Slug { get; set; } = string.Empty;

    [Required]
    [Column("tipo")]
    public TipoMarketplace Tipo { get; set; } = TipoMarketplace.MercadoLivre;

    [Column("app_id")]
    [MaxLength(100)]
    public string? AppId { get; set; }

    [Column("app_secret")]
    [MaxLength(255)]
    public string? AppSecret { get; set; }

    [Column("access_token")]
    public string? AccessToken { get; set; }

    [Column("refresh_token")]
    public string? RefreshToken { get; set; }

    [Column("token_expira_em")]
    public DateTime? TokenExpiraEm { get; set; }

    [Column("loja_vinculada_id")]
    public int? LojaVinculadaId { get; set; }

    [Column("seller_id")]
    [MaxLength(100)]
    public string? SellerId { get; set; }

    [Column("sandbox")]
    public bool Sandbox { get; set; } = true;

    [Column("ativo")]
    public bool Ativo { get; set; } = false;

    [Column("ultima_sincronizacao")]
    public DateTime? UltimaSincronizacao { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("LojaVinculadaId")]
    public Loja? LojaVinculada { get; set; }
}
