using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.CRM
{
    [Table("crm_oportunidades")]
    public class Oportunidade
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Descricao { get; set; } = string.Empty;

        [Required]
        public int PipelineId { get; set; }

        [Required, StringLength(50)]
        public string Etapa { get; set; } = "Lead"; // Lead, Qualificado, Proposta, Negociacao, FechadoGanho, FechadoPerdido

        public int? ClienteId { get; set; }

        [StringLength(200)]
        public string ClienteNome { get; set; } = string.Empty;

        [StringLength(20)]
        public string ClienteTelefone { get; set; } = string.Empty;

        [StringLength(200)]
        public string ClienteEmail { get; set; } = string.Empty;

        public int? LeadId { get; set; }

        public decimal ValorEstimado { get; set; }

        public decimal? ValorFechado { get; set; }

        public decimal Probabilidade { get; set; } = 0; // 0 a 100

        public DateTime? DataPrevisaoFechamento { get; set; }

        public DateTime? DataFechamento { get; set; }

        [StringLength(50)]
        public string MotivoPerda { get; set; } = string.Empty;

        [StringLength(50)]
        public string Responsavel { get; set; } = string.Empty;

        [StringLength(50)]
        public string Origem { get; set; } = string.Empty; // Site, Indicacao, Telefone, Feira, RedeSocial

        public int? CampanhaId { get; set; }

        [StringLength(500)]
        public string Observacoes { get; set; } = string.Empty;

        [StringLength(50)]
        public string CriadoPor { get; set; } = "sistema";

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        public DateTime? AtualizadoEm { get; set; }

        [ForeignKey("PipelineId")]
        public Pipeline Pipeline { get; set; }
    }
}