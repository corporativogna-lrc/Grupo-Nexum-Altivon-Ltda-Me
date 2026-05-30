using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

public enum TipoLancamento
{
    Receita,
    Despesa,
    Transferencia,
    Estorno,
    Taxa
}

public enum StatusLancamento
{
    Pendente,
    Pago,
    Atrasado,
    Cancelado,
    Estornado
}

[Table("financeiro")]
public class Financeiro
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("pedido_id")]
    public int? PedidoId { get; set; }

    [Column("tipo")]
    public TipoLancamento Tipo { get; set; } = TipoLancamento.Receita;

    [Column("categoria")]
    [MaxLength(100)]
    public string? Categoria { get; set; }

    [Column("descricao")]
    [MaxLength(255)]
    public string? Descricao { get; set; }

    [Required]
    [Column("valor", TypeName = "decimal(10,2)")]
    public decimal Valor { get; set; } = 0.00m;

    [Column("data_vencimento")]
    public DateTime? DataVencimento { get; set; }

    [Column("data_pagamento")]
    public DateTime? DataPagamento { get; set; }

    [Column("status")]
    public StatusLancamento Status { get; set; } = StatusLancamento.Pendente;

    [Column("meio_pagamento")]
    [MaxLength(50)]
    public string? MeioPagamento { get; set; }

    [Column("conta_bancaria")]
    [MaxLength(100)]
    public string? ContaBancaria { get; set; }

    [Column("comprovante_url")]
    [MaxLength(255)]
    public string? ComprovanteUrl { get; set; }

    [Column("observacoes")]
    public string? Observacoes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("PedidoId")]
    public Pedido? Pedido { get; set; }
}
