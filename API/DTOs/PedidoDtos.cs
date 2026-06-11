namespace NexumAltivon.API.DTOs;

public class PedidoDto
{
    public int Id { get; set; }
    public string NumeroPedido { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string Status { get; set; } = "Pendente";
    public string StatusPagamento { get; set; } = "Aguardando";
    public string? MeioPagamento { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Desconto { get; set; }
    public decimal FreteValor { get; set; }
    public decimal Total { get; set; }
    public int Parcelas { get; set; }
    public string? CupomCodigo { get; set; }
    public string Origem { get; set; } = "Site";
    public DateTime? DataPagamento { get; set; }
    public DateTime? DataEnvio { get; set; }
    public DateTime? DataEntrega { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PedidoItemDto>? Itens { get; set; }
}

public class PedidoItemDto
{
    public int Id { get; set; }
    public int? ProdutoId { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public string? SkuProduto { get; set; }
    public string? ImagemProduto { get; set; }
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal PrecoTotal { get; set; }
}

public class CriarPedidoDto
{
    public int ClienteId { get; set; }
    public int? EnderecoEntregaId { get; set; }
    public int? LojaId { get; set; }
    public List<CriarPedidoItemDto> Itens { get; set; } = new();
    public string? CupomCodigo { get; set; }
    public string? ObservacoesCliente { get; set; }
    public string Origem { get; set; } = "Site";
}

public class CriarPedidoItemDto
{
    public int ProdutoId { get; set; }
    public int Quantidade { get; set; } = 1;
    public string? Variacao { get; set; }
}

public class AtualizarStatusPedidoDto
{
    public string Status { get; set; } = string.Empty;
    public string? ObservacoesInternas { get; set; }
}
