using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

public enum TipoTransportadora
{
    Correios,
    Transportadora,
    Logistica,
    Hub
}

[Table("transportadoras")]
public class Transportadora
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

    [Column("tipo")]
    public TipoTransportadora Tipo { get; set; } = TipoTransportadora.Transportadora;

    [Column("api_endpoint")]
    [MaxLength(255)]
    public string? ApiEndpoint { get; set; }

    [Column("api_token")]
    [MaxLength(255)]
    public string? ApiToken { get; set; }

    [Column("api_sandbox")]
    public bool ApiSandbox { get; set; } = true;

    [Column("ativa")]
    public bool Ativa { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<Envio>? Envios { get; set; }
}
