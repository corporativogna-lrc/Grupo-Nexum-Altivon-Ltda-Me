using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.Financeiro
{
    [Table("erp_conciliacoes_bancarias")]
    public class ConciliacaoBancaria
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ContaBancariaId { get; set; }

        [Required]
        public DateTime DataInicio { get; set; }

        [Required]
        public DateTime DataFim { get; set; }

        public decimal SaldoInicial { get; set; }

        public decimal SaldoFinal { get; set; }

        public decimal TotalCreditos { get; set; }

        public decimal TotalDebitos { get; set; }

        public int TotalRegistros { get; set; }

        public int RegistrosConciliados { get; set; }

        public int RegistrosPendentes { get; set; }

        public int RegistrosDivergentes { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "EmAndamento"; // EmAndamento, Concluido, Divergente

        [StringLength(50)]
        public string ArquivoOFX { get; set; } = string.Empty;

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        [ForeignKey("ContaBancariaId")]
        public ContaBancaria ContaBancaria { get; set; }
    }
}