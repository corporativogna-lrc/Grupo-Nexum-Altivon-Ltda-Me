using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.Estoque
{
    [Table("erp_alertas_estoque")]
    public class AlertaEstoque
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProdutoId { get; set; }

        [StringLength(200)]
        public string ProdutoNome { get; set; } = string.Empty;

        [StringLength(20)]
        public string ProdutoSKU { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Tipo { get; set; } = "Minimo"; // Minimo, Maximo, Zerado, Vencimento

        public decimal EstoqueAtual { get; set; }

        public decimal EstoqueMinimo { get; set; }

        public decimal EstoqueMaximo { get; set; }

        [StringLength(500)]
        public string Mensagem { get; set; } = string.Empty;

        [StringLength(50)]
        public string Status { get; set; } = "Ativo"; // Ativo, Resolvido, Ignorado

        public bool Notificado { get; set; } = false;

        public DateTime? DataResolucao { get; set; }

        [StringLength(500)]
        public string Resolucao { get; set; } = string.Empty;

        public DateTime CriadoEm { get; set; } = DateTime.Now;
    }
}