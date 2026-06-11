using Microsoft.EntityFrameworkCore;
using NexumAltivon_ERP.Data;
using NexumAltivon_ERP.Models.Financeiro;
using NexumAltivon_ERP.DTOs.Financeiro;
using AutoMapper;

namespace NexumAltivon_ERP.Services.Financeiro
{
    public interface IFluxoCaixaService
    {
        Task<FluxoCaixaResponseDto> CriarAsync(FluxoCaixaCreateDto dto, string usuario);
        Task<FluxoCaixaResumoDto> ObterResumoAsync(DateTime dataInicio, DateTime dataFim, int? lojaId);
        Task<List<FluxoCaixaDiarioDto>> ObterDiarioAsync(DateTime dataInicio, DateTime dataFim, int? lojaId);
        Task<List<FluxoCaixaResponseDto>> ListarAsync(DateTime dataInicio, DateTime dataFim, string tipo, int? lojaId);
    }

    public class FluxoCaixaService : IFluxoCaixaService
    {
        private readonly GenesisDbContext _context;
        private readonly IMapper _mapper;

        public FluxoCaixaService(GenesisDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<FluxoCaixaResponseDto> CriarAsync(FluxoCaixaCreateDto dto, string usuario)
        {
            var entity = new FluxoCaixa
            {
                Data = dto.Data,
                Tipo = dto.Tipo,
                Categoria = dto.Categoria,
                Descricao = dto.Descricao,
                Valor = dto.Valor,
                FormaPagamento = dto.FormaPagamento,
                ContaPagarId = dto.ContaPagarId,
                ContaReceberId = dto.ContaReceberId,
                PedidoId = dto.PedidoId,
                NumeroDocumento = dto.NumeroDocumento,
                LojaId = dto.LojaId,
                CriadoPor = usuario,
                CriadoEm = DateTime.Now
            };

            _context.FluxosCaixa.Add(entity);
            await _context.SaveChangesAsync();

            return _mapper.Map<FluxoCaixaResponseDto>(entity);
        }

        public async Task<FluxoCaixaResumoDto> ObterResumoAsync(DateTime dataInicio, DateTime dataFim, int? lojaId)
        {
            var query = _context.FluxosCaixa
                .AsNoTracking()
                .Where(f => f.Data >= dataInicio && f.Data <= dataFim)
                .AsQueryable();

            if (lojaId.HasValue)
                query = query.Where(f => f.LojaId == lojaId);

            var entradas = await query.Where(f => f.Tipo == "Entrada").SumAsync(f => f.Valor);
            var saidas = await query.Where(f => f.Tipo == "Saida").SumAsync(f => f.Valor);

            // Saldo acumulado até o início do período
            var saldoAnterior = await _context.FluxosCaixa
                .AsNoTracking()
                .Where(f => f.Data < dataInicio)
                .SumAsync(f => f.Tipo == "Entrada" ? f.Valor : -f.Valor);

            return new FluxoCaixaResumoDto
            {
                DataInicio = dataInicio,
                DataFim = dataFim,
                TotalEntradas = entradas,
                TotalSaidas = saidas,
                SaldoPeriodo = entradas - saidas,
                SaldoAcumulado = saldoAnterior + entradas - saidas
            };
        }

        public async Task<List<FluxoCaixaDiarioDto>> ObterDiarioAsync(DateTime dataInicio, DateTime dataFim, int? lojaId)
        {
            var query = _context.FluxosCaixa
                .AsNoTracking()
                .Where(f => f.Data >= dataInicio && f.Data <= dataFim)
                .AsQueryable();

            if (lojaId.HasValue)
                query = query.Where(f => f.LojaId == lojaId);

            var dados = await query
                .GroupBy(f => f.Data.Date)
                .Select(g => new FluxoCaixaDiarioDto
                {
                    Data = g.Key,
                    Entradas = g.Where(f => f.Tipo == "Entrada").Sum(f => f.Valor),
                    Saidas = g.Where(f => f.Tipo == "Saida").Sum(f => f.Valor),
                    SaldoDia = g.Where(f => f.Tipo == "Entrada").Sum(f => f.Valor) -
                               g.Where(f => f.Tipo == "Saida").Sum(f => f.Valor)
                })
                .OrderBy(d => d.Data)
                .ToListAsync();

            return dados;
        }

        public async Task<List<FluxoCaixaResponseDto>> ListarAsync(DateTime dataInicio, DateTime dataFim, string tipo, int? lojaId)
        {
            var query = _context.FluxosCaixa
                .AsNoTracking()
                .Where(f => f.Data >= dataInicio && f.Data <= dataFim)
                .AsQueryable();

            if (!string.IsNullOrEmpty(tipo))
                query = query.Where(f => f.Tipo == tipo);
            if (lojaId.HasValue)
                query = query.Where(f => f.LojaId == lojaId);

            var itens = await query.OrderByDescending(f => f.Data).ToListAsync();
            return _mapper.Map<List<FluxoCaixaResponseDto>>(itens);
        }
    }
}