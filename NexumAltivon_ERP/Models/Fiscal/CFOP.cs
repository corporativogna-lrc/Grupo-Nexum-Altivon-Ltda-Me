using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.Fiscal
{
    [Table("erp_cfop")]
    public class CFOP
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(10)]
        public string Codigo { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string Descricao { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Tipo { get; set; } = "Saida"; // Entrada, Saida

        [StringLength(50)]
        public string Aplicacao { get; set; } = string.Empty; // Venda, Compra, Devolucao, Transferencia

        public bool Ativo { get; set; } = true;
    }
}