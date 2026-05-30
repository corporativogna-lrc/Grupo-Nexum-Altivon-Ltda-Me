using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.Fiscal
{
    [Table("erp_nfce")]
    public class NFCe
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string Numero { get; set; } = string.Empty;

        [Required, StringLength(10)]
        public string Serie { get; set; } = "1";

        [Required, StringLength(50)]
        public string ChaveAcesso { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Status { get; set; } = "Digitacao";

        [Required]
        public DateTime DataEmissao { get; set; } = DateTime.Now;

        public decimal ValorTotal { get; set; }

        public decimal ValorDesconto { get; set; }

        [StringLength(20)]
        public string CPFConsumidor { get; set; } = string.Empty;

        [StringLength(200)]
        public string NomeConsumidor { get; set; } = string.Empty;

        [StringLength(500)]
        public string ProtocoloAutorizacao { get; set; } = string.Empty;

        public DateTime? DataAutorizacao { get; set; }

        public int? LojaId { get; set; }

        [StringLength(50)]
        public string CriadoPor { get; set; } = "sistema";

        public DateTime CriadoEm { get; set; } = DateTime.Now;
    }
}