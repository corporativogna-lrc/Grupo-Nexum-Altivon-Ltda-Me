/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */
using Microsoft.EntityFrameworkCore;
using NexumAltivon.API.Data;
using NexumAltivon.API.DTOs.Admin;
using NexumAltivon.API.Models;

namespace NexumAltivon.API.Services.Admin;

public interface IAdminDashboardService
{
    Task<DashboardCompletoDto> ObterDashboardCompletoAsync();
    Task<DashboardKpiDto> ObterKpisAsync();
    Task<List<FaturamentoPorPeriodoDto>> ObterFaturamentoSemanalAsync();
    Task<List<FaturamentoPorPeriodoDto>> ObterFaturamentoMensalAsync(int meses = 12);
    Task<List<VendasPorLojaDto>> ObterVendasPorLojaAsync(DateTime? inicio = null, DateTime? fim = null);
    Task<List<ProdutosMaisVendidosDto>> ObterProdutosMaisVendidosAsync(int top = 10);
    Task<List<ClientesRecentesDto>> ObterClientesRecentesAsync(int quantidade = 10);
    Task<List<PedidosRecentesDto>> ObterPedidosRecentesAsync(int quantidade = 10);
    Task<List<LeadsRecentesDto>> ObterLeadsRecentesAsync(int quantidade = 10);
}

public class AdminDashboardService : IAdminDashboardService
{
    private readonly NexumDbContext _context;

    public AdminDashboardService(NexumDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardCompletoDto> ObterDashboardCompletoAsync()
    {
        return new DashboardCompletoDto
        {
            Kpis = await ObterKpisAsync(),
            FaturamentoSemanal = await ObterFaturamentoSemanalAsync(),
            FaturamentoMensal = await ObterFaturamentoMensalAsync(),
            VendasPorLoja = await ObterVendasPorLojaAsync(),
            ProdutosMaisVendidos = await ObterProdutosMaisVendidosAsync(),
            ClientesRecentes = await ObterClientesRecentesAsync(),
            PedidosRecentes = await ObterPedidosRecentesAsync(),
            LeadsRecentes = await ObterLeadsRecentesAsync()
        };
    }

    public async Task<DashboardKpiDto> ObterKpisAsync()
    {
        var hoje = DateTime.UtcNow.Date;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
        var inicioAno = new DateTime(hoje.Year, 1, 1);

        var pedidosHoje = await _context.Pedidos
            .Where(p => p.CreatedAt >= hoje && p.Status != StatusPedido.Cancelado && p.Status != StatusPedido.Reembolsado)
            .ToListAsync();

        var pedidosMes = await _context.Pedidos
            .Where(p => p.CreatedAt >= inicioMes && p.Status != StatusPedido.Cancelado && p.Status != StatusPedido.Reembolsado)
            .ToListAsync();

        var pedidosAno = await _context.Pedidos
            .Where(p => p.CreatedAt >= inicioAno && p.Status != StatusPedido.Cancelado && p.Status != StatusPedido.Reembolsado)
            .ToListAsync();

        var todosPedidos = await _context.Pedidos
            .Where(p => p.Status != StatusPedido.Cancelado && p.Status != StatusPedido.Reembolsado)
            .ToListAsync();

        var clientesMes = await _context.Clientes
            .Where(c => c.CreatedAt >= inicioMes)
            .CountAsync();

        var totalClientes = await _context.Clientes.CountAsync();
        var clientesAtivos = await _context.Clientes
            .Where(c => c.Status == StatusCliente.Ativo)
            .CountAsync();

        var produtosAtivos = await ProdutosPublicaveis()
            .CountAsync();

        var produtosEstoqueBaixo = await ProdutosPublicaveis()
            .Where(p => p.EstoqueAtual <= p.EstoqueMinimo)
            .CountAsync();

        var produtosSemEstoque = await ProdutosPublicaveis()
            .Where(p => p.EstoqueAtual == 0)
            .CountAsync();

        var leadsNovos = await _context.CrmLeads
            .Where(l => l.Status == StatusLead.Novo)
            .CountAsync();

        var leadsConvertidos = await _context.CrmLeads
            .Where(l => l.Status == StatusLead.Convertido)
            .CountAsync();

        var leadsEmAtendimento = await _context.CrmLeads
            .Where(l => l.Status == StatusLead.EmAtendimento)
            .CountAsync();

        var ticketMedio = todosPedidos.Any() ? todosPedidos.Average(p => p.Total) : 0;
        var taxaConversao = 0m;

        return new DashboardKpiDto
        {
            FaturamentoHoje = pedidosHoje.Sum(p => p.Total),
            FaturamentoMes = pedidosMes.Sum(p => p.Total),
            FaturamentoAno = pedidosAno.Sum(p => p.Total),
            PedidosHoje = pedidosHoje.Count,
            PedidosMes = pedidosMes.Count,
            PedidosPendentes = await _context.Pedidos.Where(p => p.Status == StatusPedido.Pendente).CountAsync(),
            PedidosEnviados = await _context.Pedidos.Where(p => p.Status == StatusPedido.Enviado).CountAsync(),
            PedidosEntregues = await _context.Pedidos.Where(p => p.Status == StatusPedido.Entregue).CountAsync(),
            ClientesNovosMes = clientesMes,
            ClientesAtivos = clientesAtivos,
            TotalClientes = totalClientes,
            TicketMedio = ticketMedio,
            TaxaConversao = taxaConversao,
            ProdutosAtivos = produtosAtivos,
            ProdutosEstoqueBaixo = produtosEstoqueBaixo,
            ProdutosSemEstoque = produtosSemEstoque,
            LeadsNovos = leadsNovos,
            LeadsConvertidos = leadsConvertidos,
            LeadsEmAtendimento = leadsEmAtendimento
        };
    }

    public async Task<List<FaturamentoPorPeriodoDto>> ObterFaturamentoSemanalAsync()
    {
        var hoje = DateTime.UtcNow.Date;
        var dias = Enumerable.Range(0, 7)
            .Select(i => hoje.AddDays(-6 + i))
            .ToList();

        var resultado = new List<FaturamentoPorPeriodoDto>();
        foreach (var dia in dias)
        {
            var pedidosDia = await _context.Pedidos
                .Where(p => p.CreatedAt >= dia && p.CreatedAt < dia.AddDays(1)
                    && p.Status != StatusPedido.Cancelado && p.Status != StatusPedido.Reembolsado)
                .ToListAsync();

            resultado.Add(new FaturamentoPorPeriodoDto
            {
                Periodo = dia.ToString("dd/MM"),
                Faturamento = pedidosDia.Sum(p => p.Total),
                QuantidadePedidos = pedidosDia.Count
            });
        }
        return resultado;
    }

    public async Task<List<FaturamentoPorPeriodoDto>> ObterFaturamentoMensalAsync(int meses = 12)
    {
        var hoje = DateTime.UtcNow;
        var resultado = new List<FaturamentoPorPeriodoDto>();

        for (int i = meses - 1; i >= 0; i--)
        {
            var mes = hoje.AddMonths(-i);
            var inicioMes = new DateTime(mes.Year, mes.Month, 1);
            var fimMes = inicioMes.AddMonths(1);

            var pedidosMes = await _context.Pedidos
                .Where(p => p.CreatedAt >= inicioMes && p.CreatedAt < fimMes
                    && p.Status != StatusPedido.Cancelado && p.Status != StatusPedido.Reembolsado)
                .ToListAsync();

            resultado.Add(new FaturamentoPorPeriodoDto
            {
                Periodo = mes.ToString("MMM/yy"),
                Faturamento = pedidosMes.Sum(p => p.Total),
                QuantidadePedidos = pedidosMes.Count
            });
        }
        return resultado;
    }

    public async Task<List<VendasPorLojaDto>> ObterVendasPorLojaAsync(DateTime? inicio = null, DateTime? fim = null)
    {
        var dataInicio = inicio ?? DateTime.UtcNow.AddMonths(-1);
        var dataFim = fim ?? DateTime.UtcNow;

        var pedidos = await _context.Pedidos
            .Include(p => p.Loja)
            .Where(p => p.CreatedAt >= dataInicio && p.CreatedAt <= dataFim
                && p.Status != StatusPedido.Cancelado && p.Status != StatusPedido.Reembolsado)
            .ToListAsync();

        var totalFaturamento = pedidos.Sum(p => p.Total);

        var vendasPorLoja = pedidos
            .GroupBy(p => new { p.LojaId, p.Loja?.Nome, p.Loja?.Slug })
            .Select(g => new VendasPorLojaDto
            {
                LojaNome = g.Key.Nome ?? "Sem Loja",
                LojaSlug = g.Key.Slug ?? "",
                Faturamento = g.Sum(p => p.Total),
                Pedidos = g.Count(),
                TicketMedio = g.Any() ? g.Average(p => p.Total) : 0,
                Percentual = totalFaturamento > 0 ? g.Sum(p => p.Total) / totalFaturamento * 100 : 0
            })
            .OrderByDescending(v => v.Faturamento)
            .ToList();

        return vendasPorLoja;
    }

    public async Task<List<ProdutosMaisVendidosDto>> ObterProdutosMaisVendidosAsync(int top = 10)
    {
        var produtos = await _context.PedidoItens
            .Include(pi => pi.Produto)
            .ThenInclude(p => p!.Loja)
            .Where(pi => pi.Pedido != null && pi.Pedido.Status != StatusPedido.Cancelado && pi.Pedido.Status != StatusPedido.Reembolsado)
            .GroupBy(pi => new { pi.ProdutoId, pi.NomeProduto, pi.ImagemProduto, pi.Produto!.Loja!.Nome })
            .Select(g => new ProdutosMaisVendidosDto
            {
                ProdutoId = g.Key.ProdutoId ?? 0,
                Nome = g.Key.NomeProduto,
                Imagem = g.Key.ImagemProduto,
                LojaNome = g.Key.Nome,
                QuantidadeVendida = g.Sum(pi => pi.Quantidade),
                ReceitaTotal = g.Sum(pi => pi.PrecoTotal)
            })
            .OrderByDescending(p => p.QuantidadeVendida)
            .Take(top)
            .ToListAsync();

        return produtos;
    }

    public async Task<List<ClientesRecentesDto>> ObterClientesRecentesAsync(int quantidade = 10)
    {
        var clientes = await _context.Clientes
            .OrderByDescending(c => c.CreatedAt)
            .Take(quantidade)
            .ToListAsync();

        var resultado = new List<ClientesRecentesDto>();
        foreach (var cliente in clientes)
        {
            var pedidosCliente = await _context.Pedidos
                .Where(p => p.ClienteId == cliente.Id && p.Status != StatusPedido.Cancelado && p.Status != StatusPedido.Reembolsado)
                .ToListAsync();

            resultado.Add(new ClientesRecentesDto
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Email = cliente.Email,
                Whatsapp = cliente.Whatsapp,
                DataCadastro = cliente.CreatedAt,
                TotalPedidos = pedidosCliente.Count,
                TotalGasto = pedidosCliente.Sum(p => p.Total)
            });
        }
        return resultado;
    }

    public async Task<List<PedidosRecentesDto>> ObterPedidosRecentesAsync(int quantidade = 10)
    {
        return await _context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Loja)
            .OrderByDescending(p => p.CreatedAt)
            .Take(quantidade)
            .Select(p => new PedidosRecentesDto
            {
                Id = p.Id,
                NumeroPedido = p.NumeroPedido,
                ClienteNome = p.Cliente != null ? p.Cliente.Nome : "N/A",
                Total = p.Total,
                Status = p.Status.ToString(),
                StatusPagamento = p.StatusPagamento.ToString(),
                LojaNome = p.Loja != null ? p.Loja.Nome : null,
                DataPedido = p.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<LeadsRecentesDto>> ObterLeadsRecentesAsync(int quantidade = 10)
    {
        return await _context.CrmLeads
            .OrderByDescending(l => l.CreatedAt)
            .Take(quantidade)
            .Select(l => new LeadsRecentesDto
            {
                Id = l.Id,
                Nome = l.Nome,
                Tipo = l.Tipo.ToString(),
                Status = l.Status.ToString(),
                Prioridade = l.Prioridade.ToString(),
                Email = l.Email,
                Whatsapp = l.Whatsapp,
                DataCriacao = l.CreatedAt
            })
            .ToListAsync();
    }

    private IQueryable<Produto> ProdutosPublicaveis() =>
        _context.Produtos.Where(produto =>
            produto.Ativo &&
            produto.LojaId > 0 &&
            produto.CategoriaId.HasValue &&
            !string.IsNullOrEmpty(produto.Nome) &&
            !string.IsNullOrEmpty(produto.Sku) &&
            !string.IsNullOrEmpty(produto.Slug) &&
            (!string.IsNullOrEmpty(produto.DescricaoCurta) || !string.IsNullOrEmpty(produto.DescricaoLonga)) &&
            !string.IsNullOrEmpty(produto.ImagemPrincipal) &&
            produto.Preco > 0 &&
            produto.Peso > 0 &&
            produto.Altura > 0 &&
            produto.Largura > 0 &&
            produto.Comprimento > 0);
}
