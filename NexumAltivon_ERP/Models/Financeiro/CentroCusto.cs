using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.Financeiro
{
    [Table("erp_centros_custo")]
    public class CentroCusto
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string Codigo { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(50)]
        public string Tipo { get; set; } = "Despesa"; // Receita, Despesa, Ambos

        [StringLength(500)]
        public string Descricao { get; set; } = string.Empty;

        public bool Ativo { get; set; } = true;
    }
}