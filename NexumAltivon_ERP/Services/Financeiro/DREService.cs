using Microsoft.EntityFrameworkCore;
using NexumAltivon_ERP.Data;
using NexumAltivon_ERP.Models.Financeiro;
using NexumAltivon_ERP.DTOs.Financeiro;
using AutoMapper;

namespace NexumAltivon_ERP.Services.Financeiro
{
    public interface IDREService
    {
        Task<DREResponseDto> CriarAsync(DRECreateDto dto);
        Task<DREResponseDto> GerarAutomaticoAsync(int ano, int? mes, string tipo, int? lojaId);
        Task<DREResponseDto> ObterPorIdAsync(int id);
        Task<List<DREResponseDto>> ListarAsync(DREFiltroDto filtro);
        Task<byte[]> GerarRelatorioPDFAsync(int id);
    }

    public class DREService : IDREService
    {
        private readonly GenesisDbContext _context;
        private readonly IMapper _mapper;

        public DREService(GenesisDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<DREResponseDto> CriarAsync(DRECreateDto dto)
        {
            var entity = new DRE
            {
                Ano = dto.Ano,
                Mes = dto.Mes,
                Tipo = dto.Tipo,
                ReceitaBruta = dto.ReceitaBruta,
                ImpostosSobreVendas = dto.ImpostosSobreVendas,
                CMV = dto.CMV,
                DespesasOperacionais = dto.DespesasOperacionais,
                DespesasAdministrativas = dto.DespesasAdministrativas,
                DespesasComerciais = dto.DespesasComerciais,
                DespesasFinanceiras = dto.DespesasFinanceiras,
                ReceitasFinanceiras = dto.ReceitasFinanceiras,
                ImpostoRenda = dto.ImpostoRenda,
                ContribuicaoSocial = dto.ContribuicaoSocial,
                LojaId = dto.LojaId,
                CriadoEm = DateTime.Now
            };

            _context.DREs.Add(entity);
            await _context.SaveChangesAsync();

            return await ObterPorIdAsync(entity.Id);
        }

        public async Task<DREResponseDto> GerarAutomaticoAsync(int ano, int? mes, string tipo, int? lojaId)
        {
            DateTime inicio, fim;
            if (mes.HasValue)
            {
                inicio = new DateTime(ano, mes.Value, 1);
                fim = inicio.AddMonths(1).AddDays(-1);
            }
            else
            {
                inicio = new DateTime(ano, 1, 1);
                fim = new DateTime(ano, 12, 31);
            }

            // Buscar dados do e-commerce (pedidos pagos)
            var receitaBruta = await _context.FluxosCaixa
                .AsNoTracking()
                .Where(f => f.Data >= inicio && f.Data <= fim && f.Tipo == "Entrada" && f.Categoria == "Vendas")
                .SumAsync(f => f.Valor);

            // Buscar despesas do período
            var despesas = await _context.ContasPagar
                .AsNoTracking()
                .Where(c => c.DataPagamento >= inicio && c.DataPagamento <= fim && c.Status == "Pago")
                .ToListAsync();

            var cmv = despesas.Where(c => c.CentroCustoId == 1).Sum(c => c.ValorPago); // Ajustar lógica real
            var despesasOp = despesas.Where(c => c.CentroCustoId == 3).Sum(c => c.ValorPago);
            var despesasAdm = despesas.Where(c => c.CentroCustoId == 4).Sum(c => c.ValorPago);
            var despesasCom = despesas.Where(c => c.CentroCustoId == 2).Sum(c => c.ValorPago);

            var dre = new DRE
            {
                Ano = ano,
                Mes = mes,
                Tipo = tipo,
                ReceitaBruta = receitaBruta,
                ImpostosSobreVendas = receitaBruta * 0.12m, // Estimativa simplificada
                CMV = cmv,
                DespesasOperacionais = despesasOp,
                DespesasAdministrativas = despesasAdm,
                DespesasComerciais = despesasCom,
                DespesasFinanceiras = 0,
                ReceitasFinanceiras = 0,
                ImpostoRenda = 0,
                ContribuicaoSocial = 0,
                LojaId = lojaId,
                CriadoEm = DateTime.Now
            };

            _context.DREs.Add(dre);
            await _context.SaveChangesAsync();

            return await ObterPorIdAsync(dre.Id);
        }

        public async Task<DREResponseDto> ObterPorIdAsync(int id)
        {
            var dre = await _context.DREs.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
            if (dre == null) throw new Exception("DRE não encontrada");

            var dto = _mapper.Map<DREResponseDto>(dre);
            dto.MargemBrutaPercentual = dto.ReceitaLiquida > 0 ? (dto.LucroBruto / dto.ReceitaLiquida) * 100 : 0;
            dto.MargemLiquidaPercentual = dto.ReceitaLiquida > 0 ? (dto.LucroLiquido / dto.ReceitaLiquida) * 100 : 0;
            return dto;
        }

        public async Task<List<DREResponseDto>> ListarAsync(DREFiltroDto filtro)
        {
            var query = _context.DREs.AsNoTracking().AsQueryable();
            if (filtro.Ano > 0) query = query.Where(d => d.Ano == filtro.Ano);
            if (filtro.Mes.HasValue) query = query.Where(d => d.Mes == filtro.Mes);
            if (!string.IsNullOrEmpty(filtro.Tipo)) query = query.Where(d => d.Tipo == filtro.Tipo);
            if (filtro.LojaId.HasValue) query = query.Where(d => d.LojaId == filtro.LojaId);

            var itens = await query.OrderByDescending(d => d.Ano).ThenByDescending(d => d.Mes).ToListAsync();
            return itens.Select(d =>
            {
                var dto = _mapper.Map<DREResponseDto>(d);
                dto.MargemBrutaPercentual = dto.ReceitaLiquida > 0 ? (dto.LucroBruto / dto.ReceitaLiquida) * 100 : 0;
                dto.MargemLiquidaPercentual = dto.ReceitaLiquida > 0 ? (dto.LucroLiquido / dto.ReceitaLiquida) * 100 : 0;
                return dto;
            }).ToList();
        }

        public async Task<byte[]> GerarRelatorioPDFAsync(int id)
        {
            await Task.Delay(100);
            return Array.Empty<byte>();
        }
    }
}