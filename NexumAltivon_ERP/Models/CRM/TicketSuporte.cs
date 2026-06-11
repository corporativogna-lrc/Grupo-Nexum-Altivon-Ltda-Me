using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.CRM
{
    [Table("crm_tickets_suporte")]
    public class TicketSuporte
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string Numero { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Assunto { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Descricao { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Status { get; set; } = "Aberto"; // Aberto, EmAtendimento, AguardandoCliente, Resolvido, Fechado, Reaberto

        [Required, StringLength(50)]
        public string Prioridade { get; set; } = "Media"; // Baixa, Media, Alta, Urgente

        [Required, StringLength(50)]
        public string Categoria { get; set; } = string.Empty; // Pedido, Pagamento, Produto, Entrega, Tecnico, Outro

        public int? ClienteId { get; set; }

        [StringLength(200)]
        public string ClienteNome { get; set; } = string.Empty;

        [StringLength(20)]
        public string ClienteTelefone { get; set; } = string.Empty;

        [StringLength(200)]
        public string ClienteEmail { get; set; } = string.Empty;

        public int? PedidoId { get; set; }

        [StringLength(50)]
        public string Responsavel { get; set; } = string.Empty;

        public DateTime? DataAtribuicao { get; set; }

        public DateTime? DataResolucao { get; set; }

        public DateTime? DataFechamento { get; set; }

        [StringLength(500)]
        public string Solucao { get; set; } = string.Empty;

        public int Avaliacao { get; set; } // 1 a 5

        [StringLength(500)]
        public string FeedbackCliente { get; set; } = string.Empty;

        public int TempoAtendimentoMinutos { get; set; }

        [StringLength(50)]
        public string CriadoPor { get; set; } = "sistema";

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        public ICollection<InteracaoTicket> Interacoes { get; set; } = new List<InteracaoTicket>();
    }
}