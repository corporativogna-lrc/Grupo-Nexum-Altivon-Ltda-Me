using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

public enum StatusPedido
{
    Pendente,
    Pago,
    EmSeparacao,
    Enviado,
    Entregue,
    Cancelado,
    Devolvido,
    Reembolsado
}

public enum StatusPagamento
{
    Aguardando,
    Aprovado,
    Recusado,
    Estornado,
    Cancelado
}

public enum OrigemPedido
{
    Site,
    Marketplace,
    Dropshipping,
    Mobile,
    API
}

[Table("pedidos")]
public class Pedido
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("numero_pedido")]
    [MaxLength(20)]
    public string NumeroPedido { get; set; } = string.Empty;

    [Required]
    [Column("cliente_id")]
    public int ClienteId { get; set; }

    [Column("endereco_entrega_id")]
    public int? EnderecoEntregaId { get; set; }

    [Column("loja_id")]
    public int? LojaId { get; set; }

    [Column("status")]
    public StatusPedido Status { get; set; } = StatusPedido.Pendente;

    [Column("status_pagamento")]
    public StatusPagamento StatusPagamento { get; set; } = StatusPagamento.Aguardando;

    [Column("meio_pagamento")]
    [MaxLength(50)]
    public string? MeioPagamento { get; set; }

    [Column("gateway_pagamento")]
    [MaxLength(50)]
    public string? GatewayPagamento { get; set; }

    [Column("gateway_transacao_id")]
    [MaxLength(100)]
    public string? GatewayTransacaoId { get; set; }

    [Column("subtotal", TypeName = "decimal(10,2)")]
    public decimal Subtotal { get; set; } = 0.00m;

    [Column("desconto", TypeName = "decimal(10,2)")]
    public decimal Desconto { get; set; } = 0.00m;

    [Column("frete_valor", TypeName = "decimal(10,2)")]
    public decimal FreteValor { get; set; } = 0.00m;

    [Column("frete_metodo")]
    [MaxLength(50)]
    public string? FreteMetodo { get; set; }

    [Column("frete_codigo_rastreio")]
    [MaxLength(50)]
    public string? FreteCodigoRastreio { get; set; }

    [Column("frete_transportadora")]
    [MaxLength(50)]
    public string? FreteTransportadora { get; set; }

    [Column("frete_prazo_dias")]
    public int FretePrazoDias { get; set; } = 0;

    [Column("total", TypeName = "decimal(10,2)")]
    public decimal Total { get; set; } = 0.00m;

    [Column("parcelas")]
    public int Parcelas { get; set; } = 1;

    [Column("juros", TypeName = "decimal(10,2)")]
    public decimal Juros { get; set; } = 0.00m;

    [Column("cupom_codigo")]
    [MaxLength(50)]
    public string? CupomCodigo { get; set; }

    [Column("cupom_desconto", TypeName = "decimal(10,2)")]
    public decimal CupomDesconto { get; set; } = 0.00m;

    [Column("observacoes_cliente")]
    public string? ObservacoesCliente { get; set; }

    [Column("observacoes_internas")]
    public string? ObservacoesInternas { get; set; }

    [Column("ip_cliente")]
    [MaxLength(45)]
    public string? IpCliente { get; set; }

    [Column("user_agent")]
    [MaxLength(255)]
    public string? UserAgent { get; set; }

    [Column("origem")]
    public OrigemPedido Origem { get; set; } = OrigemPedido.Site;

    [Column("marketplace_origem")]
    [MaxLength(50)]
    public string? MarketplaceOrigem { get; set; }

    [Column("marketplace_pedido_id")]
    [MaxLength(100)]
    public string? MarketplacePedidoId { get; set; }

    [Column("data_pagamento")]
    public DateTime? DataPagamento { get; set; }

    [Column("data_envio")]
    public DateTime? DataEnvio { get; set; }

    [Column("data_entrega")]
    public DateTime? DataEntrega { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("ClienteId")]
    public Cliente? Cliente { get; set; }

    [ForeignKey("EnderecoEntregaId")]
    public Endereco? EnderecoEntrega { get; set; }

    [ForeignKey("LojaId")]
    public Loja? Loja { get; set; }

    public ICollection<PedidoItem>? Itens { get; set; }
    public ICollection<Pagamento>? Pagamentos { get; set; }
    public ICollection<Envio>? Envios { get; set; }
}
