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
    public interface IFiscalService
    {
        Task<NotaFiscal> EmitirNFeAsync(int pedidoId, string usuario);
        Task<NotaFiscal> EmitirNFeAsync(EmitirNFeDto dto);
        Task<NotaFiscal> CancelarNFeAsync(int notaFiscalId, string justificativa, string usuario);
        Task<NotaFiscal> CancelarNFeAsync(CancelarNFeDto dto);
        Task<NotaFiscal?> ConsultarNFeAsync(int notaFiscalId);
        Task<NotaFiscal?> ObterNFePorIdAsync(int notaFiscalId);
        Task<IEnumerable<NotaFiscal>> ListarNotasFiscaisAsync(string? status = null, DateTime? inicio = null, DateTime? fim = null);
        Task<IEnumerable<NotaFiscal>> ListarNFesAsync(string? status = null, int? lojaId = null, DateTime? inicio = null, DateTime? fim = null);
        Task<ConfiguracaoFiscalDto?> ObterConfiguracaoFiscalAsync(int lojaId);
        Task<ConfiguracaoFiscalDto> SalvarConfiguracaoFiscalAsync(ConfiguracaoFiscalDto dto);
        Task<IEnumerable<object>> ListarManifestosPendentesAsync();
        Task<object> ConfirmarManifestoAsync(int id, string operacao);
        Task<ConfiguracaoImposto?> ObterConfiguracaoImpostoAsync(string ncm, string cfop, string ufOrigem, string ufDestino);
        Task CalcularImpostosItemAsync(ItemNotaFiscal item, ConfiguracaoImposto config);
        Task<byte[]> DownloadXmlNFeAsync(int notaFiscalId);
        Task<byte[]> DownloadDanfeAsync(int notaFiscalId);
    }

    public class FiscalService : IFiscalService
    {
        private readonly NexumDbContext _context;

        public FiscalService(NexumDbContext context)
        {
            _context = context;
        }

        public async Task<NotaFiscal> EmitirNFeAsync(int pedidoId, string usuario)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
                .Include(p => p.Cliente)
                .Include(p => p.Loja)
                .FirstOrDefaultAsync(p => p.Id == pedidoId)
                ?? throw new Exception("Pedido não encontrado.");

            if (pedido.Status != "Pago")
                throw new Exception("NFe so pode ser emitida para pedidos pagos.");

            var numero = await GerarNumeroNotaAsync();

            var nota = new NotaFiscal
            {
                Numero = numero,
                Serie = "1",
                Tipo = "Saida",
                NaturezaOperacao = "Venda de Mercadoria",
                EmitenteId = pedido.LojaId,
                DestinatarioId = pedido.ClienteId,
                DataEmissao = DateTime.Now,
                DataSaidaEntrada = DateTime.Now,
                Status = "Processando",
                PedidoId = pedidoId,
                LojaId = pedido.LojaId,
                CriadoEm = DateTime.Now,
                CriadoPor = usuario
            };

            _context.NotasFiscais.Add(nota);
            await _context.SaveChangesAsync();

            decimal valorTotal = 0;
            decimal valorIcms = 0;

            foreach (var itemPedido in pedido.Itens ?? new List<PedidoItem>())
            {
                var produto = itemPedido.Produto;
                if (produto == null) continue;

                var config = await ObterConfiguracaoImpostoAsync(
                    produto.Ncm ?? "9999.99.99",
                    "5102",
                    "SP",
                    pedido.Cliente?.Uf ?? "SP"
                );

                var itemNf = new ItemNotaFiscal
                {
                    NotaFiscalId = nota.Id,
                    ProdutoId = produto.Id,
                    Descricao = produto.Nome,
                    Cfop = config?.Cfop ?? "5102",
                    Ncm = produto.Ncm ?? "9999.99.99",
                    CstIcms = config?.CstIcms ?? "00",
                    CstPis = config?.CstPis ?? "01",
                    CstCofins = config?.CstCofins ?? "01",
                    Quantidade = itemPedido.Quantidade,
                    ValorUnitario = itemPedido.ValorUnitario,
                    ValorTotal = itemPedido.Quantidade * itemPedido.ValorUnitario,
                    AliquotaIcms = config?.AliquotaIcms ?? 18.00m,
                    BaseCalculoIcms = itemPedido.Quantidade * itemPedido.ValorUnitario,
                    CriadoEm = DateTime.Now
                };

                await CalcularImpostosItemAsync(itemNf, config ?? new ConfiguracaoImposto());

                valorTotal += itemNf.ValorTotal;
                valorIcms += itemNf.ValorIcms;

                _context.ItensNotaFiscal.Add(itemNf);
            }

            nota.ValorTotal = valorTotal;
            nota.ValorIcms = valorIcms;
            nota.Status = "Emitida";
            nota.ChaveAcesso = await GerarChaveAcessoAsync(nota);
            nota.ProtocoloAutorizacao = $"PROT-{DateTime.Now:yyyyMMddHHmmss}-{nota.Id}";

            await _context.SaveChangesAsync();

            // Registra saida de estoque
            var estoqueService = new EstoqueService(_context);
            foreach (var itemPedido in pedido.Itens ?? new List<PedidoItem>())
            {
                if (itemPedido.Produto != null)
                {
                    await estoqueService.RegistrarSaidaAsync(
                        itemPedido.ProdutoId,
                        itemPedido.Quantidade,
                        pedidoId,
                        nota.Numero,
                        "Saida por venda - NFe emitida",
                        pedido.LojaId,
                        usuario
                    );
                }
            }

            return nota;
        }

        public Task<NotaFiscal> EmitirNFeAsync(EmitirNFeDto dto)
        {
            return EmitirNFeAsync(dto.PedidoId, "sistema");
        }

        public async Task<NotaFiscal> CancelarNFeAsync(int notaFiscalId, string justificativa, string usuario)
        {
            var nota = await _context.NotasFiscais.FindAsync(notaFiscalId)
                ?? throw new Exception("Nota fiscal nao encontrada.");

            if (nota.Status == "Cancelada")
                throw new Exception("Nota ja esta cancelada.");

            var horasDesdeEmissao = (DateTime.Now - nota.DataEmissao).TotalHours;
            if (horasDesdeEmissao > 168)
                throw new Exception("Prazo para cancelamento expirado (168h). Use inutilizacao.");

            nota.Status = "Cancelada";
            nota.Observacoes = $"[CANCELAMENTO {DateTime.Now:dd/MM/yyyy HH:mm}] {justificativa} - por {usuario}";
            nota.AtualizadoEm = DateTime.Now;

            var itens = await _context.ItensNotaFiscal
                .Where(i => i.NotaFiscalId == notaFiscalId)
                .ToListAsync();

            var estoqueService = new EstoqueService(_context);
            foreach (var item in itens)
            {
                await estoqueService.RegistrarEntradaAsync(
                    item.ProdutoId,
                    item.Quantidade,
                    item.ValorUnitario,
                    null,
                    $"ESTORNO-{nota.Numero}",
                    "Estorno de cancelamento de NFe",
                    nota.LojaId,
                    usuario
                );
            }

            await _context.SaveChangesAsync();
            return nota;
        }

        public Task<NotaFiscal> CancelarNFeAsync(CancelarNFeDto dto)
        {
            return CancelarNFeAsync(dto.NotaFiscalId, dto.Justificativa, "sistema");
        }

        public async Task<NotaFiscal?> ConsultarNFeAsync(int notaFiscalId)
        {
            return await _context.NotasFiscais
                .Include(n => n.Itens)
                .FirstOrDefaultAsync(n => n.Id == notaFiscalId);
        }

        public Task<NotaFiscal?> ObterNFePorIdAsync(int notaFiscalId)
        {
            return ConsultarNFeAsync(notaFiscalId);
        }

        public async Task<IEnumerable<NotaFiscal>> ListarNotasFiscaisAsync(string? status = null, DateTime? inicio = null, DateTime? fim = null)
        {
            var query = _context.NotasFiscais.AsQueryable();
            if (!string.IsNullOrEmpty(status))
                query = query.Where(n => n.Status == status);
            if (inicio.HasValue)
                query = query.Where(n => n.DataEmissao >= inicio.Value);
            if (fim.HasValue)
                query = query.Where(n => n.DataEmissao <= fim.Value);
            return await query.OrderByDescending(n => n.DataEmissao).ToListAsync();
        }

        public async Task<IEnumerable<NotaFiscal>> ListarNFesAsync(string? status = null, int? lojaId = null, DateTime? inicio = null, DateTime? fim = null)
        {
            var query = _context.NotasFiscais.AsQueryable();
            if (!string.IsNullOrEmpty(status))
                query = query.Where(n => n.Status == status);
            if (lojaId.HasValue)
                query = query.Where(n => n.LojaId == lojaId.Value);
            if (inicio.HasValue)
                query = query.Where(n => n.DataEmissao >= inicio.Value);
            if (fim.HasValue)
                query = query.Where(n => n.DataEmissao <= fim.Value);
            return await query.OrderByDescending(n => n.DataEmissao).ToListAsync();
        }

        public Task<ConfiguracaoFiscalDto?> ObterConfiguracaoFiscalAsync(int lojaId)
        {
            return Task.FromResult<ConfiguracaoFiscalDto?>(new ConfiguracaoFiscalDto { LojaId = lojaId });
        }

        public Task<ConfiguracaoFiscalDto> SalvarConfiguracaoFiscalAsync(ConfiguracaoFiscalDto dto)
        {
            return Task.FromResult(dto);
        }

        public Task<IEnumerable<object>> ListarManifestosPendentesAsync()
        {
            return Task.FromResult<IEnumerable<object>>(Array.Empty<object>());
        }

        public Task<object> ConfirmarManifestoAsync(int id, string operacao)
        {
            return Task.FromResult<object>(new { id, operacao, sucesso = true });
        }

        public async Task<ConfiguracaoImposto?> ObterConfiguracaoImpostoAsync(string ncm, string cfop, string ufOrigem, string ufDestino)
        {
            return await _context.ConfiguracoesImposto
                .FirstOrDefaultAsync(c =>
                    c.Ncm == ncm &&
                    c.Cfop == cfop &&
                    c.UfOrigem == ufOrigem &&
                    c.UfDestino == ufDestino &&
                    c.Ativo);
        }

        public Task CalcularImpostosItemAsync(ItemNotaFiscal item, ConfiguracaoImposto config)
        {
            var baseCalculo = item.ValorTotal;
            item.BaseCalculoIcms = baseCalculo;
            item.ValorIcms = baseCalculo * (item.AliquotaIcms / 100);
            return Task.CompletedTask;
        }

        public async Task<byte[]> DownloadXmlNFeAsync(int notaFiscalId)
        {
            var nota = await ConsultarNFeAsync(notaFiscalId)
                ?? throw new Exception("Nota fiscal nao encontrada.");

            var xml = $@"<?xml version='1.0' encoding='UTF-8'?>
<NFe>
  <infNFe Id='NFe{nota.ChaveAcesso}'>
    <ide>
      <nNF>{nota.Numero}</nNF>
      <dhEmi>{nota.DataEmissao:yyyy-MM-ddTHH:mm:ss}</dhEmi>
    </ide>
    <total>
      <ICMSTot>
        <vNF>{nota.ValorTotal:N2}</vNF>
        <vICMS>{nota.ValorIcms:N2}</vICMS>
      </ICMSTot>
    </total>
    <protNFe>
      <infProt>
        <chNFe>{nota.ChaveAcesso}</chNFe>
        <nProt>{nota.ProtocoloAutorizacao}</nProt>
      </infProt>
    </protNFe>
  </infNFe>
</NFe>";

            return System.Text.Encoding.UTF8.GetBytes(xml);
        }

        public async Task<byte[]> DownloadDanfeAsync(int notaFiscalId)
        {
            var nota = await ConsultarNFeAsync(notaFiscalId)
                ?? throw new Exception("Nota fiscal nao encontrada.");

            var html = $@"<!DOCTYPE html><html><head><meta charset='utf-8'><title>DANFE {nota.Numero}</title></head><body>
<h2>DANFE - Documento Auxiliar da Nota Fiscal Eletronica</h2>
<p><strong>Chave:</strong> {nota.ChaveAcesso}</p>
<p><strong>Numero:</strong> {nota.Numero}</p>
<p><strong>Valor Total:</strong> R$ {nota.ValorTotal:N2}</p>
<p><strong>ICMS:</strong> R$ {nota.ValorIcms:N2}</p>
<p><strong>Protocolo:</strong> {nota.ProtocoloAutorizacao}</p>
<p style='font-size:10px;'>Grupo Nexum Altivon - www.nexumaltivon.com</p>
</body></html>";

            return System.Text.Encoding.UTF8.GetBytes(html);
        }

        private async Task<string> GerarNumeroNotaAsync()
        {
            var ultima = await _context.NotasFiscais
                .OrderByDescending(n => n.Id)
                .Select(n => n.Numero)
                .FirstOrDefaultAsync();

            var numero = 1;
            if (!string.IsNullOrEmpty(ultima) && int.TryParse(ultima, out var ultimoNumero))
                numero = ultimoNumero + 1;

            return numero.ToString().PadLeft(9, '0');
        }

        private Task<string> GerarChaveAcessoAsync(NotaFiscal nota)
        {
            var chave = $"35{DateTime.Now:yyMM}{nota.CriadoPor?.Substring(0, 3).ToUpper() ?? "NXA"}{nota.Numero.PadLeft(9, '0')}55{nota.Serie.PadLeft(3, '0')}{nota.Id.ToString().PadLeft(9, '0')}1";
            return Task.FromResult(chave.PadRight(44, '0').Substring(0, 44));
        }
    }
}
