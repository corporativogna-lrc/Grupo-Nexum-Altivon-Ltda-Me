using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models
{
    public class Carrinho
    {
        [Key]
        public Guid CarrinhoId { get; set; } = Guid.NewGuid();
        public int? ClienteId { get; set; }
        public string SessaoId { get; set; }
        public decimal Desconto { get; set; }
        public int? CupomId { get; set; }
        public string CupomCodigo { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
        public List<ItemCarrinho> Itens { get; set; } = new();
    }

    public class ItemCarrinho
    {
        [Key]
        public int ItemId { get; set; }
        public Guid CarrinhoId { get; set; }
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; }
        public string ProdutoImagem { get; set; }
        public string Sku { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal PrecoOriginal { get; set; }
        public decimal Subtotal { get; set; }
        public int LojaId { get; set; }
        public string LojaNome { get; set; }
        public int EstoqueDisponivel { get; set; }
        [ForeignKey("CarrinhoId")]
        public Carrinho Carrinho { get; set; }
    }

    public class Checkout
    {
        [Key]
        public int CheckoutId { get; set; }
        public int ClienteId { get; set; }
        public int EnderecoId { get; set; }
        public int? CupomId { get; set; }
        public string CupomCodigo { get; set; }
        public string NumeroPedido { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Desconto { get; set; }
        public decimal Frete { get; set; }
        public decimal Total { get; set; }
        public string CodigoFreteSelecionado { get; set; }
        public string Status { get; set; } = "ABERTO";
        public string Observacoes { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
        public List<CheckoutItem> Itens { get; set; } = new();
        [NotMapped]
        public List<OpcaoFreteDto> OpcoesFrete { get; set; } = new();
    }

    public class CheckoutItem
    {
        [Key]
        public int CheckoutItemId { get; set; }
        public int CheckoutId { get; set; }
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public int LojaId { get; set; }
        [ForeignKey("CheckoutId")]
        public Checkout Checkout { get; set; }
    }

    public class Pagamento
    {
        [Key]
        public int PagamentoId { get; set; }
        public int PedidoId { get; set; }
        public string Metodo { get; set; }
        public string Status { get; set; }
        public decimal Valor { get; set; }
        public string TransacaoId { get; set; }
        public string GatewayReferencia { get; set; }
        public DateTime? PagoEm { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        [ForeignKey("PedidoId")]
        public Pedido Pedido { get; set; }
    }

    public class Cupom
    {
        [Key]
        public int CupomId { get; set; }
        public string Codigo { get; set; }
        public string Tipo { get; set; } // PERCENTUAL ou VALOR_FIXO
        public decimal Valor { get; set; }
        public decimal? ValorMinimoPedido { get; set; }
        public int? QuantidadeMaxima { get; set; }
        public int QuantidadeUsada { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public bool Ativo { get; set; } = true;
    }

    public class Endereco
    {
        [Key]
        public int EnderecoId { get; set; }
        public int ClienteId { get; set; }
        public string Apelido { get; set; }
        public string Destinatario { get; set; }
        public string Cep { get; set; }
        public string Logradouro { get; set; }
        public string Numero { get; set; }
        public string Complemento { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public string Telefone { get; set; }
        public bool Principal { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }

    public class Cliente
    {
        [Key]
        public int ClienteId { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string Cpf { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string Genero { get; set; }
        public bool Vip { get; set; }
        public decimal CashbackDisponivel { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public List<Endereco> Enderecos { get; set; } = new();
    }

    public class Pedido
    {
        [Key]
        public int PedidoId { get; set; }
        public string NumeroPedido { get; set; }
        public int ClienteId { get; set; }
        public int? EnderecoId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Desconto { get; set; }
        public decimal Frete { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = "AGUARDANDO_PAGAMENTO";
        public string MetodoPagamento { get; set; }
        public string TransacaoGatewayId { get; set; }
        public string CodigoRastreio { get; set; }
        public string Transportadora { get; set; }
        public DateTime? PagoEm { get; set; }
        public DateTime? EnviadoEm { get; set; }
        public DateTime? EntregueEm { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public Cliente Cliente { get; set; }
        public List<PedidoItem> Itens { get; set; } = new();
    }

    public class PedidoItem
    {
        [Key]
        public int PedidoItemId { get; set; }
        public int PedidoId { get; set; }
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; }
        public string Sku { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public int LojaId { get; set; }
        public Pedido Pedido { get; set; }
    }

    public class Produto
    {
        [Key]
        public int ProdutoId { get; set; }
        public string Nome { get; set; }
        public string Sku { get; set; }
        public string Descricao { get; set; }
        public decimal Preco { get; set; }
        public decimal? PrecoPromocional { get; set; }
        public int Estoque { get; set; }
        public int EstoqueMinimo { get; set; }
        public string ImagemPrincipal { get; set; }
        public bool Ativo { get; set; } = true;
        public int LojaId { get; set; }
        public Loja Loja { get; set; }
    }

    public class Loja
    {
        [Key]
        public int LojaId { get; set; }
        public string Nome { get; set; }
        public string Segmento { get; set; }
        public string Descricao { get; set; }
        public string Logo { get; set; }
        public string CorPrimaria { get; set; }
        public bool Ativa { get; set; } = true;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}
