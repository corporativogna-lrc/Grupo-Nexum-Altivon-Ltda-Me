using Microsoft.EntityFrameworkCore;
using NexumAltivon_ERP.Data;
using NexumAltivon_ERP.Models.Financeiro;
using NexumAltivon_ERP.DTOs.Financeiro;
using AutoMapper;

namespace NexumAltivon_ERP.Services.Financeiro
{
    public interface IContaReceberService
    {
        Task<ContaReceberResponseDto> CriarAsync(ContaReceberCreateDto dto, string usuario);
        Task<ContaReceberResponseDto> EfetuarRecebimentoAsync(ContaReceberRecebimentoDto dto);
        Task<ContaReceberResponseDto> ObterPorIdAsync(int id);
        Task<PaginacaoResultado<ContaReceberResponseDto>> ListarAsync(ContaPagarFiltroDto filtro);
        Task<ContaReceberResumoDto> ObterResumoAsync(DateTime? dataInicio, DateTime? dataFim, int? lojaId);
        Task<bool> ExcluirAsync(int id);
    }

    public class ContaReceberService : IContaReceberService
    {
        private readonly GenesisDbContext _context;
        private readonly IMapper _mapper;
        private readonly IFluxoCaixaService _fluxoCaixa;

        public ContaReceberService(GenesisDbContext context, IMapper mapper, IFluxoCaixaService fluxoCaixa)
        {
            _context = context;
            _mapper = mapper;
            _fluxoCaixa = fluxoCaixa;
        }

        public async Task<ContaReceberResponseDto> CriarAsync(ContaReceberCreateDto dto, string usuario)
        {
            var entity = new ContaReceber
            {
                NumeroDocumento = dto.NumeroDocumento,
                ClienteId = dto.ClienteId,
                ClienteNome = dto.ClienteNome,
                CentroCustoId = dto.CentroCustoId,
                PlanoContasId = dto.PlanoContasId,
                Valor = dto.Valor,
                DataVencimento = dto.DataVencimento,
                FormaPagamento = dto.FormaPagamento,
                NumeroPedido = dto.NumeroPedido,
                ParcelaAtual = dto.ParcelaAtual ?? 1,
                TotalParcelas = dto.TotalParcelas ?? 1,
                LojaId = dto.LojaId,
                Observacoes = dto.Observacoes,
                Status = "Pendente",
                CriadoPor = usuario,
                CriadoEm = DateTime.Now
            };

            _context.ContasReceber.Add(entity);
            await _context.SaveChangesAsync();

            return await ObterPorIdAsync(entity.Id);
        }

        public async Task<ContaReceberResponseDto> EfetuarRecebimentoAsync(ContaReceberRecebimentoDto dto)
        {
            var conta = await _context.ContasReceber.FindAsync(dto.Id);
            if (conta == null) throw new Exception("Conta a receber não encontrada");
            if (conta.Status == "Recebido") throw new Exception("Conta já recebida");

            conta.ValorRecebido = dto.ValorRecebido;
            conta.ValorDesconto = dto.ValorDesconto;
            conta.ValorJuros = dto.ValorJuros;
            conta.DataRecebimento = dto.DataRecebimento;
            conta.BancoRecebimento = dto.BancoRecebimento;
            conta.Status = "Recebido";
            conta.Observacoes += $" | Recebimento em {dto.DataRecebimento:dd/MM/yyyy}: R$ {dto.ValorRecebido:N2}";
            conta.AtualizadoEm = DateTime.Now;

            await _context.SaveChangesAsync();

            await _fluxoCaixa.CriarAsync(new FluxoCaixaCreateDto
            {
                Data = dto.DataRecebimento,
                Tipo = "Entrada",
                Categoria = "Vendas",
                Descricao = $"Recebto: {conta.NumeroDocumento} - {conta.ClienteNome}",
                Valor = dto.ValorRecebido,
                FormaPagamento = conta.FormaPagamento,
                ContaReceberId = conta.Id,
                NumeroDocumento = conta.NumeroDocumento,
                LojaId = conta.LojaId
            }, conta.CriadoPor);

            return await ObterPorIdAsync(conta.Id);
        }

        public async Task<ContaReceberResponseDto> ObterPorIdAsync(int id)
        {
            var conta = await _context.ContasReceber
                .AsNoTracking()
                .Include(c => c.CentroCusto)
                .Include(c => c.PlanoContas)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (conta == null) throw new Exception("Conta a receber não encontrada");

            var dto = _mapper.Map<ContaReceberResponseDto>(conta);
            dto.CentroCustoNome = conta.CentroCusto?.Nome ?? "";
            dto.PlanoContasNome = conta.PlanoContas?.Nome ?? "";
            dto.Saldo = conta.Valor - conta.ValorRecebido;
            dto.DiasAtraso = conta.Status == "Pendente" && conta.DataVencimento < DateTime.Now
                ? (DateTime.Now - conta.DataVencimento).Days
                : 0;

            return dto;
        }

        public async Task<PaginacaoResultado<ContaReceberResponseDto>> ListarAsync(ContaPagarFiltroDto filtro)
        {
            var query = _context.ContasReceber
                .AsNoTracking()
                .Include(c => c.CentroCusto)
                .Include(c => c.PlanoContas)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filtro.Status))
                query = query.Where(c => c.Status == filtro.Status);
            if (filtro.DataInicio.HasValue)
                query = query.Where(c => c.DataVencimento >= filtro.DataInicio);
            if (filtro.DataFim.HasValue)
                query = query.Where(c => c.DataVencimento <= filtro.DataFim);
            if (filtro.LojaId.HasValue)
                query = query.Where(c => c.LojaId == filtro.LojaId);
            if (!string.IsNullOrEmpty(filtro.Busca))
                query = query.Where(c => c.ClienteNome.Contains(filtro.Busca) || c.NumeroDocumento.Contains(filtro.Busca));

            var total = await query.CountAsync();
            var itens = await query
                .OrderByDescending(c => c.DataVencimento)
                .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
                .Take(filtro.TamanhoPagina)
                .ToListAsync();

            var dtos = itens.Select(c =>
            {
                var dto = _mapper.Map<ContaReceberResponseDto>(c);
                dto.CentroCustoNome = c.CentroCusto?.Nome ?? "";
                dto.PlanoContasNome = c.PlanoContas?.Nome ?? "";
                dto.Saldo = c.Valor - c.ValorRecebido;
                dto.DiasAtraso = c.Status == "Pendente" && c.DataVencimento < DateTime.Now
                    ? (DateTime.Now - c.DataVencimento).Days
                    : 0;
                return dto;
            }).ToList();

            return new PaginacaoResultado<ContaReceberResponseDto>
            {
                Total = total,
                Pagina = filtro.Pagina,
                TamanhoPagina = filtro.TamanhoPagina,
                Itens = dtos
            };
        }

        public async Task<ContaReceberResumoDto> ObterResumoAsync(DateTime? dataInicio, DateTime? dataFim, int? lojaId)
        {
            var query = _context.ContasReceber.AsNoTracking().AsQueryable();
            if (dataInicio.HasValue) query = query.Where(c => c.DataVencimento >= dataInicio);
            if (dataFim.HasValue) query = query.Where(c => c.DataVencimento <= dataFim);
            if (lojaId.HasValue) query = query.Where(c => c.LojaId == lojaId);

            var pendente = await query.Where(c => c.Status == "Pendente").ToListAsync();
            var atrasado = await query.Where(c => c.Status == "Pendente" && c.DataVencimento < DateTime.Now).ToListAsync();
            var recebido = await query.Where(c => c.Status == "Recebido").ToListAsync();
            var total = await query.ToListAsync();

            var totalValor = total.Sum(c => c.Valor);
            var atrasadoValor = atrasado.Sum(c => c.Valor - c.ValorRecebido);

            return new ContaReceberResumoDto
            {
                TotalPendente = pendente.Sum(c => c.Valor - c.ValorRecebido),
                TotalAtrasado = atrasadoValor,
                TotalRecebido = recebido.Sum(c => c.ValorRecebido),
                QuantidadePendente = pendente.Count,
                QuantidadeAtrasado = atrasado.Count,
                InadimplenciaPercentual = totalValor > 0 ? (atrasadoValor / totalValor) * 100 : 0
            };
        }

        public async Task<bool> ExcluirAsync(int id)
        {
            var conta = await _context.ContasReceber.FindAsync(id);
            if (conta == null || conta.Status == "Recebido") return false;
            _context.ContasReceber.Remove(conta);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}