using System;

namespace NexumAltivon_ERP.DTOs.Financeiro
{
    public class ContaPagarCreateDto
    {
        public string NumeroDocumento { get; set; } = string.Empty;
        public int FornecedorId { get; set; }
        public string FornecedorNome { get; set; } = string.Empty;
        public int CentroCustoId { get; set; }
        public int PlanoContasId { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataVencimento { get; set; }
        public string FormaPagamento { get; set; } = string.Empty;
        public string NumeroNFe { get; set; } = string.Empty;
        public int? ParcelaAtual { get; set; } = 1;
        public int? TotalParcelas { get; set; } = 1;
        public int? LojaId { get; set; }
        public string Observacoes { get; set; } = string.Empty;
    }

    public class ContaPagarUpdateDto
    {
        public int Id { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataVencimento { get; set; }
        public string FormaPagamento { get; set; } = string.Empty;
        public string Observacoes { get; set; } = string.Empty;
    }

    public class ContaPagarPagamentoDto
    {
        public int Id { get; set; }
        public decimal ValorPago { get; set; }
        public decimal ValorDesconto { get; set; } = 0;
        public decimal ValorJuros { get; set; } = 0;
        public DateTime DataPagamento { get; set; } = DateTime.Now;
        public string BancoPagamento { get; set; } = string.Empty;
        public string Observacoes { get; set; } = string.Empty;
    }

    public class ContaPagarResponseDto
    {
        public int Id { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public string FornecedorNome { get; set; } = string.Empty;
        public string CentroCustoNome { get; set; } = string.Empty;
        public string PlanoContasNome { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public decimal ValorPago { get; set; }
        public decimal Saldo { get; set; }
        public DateTime DataVencimento { get; set; }
        public DateTime? DataPagamento { get; set; }
        public string Status { get; set; } = string.Empty;
        public string FormaPagamento { get; set; } = string.Empty;
        public int? ParcelaAtual { get; set; }
        public int? TotalParcelas { get; set; }
        public int DiasAtraso { get; set; }
        public string Observacoes { get; set; } = string.Empty;
        public DateTime CriadoEm { get; set; }
    }

    public class ContaPagarFiltroDto
    {
        public string Status { get; set; } = string.Empty;
        public int? FornecedorId { get; set; }
        public int? CentroCustoId { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int? LojaId { get; set; }
        public string Busca { get; set; } = string.Empty;
        public int Pagina { get; set; } = 1;
        public int TamanhoPagina { get; set; } = 20;
    }

    public class ContaPagarResumoDto
    {
        public decimal TotalPendente { get; set; }
        public decimal TotalAtrasado { get; set; }
        public decimal TotalPago { get; set; }
        public decimal TotalCancelado { get; set; }
        public int QuantidadePendente { get; set; }
        public int QuantidadeAtrasado { get; set; }
        public decimal MediaDiasAtraso { get; set; }
    }
}