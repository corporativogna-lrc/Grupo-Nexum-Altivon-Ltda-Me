using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.CRM
{
    [Table("crm_campanhas")]
    public class Campanha
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Descricao { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Tipo { get; set; } = "Email"; // Email, WhatsApp, SMS, RedeSocial, GoogleAds, Display

        [Required, StringLength(50)]
        public string Status { get; set; } = "Rascunho"; // Rascunho, Agendada, EmAndamento, Pausada, Concluida, Cancelada

        [Required]
        public DateTime DataInicio { get; set; }

        public DateTime? DataFim { get; set; }

        public decimal Orcamento { get; set; }

        public decimal CustoAtual { get; set; }

        public int Alcance { get; set; }

        public int Cliques { get; set; }

        public int LeadsGerados { get; set; }

        public int OportunidadesGeradas { get; set; }

        public int VendasGeradas { get; set; }

        public decimal ReceitaGerada { get; set; }

        public decimal ROAS => CustoAtual > 0 ? ReceitaGerada / CustoAtual : 0;

        [StringLength(500)]
        public string PublicoAlvo { get; set; } = string.Empty;

        [StringLength(500)]
        public string Conteudo { get; set; } = string.Empty;

        [StringLength(50)]
        public string CriadoPor { get; set; } = "sistema";

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        public DateTime? AtualizadoEm { get; set; }
    }
}