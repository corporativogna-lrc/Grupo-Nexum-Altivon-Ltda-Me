using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

public enum TipoEndereco
{
    Entrega,
    Cobranca,
    Ambos
}

[Table("enderecos")]
public class Endereco
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("cliente_id")]
    public int ClienteId { get; set; }

    [Column("tipo")]
    public TipoEndereco Tipo { get; set; } = TipoEndereco.Entrega;

    [Column("apelido")]
    [MaxLength(50)]
    public string Apelido { get; set; } = "Principal";

    [Required]
    [Column("cep")]
    [MaxLength(10)]
    public string Cep { get; set; } = string.Empty;

    [Required]
    [Column("logradouro")]
    [MaxLength(200)]
    public string Logradouro { get; set; } = string.Empty;

    [Required]
    [Column("numero")]
    [MaxLength(20)]
    public string Numero { get; set; } = string.Empty;

    [Column("complemento")]
    [MaxLength(100)]
    public string? Complemento { get; set; }

    [Column("bairro")]
    [MaxLength(100)]
    public string? Bairro { get; set; }

    [Column("cidade")]
    [MaxLength(100)]
    public string? Cidade { get; set; }

    [Column("estado")]
    [MaxLength(2)]
    public string? Estado { get; set; }

    [Column("pais")]
    [MaxLength(50)]
    public string Pais { get; set; } = "Brasil";

    [Column("padrao")]
    public bool Padrao { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("ClienteId")]
    public Cliente? Cliente { get; set; }
}
