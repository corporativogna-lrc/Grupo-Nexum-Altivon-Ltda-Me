using System;

namespace NexumAltivon_ERP.DTOs.Financeiro
{
    public class FluxoCaixaCreateDto
    {
        public DateTime Data { get; set; } = DateTime.Now;
        public string Tipo { get; set; } = "Entrada";
        public string Categoria { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string FormaPagamento { get; set; } = string.Empty;
        public int? ContaPagarId { get; set; }
        public int? ContaReceberId { get; set; }
        public int? PedidoId { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public int? LojaId { get; set; }
    }

    public class FluxoCaixaResponseDto
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string FormaPagamento { get; set; } = string.Empty;
        public string NumeroDocumento { get; set; } = string.Empty;
        public DateTime CriadoEm { get; set; }
    }

    public class FluxoCaixaResumoDto
    {
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public decimal TotalEntradas { get; set; }
        public decimal TotalSaidas { get; set; }
        public decimal SaldoPeriodo { get; set; }
        public decimal SaldoAcumulado { get; set; }
    }

    public class FluxoCaixaDiarioDto
    {
        public DateTime Data { get; set; }
        public decimal Entradas { get; set; }
        public decimal Saidas { get; set; }
        public decimal SaldoDia { get; set; }
    }
}