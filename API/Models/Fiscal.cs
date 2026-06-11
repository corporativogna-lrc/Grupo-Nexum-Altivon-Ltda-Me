using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

public enum StatusNfe
{
    Pendente,
    Emitida,
    Autorizada,
    Cancelada,
    Denegada,
    Inutilizada
}

[Table("fiscal")]
public class Fiscal
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("pedido_id")]
    public int PedidoId { get; set; }

    [Column("numero_nfe")]
    [MaxLength(20)]
    public string? NumeroNfe { get; set; }

    [Column("serie")]
    [MaxLength(5)]
    public string? Serie { get; set; }

    [Column("chave_acesso")]
    [MaxLength(44)]
    public string? ChaveAcesso { get; set; }

    [Column("xml_url")]
    [MaxLength(255)]
    public string? XmlUrl { get; set; }

    [Column("danfe_url")]
    [MaxLength(255)]
    public string? DanfeUrl { get; set; }

    [Column("status_nfe")]
    public StatusNfe StatusNfe { get; set; } = StatusNfe.Pendente;

    [Column("valor_total", TypeName = "decimal(10,2)")]
    public decimal? ValorTotal { get; set; }

    [Column("cfop")]
    [MaxLength(10)]
    public string? Cfop { get; set; }

    [Column("natureza_operacao")]
    [MaxLength(100)]
    public string? NaturezaOperacao { get; set; }

    [Column("data_emissao")]
    public DateTime? DataEmissao { get; set; }

    [Column("data_autorizacao")]
    public DateTime? DataAutorizacao { get; set; }

    [Column("protocolo")]
    [MaxLength(50)]
    public string? Protocolo { get; set; }

    [Column("motivo_cancelamento")]
    public string? MotivoCancelamento { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("PedidoId")]
    public Pedido? Pedido { get; set; }
}
