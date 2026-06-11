using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

public enum StatusFornecedor
{
    Ativo,
    Inativo,
    Pendente,
    Bloqueado
}

[Table("fornecedores")]
public class Fornecedor
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("razao_social")]
    [MaxLength(200)]
    public string RazaoSocial { get; set; } = string.Empty;

    [Column("nome_fantasia")]
    [MaxLength(200)]
    public string? NomeFantasia { get; set; }

    [Column("cnpj")]
    [MaxLength(18)]
    public string? Cnpj { get; set; }

    [Column("ie")]
    [MaxLength(20)]
    public string? Ie { get; set; }

    [Column("email")]
    [MaxLength(150)]
    public string? Email { get; set; }

    [Column("telefone")]
    [MaxLength(20)]
    public string? Telefone { get; set; }

    [Column("whatsapp")]
    [MaxLength(20)]
    public string? Whatsapp { get; set; }

    [Column("endereco")]
    public string? Endereco { get; set; }

    [Column("cidade")]
    [MaxLength(100)]
    public string? Cidade { get; set; }

    [Column("estado")]
    [MaxLength(2)]
    public string? Estado { get; set; }

    [Column("cep")]
    [MaxLength(10)]
    public string? Cep { get; set; }

    [Column("segmento")]
    [MaxLength(100)]
    public string? Segmento { get; set; }

    [Column("loja_vinculada_id")]
    public int? LojaVinculadaId { get; set; }

    [Column("comissao_percentual", TypeName = "decimal(5,2)")]
    public decimal ComissaoPercentual { get; set; } = 0.00m;

    [Column("prazo_entrega_dias")]
    public int PrazoEntregaDias { get; set; } = 7;

    [Column("status")]
    public StatusFornecedor Status { get; set; } = StatusFornecedor.Pendente;

    [Column("observacoes")]
    public string? Observacoes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("LojaVinculadaId")]
    public Loja? LojaVinculada { get; set; }

    public ICollection<Produto>? Produtos { get; set; }
}
