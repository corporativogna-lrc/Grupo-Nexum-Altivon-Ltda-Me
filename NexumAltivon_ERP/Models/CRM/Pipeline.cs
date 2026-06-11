using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.CRM
{
    [Table("crm_pipelines")]
    public class Pipeline
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        public int Ordem { get; set; } = 1;

        [StringLength(20)]
        public string Cor { get; set; } = "#C9A227";

        [StringLength(500)]
        public string Descricao { get; set; } = string.Empty;

        public bool Ativo { get; set; } = true;

        public ICollection<Oportunidade> Oportunidades { get; set; } = new List<Oportunidade>();
    }
}