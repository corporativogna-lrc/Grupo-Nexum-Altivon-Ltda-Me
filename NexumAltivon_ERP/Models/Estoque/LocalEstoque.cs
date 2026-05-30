using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.Estoque
{
    [Table("erp_locais_estoque")]
    public class LocalEstoque
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string Codigo { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Tipo { get; set; } = "Fisico"; // Fisico, Virtual, Consignado, Dropshipping

        [StringLength(200)]
        public string Endereco { get; set; } = string.Empty;

        [StringLength(50)]
        public string Responsavel { get; set; } = string.Empty;

        [StringLength(20)]
        public string Telefone { get; set; } = string.Empty;

        public bool Ativo { get; set; } = true;
    }
}