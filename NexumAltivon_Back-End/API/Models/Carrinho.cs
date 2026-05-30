using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

[Table("carrinho")]
public class Carrinho
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("cliente_id")]
    public int? ClienteId { get; set; }

    [Column("sessao_id")]
    [MaxLength(100)]
    public string? SessaoId { get; set; }

    [Required]
    [Column("produto_id")]
    public int ProdutoId { get; set; }

    [Required]
    [Column("quantidade")]
    public int Quantidade { get; set; } = 1;

    [Required]
    [Column("preco_unitario", TypeName = "decimal(10,2)")]
    public decimal PrecoUnitario { get; set; } = 0.00m;

    [Column("variacao")]
    [MaxLength(100)]
    public string? Variacao { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("ClienteId")]
    public Cliente? Cliente { get; set; }

    [ForeignKey("ProdutoId")]
    public Produto? Produto { get; set; }
}
