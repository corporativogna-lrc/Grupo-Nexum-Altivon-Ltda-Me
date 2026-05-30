using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NexumAltivon.ERP.Data;
using NexumAltivon.ERP.DTOs;
using NexumAltivon.ERP.Models;

namespace NexumAltivon.ERP.Services
{
    /// <summary>
    /// Serviço de gestão financeira completa — contas a pagar, receber, fluxo de caixa e DRE
    /// </summary>
    public interface IFinanceiroService
    {
        Task<ContaPagarDto> CriarContaPagarAsync(CriarContaPagarDto dto, string usuario);
        Task<ContaPagarDto> BaixarContaPagarAsync(BaixarContaPagarDto dto, string usuario);
        Task<IEnumerable<ContaPagarDto>> ListarContasPagarAsync(string? status = null, int? fornecedorId = null, DateTime? vencimentoAte = null);
        Task<IEnumerable<ContaPagarDto>> ListarContasPagarAsync(string? status, int? fornecedorId, DateTime? vencimentoDe, DateTime? vencimentoAte);
        Task<ContaPagarDto?> ObterContaPagarPorIdAsync(int id);
        Task<bool> CancelarContaPagarAsync(int id, string usuario);
        Task<ContaReceberDto> CriarContaReceberAsync(CriarContaReceberDto dto, string usuario);
        Task<ContaReceberDto> BaixarContaReceberAsync(int id, decimal valorRecebido, DateTime dataRecebimento, string formaRecebimento, string usuario);
        Task<ContaReceberDto> BaixarContaReceberAsync(BaixarContaReceberDto dto, string usuario);
        Task<IEnumerable<ContaReceberDto>> ListarContasReceberAsync(string? status = null, int? clienteId = null);
        Task<IEnumerable<ContaReceberDto>> ListarContasReceberAsync(string? status, int? clienteId, DateTime? vencimentoDe, DateTime? vencimentoAte);
        Task<ContaReceberDto?> ObterContaReceberPorIdAsync(int id);
        Task<bool> CancelarContaReceberAsync(int id, string usuario);
        Task<ResumoFinanceiroDto> ObterResumoFinanceiroAsync();
        Task<DreDto> ObterDreAsync(DateTime inicio, DateTime fim);
        Task<IEnumerable<FluxoCaixaDto>> ObterFluxoCaixaAsync(DateTime inicio, DateTime fim);
    }

    public class FinanceiroService : IFinanceiroService
    {
        private readonly NexumDbContext _context;

        public FinanceiroService(NexumDbContext context)
        {
            _context = context;
        }

        public async Task<ContaPagarDto> CriarContaPagarAsync(CriarContaPagarDto dto, string usuario)
        {
            var conta = new ContaPagar
            {
                NumeroDocumento = dto.NumeroDocumento,
                FornecedorId = dto.FornecedorId,
                Descricao = dto.Descricao,
                ValorOriginal = dto.ValorOriginal,
                DataEmissao = DateTime.Now,
                DataVencimento = dto.DataVencimento,
                Status = "Pendente",
                LojaId = dto.LojaId,
                CentroCustoId = dto.CentroCustoId,
                Observacoes = dto.Observacoes,
                CriadoEm = DateTime.Now,
                CriadoPor = usuario
            };

            _context.ContasPagar.Add(conta);
            await _context.SaveChangesAsync();

            return await ObterContaPagarPorIdAsync(conta.Id);
        }

        public async Task<ContaPagarDto> BaixarContaPagarAsync(BaixarContaPagarDto dto, string usuario)
        {
            var conta = await _context.ContasPagar.FindAsync(dto.ContaPagarId)
                ?? throw new Exception("Conta a pagar não encontrada.");

            if (conta.Status == "Pago")
                throw new Exception("Conta já está paga.");

            conta.ValorPago = dto.ValorPago;
            conta.ValorMulta = dto.ValorMulta;
            conta.ValorJuros = dto.ValorJuros;
            conta.ValorDesconto = dto.ValorDesconto;
            conta.DataPagamento = dto.DataPagamento;
            conta.FormaPagamento = dto.FormaPagamento;
            conta.NumeroBoleto = dto.NumeroBoleto;
            conta.Observacoes = $"{conta.Observacoes}\n[BAIXA {DateTime.Now:dd/MM/yyyy}] {dto.Observacoes}";
            conta.Status = "Pago";
            conta.AtualizadoEm = DateTime.Now;

            // Registra no fluxo de caixa
            var fluxo = new FluxoCaixa
            {
                Data = dto.DataPagamento,
                Tipo = "Saida",
                Descricao = $"Pagamento: {conta.Descricao} — Doc {conta.NumeroDocumento}",
                Valor = dto.ValorPago + dto.ValorMulta + dto.ValorJuros - dto.ValorDesconto,
                Categoria = "Contas a Pagar",
                ContaPagarId = conta.Id,
                FormaPagamento = dto.FormaPagamento,
                CriadoEm = DateTime.Now,
                CriadoPor = usuario
            };
            _context.FluxoCaixa.Add(fluxo);

            await _context.SaveChangesAsync();
            return await ObterContaPagarPorIdAsync(conta.Id);
        }

        public async Task<IEnumerable<ContaPagarDto>> ListarContasPagarAsync(string? status = null, int? fornecedorId = null, DateTime? vencimentoAte = null)
        {
            var query = _context.ContasPagar
                .Include(c => c.Fornecedor)
                .Include(c => c.CentroCusto)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status == status);

            if (fornecedorId.HasValue)
                query = query.Where(c => c.FornecedorId == fornecedorId.Value);

            if (vencimentoAte.HasValue)
                query = query.Where(c => c.DataVencimento <= vencimentoAte.Value);

            return await query.OrderBy(c => c.DataVencimento)
                .Select(c => new ContaPagarDto
                {
                    Id = c.Id,
                    NumeroDocumento = c.NumeroDocumento,
                    FornecedorId = c.FornecedorId,
                    FornecedorNome = c.Fornecedor != null ? c.Fornecedor.RazaoSocial : "",
                    Descricao = c.Descricao,
                    ValorOriginal = c.ValorOriginal,
                    ValorPago = c.ValorPago,
                    ValorMulta = c.ValorMulta,
                    ValorJuros = c.ValorJuros,
                    ValorDesconto = c.ValorDesconto,
                    DataEmissao = c.DataEmissao,
                    DataVencimento = c.DataVencimento,
                    DataPagamento = c.DataPagamento,
                    Status = c.Status,
                    FormaPagamento = c.FormaPagamento,
                    LojaId = c.LojaId,
                    CentroCustoId = c.CentroCustoId,
                    CentroCustoNome = c.CentroCusto != null ? c.CentroCusto.Nome : "",
                    CriadoEm = c.CriadoEm
                }).ToListAsync();
        }

        public async Task<IEnumerable<ContaPagarDto>> ListarContasPagarAsync(string? status, int? fornecedorId, DateTime? vencimentoDe, DateTime? vencimentoAte)
        {
            var contas = await ListarContasPagarAsync(status, fornecedorId, vencimentoAte);
            return vencimentoDe.HasValue ? contas.Where(c => c.DataVencimento >= vencimentoDe.Value) : contas;
        }

        public async Task<ContaReceberDto> CriarContaReceberAsync(CriarContaReceberDto dto, string usuario)
        {
            var conta = new ContaReceber
            {
                NumeroDocumento = dto.NumeroDocumento,
                ClienteId = dto.ClienteId,
                Descricao = dto.Descricao,
                ValorOriginal = dto.ValorOriginal,
                DataEmissao = DateTime.Now,
                DataVencimento = dto.DataVencimento,
                Status = "Pendente",
                LojaId = dto.LojaId,
                CentroCustoId = dto.CentroCustoId,
                NumeroPedidoReferencia = dto.NumeroPedidoReferencia,
                CriadoEm = DateTime.Now,
                CriadoPor = usuario
            };

            _context.ContasReceber.Add(conta);
            await _context.SaveChangesAsync();

            return await ObterContaReceberPorIdAsync(conta.Id);
        }

        public async Task<ContaReceberDto> BaixarContaReceberAsync(int id, decimal valorRecebido, DateTime dataRecebimento, string formaRecebimento, string usuario)
        {
            var conta = await _context.ContasReceber.FindAsync(id)
                ?? throw new Exception("Conta a receber não encontrada.");

            if (conta.Status == "Recebido")
                throw new Exception("Conta já está recebida.");

            conta.ValorRecebido = valorRecebido;
            conta.DataRecebimento = dataRecebimento;
            conta.FormaRecebimento = formaRecebimento;
            conta.Status = "Recebido";
            conta.AtualizadoEm = DateTime.Now;

            // Registra no fluxo de caixa
            var fluxo = new FluxoCaixa
            {
                Data = dataRecebimento,
                Tipo = "Entrada",
                Descricao = $"Recebimento: {conta.Descricao} — Doc {conta.NumeroDocumento}",
                Valor = valorRecebido,
                Categoria = "Contas a Receber",
                ContaReceberId = conta.Id,
                FormaPagamento = formaRecebimento,
                CriadoEm = DateTime.Now,
                CriadoPor = usuario
            };
            _context.FluxoCaixa.Add(fluxo);

            await _context.SaveChangesAsync();
            return await ObterContaReceberPorIdAsync(conta.Id);
        }

        public Task<ContaReceberDto> BaixarContaReceberAsync(BaixarContaReceberDto dto, string usuario)
        {
            return BaixarContaReceberAsync(dto.ContaReceberId, dto.ValorRecebido, dto.DataRecebimento, dto.FormaRecebimento, usuario);
        }

        public async Task<IEnumerable<ContaReceberDto>> ListarContasReceberAsync(string? status = null, int? clienteId = null)
        {
            var query = _context.ContasReceber
                .Include(c => c.Cliente)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status == status);

            if (clienteId.HasValue)
                query = query.Where(c => c.ClienteId == clienteId.Value);

            return await query.OrderBy(c => c.DataVencimento)
                .Select(c => new ContaReceberDto
                {
                    Id = c.Id,
                    NumeroDocumento = c.NumeroDocumento,
                    ClienteId = c.ClienteId,
                    ClienteNome = c.Cliente != null ? c.Cliente.Nome : "",
                    Descricao = c.Descricao,
                    ValorOriginal = c.ValorOriginal,
                    ValorRecebido = c.ValorRecebido,
                    ValorMulta = c.ValorMulta,
                    ValorJuros = c.ValorJuros,
                    ValorDesconto = c.ValorDesconto,
                    DataEmissao = c.DataEmissao,
                    DataVencimento = c.DataVencimento,
                    DataRecebimento = c.DataRecebimento,
                    Status = c.Status,
                    LojaId = c.LojaId,
                    CriadoEm = c.CriadoEm
                }).ToListAsync();
        }

        public async Task<IEnumerable<ContaReceberDto>> ListarContasReceberAsync(string? status, int? clienteId, DateTime? vencimentoDe, DateTime? vencimentoAte)
        {
            var contas = await ListarContasReceberAsync(status, clienteId);
            if (vencimentoDe.HasValue)
                contas = contas.Where(c => c.DataVencimento >= vencimentoDe.Value);
            if (vencimentoAte.HasValue)
                contas = contas.Where(c => c.DataVencimento <= vencimentoAte.Value);
            return contas;
        }

        public async Task<bool> CancelarContaPagarAsync(int id, string usuario)
        {
            var conta = await _context.ContasPagar.FindAsync(id);
            if (conta == null) return false;
            conta.Status = "Cancelado";
            conta.AtualizadoEm = DateTime.Now;
            conta.Observacoes = $"{conta.Observacoes}\n[CANCELAMENTO {DateTime.Now:dd/MM/yyyy}] por {usuario}";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelarContaReceberAsync(int id, string usuario)
        {
            var conta = await _context.ContasReceber.FindAsync(id);
            if (conta == null) return false;
            conta.Status = "Cancelado";
            conta.AtualizadoEm = DateTime.Now;
            conta.Observacoes = $"{conta.Observacoes}\n[CANCELAMENTO {DateTime.Now:dd/MM/yyyy}] por {usuario}";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ResumoFinanceiroDto> ObterResumoFinanceiroAsync()
        {
            var hoje = DateTime.Now.Date;
            var seteDias = hoje.AddDays(7);

            var contasPagar = await _context.ContasPagar
                .Where(c => c.Status == "Pendente")
                .ToListAsync();

            var contasReceber = await _context.ContasReceber
                .Where(c => c.Status == "Pendente")
                .ToListAsync();

            var saldoCaixa = await _context.ContasBancarias
                .Where(c => c.Ativo)
                .SumAsync(c => c.SaldoAtual);

            return new ResumoFinanceiroDto
            {
                TotalContasPagarHoje = contasPagar.Where(c => c.DataVencimento.Date == hoje).Sum(c => c.ValorOriginal - c.ValorPago),
                TotalContasPagarAtrasadas = contasPagar.Where(c => c.DataVencimento.Date < hoje).Sum(c => c.ValorOriginal - c.ValorPago),
                TotalContasPagarProximos7Dias = contasPagar.Where(c => c.DataVencimento.Date <= seteDias && c.DataVencimento.Date >= hoje).Sum(c => c.ValorOriginal - c.ValorPago),
                TotalContasReceberHoje = contasReceber.Where(c => c.DataVencimento.Date == hoje).Sum(c => c.ValorOriginal - c.ValorRecebido),
                TotalContasReceberAtrasadas = contasReceber.Where(c => c.DataVencimento.Date < hoje).Sum(c => c.ValorOriginal - c.ValorRecebido),
                TotalContasReceberProximos7Dias = contasReceber.Where(c => c.DataVencimento.Date <= seteDias && c.DataVencimento.Date >= hoje).Sum(c => c.ValorOriginal - c.ValorRecebido),
                SaldoCaixaAtual = saldoCaixa,
                QuantidadeContasPagarAtrasadas = contasPagar.Count(c => c.DataVencimento.Date < hoje),
                QuantidadeContasReceberAtrasadas = contasReceber.Count(c => c.DataVencimento.Date < hoje)
            };
        }

        public async Task<DreDto> ObterDreAsync(DateTime inicio, DateTime fim)
        {
            var receitas = await _context.FluxoCaixa
                .Where(f => f.Tipo == "Entrada" && f.Data >= inicio && f.Data <= fim)
                .SumAsync(f => f.Valor);

            var despesas = await _context.FluxoCaixa
                .Where(f => f.Tipo == "Saida" && f.Data >= inicio && f.Data <= fim)
                .SumAsync(f => f.Valor);

            // Simulação de impostos baseada em 12% da receita (simplificado)
            var impostos = receitas * 0.12m;

            return new DreDto
            {
                Periodo = $"{inicio:MM/yyyy} a {fim:MM/yyyy}",
                ReceitaBruta = receitas,
                Impostos = impostos,
                CustoProdutosVendidos = despesas * 0.6m,
                DespesasOperacionais = despesas * 0.25m,
                DespesasFinanceiras = despesas * 0.05m,
                ReceitasFinanceiras = receitas * 0.02m
            };
        }

        public async Task<IEnumerable<FluxoCaixaDto>> ObterFluxoCaixaAsync(DateTime inicio, DateTime fim)
        {
            return await _context.FluxoCaixa
                .Where(f => f.Data >= inicio && f.Data <= fim)
                .OrderByDescending(f => f.Data)
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

        public async Task<ContaPagarDto?> ObterContaPagarPorIdAsync(int id)
        {
            return (await ListarContasPagarAsync()).FirstOrDefault(c => c.Id == id);
        }

        public async Task<ContaReceberDto?> ObterContaReceberPorIdAsync(int id)
        {
            return (await ListarContasReceberAsync()).FirstOrDefault(c => c.Id == id);
        }
    }
}
