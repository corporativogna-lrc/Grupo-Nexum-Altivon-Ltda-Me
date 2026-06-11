using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.CRM
{
    [Table("crm_leads")]
    public class LeadCRM
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string Telefone { get; set; } = string.Empty;

        [StringLength(50)]
        public string Tipo { get; set; } = "Cliente"; // Cliente, Fornecedor, Parceiro, Dropshipping

        [StringLength(50)]
        public string Status { get; set; } = "Novo"; // Novo, Qualificado, Convertido, Descartado

        [StringLength(50)]
        public string Origem { get; set; } = string.Empty; // Site, Formulario, WhatsApp, Indicacao, Campanha

        public int? CampanhaId { get; set; }

        [StringLength(200)]
        public string Empresa { get; set; } = string.Empty;

        [StringLength(50)]
        public string Cargo { get; set; } = string.Empty;

        [StringLength(500)]
        public string Interesses { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Observacoes { get; set; } = string.Empty;

        public int? Score { get; set; } // 0 a 100

        public DateTime? DataQualificacao { get; set; }

        public int? ConvertidoParaClienteId { get; set; }

        public int? ConvertidoParaOportunidadeId { get; set; }

        [StringLength(50)]
        public string Responsavel { get; set; } = string.Empty;

        [StringLength(50)]
        public string CriadoPor { get; set; } = "sistema";

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        public DateTime? AtualizadoEm { get; set; }
    }
}