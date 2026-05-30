using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

public enum StatusEnvio
{
    Pendente,
    EtiquetaGerada,
    Coletado,
    EmTransito,
    SaiuEntrega,
    Entregue,
    Devolvido,
    Extraviado
}

[Table("envios")]
public class Envio
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("pedido_id")]
    public int PedidoId { get; set; }

    [Column("transportadora_id")]
    public int? TransportadoraId { get; set; }

    [Column("codigo_rastreio")]
    [MaxLength(50)]
    public string? CodigoRastreio { get; set; }

    [Column("etiqueta_url")]
    [MaxLength(255)]
    public string? EtiquetaUrl { get; set; }

    [Column("etiqueta_pdf")]
    public byte[]? EtiquetaPdf { get; set; }

    [Column("status_envio")]
    public StatusEnvio StatusEnvio { get; set; } = StatusEnvio.Pendente;

    [Column("preco", TypeName = "decimal(10,2)")]
    public decimal Preco { get; set; } = 0.00m;

    [Column("prazo_dias")]
    public int PrazoDias { get; set; } = 0;

    [Column("data_postagem")]
    public DateTime? DataPostagem { get; set; }

    [Column("data_entrega_estimada")]
    public DateTime? DataEntregaEstimada { get; set; }

    [Column("data_entrega_real")]
    public DateTime? DataEntregaReal { get; set; }

    [Column("eventos_rastreamento")]
    public string? EventosRastreamento { get; set; } // JSON

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("PedidoId")]
    public Pedido? Pedido { get; set; }

    [ForeignKey("TransportadoraId")]
    public Transportadora? Transportadora { get; set; }
}
