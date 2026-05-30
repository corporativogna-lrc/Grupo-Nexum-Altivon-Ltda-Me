using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.Fiscal
{
    [Table("erp_sped")]
    public class SPED
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Ano { get; set; }

        [Required]
        public int Mes { get; set; }

        [Required, StringLength(50)]
        public string Tipo { get; set; } = "Fiscal"; // Fiscal, Contribuicoes, PisCofins

        [Required, StringLength(50)]
        public string Status { get; set; } = "Gerando"; // Gerando, Gerado, Validado, Transmitido

        [StringLength(500)]
        public string CaminhoArquivo { get; set; } = string.Empty;

        public int TotalRegistros { get; set; }

        [StringLength(500)]
        public string Observacoes { get; set; } = string.Empty;

        public int? LojaId { get; set; }

        [StringLength(50)]
        public string CriadoPor { get; set; } = "sistema";

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        public DateTime? AtualizadoEm { get; set; }
    }
}