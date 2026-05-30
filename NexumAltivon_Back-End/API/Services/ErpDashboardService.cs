using Microsoft.EntityFrameworkCore;
using NexumAltivon.API.Controllers.ERP;
using NexumAltivon.API.Data;

namespace NexumAltivon.API.Services
{
    /// <summary>
    /// Serviço de Dashboard ERP — KPIs consolidados em tempo real
    /// Grupo Nexum Altivon ME | www.nexumaltivon.com
    /// </summary>
    public class ErpDashboardService : IErpDashboardService
    {
        private readonly NexumDbContext _context;
        private readonly ILogger<ErpDashboardService> _logger;

        public ErpDashboardService(NexumDbContext context, ILogger<ErpDashboardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ErpDashboardCompletoDto> ObterDashboardCompletoAsync()
        {
            _logger.LogInformation("Consultando dashboard ERP completo");

            return new ErpDashboardCompletoDto
            {
                Financeiro = await ObterKpiFinanceiroAsync(),
                Crm = await ObterKpiCrmAsync(),
                Estoque = await ObterKpiEstoqueAsync(),
                Fiscal = await ObterKpiFiscalAsync(),
                Alertas = await ObterAlertasAtivosAsync(),
                AtualizadoEm = DateTime.Now
            };
        }

        public async Task<FinanceiroKpiDto> ObterKpiFinanceiroAsync()
        {
            var hoje = DateTime.Today;
            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
            var inicioMesAnterior = inicioMes.AddMonths(-1);

            var contasReceber = await _context.ContasReceber.ToListAsync();
            var contasPagar = await _context.ContasPagar.ToListAsync();
            var fluxoCaixa = await _context.FluxoCaixa
                .Where(f => f.Data >= inicioMes)
                .ToListAsync();

            var receberVencidas = contasReceber.Where(c => c.Vencimento < hoje && c.Status != "RECEBIDO").ToList();
            var pagarVencidas = contasPagar.Where(c => c.Vencimento < hoje && c.Status != "PAGO").ToList();

            var faturamentoMes = fluxoCaixa.Where(f => f.Tipo == "ENTRADA").Sum(f => f.Valor);
            var faturamentoMesAnterior = await _context.FluxoCaixa
                .Where(f => f.Data >= inicioMesAnterior && f.Data < inicioMes && f.Tipo == "ENTRADA")
                .SumAsync(f => f.Valor);

            var variacao = faturamentoMesAnterior > 0
                ? ((faturamentoMes - faturamentoMesAnterior) / faturamentoMesAnterior) * 100
                : 0;

            var projecao7Dias = contasReceber
                .Where(c => c.Vencimento >= hoje && c.Vencimento <= hoje.AddDays(7) && c.Status == "PENDENTE")
                .Sum(c => c.Valor);

            var pedidosMes = await _context.Pedidos
                .Where(p => p.DataPedido >= inicioMes && p.Status == "PAGO")
                .CountAsync();

            var ticketMedio = pedidosMes > 0 ? faturamentoMes / pedidosMes : 0;

            return new FinanceiroKpiDto
            {
                SaldoDisponivel = await _context.ContasBancarias.SumAsync(c => c.SaldoAtual),
                ContasReceberTotal = contasReceber.Where(c => c.Status == "PENDENTE").Sum(c => c.Valor),
                ContasPagarTotal = contasPagar.Where(c => c.Status == "PENDENTE").Sum(c => c.Valor),
                ContasReceberVencidas = receberVencidas.Sum(c => c.Valor),
                ContasPagarVencidas = pagarVencidas.Sum(c => c.Valor),
                DiasContasReceberVencidas = receberVencidas.Any() ? receberVencidas.Min(c => (hoje - c.Vencimento).Days) : 0,
                DiasContasPagarVencidas = pagarVencidas.Any() ? pagarVencidas.Min(c => (hoje - c.Vencimento).Days) : 0,
                Projecao7Dias = projecao7Dias,
                FaturamentoMesAtual = faturamentoMes,
                FaturamentoMesAnterior = faturamentoMesAnterior,
                VariacaoFaturamento = Math.Round(variacao, 2),
                TicketMedio = Math.Round(ticketMedio, 2)
            };
        }

        public async Task<CrmKpiDto> ObterKpiCrmAsync()
        {
            var hoje = DateTime.Today;
            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);

            var leads = await _context.LeadsCrm.ToListAsync();
            var leadsMes = leads.Where(l => l.DataCriacao >= inicioMes).ToList();
            var convertidosMes = leadsMes.Count(l => l.Status == "CONVERTIDO");

            var taxaConversao = leadsMes.Any()
                ? (decimal)convertidosMes / leadsMes.Count * 100
                : 0;

            var pipeline = leads
                .Where(l => l.Status != "CONVERTIDO" && l.Status != "PERDIDO")
                .GroupBy(l => l.Status)
                .Select(g => new PipelineStatusDto
                {
                    Status = g.Key,
                    Quantidade = g.Count(),
                    ValorEstimado = g.Sum(l => l.ValorEstimado)
                })
                .ToList();

            return new CrmKpiDto
            {
                TotalLeads = leads.Count,
                LeadsNovosMes = leadsMes.Count,
                LeadsConvertidosMes = convertidosMes,
                TaxaConversao = Math.Round(taxaConversao, 2),
                ValorPipelineTotal = pipeline.Sum(p => p.ValorEstimado),
                TarefasPendentes = await _context.TarefasCrm.CountAsync(t => t.Status != "CONCLUIDA"),
                TarefasAtrasadas = await _context.TarefasCrm.CountAsync(t => t.Status != "CONCLUIDA" && t.DataVencimento < hoje),
                InteracoesHoje = await _context.InteracoesCrm.CountAsync(i => i.DataInteracao.Date == hoje),
                Pipeline = pipeline
            };
        }

        public async Task<EstoqueKpiDto> ObterKpiEstoqueAsync()
        {
            var produtos = await _context.Produtos.ToListAsync();
            var movimentacoes = await _context.MovimentacoesEstoque.ToListAsync();

            var itensCriticos = produtos.Count(p => p.Estoque <= 5);
            var itensAtencao = produtos.Count(p => p.Estoque > 5 && p.Estoque <= 10);

            var valorTotal = produtos.Sum(p => p.Estoque * p.PrecoCusto);
            var valorCritico = produtos.Where(p => p.Estoque <= 5).Sum(p => p.Estoque * p.PrecoCusto);

            var inicio90Dias = DateTime.Today.AddDays(-90);
            var saidas90Dias = movimentacoes
                .Where(m => m.Tipo == "SAIDA" && m.Data >= inicio90Dias)
                .GroupBy(m => m.ProdutoId)
                .Select(g => new { ProdutoId = g.Key, Total = g.Sum(m => m.Quantidade) })
                .ToList();

            var giroMedio = saidas90Dias.Any()
                ? saidas90Dias.Average(s => (decimal)s.Total / 90 * 30)
                : 0;

            return new EstoqueKpiDto
            {
                TotalProdutos = produtos.Count,
                ItensCriticos = itensCriticos,
                ItensAtencao = itensAtencao,
                ValorTotalEstoque = Math.Round(valorTotal, 2),
                ValorEstoqueCritico = Math.Round(valorCritico, 2),
                InventariosPendentes = await _context.Inventarios.CountAsync(i => i.Status == "EM_ANDAMENTO"),
                TransferenciasPendentes = await _context.MovimentacoesEstoque.CountAsync(m => m.Tipo == "TRANSFERENCIA" && m.Status == "PENDENTE"),
                GiroMedioDias = Math.Round(giroMedio, 2)
            };
        }

        public async Task<FiscalKpiDto> ObterKpiFiscalAsync()
        {
            var hoje = DateTime.Today;
            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);

            var nfesMes = await _context.NotasFiscais
                .Where(n => n.DataEmissao >= inicioMes)
                .ToListAsync();

            var valorImpostos = await _context.ItensNotaFiscal
                .Where(i => i.NotaFiscal.DataEmissao >= inicioMes)
                .SumAsync(i => i.ValorIcms + i.ValorIpi + i.ValorPis + i.ValorCofins);

            return new FiscalKpiDto
            {
                NFeEmitidasMes = nfesMes.Count(n => n.Status == "AUTORIZADA"),
                NFePendentes = await _context.NotasFiscais.CountAsync(n => n.Status == "PENDENTE"),
                NFeCanceladasMes = nfesMes.Count(n => n.Status == "CANCELADA"),
                ValorTotalNFeMes = nfesMes.Where(n => n.Status == "AUTORIZADA").Sum(n => n.ValorTotal),
                ValorImpostosMes = Math.Round(valorImpostos, 2),
                AlertasFiscais = await _context.NotasFiscais.CountAsync(n => n.Status == "REJEITADA" || n.Status == "DENEGADA")
            };
        }

        public async Task<ResumoExecutivoDto> ObterResumoExecutivoAsync()
        {
            var hoje = DateTime.Today;
            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);

            var faturamento = await _context.FluxoCaixa
                .Where(f => f.Data >= inicioMes && f.Tipo == "ENTRADA")
                .SumAsync(f => f.Valor);

            var despesas = await _context.FluxoCaixa
                .Where(f => f.Data >= inicioMes && f.Tipo == "SAIDA")
                .SumAsync(f => f.Valor);

            var lucro = faturamento - despesas;
            var margem = faturamento > 0 ? (lucro / faturamento) * 100 : 0;

            var pedidosMes = await _context.Pedidos
                .Where(p => p.DataPedido >= inicioMes && p.Status == "PAGO")
                .CountAsync();

            var clientesAtivos = await _context.Clientes
                .Where(c => c.Pedidos.Any(p => p.DataPedido >= inicioMes.AddMonths(-3)))
                .CountAsync();

            var marketingSpend = await _context.ContasPagar
                .Where(c => c.CentroCusto == "MARKETING" && c.Data >= inicioMes.AddMonths(-3))
                .SumAsync(c => c.Valor);

            var novosClientes3M = await _context.Clientes
                .Where(c => c.DataCadastro >= inicioMes.AddMonths(-3))
                .CountAsync();

            var cac = novosClientes3M > 0 ? marketingSpend / novosClientes3M : 0;
            var ticketMedio = pedidosMes > 0 ? faturamento / pedidosMes : 0;
            var ltv = ticketMedio * 3;

            return new ResumoExecutivoDto
            {
                FaturamentoTotal = Math.Round(faturamento, 2),
                LucroLiquido = Math.Round(lucro, 2),
                MargemLiquida = Math.Round(margem, 2),
                PedidosMes = pedidosMes,
                ClientesAtivos = clientesAtivos,
                CAC = Math.Round(cac, 2),
                LTV = Math.Round(ltv, 2),
                IndiceSatisfacao = 4.7m
            };
        }

        public async Task<List<MensalFinanceiroDto>> ObterEvolucaoFinanceira12MesesAsync()
        {
            var hoje = DateTime.Today;
            var resultado = new List<MensalFinanceiroDto>();

            for (int i = 11; i >= 0; i--)
            {
                var inicio = new DateTime(hoje.Year, hoje.Month, 1).AddMonths(-i);
                var fim = inicio.AddMonths(1).AddDays(-1);

                var receitas = await _context.FluxoCaixa
                    .Where(f => f.Data >= inicio && f.Data <= fim && f.Tipo == "ENTRADA")
                    .SumAsync(f => f.Valor);

                var despesas = await _context.FluxoCaixa
                    .Where(f => f.Data >= inicio && f.Data <= fim && f.Tipo == "SAIDA")
                    .SumAsync(f => f.Valor);

                resultado.Add(new MensalFinanceiroDto
                {
                    Mes = inicio.ToString("MMM/yyyy"),
                    Receitas = Math.Round(receitas, 2),
                    Despesas = Math.Round(despesas, 2),
                    Lucro = Math.Round(receitas - despesas, 2)
                });
            }

            return resultado;
        }

        public async Task<List<PipelineDistribuicaoDto>> ObterDistribuicaoPipelineAsync()
        {
            var cores = new Dictionary<string, string>
            {
                ["NOVO"] = "#3B82F6",
                ["CONTATO"] = "#8B5CF6",
                ["PROPOSTA"] = "#F59E0B",
                ["NEGOCIACAO"] = "#EF4444",
                ["CONVERTIDO"] = "#10B981",
                ["PERDIDO"] = "#6B7280"
            };

            var leads = await _context.LeadsCrm.ToListAsync();

            return leads
                .GroupBy(l => l.Status)
                .Select(g => new PipelineDistribuicaoDto
                {
                    Status = g.Key,
                    Quantidade = g.Count(),
                    Valor = g.Sum(l => l.ValorEstimado),
                    Cor = cores.GetValueOrDefault(g.Key, "#6B7280")
                })
                .OrderByDescending(p => p.Valor)
                .ToList();
        }

        public async Task<List<GiroEstoqueDto>> ObterTopGiroEstoqueAsync(int top)
        {
            var inicio90Dias = DateTime.Today.AddDays(-90);

            var saidas = await _context.MovimentacoesEstoque
                .Where(m => m.Tipo == "SAIDA" && m.Data >= inicio90Dias)
                .GroupBy(m => m.ProdutoId)
                .Select(g => new
                {
                    ProdutoId = g.Key,
                    QuantidadeVendida = g.Sum(m => m.Quantidade)
                })
                .OrderByDescending(g => g.QuantidadeVendida)
                .Take(top)
                .ToListAsync();

            var produtoIds = saidas.Select(s => s.ProdutoId).ToList();
            var produtos = await _context.Produtos
                .Where(p => produtoIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            return saidas.Select(s =>
            {
                var produto = produtos.GetValueOrDefault(s.ProdutoId);
                var saldo = produto?.Estoque ?? 0;
                var giro = saldo > 0 ? (decimal)s.QuantidadeVendida / saldo * 30 : 0;

                return new GiroEstoqueDto
                {
                    ProdutoId = s.ProdutoId,
                    ProdutoNome = produto?.Nome ?? "Desconhecido",
                    QuantidadeVendida = s.QuantidadeVendida,
                    SaldoAtual = saldo,
                    GiroDias = Math.Round(giro, 2)
                };
            }).ToList();
        }

        public async Task<List<AlertaErpDto>> ObterAlertasAtivosAsync()
        {
            var alertas = new List<AlertaErpDto>();
            var hoje = DateTime.Today;

            var estoqueCritico = await _context.Produtos
                .Where(p => p.Estoque <= 5)
                .Take(5)
                .ToListAsync();

            foreach (var produto in estoqueCritico)
            {
                alertas.Add(new AlertaErpDto
                {
                    Tipo = "ESTOQUE",
                    Severidade = "ALTA",
                    Mensagem = $"Produto '{produto.Nome}' com estoque crítico: {produto.Estoque} unidades",
                    LinkAcao = $"/erp/estoque/produto/{produto.Id}",
                    GeradoEm = DateTime.Now
                });
            }

            var contasVencidas = await _context.ContasPagar
                .Where(c => c.Vencimento < hoje && c.Status != "PAGO")
                .Take(5)
                .ToListAsync();

            foreach (var conta in contasVencidas)
            {
                alertas.Add(new AlertaErpDto
                {
                    Tipo = "FINANCEIRO",
                    Severidade = "ALTA",
                    Mensagem = $"Conta a pagar vencida: {conta.Descricao} — R$ {conta.Valor:N2} (vencida há {(hoje - conta.Vencimento).Days} dias)",
                    LinkAcao = $"/erp/financeiro/contas-pagar/{conta.Id}",
                    GeradoEm = DateTime.Now
                });
            }

            var tarefasAtrasadas = await _context.TarefasCrm
                .Where(t => t.Status != "CONCLUIDA" && t.DataVencimento < hoje)
                .Take(5)
                .ToListAsync();

            foreach (var tarefa in tarefasAtrasadas)
            {
                alertas.Add(new AlertaErpDto
                {
                    Tipo = "CRM",
                    Severidade = "MEDIA",
                    Mensagem = $"Tarefa atrasada: {tarefa.Titulo} (vencida em {tarefa.DataVencimento:dd/MM/yyyy})",
                    LinkAcao = $"/erp/crm/tarefas/{tarefa.Id}",
                    GeradoEm = DateTime.Now
                });
            }

            var nfesPendentes = await _context.NotasFiscais
                .Where(n => n.Status == "PENDENTE")
                .CountAsync();

            if (nfesPendentes > 0)
            {
                alertas.Add(new AlertaErpDto
                {
                    Tipo = "FISCAL",
                    Severidade = "MEDIA",
                    Mensagem = $"{nfesPendentes} nota(s) fiscal(is) pendente(s) de emissão",
                    LinkAcao = "/erp/fiscal/pendentes",
                    GeradoEm = DateTime.Now
                });
            }

            return alertas.OrderBy(a => a.Severidade == "ALTA" ? 0 : 1).ToList();
        }
    }
}
