/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.Estoque
{
    [Table("erp_inventarios")]
    public class Inventario
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string Numero { get; set; } = string.Empty;

        [Required]
        public int LocalEstoqueId { get; set; }

        [StringLength(200)]
        public string LocalEstoqueNome { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Status { get; set; } = "EmAndamento"; // EmAndamento, Concluido, Cancelado

        [Required]
        public DateTime DataInicio { get; set; } = DateTime.Now;

        public DateTime? DataFim { get; set; }

        public int TotalProdutos { get; set; }

        public int ProdutosDivergentes { get; set; }

        public decimal ValorDivergencia { get; set; }

        [StringLength(500)]
        public string Observacoes { get; set; } = string.Empty;

        [StringLength(50)]
        public string CriadoPor { get; set; } = "sistema";

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        public ICollection<ItemInventario> Itens { get; set; } = new List<ItemInventario>();

        [ForeignKey("LocalEstoqueId")]
        public LocalEstoque? LocalEstoque { get; set; }
    }
}
