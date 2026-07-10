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
    [Table("erp_contas_bancarias")]
    public class ContaBancaria
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BancoId { get; set; }

        [Required, StringLength(20)]
        public string Agencia { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Conta { get; set; } = string.Empty;

        [StringLength(20)]
        public string Digito { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Titular { get; set; } = string.Empty;

        [StringLength(20)]
        public string Tipo { get; set; } = "Corrente"; // Corrente, Poupanca, Investimento

        public decimal SaldoAtual { get; set; } = 0;

        public decimal SaldoInicial { get; set; } = 0;

        public bool Ativo { get; set; } = true;

        [StringLength(500)]
        public string Observacoes { get; set; } = string.Empty;

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        [ForeignKey("BancoId")]
        public Banco? Banco { get; set; }
    }
}
