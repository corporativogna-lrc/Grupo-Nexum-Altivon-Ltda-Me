using Microsoft.EntityFrameworkCore;
using NexumAltivon_ERP.Data;
using NexumAltivon_ERP.Models.Financeiro;
using NexumAltivon_ERP.DTOs.Financeiro;
using AutoMapper;

namespace NexumAltivon_ERP.Services.Financeiro
{
    public interface IBancoService
    {
        Task<List<BancoDto>> ListarBancosAsync();
        Task<ContaBancariaResponseDto> CriarContaAsync(ContaBancariaCreateDto dto);
        Task<ContaBancariaResponseDto> ObterContaPorIdAsync(int id);
        Task<List<ContaBancariaResponseDto>> ListarContasAsync();
        Task<MovimentacaoBancariaResponseDto> RegistrarMovimentacaoAsync(MovimentacaoBancariaCreateDto dto);
        Task<List<MovimentacaoBancariaResponseDto>> ListarMovimentacoesAsync(int contaId, DateTime? inicio, DateTime? fim);
        Task<bool> ExcluirContaAsync(int id);
    }

    public class BancoService : IBancoService
    {
        private readonly GenesisDbContext _context;
        private readonly IMapper _mapper;

        public BancoService(GenesisDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<BancoDto>> ListarBancosAsync()
        {
            var bancos = await _context.Bancos.AsNoTracking().Where(b => b.Ativo).ToListAsync();
            return _mapper.Map<List<BancoDto>>(bancos);
        }

        public async Task<ContaBancariaResponseDto> CriarContaAsync(ContaBancariaCreateDto dto)
        {
            var entity = new ContaBancaria
            {
                BancoId = dto.BancoId,
                Agencia = dto.Agencia,
                Conta = dto.Conta,
                Digito = dto.Digito,
                Titular = dto.Titular,
                Tipo = dto.Tipo,
                SaldoInicial = dto.SaldoInicial,
                SaldoAtual = dto.SaldoInicial,
                Observacoes = dto.Observacoes
            };

            _context.ContasBancarias.Add(entity);
            await _context.SaveChangesAsync();

            return await ObterContaPorIdAsync(entity.Id);
        }

        public async Task<ContaBancariaResponseDto> ObterContaPorIdAsync(int id)
        {
            var conta = await _context.ContasBancarias
                .AsNoTracking()
                .Include(c => c.Banco)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (conta == null) throw new Exception("Conta bancária não encontrada");

            var dto = _mapper.Map<ContaBancariaResponseDto>(conta);
            dto.BancoNome = conta.Banco?.Nome ?? "";
            dto.BancoCodigo = conta.Banco?.Codigo ?? "";
            return dto;
        }

        public async Task<List<ContaBancariaResponseDto>> ListarContasAsync()
        {
            var contas = await _context.ContasBancarias
                .AsNoTracking()
                .Include(c => c.Banco)
                .Where(c => c.Ativo)
                .ToListAsync();

            return contas.Select(c =>
            {
                var dto = _mapper.Map<ContaBancariaResponseDto>(c);
                dto.BancoNome = c.Banco?.Nome ?? "";
                dto.BancoCodigo = c.Banco?.Codigo ?? "";
                return dto;
            }).ToList();
        }

        public async Task<MovimentacaoBancariaResponseDto> RegistrarMovimentacaoAsync(MovimentacaoBancariaCreateDto dto)
        {
            var entity = new MovimentacaoBancaria
            {
                ContaBancariaId = dto.ContaBancariaId,
                Tipo = dto.Tipo,
                Descricao = dto.Descricao,
                Valor = dto.Valor,
                NumeroDocumento = dto.NumeroDocumento,
                ContaPagarId = dto.ContaPagarId,
                ContaReceberId = dto.ContaReceberId
            };

            // Atualizar saldo
            var conta = await _context.ContasBancarias.FindAsync(dto.ContaBancariaId);
            if (conta != null)
            {
                conta.SaldoAtual += dto.Tipo == "Credito" ? dto.Valor : -dto.Valor;
            }

            _context.MovimentacoesBancarias.Add(entity);
            await _context.SaveChangesAsync();

            return _mapper.Map<MovimentacaoBancariaResponseDto>(entity);
        }

        public async Task<List<MovimentacaoBancariaResponseDto>> ListarMovimentacoesAsync(int contaId, DateTime? inicio, DateTime? fim)
        {
            var query = _context.MovimentacoesBancarias
                .AsNoTracking()
                .Where(m => m.ContaBancariaId == contaId)
                .AsQueryable();

            if (inicio.HasValue) query = query.Where(m => m.Data >= inicio);
            if (fim.HasValue) query = query.Where(m => m.Data <= fim);

            var itens = await query.OrderByDescending(m => m.Data).ToListAsync();
            return _mapper.Map<List<MovimentacaoBancariaResponseDto>>(itens);
        }

        public async Task<bool> ExcluirContaAsync(int id)
        {
            var conta = await _context.ContasBancarias.FindAsync(id);
            if (conta == null) return false;
            conta.Ativo = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}