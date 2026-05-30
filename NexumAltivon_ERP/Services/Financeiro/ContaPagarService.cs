using Microsoft.EntityFrameworkCore;
using NexumAltivon_ERP.Data;
using NexumAltivon_ERP.Models.Financeiro;
using NexumAltivon_ERP.DTOs.Financeiro;
using AutoMapper;

namespace NexumAltivon_ERP.Services.Financeiro
{
    public interface IContaPagarService
    {
        Task<ContaPagarResponseDto> CriarAsync(ContaPagarCreateDto dto, string usuario);
        Task<ContaPagarResponseDto> AtualizarAsync(int id, ContaPagarUpdateDto dto);
        Task<ContaPagarResponseDto> EfetuarPagamentoAsync(ContaPagarPagamentoDto dto);
        Task<ContaPagarResponseDto> ObterPorIdAsync(int id);
        Task<PaginacaoResultado<ContaPagarResponseDto>> ListarAsync(ContaPagarFiltroDto filtro);
        Task<ContaPagarResumoDto> ObterResumoAsync(DateTime? dataInicio, DateTime? dataFim, int? lojaId);
        Task<bool> ExcluirAsync(int id);
        Task<byte[]> GerarRelatorioPDFAsync(ContaPagarFiltroDto filtro);
    }

    public class ContaPagarService : IContaPagarService
    {
        private readonly GenesisDbContext _context;
        private readonly IMapper _mapper;
        private readonly IFluxoCaixaService _fluxoCaixa;

        public ContaPagarService(GenesisDbContext context, IMapper mapper, IFluxoCaixaService fluxoCaixa)
        {
            _context = context;
            _mapper = mapper;
            _fluxoCaixa = fluxoCaixa;
        }

        public async Task<ContaPagarResponseDto> CriarAsync(ContaPagarCreateDto dto, string usuario)
        {
            var entity = new ContaPagar
            {
                NumeroDocumento = dto.NumeroDocumento,
                FornecedorId = dto.FornecedorId,
                FornecedorNome = dto.FornecedorNome,
                CentroCustoId = dto.CentroCustoId,
                PlanoContasId = dto.PlanoContasId,
                Valor = dto.Valor,
                DataVencimento = dto.DataVencimento,
                FormaPagamento = dto.FormaPagamento,
                NumeroNFe = dto.NumeroNFe,
                ParcelaAtual = dto.ParcelaAtual ?? 1,
                TotalParcelas = dto.TotalParcelas ?? 1,
                LojaId = dto.LojaId,
                Observacoes = dto.Observacoes,
                Status = "Pendente",
                CriadoPor = usuario,
                CriadoEm = DateTime.Now
            };

            _context.ContasPagar.Add(entity);
            await _context.SaveChangesAsync();

            return await ObterPorIdAsync(entity.Id);
        }

        public async Task<ContaPagarResponseDto> EfetuarPagamentoAsync(ContaPagarPagamentoDto dto)
        {
            var conta = await _context.ContasPagar.FindAsync(dto.Id);
            if (conta == null) throw new Exception("Conta a pagar não encontrada");
            if (conta.Status == "Pago") throw new Exception("Conta já está paga");

            conta.ValorPago = dto.ValorPago;
            conta.ValorDesconto = dto.ValorDesconto;
            conta.ValorJuros = dto.ValorJuros;
            conta.DataPagamento = dto.DataPagamento;
            conta.BancoPagamento = dto.BancoPagamento;
            conta.Status = "Pago";
            conta.Observacoes += $" | Pagamento em {dto.DataPagamento:dd/MM/yyyy}: R$ {dto.ValorPago:N2} {dto.Observacoes}";
            conta.AtualizadoEm = DateTime.Now;

            await _context.SaveChangesAsync();

            // Registrar no fluxo de caixa
            await _fluxoCaixa.CriarAsync(new FluxoCaixaCreateDto
            {
                Data = dto.DataPagamento,
                Tipo = "Saida",
                Categoria = "Despesas",
                Descricao = $"Pagto: {conta.NumeroDocumento} - {conta.FornecedorNome}",
                Valor = dto.ValorPago,
                FormaPagamento = conta.FormaPagamento,
                ContaPagarId = conta.Id,
                NumeroDocumento = conta.NumeroDocumento,
                LojaId = conta.LojaId
            }, conta.CriadoPor);

            return await ObterPorIdAsync(conta.Id);
        }

        public async Task<ContaPagarResponseDto> ObterPorIdAsync(int id)
        {
            var conta = await _context.ContasPagar
                .AsNoTracking()
                .Include(c => c.CentroCusto)
                .Include(c => c.PlanoContas)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (conta == null) throw new Exception("Conta a pagar não encontrada");

            var dto = _mapper.Map<ContaPagarResponseDto>(conta);
            dto.CentroCustoNome = conta.CentroCusto?.Nome ?? "";
            dto.PlanoContasNome = conta.PlanoContas?.Nome ?? "";
            dto.Saldo = conta.Valor - conta.ValorPago;
            dto.DiasAtraso = conta.Status == "Pendente" && conta.DataVencimento < DateTime.Now
                ? (DateTime.Now - conta.DataVencimento).Days
                : 0;

            return dto;
        }

        public async Task<PaginacaoResultado<ContaPagarResponseDto>> ListarAsync(ContaPagarFiltroDto filtro)
        {
            var query = _context.ContasPagar
                .AsNoTracking()
                .Include(c => c.CentroCusto)
                .Include(c => c.PlanoContas)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filtro.Status))
                query = query.Where(c => c.Status == filtro.Status);

            if (filtro.FornecedorId.HasValue)
                query = query.Where(c => c.FornecedorId == filtro.FornecedorId);

            if (filtro.CentroCustoId.HasValue)
                query = query.Where(c => c.CentroCustoId == filtro.CentroCustoId);

            if (filtro.DataInicio.HasValue)
                query = query.Where(c => c.DataVencimento >= filtro.DataInicio);

            if (filtro.DataFim.HasValue)
                query = query.Where(c => c.DataVencimento <= filtro.DataFim);

            if (filtro.LojaId.HasValue)
                query = query.Where(c => c.LojaId == filtro.LojaId);

            if (!string.IsNullOrEmpty(filtro.Busca))
                query = query.Where(c => c.FornecedorNome.Contains(filtro.Busca) ||
                                          c.NumeroDocumento.Contains(filtro.Busca));

            var total = await query.CountAsync();
            var itens = await query
                .OrderByDescending(c => c.DataVencimento)
                .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
                .Take(filtro.TamanhoPagina)
                .ToListAsync();

            var dtos = itens.Select(c =>
            {
                var dto = _mapper.Map<ContaPagarResponseDto>(c);
                dto.CentroCustoNome = c.CentroCusto?.Nome ?? "";
                dto.PlanoContasNome = c.PlanoContas?.Nome ?? "";
                dto.Saldo = c.Valor - c.ValorPago;
                dto.DiasAtraso = c.Status == "Pendente" && c.DataVencimento < DateTime.Now
                    ? (DateTime.Now - c.DataVencimento).Days
                    : 0;
                return dto;
            }).ToList();

            return new PaginacaoResultado<ContaPagarResponseDto>
            {
                Total = total,
                Pagina = filtro.Pagina,
                TamanhoPagina = filtro.TamanhoPagina,
                Itens = dtos
            };
        }

        public async Task<ContaPagarResumoDto> ObterResumoAsync(DateTime? dataInicio, DateTime? dataFim, int? lojaId)
        {
            var query = _context.ContasPagar.AsNoTracking().AsQueryable();

            if (dataInicio.HasValue)
                query = query.Where(c => c.DataVencimento >= dataInicio);
            if (dataFim.HasValue)
                query = query.Where(c => c.DataVencimento <= dataFim);
            if (lojaId.HasValue)
                query = query.Where(c => c.LojaId == lojaId);

            var pendente = await query.Where(c => c.Status == "Pendente").ToListAsync();
            var atrasado = await query.Where(c => c.Status == "Pendente" && c.DataVencimento < DateTime.Now).ToListAsync();
            var pago = await query.Where(c => c.Status == "Pago").ToListAsync();
            var cancelado = await query.Where(c => c.Status == "Cancelado").ToListAsync();

            return new ContaPagarResumoDto
            {
                TotalPendente = pendente.Sum(c => c.Valor - c.ValorPago),
                TotalAtrasado = atrasado.Sum(c => c.Valor - c.ValorPago),
                TotalPago = pago.Sum(c => c.ValorPago),
                TotalCancelado = cancelado.Sum(c => c.Valor),
                QuantidadePendente = pendente.Count,
                QuantidadeAtrasado = atrasado.Count,
                MediaDiasAtraso = atrasado.Any()
                    ? (decimal)atrasado.Average(c => (DateTime.Now - c.DataVencimento).Days)
                    : 0m
            };
        }

        public async Task<ContaPagarResponseDto> AtualizarAsync(int id, ContaPagarUpdateDto dto)
        {
            var conta = await _context.ContasPagar.FindAsync(id);
            if (conta == null) throw new Exception("Conta a pagar não encontrada");
            if (conta.Status == "Pago") throw new Exception("Não é possível alterar conta já paga");

            conta.Valor = dto.Valor;
            conta.DataVencimento = dto.DataVencimento;
            conta.FormaPagamento = dto.FormaPagamento;
            conta.Observacoes = dto.Observacoes;
            conta.AtualizadoEm = DateTime.Now;

            await _context.SaveChangesAsync();
            return await ObterPorIdAsync(id);
        }

        public async Task<bool> ExcluirAsync(int id)
        {
            var conta = await _context.ContasPagar.FindAsync(id);
            if (conta == null || conta.Status == "Pago") return false;

            _context.ContasPagar.Remove(conta);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<byte[]> GerarRelatorioPDFAsync(ContaPagarFiltroDto filtro)
        {
            // Stub para geração de PDF - implementar com iTextSharp ou DinkToPdf
            await Task.Delay(100);
            return Array.Empty<byte>();
        }
    }

    public class PaginacaoResultado<T>
    {
        public int Total { get; set; }
        public int Pagina { get; set; }
        public int TamanhoPagina { get; set; }
        public int TotalPaginas => (int)Math.Ceiling((double)Total / TamanhoPagina);
        public List<T> Itens { get; set; } = new List<T>();
    }
}
