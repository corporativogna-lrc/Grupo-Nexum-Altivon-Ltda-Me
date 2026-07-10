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
    [Table("erp_contas_receber")]
    public class ContaReceber
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Required]
        public int ClienteId { get; set; }

        [StringLength(200)]
        public string ClienteNome { get; set; } = string.Empty;

        [Required]
        public int CentroCustoId { get; set; }

        [Required]
        public int PlanoContasId { get; set; }

        [Required]
        public decimal Valor { get; set; }

        public decimal ValorRecebido { get; set; } = 0;

        public decimal ValorDesconto { get; set; } = 0;

        public decimal ValorJuros { get; set; } = 0;

        [Required]
        public DateTime DataEmissao { get; set; } = DateTime.Now;

        [Required]
        public DateTime DataVencimento { get; set; }

        public DateTime? DataRecebimento { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pendente"; // Pendente, Recebido, Atrasado, Cancelado, Protestado

        [StringLength(50)]
        public string FormaPagamento { get; set; } = string.Empty;

        [StringLength(100)]
        public string BancoRecebimento { get; set; } = string.Empty;

        [StringLength(500)]
        public string Observacoes { get; set; } = string.Empty;

        [StringLength(50)]
        public string NumeroPedido { get; set; } = string.Empty;

        public int? ParcelaAtual { get; set; } = 1;

        public int? TotalParcelas { get; set; } = 1;

        public int? LojaId { get; set; }

        [StringLength(50)]
        public string CriadoPor { get; set; } = "sistema";

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        public DateTime? AtualizadoEm { get; set; }

        [ForeignKey("CentroCustoId")]
        public CentroCusto? CentroCusto { get; set; }

        [ForeignKey("PlanoContasId")]
        public PlanoContas? PlanoContas { get; set; }
    }
}
