using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

[Table("categorias")]
public class Categoria
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("loja_id")]
    public int LojaId { get; set; }

    [Required]
    [Column("nome")]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [Column("slug")]
    [MaxLength(100)]
    public string Slug { get; set; } = string.Empty;

    [Column("descricao")]
    public string? Descricao { get; set; }

    [Column("imagem")]
    [MaxLength(255)]
    public string? Imagem { get; set; }

    [Column("categoria_pai_id")]
    public int? CategoriaPaiId { get; set; }

    [Column("ordem")]
    public int Ordem { get; set; } = 0;

    [Column("ativa")]
    public bool Ativa { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("LojaId")]
    public Loja? Loja { get; set; }

    [ForeignKey("CategoriaPaiId")]
    public Categoria? CategoriaPai { get; set; }

    public ICollection<Categoria>? SubCategorias { get; set; }
    public ICollection<Produto>? Produtos { get; set; }
}
