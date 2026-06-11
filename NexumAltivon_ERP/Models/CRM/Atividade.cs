using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.CRM
{
    [Table("crm_atividades")]
    public class Atividade
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Descricao { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Tipo { get; set; } = "Ligacao"; // Ligacao, Email, Reuniao, Visita, Tarefa, Proposta

        [Required, StringLength(50)]
        public string Status { get; set; } = "Pendente"; // Pendente, EmAndamento, Concluida, Cancelada

        [Required]
        public DateTime DataAgendamento { get; set; }

        public DateTime? DataConclusao { get; set; }

        public int? OportunidadeId { get; set; }

        public int? ClienteId { get; set; }

        public int? LeadId { get; set; }

        [StringLength(200)]
        public string ClienteNome { get; set; } = string.Empty;

        [StringLength(50)]
        public string Responsavel { get; set; } = string.Empty;

        [StringLength(50)]
        public string Prioridade { get; set; } = "Media"; // Baixa, Media, Alta, Urgente

        [StringLength(500)]
        public string Resultado { get; set; } = string.Empty;

        [StringLength(500)]
        public string Observacoes { get; set; } = string.Empty;

        public bool NotificacaoEnviada { get; set; } = false;

        [StringLength(50)]
        public string CriadoPor { get; set; } = "sistema";

        public DateTime CriadoEm { get; set; } = DateTime.Now;
    }
}