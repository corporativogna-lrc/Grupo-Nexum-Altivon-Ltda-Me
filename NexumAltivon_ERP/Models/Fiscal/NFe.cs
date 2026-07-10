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

namespace NexumAltivon_ERP.Models.Fiscal
{
    [Table("erp_nfe")]
    public class NFe
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string Numero { get; set; } = string.Empty;

        [Required, StringLength(10)]
        public string Serie { get; set; } = "1";

        [Required, StringLength(50)]
        public string ChaveAcesso { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Status { get; set; } = "Digitacao"; // Digitacao, Validada, Autorizada, Cancelada, Denegada, Inutilizada

        [Required]
        public DateTime DataEmissao { get; set; } = DateTime.Now;

        public DateTime? DataSaida { get; set; }

        [Required, StringLength(2)]
        public string TipoOperacao { get; set; } = "1"; // 0=Entrada, 1=Saida

        [Required]
        public int CFOPId { get; set; }

        [Required]
        public int DestinatarioId { get; set; }

        [StringLength(200)]
        public string DestinatarioNome { get; set; } = string.Empty;

        [StringLength(20)]
        public string DestinatarioCNPJ { get; set; } = string.Empty;

        [StringLength(20)]
        public string DestinatarioIE { get; set; } = string.Empty;

        [StringLength(200)]
        public string DestinatarioEndereco { get; set; } = string.Empty;

        [StringLength(10)]
        public string DestinatarioUF { get; set; } = string.Empty;

        public decimal ValorTotalProdutos { get; set; }

        public decimal ValorTotalICMS { get; set; }

        public decimal ValorTotalIPI { get; set; }

        public decimal ValorTotalPIS { get; set; }

        public decimal ValorTotalCOFINS { get; set; }

        public decimal ValorTotalFrete { get; set; }

        public decimal ValorTotalSeguro { get; set; }

        public decimal ValorTotalDesconto { get; set; }

        public decimal ValorTotalOutras { get; set; }

        public decimal ValorTotalNF { get; set; }

        [StringLength(500)]
        public string InformacoesAdicionais { get; set; } = string.Empty;

        [StringLength(500)]
        public string ProtocoloAutorizacao { get; set; } = string.Empty;

        public DateTime? DataAutorizacao { get; set; }

        [StringLength(500)]
        public string MotivoCancelamento { get; set; } = string.Empty;

        public int? PedidoId { get; set; }

        public int? LojaId { get; set; }

        [StringLength(50)]
        public string CriadoPor { get; set; } = "sistema";

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        public DateTime? AtualizadoEm { get; set; }

        [ForeignKey("CFOPId")]
        public CFOP? CFOP { get; set; }

        public ICollection<ItemNFe> Itens { get; set; } = new List<ItemNFe>();
    }
}
