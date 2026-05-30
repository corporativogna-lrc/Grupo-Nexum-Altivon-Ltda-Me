using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

public enum TipoDropshipping
{
    AliExpress,
    CJDropshipping,
    Dropi,
    Cartpanda,
    Nuvemshop,
    Outro
}

[Table("dropshipping_config")]
public class DropshippingConfig
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("nome")]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [Column("slug")]
    [MaxLength(50)]
    public string Slug { get; set; } = string.Empty;

    [Required]
    [Column("tipo")]
    public TipoDropshipping Tipo { get; set; } = TipoDropshipping.AliExpress;

    [Column("api_endpoint")]
    [MaxLength(255)]
    public string? ApiEndpoint { get; set; }

    [Column("api_key")]
    [MaxLength(255)]
    public string? ApiKey { get; set; }

    [Column("api_secret")]
    [MaxLength(255)]
    public string? ApiSecret { get; set; }

    [Column("ativo")]
    public bool Ativo { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
