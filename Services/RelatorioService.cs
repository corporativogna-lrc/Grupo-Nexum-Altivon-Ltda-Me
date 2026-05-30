using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NexumAltivon.ERP.Data;
using NexumAltivon.ERP.DTOs;
using NexumAltivon.ERP.Models;

namespace NexumAltivon.ERP.Services
{
    /// <summary>
    /// Serviço de relatórios gerenciais — DRE, Fluxo de Caixa, Posição de Estoque, Pipeline CRM
    /// Gera saída em formato estruturado para exportação PDF/Excel posterior
    /// </summary>
    public interface IRelatorioService
    {
        Task<DreDto> GerarDREAsync(DateTime inicio, DateTime fim);
        Task<IEnumerable<FluxoCaixaDto>> GerarFluxoCaixaAsync(DateTime inicio, DateTime fim);
        Task<IEnumerable<PosicaoEstoqueDto>> GerarPosicaoEstoqueAsync(int? lojaId = null, int? produtoId = null, bool apenasBaixo = false);
        Task<IEnumerable<ContasVencidasDto>> GerarContasVencidasAsync();
        Task<IEnumerable<VendasPorLojaDto>> GerarVendasPorLojaAsync(DateTime inicio, DateTime fim);
        Task<ResumoGeralErpDto> GerarResumoGeralAsync();
        Task<RelatorioGeradoDto> GerarRelatorioAsync(FiltroRelatorioDto filtro, int usuarioId);
        Task<object> ObterDadosRelatorioAsync(FiltroRelatorioDto filtro);
        Task<byte[]> GerarExcelAsync(object dados, string tipoRelatorio);
        Task<byte[]> GerarPdfAsync(string html);
        Task<byte[]> ExportarDREPdfAsync(DateTime inicio, DateTime fim);
        Task<byte[]> ExportarPosicaoEstoqueExcelAsync(int? lojaId = null);
    }

    public class RelatorioService : IRelatorioService
    {
        private readonly NexumDbContext _context;

        public RelatorioService(NexumDbContext context)
        {
            _context = context;
        }

        public async Task<DreDto> GerarDREAsync(DateTime inicio, DateTime fim)
        {
            var receitas = await _context.FluxoCaixa
                .Where(f => f.Tipo == "Entrada" && f.Data >= inicio && f.Data <= fim)
                .SumAsync(f => f.Valor);

            var despesas = await _context.FluxoCaixa
                .Where(f => f.Tipo == "Saida" && f.Data >= inicio && f.Data <= fim)
                .SumAsync(f => f.Valor);

            var impostos = receitas * 0.12m;
            var cmv = despesas * 0.60m;
            var despesasOperacionais = despesas * 0.25m;
            var despesasFinanceiras = despesas * 0.05m;
            var receitasFinanceiras = receitas * 0.02m;

            return new DreDto
            {
                Periodo = $"{inicio:dd/MM/yyyy} a {fim:dd/MM/yyyy}",
                ReceitaBruta = receitas,
                Impostos = impostos,
                CustoProdutosVendidos = cmv,
                DespesasOperacionais = despesasOperacionais,
                DespesasFinanceiras = despesasFinanceiras,
                ReceitasFinanceiras = receitasFinanceiras
            };
        }

        public async Task<IEnumerable<FluxoCaixaDto>> GerarFluxoCaixaAsync(DateTime inicio, DateTime fim)
        {
            return await _context.FluxoCaixa
                .Where(f => f.Data >= inicio && f.Data <= fim)
                .OrderBy(f => f.Data)
                .Select(f => new FluxoCaixaDto
                {
                    Id = f.Id,
                    Data = f.Data,
                    Tipo = f.Tipo,
                    Descricao = f.Descricao,
                    Valor = f.Valor,
                    Categoria = f.Categoria,
                    FormaPagamento = f.FormaPagamento,
                    ContaBancaria = f.ContaBancaria,
                    CriadoEm = f.CriadoEm
                }).ToListAsync();
        }

        public async Task<IEnumerable<PosicaoEstoqueDto>> GerarPosicaoEstoqueAsync(int? lojaId = null, int? produtoId = null, bool apenasBaixo = false)
        {
            var query = _context.Produtos
                .Include(p => p.Loja)
                .AsQueryable();

            if (lojaId.HasValue)
                query = query.Where(p => p.LojaId == lojaId.Value);

            if (produtoId.HasValue)
                query = query.Where(p => p.Id == produtoId.Value);

            var produtos = await query.ToListAsync();

            var movimentacoes = await _context.MovimentacoesEstoque
                .GroupBy(m => m.ProdutoId)
                .Select(g => new
                {
                    ProdutoId = g.Key,
                    UltimaEntrada = g.Where(m => m.Tipo == "Entrada").OrderByDescending(m => m.DataMovimentacao).Select(m => m.DataMovimentacao).FirstOrDefault(),
                    UltimaSaida = g.Where(m => m.Tipo == "Saida").OrderByDescending(m => m.DataMovimentacao).Select(m => m.DataMovimentacao).FirstOrDefault(),
                    TotalEntradas = g.Where(m => m.Tipo == "Entrada").Sum(m => m.Quantidade),
                    TotalSaidas = g.Where(m => m.Tipo == "Saida").Sum(m => m.Quantidade)
                }).ToListAsync();

            var posicoes = produtos.Select(p =>
            {
                var mov = movimentacoes.FirstOrDefault(m => m.ProdutoId == p.Id);
                return new PosicaoEstoqueDto
                {
                    ProdutoId = p.Id,
                    Nome = p.Nome,
                    Sku = p.Sku,
                    EstoqueAtual = p.EstoqueAtual,
                    EstoqueMinimo = p.EstoqueMinimo ?? 0,
                    CustoMedio = p.CustoMedio,
                    ValorTotalEstoque = p.EstoqueAtual * p.CustoMedio,
                    Status = p.EstoqueAtual <= p.EstoqueMinimo ? "CRITICO" :
                             p.EstoqueAtual <= p.EstoqueMinimo * 1.5m ? "ATENCAO" : "NORMAL",
                    LojaNome = p.Loja?.Nome ?? "Sem Loja",
                    UltimaEntrada = mov?.UltimaEntrada,
                    UltimaSaida = mov?.UltimaSaida,
                    Giro = mov != null && mov.TotalEntradas > 0 ? (mov.TotalSaidas / mov.TotalEntradas) * 100 : 0
                };
            }).ToList();

            return apenasBaixo ? posicoes.Where(p => p.EstoqueAtual <= p.EstoqueMinimo).ToList() : posicoes;
        }

        public async Task<IEnumerable<ContasVencidasDto>> GerarContasVencidasAsync()
        {
            var hoje = DateTime.Now.Date;

            var contasPagar = await _context.ContasPagar
                .Where(c => c.Status == "Pendente" && c.DataVencimento.Date < hoje)
                .Include(c => c.Fornecedor)
                .Select(c => new ContasVencidasDto
                {
                    Tipo = "Pagar",
                    Documento = c.NumeroDocumento,
                    Pessoa = c.Fornecedor != null ? c.Fornecedor.RazaoSocial : "",
                    Valor = c.ValorOriginal - c.ValorPago,
                    DataVencimento = c.DataVencimento,
                    DiasAtraso = (hoje - c.DataVencimento.Date).Days,
                    Status = "ATRASADO"
                }).ToListAsync();

            var contasReceber = await _context.ContasReceber
                .Where(c => c.Status == "Pendente" && c.DataVencimento.Date < hoje)
                .Include(c => c.Cliente)
                .Select(c => new ContasVencidasDto
                {
                    Tipo = "Receber",
                    Documento = c.NumeroDocumento,
                    Pessoa = c.Cliente != null ? c.Cliente.Nome : "",
                    Valor = c.ValorOriginal - c.ValorRecebido,
                    DataVencimento = c.DataVencimento,
                    DiasAtraso = (hoje - c.DataVencimento.Date).Days,
                    Status = "ATRASADO"
                }).ToListAsync();

            return contasPagar.Concat(contasReceber).OrderByDescending(c => c.DiasAtraso);
        }

        public async Task<IEnumerable<VendasPorLojaDto>> GerarVendasPorLojaAsync(DateTime inicio, DateTime fim)
        {
            // Busca pedidos pagos no período
            var pedidos = await _context.Pedidos
                .Where(p => p.Status == "Pago" && p.DataPedido >= inicio && p.DataPedido <= fim)
                .Include(p => p.Loja)
                .ToListAsync();

            return pedidos
                .GroupBy(p => p.LojaId)
                .Select(g => new VendasPorLojaDto
                {
                    LojaId = g.Key,
                    LojaNome = g.First().Loja?.Nome ?? "Sem Loja",
                    QuantidadePedidos = g.Count(),
                    ValorTotal = g.Sum(p => p.ValorTotal),
                    TicketMedio = g.Count() > 0 ? g.Sum(p => p.ValorTotal) / g.Count() : 0,
                    PercentualRepresentacao = 0 // Calculado posteriormente
                })
                .ToList();
        }

        public async Task<ResumoGeralErpDto> GerarResumoGeralAsync()
        {
            var hoje = DateTime.Now;
            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);

            var leadsAtivos = await _context.LeadsCRM.CountAsync(l => l.Status != "Convertido" && l.Status != "Perdido");
            var leadsConvertidosMes = await _context.LeadsCRM.CountAsync(l => l.Status == "Convertido" && l.DataConversao >= inicioMes);

            var produtosCriticos = await _context.Produtos.CountAsync(p => p.EstoqueAtual <= p.EstoqueMinimo && p.Ativo);

            var fornecedoresAtivos = await _context.Fornecedores.CountAsync(f => f.Status == "Ativo");
            var fornecedoresDropshipping = await _context.Fornecedores.CountAsync(f => f.Dropshipping && f.Status == "Ativo");
            var contasPagar = await _context.ContasPagar.Where(c => c.Status == "Pendente").ToListAsync();
            var contasReceber = await _context.ContasReceber.Where(c => c.Status == "Pendente").ToListAsync();
            var saldoCaixa = await _context.ContasBancarias.Where(c => c.Ativo).SumAsync(c => c.SaldoAtual);

            return new ResumoGeralErpDto
            {
                TotalContasPagarHoje = contasPagar.Where(c => c.DataVencimento.Date == hoje.Date).Sum(c => c.ValorOriginal - c.ValorPago),
                TotalContasPagarAtrasadas = contasPagar.Where(c => c.DataVencimento.Date < hoje.Date).Sum(c => c.ValorOriginal - c.ValorPago),
                TotalContasReceberHoje = contasReceber.Where(c => c.DataVencimento.Date == hoje.Date).Sum(c => c.ValorOriginal - c.ValorRecebido),
                TotalContasReceberAtrasadas = contasReceber.Where(c => c.DataVencimento.Date < hoje.Date).Sum(c => c.ValorOriginal - c.ValorRecebido),
                SaldoCaixaAtual = saldoCaixa,
                LeadsAtivos = leadsAtivos,
                LeadsConvertidosMes = leadsConvertidosMes,
                ProdutosEstoqueCritico = produtosCriticos,
                FornecedoresAtivos = fornecedoresAtivos,
                FornecedoresDropshipping = fornecedoresDropshipping,
                DataAtualizacao = hoje
            };
        }

        public async Task<RelatorioGeradoDto> GerarRelatorioAsync(FiltroRelatorioDto filtro, int usuarioId)
        {
            return new RelatorioGeradoDto
            {
                TipoRelatorio = filtro.TipoRelatorio,
                Dados = await ObterDadosRelatorioAsync(filtro)
            };
        }

        public async Task<object> ObterDadosRelatorioAsync(FiltroRelatorioDto filtro)
        {
            var inicio = filtro.DataInicio ?? DateTime.Now.AddDays(-30);
            var fim = filtro.DataFim ?? DateTime.Now;

            return filtro.TipoRelatorio?.ToUpperInvariant() switch
            {
                "DRE" => await GerarDREAsync(inicio, fim),
                "ESTOQUE" => await GerarPosicaoEstoqueAsync(filtro.LojaId),
                "CONTAS_VENCIDAS" => await GerarContasVencidasAsync(),
                "VENDAS_LOJA" => await GerarVendasPorLojaAsync(inicio, fim),
                _ => await GerarResumoGeralAsync()
            };
        }

        public Task<byte[]> GerarExcelAsync(object dados, string tipoRelatorio)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(dados);
            return Task.FromResult(System.Text.Encoding.UTF8.GetBytes(json));
        }

        public Task<byte[]> GerarPdfAsync(string html)
        {
            return Task.FromResult(System.Text.Encoding.UTF8.GetBytes(html));
        }

        public async Task<byte[]> ExportarDREPdfAsync(DateTime inicio, DateTime fim)
        {
            var dre = await GerarDREAsync(inicio, fim);

            // Estrutura HTML para geração de PDF (usar DinkToPdf ou similar no projeto)
            var html = $@"<!DOCTYPE html>
<html><head><meta charset='utf-8'>
<style>
body {{ font-family: Arial, sans-serif; margin: 40px; color: #333; }}
h1 {{ color: #C9A227; border-bottom: 3px solid #C9A227; padding-bottom: 10px; }}
h2 {{ color: #0A0A0A; margin-top: 30px; }}
table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
th {{ background: #0A0A0A; color: #C9A227; padding: 12px; text-align: left; }}
td {{ padding: 10px; border-bottom: 1px solid #ddd; }}
.total {{ font-weight: bold; background: #f5f5f5; }}
.lucro {{ color: green; font-weight: bold; }}
.prejuizo {{ color: red; font-weight: bold; }}
.footer {{ margin-top: 40px; font-size: 12px; color: #999; text-align: center; }}
</style></head><body>
<h1>DRE — Grupo Nexum Altivon</h1>
<p><strong>Período:</strong> {dre.Periodo}</p>
<p><strong>Emitido em:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>

<h2>Demonstração do Resultado do Exercício</h2>
<table>
<tr><th>Descrição</th><th style='text-align:right'>Valor (R$)</th><th style='text-align:right'>%</th></tr>
<tr><td>Receita Bruta</td><td style='text-align:right'>{dre.ReceitaBruta:N2}</td><td style='text-align:right'>100,00%</td></tr>
<tr><td>(-) Impostos</td><td style='text-align:right'>({dre.Impostos:N2})</td><td style='text-align:right'>{(dre.ReceitaBruta > 0 ? dre.Impostos / dre.ReceitaBruta * 100 : 0):N2}%</td></tr>
<tr class='total'><td>(=) Receita Líquida</td><td style='text-align:right'>{dre.ReceitaLiquida:N2}</td><td style='text-align:right'>{(dre.ReceitaBruta > 0 ? dre.ReceitaLiquida / dre.ReceitaBruta * 100 : 0):N2}%</td></tr>
<tr><td>(-) CMV</td><td style='text-align:right'>({dre.CustoProdutosVendidos:N2})</td><td style='text-align:right'>{(dre.ReceitaLiquida > 0 ? dre.CustoProdutosVendidos / dre.ReceitaLiquida * 100 : 0):N2}%</td></tr>
<tr class='total'><td>(=) Lucro Bruto</td><td style='text-align:right'>{dre.LucroBruto:N2}</td><td style='text-align:right'>{(dre.ReceitaLiquida > 0 ? dre.LucroBruto / dre.ReceitaLiquida * 100 : 0):N2}%</td></tr>
<tr><td>(-) Despesas Operacionais</td><td style='text-align:right'>({dre.DespesasOperacionais:N2})</td><td style='text-align:right'>{(dre.ReceitaLiquida > 0 ? dre.DespesasOperacionais / dre.ReceitaLiquida * 100 : 0):N2}%</td></tr>
<tr class='total'><td>(=) Lucro Operacional</td><td style='text-align:right'>{dre.LucroOperacional:N2}</td><td style='text-align:right'>{(dre.ReceitaLiquida > 0 ? dre.LucroOperacional / dre.ReceitaLiquida * 100 : 0):N2}%</td></tr>
<tr><td>(-) Despesas Financeiras</td><td style='text-align:right'>({dre.DespesasFinanceiras:N2})</td><td style='text-align:right'>{(dre.ReceitaLiquida > 0 ? dre.DespesasFinanceiras / dre.ReceitaLiquida * 100 : 0):N2}%</td></tr>
<tr><td>(+) Receitas Financeiras</td><td style='text-align:right'>{dre.ReceitasFinanceiras:N2}</td><td style='text-align:right'>{(dre.ReceitaLiquida > 0 ? dre.ReceitasFinanceiras / dre.ReceitaLiquida * 100 : 0):N2}%</td></tr>
<tr class='total {(dre.LucroLiquido >= 0 ? "lucro" : "prejuizo")}'><td>(=) LUCRO LÍQUIDO</td><td style='text-align:right'>{dre.LucroLiquido:N2}</td><td style='text-align:right'>{(dre.ReceitaLiquida > 0 ? dre.LucroLiquido / dre.ReceitaLiquida * 100 : 0):N2}%</td></tr>
</table>

<div class='footer'>
Grupo Nexum Altivon ME — www.nexumaltivon.com<br>
Sistema GenesisGest.Net — ERP/CRM Integrado
</div>
</body></html>";

            // Retorna HTML como bytes — no projeto real, converter com DinkToPdf ou PuppeteerSharp
            return System.Text.Encoding.UTF8.GetBytes(html);
        }

        public async Task<byte[]> ExportarPosicaoEstoqueExcelAsync(int? lojaId = null)
        {
            var posicoes = await GerarPosicaoEstoqueAsync(lojaId);

            // Gera CSV estruturado (pode ser convertido para Excel com EPPlus no projeto)
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("ID,SKU,Nome,Estoque Atual,Estoque Minimo,Custo Medio,Valor Total,Status,Loja,Ultima Entrada,Ultima Saida");

            foreach (var p in posicoes)
            {
                csv.AppendLine($"{p.ProdutoId},{p.Sku},\"{p.Nome}\",{p.EstoqueAtual},{p.EstoqueMinimo},{p.CustoMedio:N2},{p.ValorTotalEstoque:N2},{p.Status},{p.LojaNome},{p.UltimaEntrada:dd/MM/yyyy},{p.UltimaSaida:dd/MM/yyyy}");
            }

            return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        }
    }

    // DTOs auxiliares de relatório
    public class PosicaoEstoqueDto
    {
        public int ProdutoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Sku { get; set; }
        public decimal EstoqueAtual { get; set; }
        public decimal EstoqueMinimo { get; set; }
        public decimal CustoMedio { get; set; }
        public decimal ValorTotalEstoque { get; set; }
        public string Status { get; set; } = string.Empty;
        public string LojaNome { get; set; } = string.Empty;
        public DateTime? UltimaEntrada { get; set; }
        public DateTime? UltimaSaida { get; set; }
        public decimal Giro { get; set; }
    }

    public class ContasVencidasDto
    {
        public string Tipo { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string Pessoa { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataVencimento { get; set; }
        public int DiasAtraso { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class VendasPorLojaDto
    {
        public int LojaId { get; set; }
        public string LojaNome { get; set; } = string.Empty;
        public int QuantidadePedidos { get; set; }
        public decimal ValorTotal { get; set; }
        public decimal TicketMedio { get; set; }
        public decimal PercentualRepresentacao { get; set; }
    }

    public class ResumoGeralErpDto
    {
        public decimal TotalContasPagarHoje { get; set; }
        public decimal TotalContasPagarAtrasadas { get; set; }
        public decimal TotalContasReceberHoje { get; set; }
        public decimal TotalContasReceberAtrasadas { get; set; }
        public decimal SaldoCaixaAtual { get; set; }
        public int LeadsAtivos { get; set; }
        public int LeadsConvertidosMes { get; set; }
        public int ProdutosEstoqueCritico { get; set; }
        public int FornecedoresAtivos { get; set; }
        public int FornecedoresDropshipping { get; set; }
        public DateTime DataAtualizacao { get; set; }
    }
}
