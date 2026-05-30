using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.Estoque
{
    [Table("erp_transferencias_estoque")]
    public class TransferenciaEstoque
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string Numero { get; set; } = string.Empty;

        [Required]
        public int OrigemId { get; set; }

        [StringLength(100)]
        public string OrigemNome { get; set; } = string.Empty;

        [Required]
        public int DestinoId { get; set; }

        [StringLength(100)]
        public string DestinoNome { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Status { get; set; } = "Pendente"; // Pendente, EmTransito, Concluida, Cancelada

        [Required]
        public DateTime DataSolicitacao { get; set; } = DateTime.Now;

        public DateTime? DataEnvio { get; set; }

        public DateTime? DataRecebimento { get; set; }

        public int TotalProdutos { get; set; }

        public decimal ValorTotal { get; set; }

        [StringLength(500)]
        public string Observacoes { get; set; } = string.Empty;

        [StringLength(50)]
        public string SolicitadoPor { get; set; } = string.Empty;

        [StringLength(50)]
        public string EnviadoPor { get; set; } = string.Empty;

        [StringLength(50)]
        public string RecebidoPor { get; set; } = string.Empty;

        [StringLength(50)]
        public string CriadoPor { get; set; } = "sistema";

        public DateTime CriadoEm { get; set; } = DateTime.Now;
    }
}