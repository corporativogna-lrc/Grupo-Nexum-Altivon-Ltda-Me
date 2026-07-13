/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

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

            var receitaBruta = await _context.FluxosCaixa
                .AsNoTracking()
                .Where(f => f.Data >= inicio && f.Data <= fim && f.Tipo == "Entrada" && f.Categoria == "Vendas")
                .SumAsync(f => f.Valor);

            var despesas = await _context.ContasPagar
                .AsNoTracking()
                .Include(c => c.PlanoContas)
                .Where(c => c.DataPagamento >= inicio && c.DataPagamento <= fim && c.Status == "Pago")
                .ToListAsync();

            var receitasFinanceiras = await _context.FluxosCaixa
                .AsNoTracking()
                .Where(f => f.Data >= inicio && f.Data <= fim && f.Tipo == "Entrada" && f.Categoria.Contains("Financeir"))
                .SumAsync(f => f.Valor);

            var cmv = despesas.Where(c => ClassificacaoContem(c, "cmv", "custo", "mercadoria", "produto", "insumo")).Sum(c => c.ValorPago);
            var impostosSobreVendas = despesas.Where(c => ClassificacaoContem(c, "icms", "iss", "pis", "cofins", "imposto sobre venda", "tributo venda")).Sum(c => c.ValorPago);
            var impostoRenda = despesas.Where(c => ClassificacaoContem(c, "imposto de renda", "irpj")).Sum(c => c.ValorPago);
            var contribuicaoSocial = despesas.Where(c => ClassificacaoContem(c, "contribuicao social", "contribuição social", "csll")).Sum(c => c.ValorPago);
            var despesasFinanceiras = despesas.Where(c => ClassificacaoContem(c, "financeir", "juros", "tarifa", "banco")).Sum(c => c.ValorPago);
            var despesasCom = despesas.Where(c => ClassificacaoContem(c, "comercial", "marketing", "venda", "comissao", "comissão")).Sum(c => c.ValorPago);
            var despesasAdm = despesas.Where(c => ClassificacaoContem(c, "administrativ", "admin", "escritorio", "escritório")).Sum(c => c.ValorPago);
            var despesasOp = despesas
                .Where(c =>
                    !ClassificacaoContem(c, "cmv", "custo", "mercadoria", "produto", "insumo") &&
                    !ClassificacaoContem(c, "icms", "iss", "pis", "cofins", "imposto sobre venda", "tributo venda") &&
                    !ClassificacaoContem(c, "imposto de renda", "irpj", "contribuicao social", "contribuição social", "csll") &&
                    !ClassificacaoContem(c, "financeir", "juros", "tarifa", "banco") &&
                    !ClassificacaoContem(c, "comercial", "marketing", "venda", "comissao", "comissão") &&
                    !ClassificacaoContem(c, "administrativ", "admin", "escritorio", "escritório"))
                .Sum(c => c.ValorPago);

            var dre = new DRE
            {
                Ano = ano,
                Mes = mes,
                Tipo = tipo,
                ReceitaBruta = receitaBruta,
                ImpostosSobreVendas = impostosSobreVendas,
                CMV = cmv,
                DespesasOperacionais = despesasOp,
                DespesasAdministrativas = despesasAdm,
                DespesasComerciais = despesasCom,
                DespesasFinanceiras = despesasFinanceiras,
                ReceitasFinanceiras = receitasFinanceiras,
                ImpostoRenda = impostoRenda,
                ContribuicaoSocial = contribuicaoSocial,
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
            var dre = await _context.DREs.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
            if (dre == null) throw new Exception("DRE não encontrada");

            var margemBruta = dre.ReceitaLiquida > 0 ? dre.LucroBruto / dre.ReceitaLiquida * 100 : 0;
            var margemLiquida = dre.ReceitaLiquida > 0 ? dre.LucroLiquido / dre.ReceitaLiquida * 100 : 0;
            var periodo = dre.Mes.HasValue ? $"{dre.Mes:00}/{dre.Ano}" : dre.Ano.ToString();

            var linhas = new List<string>
            {
                "GenesisGest.Net - Demonstrativo do Resultado do Exercicio",
                $"DRE Id: {dre.Id} | Periodo: {periodo} | Tipo: {dre.Tipo} | LojaId: {dre.LojaId?.ToString() ?? "Todas"}",
                $"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm:ss}",
                string.Empty,
                $"Receita bruta..................... {FormatarMoeda(dre.ReceitaBruta)}",
                $"(-) Impostos sobre vendas......... {FormatarMoeda(dre.ImpostosSobreVendas)}",
                $"(=) Receita liquida............... {FormatarMoeda(dre.ReceitaLiquida)}",
                $"(-) CMV........................... {FormatarMoeda(dre.CMV)}",
                $"(=) Lucro bruto................... {FormatarMoeda(dre.LucroBruto)}",
                string.Empty,
                $"(-) Despesas operacionais......... {FormatarMoeda(dre.DespesasOperacionais)}",
                $"(-) Despesas administrativas...... {FormatarMoeda(dre.DespesasAdministrativas)}",
                $"(-) Despesas comerciais........... {FormatarMoeda(dre.DespesasComerciais)}",
                $"(-) Despesas financeiras.......... {FormatarMoeda(dre.DespesasFinanceiras)}",
                $"(+) Receitas financeiras.......... {FormatarMoeda(dre.ReceitasFinanceiras)}",
                $"(=) LAIR.......................... {FormatarMoeda(dre.LAIR)}",
                string.Empty,
                $"(-) Imposto de renda.............. {FormatarMoeda(dre.ImpostoRenda)}",
                $"(-) Contribuicao social........... {FormatarMoeda(dre.ContribuicaoSocial)}",
                $"(=) Lucro liquido................. {FormatarMoeda(dre.LucroLiquido)}",
                $"EBITDA............................ {FormatarMoeda(dre.EBITDA)}",
                string.Empty,
                $"Margem bruta...................... {margemBruta:N2}%",
                $"Margem liquida.................... {margemLiquida:N2}%"
            };

            return PdfFinanceiroBuilder.Gerar("DRE", linhas);
        }

        private static bool ClassificacaoContem(ContaPagar conta, params string[] termos)
        {
            var texto = $"{conta.PlanoContas?.Codigo} {conta.PlanoContas?.Nome} {conta.PlanoContas?.Tipo} {conta.PlanoContas?.Descricao} {conta.FornecedorNome} {conta.Observacoes}";
            return termos.Any(termo => texto.Contains(termo, StringComparison.OrdinalIgnoreCase));
        }

        private static string FormatarMoeda(decimal valor)
        {
            return valor.ToString("C", new System.Globalization.CultureInfo("pt-BR"));
        }
    }
}
