using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.Financeiro
{
    [Table("erp_plano_contas")]
    public class PlanoContas
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string Codigo { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Tipo { get; set; } = string.Empty; // Ativo, Passivo, Patrimonio, Receita, Custo, Despesa

        [Required, StringLength(10)]
        public string Natureza { get; set; } = "Debito"; // Debito, Credito

        public int? PaiId { get; set; }

        [StringLength(500)]
        public string Descricao { get; set; } = string.Empty;

        public bool Ativo { get; set; } = true;

        [ForeignKey("PaiId")]
        public PlanoContas Pai { get; set; }
    }
}