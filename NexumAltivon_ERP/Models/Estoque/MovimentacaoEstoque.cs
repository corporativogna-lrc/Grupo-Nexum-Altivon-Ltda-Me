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

namespace NexumAltivon_ERP.Models.Estoque
{
    [Table("erp_movimentacoes_estoque")]
    public class MovimentacaoEstoque
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

        [Required, StringLength(50)]
        public string Tipo { get; set; } = "Entrada"; // Entrada, Saida, Transferencia, Ajuste, Inventario, Devolucao

        [Required, StringLength(50)]
        public string Motivo { get; set; } = string.Empty; // Compra, Venda, Transferencia, Perda, Ajuste, Devolucao

        [Required]
        public decimal Quantidade { get; set; }

        public decimal CustoUnitario { get; set; }

        public decimal CustoTotal => Quantidade * CustoUnitario;

        public decimal EstoqueAnterior { get; set; }

        public decimal EstoqueAtual { get; set; }

        public int? PedidoId { get; set; }

        public int? NFeId { get; set; }

        public int? FornecedorId { get; set; }

        [StringLength(200)]
        public string FornecedorNome { get; set; } = string.Empty;

        public int? LocalDestinoId { get; set; }

        [StringLength(500)]
        public string Observacoes { get; set; } = string.Empty;

        [Required]
        public DateTime DataMovimentacao { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string CriadoPor { get; set; } = "sistema";

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        [ForeignKey("LocalEstoqueId")]
        public LocalEstoque? LocalEstoque { get; set; }
    }
}
