using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.Fiscal
{
    [Table("erp_impostos")]
    public class Imposto
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Nome { get; set; } = string.Empty; // ICMS, IPI, PIS, COFINS, ISS, II

        [Required, StringLength(20)]
        public string Sigla { get; set; } = string.Empty;

        public decimal AliquotaPadrao { get; set; }

        [StringLength(50)]
        public string Tipo { get; set; } = "Estadual"; // Federal, Estadual, Municipal

        [StringLength(500)]
        public string Descricao { get; set; } = string.Empty;

        public bool Ativo { get; set; } = true;
    }
}