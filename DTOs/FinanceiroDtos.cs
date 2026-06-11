using System;
using System.ComponentModel.DataAnnotations;

namespace NexumAltivon.ERP.DTOs
{
    public class ContaPagarDto
    {
        public int Id { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public int FornecedorId { get; set; }
        public string FornecedorNome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal ValorOriginal { get; set; }
        public decimal ValorPago { get; set; }
        public decimal Saldo => ValorOriginal - ValorPago - ValorDesconto + ValorMulta + ValorJuros;
        public decimal ValorMulta { get; set; }
        public decimal ValorJuros { get; set; }
        public decimal ValorDesconto { get; set; }
        public DateTime DataEmissao { get; set; }
        public DateTime DataVencimento { get; set; }
        public DateTime? DataPagamento { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? FormaPagamento { get; set; }
        public int? LojaId { get; set; }
        public string? LojaNome { get; set; }
        public int CentroCustoId { get; set; }
        public string? CentroCustoNome { get; set; }
        public int DiasAtraso => DataPagamento.HasValue ? 0 : (DateTime.Now > DataVencimento ? (DateTime.Now - DataVencimento).Days : 0);
        public DateTime CriadoEm { get; set; }
    }

    public class CriarContaPagarDto
    {
        [Required, StringLength(20)]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Required]
        public int FornecedorId { get; set; }

        [Required, StringLength(200)]
        public string Descricao { get; set; } = string.Empty;

        [Required, Range(0.01, double.MaxValue)]
        public decimal ValorOriginal { get; set; }

        [Required]
        public DateTime DataVencimento { get; set; }

        public int? LojaId { get; set; }

        [Required]
        public int CentroCustoId { get; set; }

        public string? Observacoes { get; set; }
    }

    public class BaixarContaPagarDto
    {
        [Required]
        public int ContaPagarId { get; set; }

        [Required]
        public DateTime DataPagamento { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal ValorPago { get; set; }

        public decimal ValorMulta { get; set; }
        public decimal ValorJuros { get; set; }
        public decimal ValorDesconto { get; set; }

        [Required, StringLength(50)]
        public string FormaPagamento { get; set; } = string.Empty;

        public string? NumeroBoleto { get; set; }
        public string? Observacoes { get; set; }
    }

    public class ContaReceberDto
    {
        public int Id { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public int ClienteId { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal ValorOriginal { get; set; }
        public decimal ValorRecebido { get; set; }
        public decimal Saldo => ValorOriginal - ValorRecebido - ValorDesconto + ValorMulta + ValorJuros;
        public decimal ValorMulta { get; set; }
        public decimal ValorJuros { get; set; }
        public decimal ValorDesconto { get; set; }
        public DateTime DataEmissao { get; set; }
        public DateTime DataVencimento { get; set; }
        public DateTime? DataRecebimento { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? LojaId { get; set; }
        public int DiasAtraso => DataRecebimento.HasValue ? 0 : (DateTime.Now > DataVencimento ? (DateTime.Now - DataVencimento).Days : 0);
        public DateTime CriadoEm { get; set; }
    }

    public class CriarContaReceberDto
    {
        [Required, StringLength(20)]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Required]
        public int ClienteId { get; set; }

        [Required, StringLength(200)]
        public string Descricao { get; set; } = string.Empty;

        [Required, Range(0.01, double.MaxValue)]
        public decimal ValorOriginal { get; set; }

        [Required]
        public DateTime DataVencimento { get; set; }

        public int? LojaId { get; set; }
        public int CentroCustoId { get; set; }
        public string? NumeroPedidoReferencia { get; set; }
    }

    public class BaixarContaReceberDto
    {
        [Required]
        public int ContaReceberId { get; set; }

        [Required]
        public DateTime DataRecebimento { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal ValorRecebido { get; set; }

        [Required, StringLength(50)]
        public string FormaRecebimento { get; set; } = string.Empty;
    }

    public class FluxoCaixaDto
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string? Categoria { get; set; }
        public string? FormaPagamento { get; set; }
        public string? ContaBancaria { get; set; }
        public DateTime CriadoEm { get; set; }
    }

    public class ResumoFinanceiroDto
    {
        public decimal TotalContasPagarHoje { get; set; }
        public decimal TotalContasPagarAtrasadas { get; set; }
        public decimal TotalContasPagarProximos7Dias { get; set; }
        public decimal TotalContasReceberHoje { get; set; }
        public decimal TotalContasReceberAtrasadas { get; set; }
        public decimal TotalContasReceberProximos7Dias { get; set; }
        public decimal SaldoPrevisto7Dias => TotalContasReceberProximos7Dias - TotalContasPagarProximos7Dias;
        public decimal SaldoCaixaAtual { get; set; }
        public int QuantidadeContasPagarAtrasadas { get; set; }
        public int QuantidadeContasReceberAtrasadas { get; set; }
    }

    public class DreDto
    {
        public string Periodo { get; set; } = string.Empty;
        public decimal ReceitaBruta { get; set; }
        public decimal Impostos { get; set; }
        public decimal ReceitaLiquida => ReceitaBruta - Impostos;
        public decimal CustoProdutosVendidos { get; set; }
        public decimal LucroBruto => ReceitaLiquida - CustoProdutosVendidos;
        public decimal DespesasOperacionais { get; set; }
        public decimal LucroOperacional => LucroBruto - DespesasOperacionais;
        public decimal DespesasFinanceiras { get; set; }
        public decimal ReceitasFinanceiras { get; set; }
        public decimal LucroLiquido => LucroOperacional - DespesasFinanceiras + ReceitasFinanceiras;
        public decimal MargemBruta => ReceitaLiquida > 0 ? (LucroBruto / ReceitaLiquida) * 100 : 0;
        public decimal MargemLiquida => ReceitaLiquida > 0 ? (LucroLiquido / ReceitaLiquida) * 100 : 0;
    }
}
