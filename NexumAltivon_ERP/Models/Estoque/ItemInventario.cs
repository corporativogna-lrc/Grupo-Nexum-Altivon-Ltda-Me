using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.Estoque
{
    [Table("erp_inventario_itens")]
    public class ItemInventario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InventarioId { get; set; }

        [Required]
        public int ProdutoId { get; set; }

        [StringLength(200)]
        public string ProdutoNome { get; set; } = string.Empty;

        [StringLength(20)]
        public string ProdutoSKU { get; set; } = string.Empty;

        public decimal EstoqueSistema { get; set; }

        public decimal EstoqueFisico { get; set; }

        public decimal Diferenca => EstoqueFisico - EstoqueSistema;

        public decimal CustoUnitario { get; set; }

        public decimal ValorDiferenca => Diferenca * CustoUnitario;

        [StringLength(50)]
        public string Status { get; set; } = "Pendente"; // Pendente, Ajustado, Divergente

        [StringLength(500)]
        public string Observacoes { get; set; } = string.Empty;

        [StringLength(50)]
        public string ContadoPor { get; set; } = string.Empty;

        public DateTime? DataContagem { get; set; }

        [ForeignKey("InventarioId")]
        public Inventario Inventario { get; set; }
    }
}