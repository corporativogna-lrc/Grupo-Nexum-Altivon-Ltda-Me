using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NexumAltivon.API.DTOs
{
    public class GerarEtiquetaRequest
    {
        [Required]
        public int PedidoId { get; set; }
        public int? TransportadoraId { get; set; }
        public string ServicoFrete { get; set; } = string.Empty;
    }

    public class EtiquetaDto
    {
        public int EtiquetaId { get; set; }
        public int PedidoId { get; set; }
        public string NumeroPedido { get; set; } = string.Empty;
        public string Transportadora { get; set; } = string.Empty;
        public string CodigoRastreio { get; set; } = string.Empty;
        public string UrlEtiquetaPdf { get; set; } = string.Empty;
        public string UrlEtiquetaZpl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime GeradaEm { get; set; }
    }

    public class RastreamentoDto
    {
        public Guid ConsultaId { get; set; }
        public int PedidoId { get; set; }
        public string NumeroPedido { get; set; } = string.Empty;
        public string? MetodoFrete { get; set; }
        public string StatusInterno { get; set; } = string.Empty;
        public DateTime? DataEnvio { get; set; }
        public DateTime? DataEntrega { get; set; }
        public string CodigoRastreio { get; set; } = string.Empty;
        public string Transportadora { get; set; } = string.Empty;
        public string StatusAtual { get; set; } = string.Empty;
        public string DescricaoStatus { get; set; } = string.Empty;
        public DateTime? PrevisaoEntrega { get; set; }
        public List<EventoRastreamentoDto> Eventos { get; set; } = new();
    }

    public class EventoRastreamentoDto
    {
        public DateTime DataHora { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Local { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
    }

    public class AtualizarStatusEnvioRequest
    {
        [Required]
        public int PedidoId { get; set; }
        [Required]
        public string Status { get; set; } = string.Empty; // SEPARADO, ENVIADO, EM_TRANSITO, ENTREGUE, DEVOLVIDO
        public string CodigoRastreio { get; set; } = string.Empty;
        public DateTime? DataEnvio { get; set; }
        public DateTime? DataEntrega { get; set; }
    }

    public class TransportadoraDto
    {
        public int TransportadoraId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string CodigoApi { get; set; } = string.Empty;
        public bool Ativa { get; set; }
        public decimal? TaxaAdicional { get; set; }
    }

    public class DashboardLogisticaDto
    {
        public int TotalPedidosHoje { get; set; }
        public int PedidosSeparacao { get; set; }
        public int PedidosTransito { get; set; }
        public int PedidosEntreguesHoje { get; set; }
        public int PedidosAtrasados { get; set; }
        public List<PedidoLogisticaDto> PedidosPendentes { get; set; } = new();
    }

    public class PedidoLogisticaDto
    {
        public int PedidoId { get; set; }
        public string NumeroPedido { get; set; } = string.Empty;
        public string ClienteNome { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Transportadora { get; set; } = string.Empty;
        public string CodigoRastreio { get; set; } = string.Empty;
        public DateTime? PrevisaoEntrega { get; set; }
        public int DiasAtraso { get; set; }
    }
}
