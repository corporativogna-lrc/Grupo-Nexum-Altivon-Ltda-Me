using System;
using System.ComponentModel.DataAnnotations;

namespace NexumAltivon.API.DTOs
{
    public class PagamentoDto
    {
        public int PagamentoId { get; set; }
        public int PedidoId { get; set; }
        public string NumeroPedido { get; set; }
        public string Metodo { get; set; }
        public string Status { get; set; }
        public decimal Valor { get; set; }
        public string TransacaoId { get; set; }
        public string GatewayReferencia { get; set; }
        public DateTime? PagoEm { get; set; }
        public DateTime CriadoEm { get; set; }
    }

    public class WebhookMercadoPagoDto
    {
        public long Id { get; set; }
        public string Action { get; set; }
        public string ApiVersion { get; set; }
        public DataWebhookDto Data { get; set; }
        public DateTime DateCreated { get; set; }
        public bool LiveMode { get; set; }
        public string Type { get; set; }
        public string UserId { get; set; }
    }

    public class DataWebhookDto
    {
        public string Id { get; set; }
    }

    public class ConsultaPagamentoDto
    {
        public int PedidoId { get; set; }
        public string NumeroPedido { get; set; }
        public string StatusPagamento { get; set; }
        public string StatusPedido { get; set; }
        public decimal ValorPago { get; set; }
        public DateTime? DataPagamento { get; set; }
        public string MetodoPagamento { get; set; }
    }

    public class ReembolsoRequest
    {
        [Required]
        public int PedidoId { get; set; }
        public decimal? ValorParcial { get; set; }
        public string Motivo { get; set; }
    }

    public class ReembolsoDto
    {
        public bool Sucesso { get; set; }
        public string TransacaoId { get; set; }
        public decimal ValorReembolsado { get; set; }
        public string Status { get; set; }
        public string Mensagem { get; set; }
    }
}
