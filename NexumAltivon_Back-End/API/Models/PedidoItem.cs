using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

public enum TipoFulfillment
{
    Proprio,
    Dropshipping,
    Marketplace
}

public enum StatusItemPedido
{
    Pendente,
    Separado,
    Enviado,
    Entregue,
    Cancelado
}

[Table("pedido_itens")]
public class PedidoItem
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("pedido_id")]
    public int PedidoId { get; set; }

    [Column("produto_id")]
    public int? ProdutoId { get; set; }

    [Required]
    [Column("nome_produto")]
    [MaxLength(200)]
    public string NomeProduto { get; set; } = string.Empty;

    [Column("sku_produto")]
    [MaxLength(50)]
    public string? SkuProduto { get; set; }

    [Column("imagem_produto")]
    [MaxLength(255)]
    public string? ImagemProduto { get; set; }

    [Required]
    [Column("quantidade")]
    public int Quantidade { get; set; } = 1;

    [Required]
    [Column("preco_unitario", TypeName = "decimal(10,2)")]
    public decimal PrecoUnitario { get; set; } = 0.00m;

    [Required]
    [Column("preco_total", TypeName = "decimal(10,2)")]
    public decimal PrecoTotal { get; set; } = 0.00m;

    [Column("desconto_item", TypeName = "decimal(10,2)")]
    public decimal DescontoItem { get; set; } = 0.00m;

    [Column("fornecedor_id")]
    public int? FornecedorId { get; set; }

    [Column("comissao_fornecedor", TypeName = "decimal(10,2)")]
    public decimal ComissaoFornecedor { get; set; } = 0.00m;

    [Column("tipo_fulfillment")]
    public TipoFulfillment TipoFulfillment { get; set; } = TipoFulfillment.Proprio;

    [Column("status_item")]
    public StatusItemPedido StatusItem { get; set; } = StatusItemPedido.Pendente;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("PedidoId")]
    public Pedido? Pedido { get; set; }

    [ForeignKey("ProdutoId")]
    public Produto? Produto { get; set; }

    [ForeignKey("FornecedorId")]
    public Fornecedor? Fornecedor { get; set; }
}
