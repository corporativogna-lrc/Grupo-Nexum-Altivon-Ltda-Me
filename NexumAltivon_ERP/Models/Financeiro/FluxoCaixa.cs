using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.Financeiro
{
    [Table("erp_fluxo_caixa")]
    public class FluxoCaixa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Data { get; set; }

        [Required, StringLength(50)]
        public string Tipo { get; set; } = "Entrada"; // Entrada, Saida

        [Required, StringLength(100)]
        public string Categoria { get; set; } = string.Empty; // Vendas, Despesas, Investimentos

        [Required, StringLength(200)]
        public string Descricao { get; set; } = string.Empty;

        [Required]
        public decimal Valor { get; set; }

        [StringLength(50)]
        public string FormaPagamento { get; set; } = string.Empty;

        public int? ContaPagarId { get; set; }

        public int? ContaReceberId { get; set; }

        public int? PedidoId { get; set; }

        [StringLength(50)]
        public string NumeroDocumento { get; set; } = string.Empty;

        public int? LojaId { get; set; }

        [StringLength(50)]
        public string CriadoPor { get; set; } = "sistema";

        public DateTime CriadoEm { get; set; } = DateTime.Now;
    }
}