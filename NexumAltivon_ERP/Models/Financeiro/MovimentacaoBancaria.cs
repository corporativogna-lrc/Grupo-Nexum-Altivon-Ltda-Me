/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.Financeiro
{
    [Table("erp_movimentacoes_bancarias")]
    public class MovimentacaoBancaria
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ContaBancariaId { get; set; }

        [Required]
        public DateTime Data { get; set; } = DateTime.Now;

        [Required, StringLength(50)]
        public string Tipo { get; set; } = "Credito"; // Credito, Debito, Transferencia, TED, DOC, PIX

        [Required, StringLength(200)]
        public string Descricao { get; set; } = string.Empty;

        [Required]
        public decimal Valor { get; set; }

        [StringLength(50)]
        public string NumeroDocumento { get; set; } = string.Empty;

        public int? ContaPagarId { get; set; }

        public int? ContaReceberId { get; set; }

        [StringLength(50)]
        public string StatusConciliacao { get; set; } = "Pendente"; // Pendente, Conciliado, Divergente

        [StringLength(500)]
        public string Observacoes { get; set; } = string.Empty;

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        [ForeignKey("ContaBancariaId")]
        public ContaBancaria? ContaBancaria { get; set; }
    }
}
