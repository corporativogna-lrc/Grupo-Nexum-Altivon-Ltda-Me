using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.ERP.Models
{
    /// <summary>
    /// Contas a Pagar — gestão de obrigações financeiras do grupo
    /// </summary>
    [Table("erp_contas_pagar")]
    public class ContaPagar
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Required]
        public int FornecedorId { get; set; }

        [ForeignKey("FornecedorId")]
        public virtual Fornecedor? Fornecedor { get; set; }

        [Required, StringLength(200)]
        public string Descricao { get; set; } = string.Empty;

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal ValorOriginal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorPago { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorMulta { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorJuros { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorDesconto { get; set; }

        [Required]
        public DateTime DataEmissao { get; set; }

        [Required]
        public DateTime DataVencimento { get; set; }

        public DateTime? DataPagamento { get; set; }

        [Required, StringLength(20)]
        public string Status { get; set; } = "Pendente"; // Pendente, Pago, Atrasado, Cancelado

        [StringLength(50)]
        public string? FormaPagamento { get; set; }

        [StringLength(100)]
        public string? NumeroBoleto { get; set; }

        [StringLength(500)]
        public string? Observacoes { get; set; }

        public int? LojaId { get; set; }

        [Required]
        public int CentroCustoId { get; set; }

        [ForeignKey("CentroCustoId")]
        public virtual CentroCusto? CentroCusto { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.Now;
        public DateTime? AtualizadoEm { get; set; }
        [StringLength(100)]
        public string? CriadoPor { get; set; }
    }

    /// <summary>
    /// Contas a Receber — gestão de receitas e recebíveis
    /// </summary>
    [Table("erp_contas_receber")]
    public class ContaReceber
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Required]
        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public virtual Cliente? Cliente { get; set; }

        [Required, StringLength(200)]
        public string Descricao { get; set; } = string.Empty;

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal ValorOriginal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorRecebido { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorMulta { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorJuros { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorDesconto { get; set; }

        [Required]
        public DateTime DataEmissao { get; set; }

        [Required]
        public DateTime DataVencimento { get; set; }

        public DateTime? DataRecebimento { get; set; }

        [Required, StringLength(20)]
        public string Status { get; set; } = "Pendente"; // Pendente, Recebido, Atrasado, Cancelado, Protestado

        [StringLength(50)]
        public string? FormaRecebimento { get; set; }

        [StringLength(100)]
        public string? NumeroPedidoReferencia { get; set; }

        public int? LojaId { get; set; }

        [Required]
        public int CentroCustoId { get; set; }

        [StringLength(500)]
        public string? Observacoes { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.Now;
        public DateTime? AtualizadoEm { get; set; }
        [StringLength(100)]
        public string? CriadoPor { get; set; }
    }

    /// <summary>
    /// Centro de Custo — estrutura analítica financeira
    /// </summary>
    [Table("erp_centros_custo")]
    public class CentroCusto
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string Codigo { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descricao { get; set; }

        public int? PaiId { get; set; }

        [ForeignKey("PaiId")]
        public virtual CentroCusto? Pai { get; set; }

        [Required, StringLength(20)]
        public string Tipo { get; set; } = "Sintetico"; // Analitico, Sintetico

        public bool Ativo { get; set; } = true;

        public DateTime CriadoEm { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Fluxo de Caixa — projeção e movimentação diária
    /// </summary>
    [Table("erp_fluxo_caixa")]
    public class FluxoCaixa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Data { get; set; }

        [Required, StringLength(50)]
        public string Tipo { get; set; } = string.Empty; // Entrada, Saida, Transferencia

        [Required, StringLength(100)]
        public string Descricao { get; set; } = string.Empty;

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal Valor { get; set; }

        [StringLength(50)]
        public string? Categoria { get; set; }

        public int? ContaPagarId { get; set; }
        public int? ContaReceberId { get; set; }
        public int? PedidoId { get; set; }

        [StringLength(50)]
        public string? FormaPagamento { get; set; }

        [StringLength(100)]
        public string? ContaBancaria { get; set; }

        [StringLength(500)]
        public string? Observacoes { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.Now;
        [StringLength(100)]
        public string? CriadoPor { get; set; }
    }

    /// <summary>
    /// Conta Bancária — cadastro de contas e carteiras
    /// </summary>
    [Table("erp_contas_bancarias")]
    public class ContaBancaria
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Banco { get; set; } = string.Empty;

        [StringLength(10)]
        public string? Agencia { get; set; }

        [StringLength(20)]
        public string? Conta { get; set; }

        [StringLength(20)]
        public string? TipoConta { get; set; } // Corrente, Poupanca, Investimento

        [Column(TypeName = "decimal(18,2)")]
        public decimal SaldoAtual { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SaldoInicial { get; set; }

        public bool Ativo { get; set; } = true;

        public DateTime CriadoEm { get; set; } = DateTime.Now;
    }
}
