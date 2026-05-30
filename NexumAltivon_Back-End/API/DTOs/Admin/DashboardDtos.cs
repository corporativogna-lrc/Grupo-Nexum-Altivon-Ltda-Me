namespace NexumAltivon.API.DTOs.Admin;

public class DashboardKpiDto
{
    public decimal FaturamentoHoje { get; set; }
    public decimal FaturamentoMes { get; set; }
    public decimal FaturamentoAno { get; set; }
    public int PedidosHoje { get; set; }
    public int PedidosMes { get; set; }
    public int PedidosPendentes { get; set; }
    public int PedidosEnviados { get; set; }
    public int PedidosEntregues { get; set; }
    public int ClientesNovosMes { get; set; }
    public int ClientesAtivos { get; set; }
    public int TotalClientes { get; set; }
    public decimal TicketMedio { get; set; }
    public decimal TaxaConversao { get; set; }
    public int ProdutosAtivos { get; set; }
    public int ProdutosEstoqueBaixo { get; set; }
    public int ProdutosSemEstoque { get; set; }
    public int LeadsNovos { get; set; }
    public int LeadsConvertidos { get; set; }
    public int LeadsEmAtendimento { get; set; }
}

public class FaturamentoPorPeriodoDto
{
    public string Periodo { get; set; } = string.Empty;
    public decimal Faturamento { get; set; }
    public int QuantidadePedidos { get; set; }
}

public class VendasPorLojaDto
{
    public string LojaNome { get; set; } = string.Empty;
    public string LojaSlug { get; set; } = string.Empty;
    public decimal Faturamento { get; set; }
    public int Pedidos { get; set; }
    public decimal TicketMedio { get; set; }
    public decimal Percentual { get; set; }
}

public class ProdutosMaisVendidosDto
{
    public int ProdutoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Imagem { get; set; }
    public string LojaNome { get; set; } = string.Empty;
    public int QuantidadeVendida { get; set; }
    public decimal ReceitaTotal { get; set; }
}

public class ClientesRecentesDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Whatsapp { get; set; }
    public DateTime DataCadastro { get; set; }
    public int TotalPedidos { get; set; }
    public decimal TotalGasto { get; set; }
}

public class PedidosRecentesDto
{
    public int Id { get; set; }
    public string NumeroPedido { get; set; } = string.Empty;
    public string ClienteNome { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusPagamento { get; set; } = string.Empty;
    public string? LojaNome { get; set; }
    public DateTime DataPedido { get; set; }
}

public class LeadsRecentesDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Prioridade { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Whatsapp { get; set; }
    public DateTime DataCriacao { get; set; }
}

public class DashboardCompletoDto
{
    public DashboardKpiDto Kpis { get; set; } = new();
    public List<FaturamentoPorPeriodoDto> FaturamentoSemanal { get; set; } = new();
    public List<FaturamentoPorPeriodoDto> FaturamentoMensal { get; set; } = new();
    public List<VendasPorLojaDto> VendasPorLoja { get; set; } = new();
    public List<ProdutosMaisVendidosDto> ProdutosMaisVendidos { get; set; } = new();
    public List<ClientesRecentesDto> ClientesRecentes { get; set; } = new();
    public List<PedidosRecentesDto> PedidosRecentes { get; set; } = new();
    public List<LeadsRecentesDto> LeadsRecentes { get; set; } = new();
}
