using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NexumAltivon.API.Data;
using NexumAltivon.API.Models;

namespace NexumAltivon.API.Services
{
    public interface IPedidoService
    {
        Task<Pedido> GerarPedidoDoCheckoutAsync(Checkout checkout);
        Task<string> GerarNumeroPedidoAsync();
    }

    public class PedidoService : IPedidoService
    {
        private readonly NexumDbContext _context;

        public PedidoService(NexumDbContext context)
        {
            _context = context;
        }

        public async Task<Pedido> GerarPedidoDoCheckoutAsync(Checkout checkout)
        {
            var numero = await GerarNumeroPedidoAsync();
            var pedido = new Pedido
            {
                NumeroPedido = numero,
                ClienteId = checkout.ClienteId,
                EnderecoId = checkout.EnderecoId,
                Subtotal = checkout.Subtotal,
                Desconto = checkout.Desconto,
                Frete = checkout.Frete,
                Total = checkout.Total,
                Status = "AGUARDANDO_PAGAMENTO",
                CriadoEm = DateTime.UtcNow
            };

            foreach (var item in checkout.Itens)
            {
                pedido.Itens.Add(new PedidoItem
                {
                    ProdutoId = item.ProdutoId,
                    ProdutoNome = item.ProdutoNome,
                    Sku = item.Sku ?? "",
                    Quantidade = item.Quantidade,
                    PrecoUnitario = item.PrecoUnitario,
                    Subtotal = item.Subtotal,
                    LojaId = item.LojaId
                });
            }

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();
            return pedido;
        }

        public async Task<string> GerarNumeroPedidoAsync()
        {
            var hoje = DateTime.UtcNow;
            var prefixo = $"NX{hoje:yyMMdd}";
            var sequencial = await _context.Pedidos.CountAsync(p => p.CriadoEm.Date == hoje.Date) + 1;
            return $"{prefixo}{sequencial:D3}X{new Random().Next(100, 999)}";
        }
    }
}
