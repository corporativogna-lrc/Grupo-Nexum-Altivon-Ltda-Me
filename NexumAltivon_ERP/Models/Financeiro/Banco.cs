using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.Financeiro
{
    [Table("erp_bancos")]
    public class Banco
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(10)]
        public string Codigo { get; set; } = string.Empty; // 001, 341, etc

        [Required, StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(20)]
        public string ISPB { get; set; } = string.Empty;

        public bool Ativo { get; set; } = true;
    }
}