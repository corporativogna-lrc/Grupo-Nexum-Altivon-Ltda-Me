using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.CRM
{
    [Table("crm_segmentos_clientes")]
    public class SegmentoCliente
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(500)]
        public string Descricao { get; set; } = string.Empty;

        [StringLength(20)]
        public string Cor { get; set; } = "#C9A227";

        public int Prioridade { get; set; } = 1;

        public decimal? TicketMedioMinimo { get; set; }

        public decimal? TicketMedioMaximo { get; set; }

        public int? FrequenciaMinimaDias { get; set; }

        public int? FrequenciaMaximaDias { get; set; }

        public bool Ativo { get; set; } = true;
    }
}