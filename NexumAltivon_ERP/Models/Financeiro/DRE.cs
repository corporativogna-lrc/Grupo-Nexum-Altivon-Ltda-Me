using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.Financeiro
{
    [Table("erp_dre")]
    public class DRE
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Ano { get; set; }

        public int? Mes { get; set; }

        [Required, StringLength(50)]
        public string Tipo { get; set; } = "Mensal"; // Mensal, Trimestral, Anual

        public decimal ReceitaBruta { get; set; }

        public decimal ImpostosSobreVendas { get; set; }

        public decimal ReceitaLiquida => ReceitaBruta - ImpostosSobreVendas;

        public decimal CMV { get; set; }

        public decimal LucroBruto => ReceitaLiquida - CMV;

        public decimal DespesasOperacionais { get; set; }

        public decimal DespesasAdministrativas { get; set; }

        public decimal DespesasComerciais { get; set; }

        public decimal DespesasFinanceiras { get; set; }

        public decimal ReceitasFinanceiras { get; set; }

        public decimal LAIR => LucroBruto - DespesasOperacionais - DespesasAdministrativas - DespesasComerciais - DespesasFinanceiras + ReceitasFinanceiras;

        public decimal ImpostoRenda { get; set; }

        public decimal ContribuicaoSocial { get; set; }

        public decimal LucroLiquido => LAIR - ImpostoRenda - ContribuicaoSocial;

        public decimal EBITDA => LucroLiquido + ImpostoRenda + ContribuicaoSocial + DespesasFinanceiras; // Simplificado

        public int? LojaId { get; set; }

        [StringLength(50)]
        public string CriadoPor { get; set; } = "sistema";

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        public DateTime? AtualizadoEm { get; set; }
    }
}