/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.Fiscal
{
    [Table("erp_nfe_itens")]
    public class ItemNFe
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int NFeId { get; set; }

        public int NumeroItem { get; set; }

        [Required]
        public int ProdutoId { get; set; }

        [StringLength(200)]
        public string ProdutoNome { get; set; } = string.Empty;

        [StringLength(20)]
        public string ProdutoSKU { get; set; } = string.Empty;

        [StringLength(20)]
        public string ProdutoEAN { get; set; } = string.Empty;

        [StringLength(10)]
        public string NCM { get; set; } = string.Empty;

        [StringLength(10)]
        public string CFOP { get; set; } = string.Empty;

        [StringLength(10)]
        public string Unidade { get; set; } = "UN";

        public decimal Quantidade { get; set; }

        public decimal ValorUnitario { get; set; }

        public decimal ValorTotal { get; set; }

        public decimal ValorDesconto { get; set; }

        public decimal ValorFrete { get; set; }

        public decimal ValorSeguro { get; set; }

        public decimal ValorOutras { get; set; }

        public decimal AliquotaICMS { get; set; }

        public decimal ValorICMS { get; set; }

        public decimal AliquotaIPI { get; set; }

        public decimal ValorIPI { get; set; }

        public decimal AliquotaPIS { get; set; }

        public decimal ValorPIS { get; set; }

        public decimal AliquotaCOFINS { get; set; }

        public decimal ValorCOFINS { get; set; }

        [StringLength(50)]
        public string CSTICMS { get; set; } = string.Empty;

        [StringLength(50)]
        public string CSTIPI { get; set; } = string.Empty;

        [StringLength(50)]
        public string CSTPIS { get; set; } = string.Empty;

        [StringLength(50)]
        public string CSTCOFINS { get; set; } = string.Empty;

        [ForeignKey("NFeId")]
        public NFe? NFe { get; set; }
    }
}
