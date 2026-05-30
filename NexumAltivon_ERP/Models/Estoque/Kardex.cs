using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.Estoque
{
    [Table("erp_kardex")]
    public class Kardex
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProdutoId { get; set; }

        [StringLength(200)]
        public string ProdutoNome { get; set; } = string.Empty;

        [StringLength(20)]
        public string ProdutoSKU { get; set; } = string.Empty;

        [Required]
        public int LocalEstoqueId { get; set; }

        [Required]
        public DateTime Data { get; set; } = DateTime.Now;

        [Required, StringLength(50)]
        public string Tipo { get; set; } = "Entrada"; // Entrada, Saida, Ajuste, Transferencia

        [Required, StringLength(50)]
        public string Documento { get; set; } = string.Empty; // NFe, Pedido, Inventario, etc

        [StringLength(50)]
        public string NumeroDocumento { get; set; } = string.Empty;

        public decimal Quantidade { get; set; }

        public decimal ValorUnitario { get; set; }

        public decimal ValorTotal { get; set; }

        public decimal SaldoQuantidade { get; set; }

        public decimal SaldoValor { get; set; }

        [StringLength(500)]
        public string Observacoes { get; set; } = string.Empty;

        [StringLength(50)]
        public string CriadoPor { get; set; } = "sistema";

        public DateTime CriadoEm { get; set; } = DateTime.Now;
    }
}