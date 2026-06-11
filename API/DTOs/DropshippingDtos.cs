using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NexumAltivon.API.DTOs
{
    public class DropshippingPedidoDto
    {
        public int DropshippingId { get; set; }
        public int PedidoId { get; set; }
        public string NumeroPedido { get; set; }
        public int FornecedorId { get; set; }
        public string FornecedorNome { get; set; }
        public string Status { get; set; }
        public decimal ValorProdutos { get; set; }
        public decimal ComissaoPercentual { get; set; }
        public decimal ValorComissao { get; set; }
        public decimal ValorFornecedor { get; set; }
        public DateTime CriadoEm { get; set; }
        public DateTime? EnviadoEm { get; set; }
        public string CodigoRastreio { get; set; }
        public List<DropshippingItemDto> Itens { get; set; } = new();
    }

    public class DropshippingItemDto
    {
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoCusto { get; set; }
        public decimal PrecoVenda { get; set; }
    }

    public class RoteiarPedidoRequest
    {
        [Required]
        public int PedidoId { get; set; }
        public bool AutoSelecionarFornecedor { get; set; } = true;
    }

    public class FornecedorDropshippingDto
    {
        public int FornecedorId { get; set; }
        public string RazaoSocial { get; set; }
        public string Cnpj { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public decimal ComissaoPadrao { get; set; }
        public int PrazoEnvioDias { get; set; }
        public bool Ativo { get; set; }
        public List<string> SegmentosAtendidos { get; set; } = new();
    }

    public class AtualizarStatusDropshippingRequest
    {
        [Required]
        public string Status { get; set; } // ENVIADO, ENTREGUE, CANCELADO
        public string CodigoRastreio { get; set; }
        public string UrlRastreio { get; set; }
    }

    public class ComissaoDropshippingDto
    {
        public int FornecedorId { get; set; }
        public string FornecedorNome { get; set; }
        public int TotalPedidos { get; set; }
        public decimal TotalVendido { get; set; }
        public decimal TotalComissao { get; set; }
        public decimal TotalPagoFornecedor { get; set; }
        public decimal SaldoPendente { get; set; }
    }
}
