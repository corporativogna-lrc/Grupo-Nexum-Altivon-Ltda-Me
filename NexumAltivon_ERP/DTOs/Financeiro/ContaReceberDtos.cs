using System;

namespace NexumAltivon_ERP.DTOs.Financeiro
{
    public class ContaReceberCreateDto
    {
        public string NumeroDocumento { get; set; } = string.Empty;
        public int ClienteId { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public int CentroCustoId { get; set; }
        public int PlanoContasId { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataVencimento { get; set; }
        public string FormaPagamento { get; set; } = string.Empty;
        public string NumeroPedido { get; set; } = string.Empty;
        public int? ParcelaAtual { get; set; } = 1;
        public int? TotalParcelas { get; set; } = 1;
        public int? LojaId { get; set; }
        public string Observacoes { get; set; } = string.Empty;
    }

    public class ContaReceberRecebimentoDto
    {
        public int Id { get; set; }
        public decimal ValorRecebido { get; set; }
        public decimal ValorDesconto { get; set; } = 0;
        public decimal ValorJuros { get; set; } = 0;
        public DateTime DataRecebimento { get; set; } = DateTime.Now;
        public string BancoRecebimento { get; set; } = string.Empty;
        public string Observacoes { get; set; } = string.Empty;
    }

    public class ContaReceberResponseDto
    {
        public int Id { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public string ClienteNome { get; set; } = string.Empty;
        public string CentroCustoNome { get; set; } = string.Empty;
        public string PlanoContasNome { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public decimal ValorRecebido { get; set; }
        public decimal Saldo { get; set; }
        public DateTime DataVencimento { get; set; }
        public DateTime? DataRecebimento { get; set; }
        public string Status { get; set; } = string.Empty;
        public string FormaPagamento { get; set; } = string.Empty;
        public string NumeroPedido { get; set; } = string.Empty;
        public int DiasAtraso { get; set; }
        public DateTime CriadoEm { get; set; }
    }

    public class ContaReceberResumoDto
    {
        public decimal TotalPendente { get; set; }
        public decimal TotalAtrasado { get; set; }
        public decimal TotalRecebido { get; set; }
        public int QuantidadePendente { get; set; }
        public int QuantidadeAtrasado { get; set; }
        public decimal InadimplenciaPercentual { get; set; }
    }
}