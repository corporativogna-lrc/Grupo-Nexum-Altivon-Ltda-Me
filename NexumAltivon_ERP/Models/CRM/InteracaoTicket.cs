using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.CRM
{
    [Table("crm_interacoes_tickets")]
    public class InteracaoTicket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TicketId { get; set; }

        [Required, StringLength(50)]
        public string Tipo { get; set; } = "Resposta"; // Resposta, NotaInterna, Transferencia, Escalacao

        [Required, StringLength(2000)]
        public string Conteudo { get; set; } = string.Empty;

        [StringLength(50)]
        public string Autor { get; set; } = string.Empty;

        public bool Interno { get; set; } = false;

        [StringLength(500)]
        public string Anexos { get; set; } = string.Empty;

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        [ForeignKey("TicketId")]
        public TicketSuporte Ticket { get; set; }
    }
}