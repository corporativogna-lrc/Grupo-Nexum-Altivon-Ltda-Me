using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models
{
    public class MarketplaceProduto
    {
        [Key]
        public int MarketplaceProdutoId { get; set; }
        public int ProdutoId { get; set; }
        public string Canal { get; set; } // mercadolivre, shopee, amazon
        public string IdExterno { get; set; }
        public string Url { get; set; }
        public string Status { get; set; }
        public decimal PrecoExterno { get; set; }
        public int EstoqueExterno { get; set; }
        public DateTime? SyncEm { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        [ForeignKey("ProdutoId")]
        public Produto Produto { get; set; }
    }

    public class DropshippingPedido
    {
        [Key]
        public int DropshippingId { get; set; }
        public int PedidoId { get; set; }
        public int FornecedorId { get; set; }
        public string Status { get; set; } = "AGUARDANDO_ENVIO";
        public decimal ValorProdutos { get; set; }
        public decimal ComissaoPercentual { get; set; }
        public decimal ValorComissao { get; set; }
        public decimal ValorFornecedor { get; set; }
        public string CodigoRastreio { get; set; }
        public string UrlRastreio { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? EnviadoEm { get; set; }
        public List<DropshippingItem> Itens { get; set; } = new();
        [ForeignKey("PedidoId")]
        public Pedido Pedido { get; set; }
        [ForeignKey("FornecedorId")]
        public Fornecedor Fornecedor { get; set; }
    }

    public class DropshippingItem
    {
        [Key]
        public int DropshippingItemId { get; set; }
        public int DropshippingId { get; set; }
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoCusto { get; set; }
        public decimal PrecoVenda { get; set; }
        [ForeignKey("DropshippingId")]
        public DropshippingPedido DropshippingPedido { get; set; }
    }

    public class Fornecedor
    {
        [Key]
        public int FornecedorId { get; set; }
        public string RazaoSocial { get; set; }
        public string NomeFantasia { get; set; }
        public string Cnpj { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string Endereco { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public bool AceitaDropshipping { get; set; }
        public decimal ComissaoDropshipping { get; set; } = 15.0m;
        public int PrazoEnvioDias { get; set; } = 3;
        public bool Ativo { get; set; } = true;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public List<FornecedorProduto> Produtos { get; set; } = new();
    }

    public class FornecedorProduto
    {
        [Key]
        public int FornecedorProdutoId { get; set; }
        public int FornecedorId { get; set; }
        public int ProdutoId { get; set; }
        public decimal PrecoCusto { get; set; }
        public int PrazoEntregaDias { get; set; }
        public bool Ativo { get; set; } = true;
    }

    public class Transportadora
    {
        [Key]
        public int TransportadoraId { get; set; }
        public string Nome { get; set; }
        public string CodigoApi { get; set; }
        public string UrlApi { get; set; }
        public bool Ativa { get; set; } = true;
        public decimal? TaxaAdicional { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }

    public class EtiquetaEnvio
    {
        [Key]
        public int EtiquetaId { get; set; }
        public int PedidoId { get; set; }
        public int TransportadoraId { get; set; }
        public string CodigoRastreio { get; set; }
        public string UrlEtiquetaPdf { get; set; }
        public string UrlEtiquetaZpl { get; set; }
        public string Status { get; set; } = "GERADA";
        public DateTime GeradaEm { get; set; } = DateTime.UtcNow;
    }

    public class SyncLog
    {
        [Key]
        public int SyncLogId { get; set; }
        public string Tipo { get; set; } // ESTOQUE, PRECO, PEDIDO
        public int ProdutoId { get; set; }
        public string Canal { get; set; }
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; }
        public DateTime DataHora { get; set; } = DateTime.UtcNow;
    }

    // Extensões das entidades existentes
    public partial class Produto
    {
        public string CodigoErp { get; set; }
        public decimal PrecoCusto { get; set; }
        public string Unidade { get; set; }
        public string Ncm { get; set; }
        public string Cest { get; set; }
        public decimal PesoKg { get; set; }
        public DateTime? SincronizadoErpEm { get; set; }
        public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
    }

    public partial class Cliente
    {
        public string CodigoErp { get; set; }
        public DateTime? SincronizadoErpEm { get; set; }
    }

    public partial class Pedido
    {
        public string Origem { get; set; } = "site"; // site, mercadolivre, shopee, amazon
        public string CodigoErp { get; set; }
        public string CidadeDestino { get; set; }
        public DateTime? SincronizadoErpEm { get; set; }
    }
}
