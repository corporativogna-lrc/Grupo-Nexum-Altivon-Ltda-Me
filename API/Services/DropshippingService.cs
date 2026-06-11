using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NexumAltivon.API.Data;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Models;
using NexumAltivon.API.Services;

namespace NexumAltivon.API.Services
{
    public interface IDropshippingService
    {
        Task<DropshippingPedidoDto> RoteiarPedidoAsync(int pedidoId, bool autoSelecionar);
        Task<List<DropshippingPedidoDto>> ObterPedidosPendentesAsync();
        Task<bool> AtualizarStatusAsync(int dropshippingId, string status, string codigoRastreio, string urlRastreio);
        Task<List<FornecedorDropshippingDto>> ListarFornecedoresAsync();
        Task<ComissaoDropshippingDto> ObterComissaoFornecedorAsync(int fornecedorId, DateTime inicio, DateTime fim);
        Task<bool> NotificarFornecedorAsync(int dropshippingId);
    }

    public class DropshippingService : IDropshippingService
    {
        private readonly NexumDbContext _context;
        private readonly INotificacaoService _notificacao;
        private readonly ILogAuditoriaService _auditoria;
        private readonly ILogger<DropshippingService> _logger;

        public DropshippingService(
            NexumDbContext context,
            INotificacaoService notificacao,
            ILogAuditoriaService auditoria,
            ILogger<DropshippingService> logger)
        {
            _context = context;
            _notificacao = notificacao;
            _auditoria = auditoria;
            _logger = logger;
        }

        public async Task<DropshippingPedidoDto> RoteiarPedidoAsync(int pedidoId, bool autoSelecionar)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(p => p.PedidoId == pedidoId && p.Status == "PAGO");

            if (pedido == null) throw new ArgumentException("Pedido não encontrado ou não está pago.");

            // Verificar se já foi roteado
            var existente = await _context.DropshippingPedidos.FirstOrDefaultAsync(d => d.PedidoId == pedidoId);
            if (existente != null) throw new InvalidOperationException("Pedido já roteado para dropshipping.");

            // Agrupar itens por fornecedor (simplificado: 1 fornecedor por pedido na v1)
            var fornecedores = await _context.Fornecedores
                .Where(f => f.Ativo && f.AceitaDropshipping)
                .ToListAsync();

            if (!fornecedores.Any()) throw new InvalidOperationException("Nenhum fornecedor de dropshipping disponível.");

            // Seleção inteligente: menor preço de custo ou menor prazo
            Fornecedor fornecedorSelecionado;
            if (autoSelecionar)
            {
                // Heurística: fornecedor com menor custo total dos itens
                var custos = fornecedores.Select(f => new
                {
                    Fornecedor = f,
                    CustoTotal = pedido.Itens.Sum(i =>
                        _context.FornecedorProdutos
                            .Where(fp => fp.FornecedorId == f.FornecedorId && fp.ProdutoId == i.ProdutoId)
                            .Select(fp => (decimal?)fp.PrecoCusto)
                            .FirstOrDefault() ?? i.PrecoUnitario * 0.6m)
                });
                fornecedorSelecionado = custos.OrderBy(c => c.CustoTotal).First().Fornecedor;
            }
            else
            {
                fornecedorSelecionado = fornecedores.First(); // Admin escolherá via painel futuramente
            }

            var ds = new DropshippingPedido
            {
                PedidoId = pedidoId,
                FornecedorId = fornecedorSelecionado.FornecedorId,
                Status = "AGUARDANDO_ENVIO",
                ComissaoPercentual = fornecedorSelecionado.ComissaoDropshipping,
                CriadoEm = DateTime.UtcNow
            };

            foreach (var item in pedido.Itens)
            {
                var custo = await _context.FornecedorProdutos
                    .Where(fp => fp.FornecedorId == fornecedorSelecionado.FornecedorId && fp.ProdutoId == item.ProdutoId)
                    .Select(fp => (decimal?)fp.PrecoCusto)
                    .FirstOrDefaultAsync() ?? item.PrecoUnitario * 0.6m;

                ds.Itens.Add(new DropshippingItem
                {
                    ProdutoId = item.ProdutoId,
                    ProdutoNome = item.ProdutoNome,
                    Quantidade = item.Quantidade,
                    PrecoCusto = custo,
                    PrecoVenda = item.PrecoUnitario
                });
            }

            ds.ValorProdutos = ds.Itens.Sum(i => i.PrecoCusto * i.Quantidade);
            ds.ValorComissao = ds.Itens.Sum(i => (i.PrecoVenda - i.PrecoCusto) * i.Quantidade);
            ds.ValorFornecedor = ds.ValorProdutos;

            _context.DropshippingPedidos.Add(ds);
            pedido.Status = "ROTEADO_DROPSHIPPING";
            await _context.SaveChangesAsync();

            await _notificacao.EnviarEmailAsync(
                fornecedorSelecionado.Email,
                $"Novo Pedido Dropshipping - {pedido.NumeroPedido}",
                $"<p>Pedido <strong>{pedido.NumeroPedido}</strong> aguardando envio.</p><p>Valor produtos: R$ {ds.ValorFornecedor:N2}</p>");

            await _auditoria.RegistrarAsync("DROPSHIPPING", $"Pedido {pedidoId} roteado para fornecedor {fornecedorSelecionado.FornecedorId}", null);

            return MapearDto(ds);
        }

        public async Task<List<DropshippingPedidoDto>> ObterPedidosPendentesAsync()
        {
            var pendentes = await _context.DropshippingPedidos
                .Include(d => d.Fornecedor)
                .Include(d => d.Pedido)
                .Include(d => d.Itens)
                .Where(d => d.Status == "AGUARDANDO_ENVIO" || d.Status == "ENVIADO")
                .OrderBy(d => d.CriadoEm)
                .ToListAsync();

            return pendentes.Select(MapearDto).ToList();
        }

        public async Task<bool> AtualizarStatusAsync(int dropshippingId, string status, string codigoRastreio, string urlRastreio)
        {
            var ds = await _context.DropshippingPedidos
                .Include(d => d.Pedido)
                .FirstOrDefaultAsync(d => d.DropshippingId == dropshippingId);

            if (ds == null) return false;

            ds.Status = status;
            if (!string.IsNullOrEmpty(codigoRastreio)) ds.CodigoRastreio = codigoRastreio;
            if (!string.IsNullOrEmpty(urlRastreio)) ds.UrlRastreio = urlRastreio;

            if (status == "ENVIADO") ds.EnviadoEm = DateTime.UtcNow;

            // Atualizar pedido principal
            switch (status)
            {
                case "ENVIADO":
                    ds.Pedido.Status = "ENVIADO";
                    ds.Pedido.CodigoRastreio = codigoRastreio;
                    ds.Pedido.EnviadoEm = DateTime.UtcNow;
                    break;
                case "ENTREGUE":
                    ds.Pedido.Status = "ENTREGUE";
                    ds.Pedido.EntregueEm = DateTime.UtcNow;
                    break;
                case "CANCELADO":
                    ds.Pedido.Status = "CANCELADO";
                    break;
            }

            await _context.SaveChangesAsync();

            // Notificar cliente
            var cliente = await _context.Clientes.FindAsync(ds.Pedido.ClienteId);
            if (cliente != null)
            {
                var msg = status switch
                {
                    "ENVIADO" => $"Seu pedido {ds.Pedido.NumeroPedido} foi enviado! Código de rastreio: {codigoRastreio}",
                    "ENTREGUE" => $"Seu pedido {ds.Pedido.NumeroPedido} foi entregue! Obrigado por comprar conosco.",
                    _ => $"Status do pedido {ds.Pedido.NumeroPedido} atualizado para: {status}"
                };
                await _notificacao.EnviarStatusPedidoAsync(cliente, ds.Pedido, msg);
            }

            return true;
        }

        public async Task<List<FornecedorDropshippingDto>> ListarFornecedoresAsync()
        {
            var fornecedores = await _context.Fornecedores
                .Where(f => f.AceitaDropshipping)
                .ToListAsync();

            return fornecedores.Select(f => new FornecedorDropshippingDto
            {
                FornecedorId = f.FornecedorId,
                RazaoSocial = f.RazaoSocial,
                Cnpj = f.Cnpj,
                Email = f.Email,
                Telefone = f.Telefone,
                ComissaoPadrao = f.ComissaoDropshipping,
                PrazoEnvioDias = f.PrazoEnvioDias,
                Ativo = f.Ativo
            }).ToList();
        }

        public async Task<ComissaoDropshippingDto> ObterComissaoFornecedorAsync(int fornecedorId, DateTime inicio, DateTime fim)
        {
            var pedidos = await _context.DropshippingPedidos
                .Where(d => d.FornecedorId == fornecedorId && d.CriadoEm >= inicio && d.CriadoEm <= fim)
                .ToListAsync();

            var fornecedor = await _context.Fornecedores.FindAsync(fornecedorId);

            return new ComissaoDropshippingDto
            {
                FornecedorId = fornecedorId,
                FornecedorNome = fornecedor?.RazaoSocial,
                TotalPedidos = pedidos.Count,
                TotalVendido = pedidos.Sum(d => d.Itens.Sum(i => i.PrecoVenda * i.Quantidade)),
                TotalComissao = pedidos.Sum(d => d.ValorComissao),
                TotalPagoFornecedor = pedidos.Sum(d => d.ValorFornecedor),
                SaldoPendente = pedidos.Where(d => d.Status != "PAGO_FORNECEDOR").Sum(d => d.ValorFornecedor)
            };
        }

        public async Task<bool> NotificarFornecedorAsync(int dropshippingId)
        {
            var ds = await _context.DropshippingPedidos
                .Include(d => d.Fornecedor)
                .Include(d => d.Pedido)
                .FirstOrDefaultAsync(d => d.DropshippingId == dropshippingId);

            if (ds == null) return false;

            var corpo = $@"
<h2>Novo Pedido Dropshipping - {ds.Pedido.NumeroPedido}</h2>
<p><strong>Fornecedor:</strong> {ds.Fornecedor.RazaoSocial}</p>
<p><strong>Valor Produtos:</strong> R$ {ds.ValorFornecedor:N2}</p>
<p><strong>Comissão Nexum:</strong> R$ {ds.ValorComissao:N2}</p>
<p><strong>Prazo de Envio:</strong> {ds.Fornecedor.PrazoEnvioDias} dias úteis</p>
<p>Acesse o painel para imprimir a etiqueta e confirmar envio.</p>";

            await _notificacao.EnviarEmailAsync(ds.Fornecedor.Email, $"Pedido Dropshipping {ds.Pedido.NumeroPedido}", corpo);
            return true;
        }

        private DropshippingPedidoDto MapearDto(DropshippingPedido ds)
        {
            return new DropshippingPedidoDto
            {
                DropshippingId = ds.DropshippingId,
                PedidoId = ds.PedidoId,
                NumeroPedido = ds.Pedido?.NumeroPedido,
                FornecedorId = ds.FornecedorId,
                FornecedorNome = ds.Fornecedor?.RazaoSocial,
                Status = ds.Status,
                ValorProdutos = ds.ValorProdutos,
                ComissaoPercentual = ds.ComissaoPercentual,
                ValorComissao = ds.ValorComissao,
                ValorFornecedor = ds.ValorFornecedor,
                CriadoEm = ds.CriadoEm,
                EnviadoEm = ds.EnviadoEm,
                CodigoRastreio = ds.CodigoRastreio,
                Itens = ds.Itens.Select(i => new DropshippingItemDto
                {
                    ProdutoId = i.ProdutoId,
                    ProdutoNome = i.ProdutoNome,
                    Quantidade = i.Quantidade,
                    PrecoCusto = i.PrecoCusto,
                    PrecoVenda = i.PrecoVenda
                }).ToList()
            };
        }
    }
}
