using System;

namespace NexumAltivon_ERP.DTOs.Financeiro
{
    public class DRECreateDto
    {
        public int Ano { get; set; }
        public int? Mes { get; set; }
        public string Tipo { get; set; } = "Mensal";
        public decimal ReceitaBruta { get; set; }
        public decimal ImpostosSobreVendas { get; set; }
        public decimal CMV { get; set; }
        public decimal DespesasOperacionais { get; set; }
        public decimal DespesasAdministrativas { get; set; }
        public decimal DespesasComerciais { get; set; }
        public decimal DespesasFinanceiras { get; set; }
        public decimal ReceitasFinanceiras { get; set; }
        public decimal ImpostoRenda { get; set; }
        public decimal ContribuicaoSocial { get; set; }
        public int? LojaId { get; set; }
    }

    public class DREResponseDto
    {
        public int Id { get; set; }
        public int Ano { get; set; }
        public int? Mes { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal ReceitaBruta { get; set; }
        public decimal ImpostosSobreVendas { get; set; }
        public decimal ReceitaLiquida { get; set; }
        public decimal CMV { get; set; }
        public decimal LucroBruto { get; set; }
        public decimal DespesasOperacionais { get; set; }
        public decimal DespesasAdministrativas { get; set; }
        public decimal DespesasComerciais { get; set; }
        public decimal DespesasFinanceiras { get; set; }
        public decimal ReceitasFinanceiras { get; set; }
        public decimal LAIR { get; set; }
        public decimal ImpostoRenda { get; set; }
        public decimal ContribuicaoSocial { get; set; }
        public decimal LucroLiquido { get; set; }
        public decimal MargemBrutaPercentual { get; set; }
        public decimal MargemLiquidaPercentual { get; set; }
        public decimal EBITDA { get; set; }
        public DateTime CriadoEm { get; set; }
    }

    public class DREFiltroDto
    {
        public int Ano { get; set; }
        public int? Mes { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public int? LojaId { get; set; }
    }
}