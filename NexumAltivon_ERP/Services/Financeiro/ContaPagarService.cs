/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Globalization;
using System.Text;
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
            var query = _context.ContasPagar
                .AsNoTracking()
                .Include(c => c.CentroCusto)
                .Include(c => c.PlanoContas)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.Status))
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

            if (!string.IsNullOrWhiteSpace(filtro.Busca))
                query = query.Where(c => c.FornecedorNome.Contains(filtro.Busca) ||
                                          c.NumeroDocumento.Contains(filtro.Busca));

            var contas = await query
                .OrderBy(c => c.DataVencimento)
                .ThenBy(c => c.FornecedorNome)
                .ThenBy(c => c.NumeroDocumento)
                .ToListAsync();

            var culture = new CultureInfo("pt-BR");
            var totalValor = contas.Sum(c => c.Valor);
            var totalPago = contas.Sum(c => c.ValorPago);
            var totalSaldo = contas.Sum(c => c.Valor - c.ValorPago);
            var totalAtrasado = contas
                .Where(c => c.Status == "Pendente" && c.DataVencimento.Date < DateTime.Today)
                .Sum(c => c.Valor - c.ValorPago);

            var linhas = new List<string>
            {
                "GenesisGest.Net - Relatorio de Contas a Pagar",
                $"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm:ss}",
                $"Status: {ValorFiltro(filtro.Status)} | FornecedorId: {ValorFiltro(filtro.FornecedorId)} | CentroCustoId: {ValorFiltro(filtro.CentroCustoId)} | LojaId: {ValorFiltro(filtro.LojaId)}",
                $"Periodo de vencimento: {ValorFiltro(filtro.DataInicio)} ate {ValorFiltro(filtro.DataFim)} | Busca: {ValorFiltro(filtro.Busca)}",
                $"Quantidade: {contas.Count} | Valor: {FormatarMoeda(totalValor, culture)} | Pago: {FormatarMoeda(totalPago, culture)} | Saldo: {FormatarMoeda(totalSaldo, culture)} | Atrasado: {FormatarMoeda(totalAtrasado, culture)}",
                string.Empty,
                "Vencimento Documento        Fornecedor                    Status       Valor          Pago        Saldo",
                "---------- ---------------- ----------------------------- ---------- ------------- ------------- -------------"
            };

            foreach (var conta in contas)
            {
                var saldo = conta.Valor - conta.ValorPago;
                var documento = Limitar(conta.NumeroDocumento, 16).PadRight(16);
                var fornecedor = Limitar(conta.FornecedorNome, 29).PadRight(29);
                var status = Limitar(conta.Status, 10).PadRight(10);
                linhas.Add($"{conta.DataVencimento:dd/MM/yyyy} {documento} {fornecedor} {status} {FormatarMoeda(conta.Valor, culture).PadLeft(13)} {FormatarMoeda(conta.ValorPago, culture).PadLeft(13)} {FormatarMoeda(saldo, culture).PadLeft(13)}");

                var classificacao = $"{conta.CentroCusto?.Nome ?? "Sem centro de custo"} / {conta.PlanoContas?.Nome ?? "Sem plano de contas"}";
                if (!string.IsNullOrWhiteSpace(classificacao))
                {
                    linhas.Add($"           Classificacao: {Limitar(classificacao, 90)}");
                }

                if (!string.IsNullOrWhiteSpace(conta.NumeroNFe) || !string.IsNullOrWhiteSpace(conta.FormaPagamento))
                {
                    linhas.Add($"           NFe: {ValorFiltro(conta.NumeroNFe)} | Forma: {ValorFiltro(conta.FormaPagamento)} | Parcela: {conta.ParcelaAtual ?? 1}/{conta.TotalParcelas ?? 1}");
                }
            }

            if (contas.Count == 0)
            {
                linhas.Add("Nenhuma conta encontrada para os filtros informados.");
            }

            return PdfFinanceiroBuilder.Gerar("Relatorio de Contas a Pagar", linhas);
        }

        private static string ValorFiltro(object? valor)
        {
            return valor switch
            {
                null => "Todos",
                DateTime data => data.ToString("dd/MM/yyyy"),
                string texto when string.IsNullOrWhiteSpace(texto) => "Todos",
                _ => valor.ToString() ?? "Todos"
            };
        }

        private static string FormatarMoeda(decimal valor, CultureInfo culture)
        {
            return valor.ToString("C", culture);
        }

        private static string Limitar(string? texto, int tamanho)
        {
            var normalizado = PdfFinanceiroBuilder.NormalizarTexto(texto ?? string.Empty);
            return normalizado.Length <= tamanho ? normalizado : normalizado[..tamanho];
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

    internal static class PdfFinanceiroBuilder
    {
        private const int LinhasPorPagina = 58;
        private const int MargemEsquerda = 36;
        private const int PrimeiraLinhaY = 806;
        private const int LarguraPagina = 595;
        private const int AlturaPagina = 842;

        public static byte[] Gerar(string titulo, IReadOnlyList<string> linhas)
        {
            var paginas = linhas
                .Select(NormalizarTexto)
                .Chunk(LinhasPorPagina)
                .Select(chunk => chunk.ToList())
                .ToList();

            if (paginas.Count == 0)
            {
                paginas.Add(new List<string> { NormalizarTexto(titulo) });
            }

            var objetos = new SortedDictionary<int, string>
            {
                [1] = "<< /Type /Catalog /Pages 2 0 R >>",
                [3] = "<< /Type /Font /Subtype /Type1 /BaseFont /Courier >>"
            };

            var kids = new List<string>();
            for (var paginaIndex = 0; paginaIndex < paginas.Count; paginaIndex++)
            {
                var paginaObjetoId = 4 + paginaIndex * 2;
                var conteudoObjetoId = paginaObjetoId + 1;
                kids.Add($"{paginaObjetoId} 0 R");

                objetos[paginaObjetoId] = $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {LarguraPagina} {AlturaPagina}] /Resources << /Font << /F1 3 0 R >> >> /Contents {conteudoObjetoId} 0 R >>";

                var conteudo = MontarConteudoPagina(paginas[paginaIndex], paginaIndex + 1, paginas.Count);
                var conteudoBytes = Encoding.ASCII.GetByteCount(conteudo);
                objetos[conteudoObjetoId] = $"<< /Length {conteudoBytes} >>\nstream\n{conteudo}\nendstream";
            }

            objetos[2] = $"<< /Type /Pages /Kids [{string.Join(' ', kids)}] /Count {paginas.Count} >>";

            return MontarPdf(objetos);
        }

        public static string NormalizarTexto(string texto)
        {
            var decomposed = texto.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);
            foreach (var character in decomposed)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(character);
                if (category == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                builder.Append(character switch
                {
                    '\u2013' or '\u2014' => '-',
                    '\u2018' or '\u2019' => '\'',
                    '\u201c' or '\u201d' => '"',
                    '\u00a0' => ' ',
                    _ when character <= 127 => character,
                    _ => '?'
                });
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }

        private static string MontarConteudoPagina(IReadOnlyList<string> linhas, int paginaAtual, int totalPaginas)
        {
            var builder = new StringBuilder();
            builder.AppendLine("BT");
            builder.AppendLine("/F1 8 Tf");
            builder.AppendLine("11 TL");
            builder.AppendLine($"{MargemEsquerda} {PrimeiraLinhaY} Td");

            foreach (var linha in linhas)
            {
                builder.Append('(').Append(Escapar(linha)).AppendLine(") Tj");
                builder.AppendLine("T*");
            }

            var rodape = $"Pagina {paginaAtual}/{totalPaginas}";
            builder.AppendLine($"0 -{Math.Max(0, LinhasPorPagina - linhas.Count + 2) * 11} Td");
            builder.Append('(').Append(Escapar(rodape)).AppendLine(") Tj");
            builder.AppendLine("ET");
            return builder.ToString();
        }

        private static byte[] MontarPdf(SortedDictionary<int, string> objetos)
        {
            var builder = new StringBuilder();
            var offsets = new Dictionary<int, int> { [0] = 0 };

            builder.AppendLine("%PDF-1.4");
            builder.AppendLine("%GenesisGest.Net");

            foreach (var item in objetos)
            {
                offsets[item.Key] = Encoding.ASCII.GetByteCount(builder.ToString());
                builder.Append(item.Key).AppendLine(" 0 obj");
                builder.AppendLine(item.Value);
                builder.AppendLine("endobj");
            }

            var xrefOffset = Encoding.ASCII.GetByteCount(builder.ToString());
            var totalObjetos = objetos.Keys.Max() + 1;

            builder.AppendLine("xref");
            builder.Append("0 ").Append(totalObjetos).AppendLine();
            builder.AppendLine("0000000000 65535 f ");

            for (var id = 1; id < totalObjetos; id++)
            {
                builder.Append(offsets[id].ToString("D10", CultureInfo.InvariantCulture)).AppendLine(" 00000 n ");
            }

            builder.AppendLine("trailer");
            builder.Append("<< /Size ").Append(totalObjetos).AppendLine(" /Root 1 0 R >>");
            builder.AppendLine("startxref");
            builder.AppendLine(xrefOffset.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("%%EOF");

            return Encoding.ASCII.GetBytes(builder.ToString());
        }

        private static string Escapar(string texto)
        {
            return texto
                .Replace("\\", "\\\\")
                .Replace("(", "\\(")
                .Replace(")", "\\)");
        }
    }
}
