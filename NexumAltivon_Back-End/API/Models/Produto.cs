/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

public enum TipoProduto
{
    Proprio,
    Dropshipping,
    Marketplace,
    Afiliado
}

[Table("produtos")]
public class Produto
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("loja_id")]
    public int LojaId { get; set; }

    [Column("categoria_id")]
    public int? CategoriaId { get; set; }

    [Required]
    [Column("sku")]
    [MaxLength(50)]
    public string Sku { get; set; } = string.Empty;

    [Required]
    [Column("nome")]
    [MaxLength(200)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [Column("slug")]
    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    [Column("descricao_curta")]
    [MaxLength(500)]
    public string? DescricaoCurta { get; set; }

    [Column("descricao_longa")]
    public string? DescricaoLonga { get; set; }

    [Required]
    [Column("preco", TypeName = "decimal(10,2)")]
    public decimal Preco { get; set; } = 0.00m;

    [Column("preco_promocional", TypeName = "decimal(10,2)")]
    public decimal? PrecoPromocional { get; set; }

    [Column("custo", TypeName = "decimal(10,2)")]
    public decimal Custo { get; set; } = 0.00m;

    [Column("peso", TypeName = "decimal(8,3)")]
    public decimal Peso { get; set; } = 0.000m;

    [Column("altura", TypeName = "decimal(8,2)")]
    public decimal Altura { get; set; } = 0.00m;

    [Column("largura", TypeName = "decimal(8,2)")]
    public decimal Largura { get; set; } = 0.00m;

    [Column("comprimento", TypeName = "decimal(8,2)")]
    public decimal Comprimento { get; set; } = 0.00m;

    [Column("imagem_principal")]
    [MaxLength(255)]
    public string? ImagemPrincipal { get; set; }

    [Column("imagens_galeria")]
    public string? ImagensGaleria { get; set; } // JSON

    [Column("estoque_minimo")]
    public int EstoqueMinimo { get; set; } = 5;

    [Column("estoque_atual")]
    public int EstoqueAtual { get; set; } = 0;

    [Column("estoque_reservado")]
    public int EstoqueReservado { get; set; } = 0;

    [Column("tipo_produto")]
    public TipoProduto TipoProduto { get; set; } = TipoProduto.Proprio;

    [Column("fornecedor_id")]
    public int? FornecedorId { get; set; }

    [Column("marca")]
    [MaxLength(100)]
    public string? Marca { get; set; }

    [Column("tags")]
    [MaxLength(255)]
    public string? Tags { get; set; }

    [Column("seo_titulo")]
    [MaxLength(200)]
    public string? SeoTitulo { get; set; }

    [Column("seo_descricao")]
    [MaxLength(500)]
    public string? SeoDescricao { get; set; }

    [Column("seo_keywords")]
    [MaxLength(255)]
    public string? SeoKeywords { get; set; }

    [Column("codigo_barras")]
    [MaxLength(64)]
    public string? CodigoBarras { get; set; }

    [Column("qr_code")]
    [MaxLength(500)]
    public string? QrCode { get; set; }

    [Column("identificacao_estoque")]
    [MaxLength(500)]
    public string? IdentificacaoEstoque { get; set; }

    [Column("destaque")]
    public bool Destaque { get; set; } = false;

    [Column("ativo")]
    public bool Ativo { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("LojaId")]
    public Loja? Loja { get; set; }

    [ForeignKey("CategoriaId")]
    public Categoria? Categoria { get; set; }

    [ForeignKey("FornecedorId")]
    public Fornecedor? Fornecedor { get; set; }

    public ICollection<PedidoItem>? ItensPedido { get; set; }
    public ICollection<Carrinho>? Carrinhos { get; set; }
}
