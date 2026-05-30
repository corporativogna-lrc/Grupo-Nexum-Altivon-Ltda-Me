using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

[Table("lojas")]
public class Loja
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
    [Column("segmento")]
    [MaxLength(100)]
    public string Segmento { get; set; } = string.Empty;

    [Column("descricao")]
    public string? Descricao { get; set; }

    [Column("logo")]
    [MaxLength(255)]
    public string? Logo { get; set; }

    [Column("banner")]
    [MaxLength(255)]
    public string? Banner { get; set; }

    [Column("cor_primaria")]
    [MaxLength(7)]
    public string CorPrimaria { get; set; } = "#C9A227";

    [Column("cor_secundaria")]
    [MaxLength(7)]
    public string CorSecundaria { get; set; } = "#1E3A5F";

    [Column("dominio")]
    [MaxLength(100)]
    public string? Dominio { get; set; }

    [Column("ativa")]
    public bool Ativa { get; set; } = true;

    [Column("ordem_exibicao")]
    public int OrdemExibicao { get; set; } = 0;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<Produto>? Produtos { get; set; }
    public ICollection<Categoria>? Categorias { get; set; }
    public ICollection<Pedido>? Pedidos { get; set; }
}
