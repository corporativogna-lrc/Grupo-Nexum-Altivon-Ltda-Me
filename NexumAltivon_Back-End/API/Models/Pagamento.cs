using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

public enum MetodoPagamento
{
    PIX,
    CartaoCredito,
    CartaoDebito,
    Boleto,
    Transferencia,
    Wallet,
    Outro
}

public enum StatusPagamentoDetalhado
{
    Pendente,
    Processando,
    Aprovado,
    Recusado,
    Estornado,
    Cancelado,
    Chargeback
}

[Table("pagamentos")]
public class Pagamento
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("pedido_id")]
    public int PedidoId { get; set; }

    [Required]
    [Column("gateway")]
    [MaxLength(50)]
    public string Gateway { get; set; } = string.Empty;

    [Column("gateway_transacao_id")]
    [MaxLength(100)]
    public string? GatewayTransacaoId { get; set; }

    [Required]
    [Column("metodo")]
    public MetodoPagamento Metodo { get; set; } = MetodoPagamento.PIX;

    [Column("status")]
    public StatusPagamentoDetalhado Status { get; set; } = StatusPagamentoDetalhado.Pendente;

    [Required]
    [Column("valor", TypeName = "decimal(10,2)")]
    public decimal Valor { get; set; } = 0.00m;

    [Column("valor_liquido", TypeName = "decimal(10,2)")]
    public decimal? ValorLiquido { get; set; }

    [Column("taxa_gateway", TypeName = "decimal(10,2)")]
    public decimal TaxaGateway { get; set; } = 0.00m;

    [Column("parcelas")]
    public int Parcelas { get; set; } = 1;

    [Column("bandeira")]
    [MaxLength(20)]
    public string? Bandeira { get; set; }

    [Column("ultimos_digitos")]
    [MaxLength(4)]
    public string? UltimosDigitos { get; set; }

    [Column("nsu")]
    [MaxLength(50)]
    public string? Nsu { get; set; }

    [Column("autorizacao_codigo")]
    [MaxLength(50)]
    public string? AutorizacaoCodigo { get; set; }

    [Column("pix_qrcode")]
    public string? PixQrcode { get; set; }

    [Column("pix_expiracao")]
    public DateTime? PixExpiracao { get; set; }

    [Column("boleto_url")]
    [MaxLength(255)]
    public string? BoletoUrl { get; set; }

    [Column("boleto_codigo_barras")]
    [MaxLength(50)]
    public string? BoletoCodigoBarras { get; set; }

    [Column("boleto_vencimento")]
    public DateTime? BoletoVencimento { get; set; }

    [Column("webhook_payload")]
    public string? WebhookPayload { get; set; } // JSON

    [Column("data_processamento")]
    public DateTime? DataProcessamento { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("PedidoId")]
    public Pedido? Pedido { get; set; }
}
