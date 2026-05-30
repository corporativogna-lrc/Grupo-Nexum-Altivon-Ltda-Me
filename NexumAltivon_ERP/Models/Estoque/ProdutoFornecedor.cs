using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.Estoque
{
    [Table("erp_produtos_fornecedores")]
    public class ProdutoFornecedor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProdutoId { get; set; }

        [Required]
        public int FornecedorId { get; set; }

        [StringLength(200)]
        public string FornecedorNome { get; set; } = string.Empty;

        [StringLength(50)]
        public string CodigoFornecedor { get; set; } = string.Empty; // SKU do fornecedor

        public decimal PrecoCusto { get; set; }

        public decimal PrecoTabela { get; set; }

        public int PrazoEntregaDias { get; set; }

        public decimal QuantidadeMinima { get; set; }

        public bool FornecedorPrincipal { get; set; } = false;

        public bool Ativo { get; set; } = true;

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        public DateTime? AtualizadoEm { get; set; }
    }
}