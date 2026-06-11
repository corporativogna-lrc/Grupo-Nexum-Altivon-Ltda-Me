using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NexumAltivon.API.Data;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Models;

namespace NexumAltivon.API.Services
{
    public interface IMercadoLivreService
    {
        Task<MlProdutoPublicadoDto> PublicarProdutoAsync(int produtoId, string categoriaMl, decimal? precoEspecifico, int? estoqueEspecifico);
        Task<bool> AtualizarPrecoEstoqueAsync(string mlItemId, decimal preco, int estoque);
        Task<bool> PausarAnuncioAsync(string mlItemId);
        Task<bool> AtivarAnuncioAsync(string mlItemId);
        Task<bool> ExcluirAnuncioAsync(string mlItemId);
        Task<MlPedidoRecebidoDto> ImportarPedidoAsync(string mlOrderId);
        Task<List<MlPedidoRecebidoDto>> ImportarPedidosPendentesAsync();
        Task<bool> MarcarEnviadoAsync(string mlOrderId, string codigoRastreio);
        Task<SyncStatusDto> ObterStatusSyncAsync(int produtoId);
    }

    public class MercadoLivreService : IMercadoLivreService
    {
        private readonly HttpClient _httpClient;
        private readonly NexumDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<MercadoLivreService> _logger;
        private readonly string _accessToken;
        private readonly string _sellerId;

        public MercadoLivreService(IHttpClientFactory factory, NexumDbContext context, IConfiguration config, ILogger<MercadoLivreService> logger)
        {
            _httpClient = factory.CreateClient("MercadoLivre");
            _context = context;
            _config = config;
            _logger = logger;
            _accessToken = _config["Integracoes:MercadoLivre:AccessToken"];
            _sellerId = _config["Integracoes:MercadoLivre:SellerId"];
            _httpClient.BaseAddress = new Uri("https://api.mercadolibre.com");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
        }

        public async Task<MlProdutoPublicadoDto> PublicarProdutoAsync(int produtoId, string categoriaMl, decimal? precoEspecifico, int? estoqueEspecifico)
        {
            var produto = await _context.Produtos
                .Include(p => p.Loja)
                .FirstOrDefaultAsync(p => p.ProdutoId == produtoId && p.Ativo);
            if (produto == null) throw new ArgumentException("Produto não encontrado.");

            var preco = precoEspecifico ?? (produto.PrecoPromocional > 0 ? produto.PrecoPromocional : produto.Preco);
            var estoque = estoqueEspecifico ?? produto.Estoque;

            var request = new
            {
                title = produto.Nome,
                category_id = categoriaMl ?? "MLB3530",
                price = (double)preco,
                currency_id = "BRL",
                available_quantity = estoque,
                buying_mode = "buy_it_now",
                condition = "new",
                listing_type_id = "gold_pro",
                description = new { plain_text = produto.Descricao },
                pictures = new[] { new { source = produto.ImagemPrincipal } },
                attributes = new[]
                {
                    new { id = "BRAND", value_name = produto.Loja?.Nome ?? "Nexum Altivon" },
                    new { id = "MODEL", value_name = produto.Sku }
                }
            };

            var response = await _httpClient.PostAsJsonAsync("/items", request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Erro ao publicar no ML: {Content}", content);
                throw new InvalidOperationException($"Erro Mercado Livre: {content}");
            }

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            var itemId = root.GetProperty("id").GetString();
            var permalink = root.GetProperty("permalink").GetString();
            var status = root.GetProperty("status").GetString();

            // Salvar vínculo no banco
            var sync = new MarketplaceProduto
            {
                ProdutoId = produtoId,
                Canal = "mercadolivre",
                IdExterno = itemId,
                Url = permalink,
                Status = status,
                PrecoExterno = preco,
                EstoqueExterno = estoque,
                SyncEm = DateTime.UtcNow
            };
            _context.MarketplaceProdutos.Add(sync);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Produto {ProdutoId} publicado no ML: {ItemId}", produtoId, itemId);

            return new MlProdutoPublicadoDto
            {
                ProdutoId = produtoId,
                MlItemId = itemId,
                Permalink = permalink,
                Status = status,
                PrecoMl = preco,
                EstoqueMl = estoque,
                PublicadoEm = DateTime.UtcNow
            };
        }

        public async Task<bool> AtualizarPrecoEstoqueAsync(string mlItemId, decimal preco, int estoque)
        {
            var request = new { price = (double)preco, available_quantity = estoque };
            var response = await _httpClient.PutAsJsonAsync($"/items/{mlItemId}", request);
            var success = response.IsSuccessStatusCode;
            if (!success) _logger.LogWarning("Falha ao atualizar ML {ItemId}: {Status}", mlItemId, response.StatusCode);
            return success;
        }

        public async Task<bool> PausarAnuncioAsync(string mlItemId)
        {
            var response = await _httpClient.PutAsJsonAsync($"/items/{mlItemId}", new { status = "paused" });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AtivarAnuncioAsync(string mlItemId)
        {
            var response = await _httpClient.PutAsJsonAsync($"/items/{mlItemId}", new { status = "active" });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ExcluirAnuncioAsync(string mlItemId)
        {
            var response = await _httpClient.PutAsJsonAsync($"/items/{mlItemId}", new { status = "closed", deleted = true });
            if (response.IsSuccessStatusCode)
            {
                var sync = await _context.MarketplaceProdutos.FirstOrDefaultAsync(m => m.IdExterno == mlItemId && m.Canal == "mercadolivre");
                if (sync != null) { sync.Status = "excluido"; sync.SyncEm = DateTime.UtcNow; }
                await _context.SaveChangesAsync();
            }
            return response.IsSuccessStatusCode;
        }

        public async Task<MlPedidoRecebidoDto> ImportarPedidoAsync(string mlOrderId)
        {
            var response = await _httpClient.GetAsync($"/orders/{mlOrderId}");
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            var order = new MlPedidoRecebidoDto
            {
                MlOrderId = mlOrderId,
                Status = root.GetProperty("status").GetString(),
                CriadoEm = root.GetProperty("date_created").GetDateTime(),
                Total = (decimal)root.GetProperty("total_amount").GetDouble()
            };

            // Itens
            foreach (var item in root.GetProperty("order_items").EnumerateArray())
            {
                order.Itens.Add(new MlItemPedidoDto
                {
                    MlItemId = item.GetProperty("item").GetProperty("id").GetString(),
                    Titulo = item.GetProperty("item").GetProperty("title").GetString(),
                    Quantidade = item.GetProperty("quantity").GetInt32(),
                    PrecoUnitario = (decimal)item.GetProperty("unit_price").GetDouble()
                });
            }

            // Comprador
            var buyer = root.GetProperty("buyer");
            order.Comprador = new MlCompradorDto
            {
                MlUserId = buyer.GetProperty("id").GetInt64().ToString(),
                Nome = buyer.GetProperty("nickname").GetString(),
                Email = $"ml_{buyer.GetProperty("id").GetInt64()}@mercadolivre.com" // ML não expõe e-mail real via API pública
            };

            // Envio
            if (root.TryGetProperty("shipping", out var shipping))
            {
                order.Envio = new MlEnvioDto
                {
                    Modo = shipping.GetProperty("mode").GetString(),
                    Status = shipping.GetProperty("status").GetString(),
                    CodigoRastreio = shipping.TryGetProperty("tracking_number", out var track) ? track.GetString() : null
                };
            }

            // Criar ou atualizar pedido interno
            await CriarPedidoInternoDoMl(order);
            return order;
        }

        public async Task<List<MlPedidoRecebidoDto>> ImportarPedidosPendentesAsync()
        {
            var response = await _httpClient.GetAsync($"/orders/search?seller={_sellerId}&order.status=paid&limit=50");
            if (!response.IsSuccessStatusCode) return new List<MlPedidoRecebidoDto>();

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var results = doc.RootElement.GetProperty("results");
            var pedidos = new List<MlPedidoRecebidoDto>();

            foreach (var order in results.EnumerateArray())
            {
                var mlOrderId = order.GetProperty("id").GetInt64().ToString();
                // Evitar duplicados
                var existente = await _context.Pedidos.AnyAsync(p => p.TransacaoGatewayId == mlOrderId && p.Origem == "mercadolivre");
                if (!existente)
                {
                    var pedido = await ImportarPedidoAsync(mlOrderId);
                    if (pedido != null) pedidos.Add(pedido);
                }
            }

            return pedidos;
        }

        public async Task<bool> MarcarEnviadoAsync(string mlOrderId, string codigoRastreio)
        {
            // Mercado Livre exige envio via API de shipments
            var response = await _httpClient.PostAsJsonAsync($"/orders/{mlOrderId}/shipments", new { });
            return response.IsSuccessStatusCode;
        }

        public async Task<SyncStatusDto> ObterStatusSyncAsync(int produtoId)
        {
            var produto = await _context.Produtos.FindAsync(produtoId);
            var syncs = await _context.MarketplaceProdutos
                .Where(m => m.ProdutoId == produtoId && m.Canal == "mercadolivre")
                .ToListAsync();

            return new SyncStatusDto
            {
                ProdutoId = produtoId,
                ProdutoNome = produto?.Nome,
                Canais = syncs.Select(s => new CanalSyncDto
                {
                    Canal = "mercadolivre",
                    Status = s.Status,
                    IdExterno = s.IdExterno,
                    Url = s.Url,
                    SyncEm = s.SyncEm
                }).ToList(),
                UltimaSync = syncs.Max(s => s.SyncEm)
            };
        }

        private async Task CriarPedidoInternoDoMl(MlPedidoRecebidoDto mlOrder)
        {
            // Buscar ou criar cliente
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Email == mlOrder.Comprador.Email);
            if (cliente == null)
            {
                cliente = new Cliente
                {
                    Nome = mlOrder.Comprador.Nome,
                    Email = mlOrder.Comprador.Email,
                    Telefone = mlOrder.Comprador.Telefone,
                    CriadoEm = DateTime.UtcNow
                };
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();
            }

            var pedido = new Pedido
            {
                NumeroPedido = $"ML-{mlOrder.MlOrderId}",
                ClienteId = cliente.ClienteId,
                Total = mlOrder.Total,
                Status = "PAGO",
                MetodoPagamento = "mercadolivre",
                TransacaoGatewayId = mlOrder.MlOrderId,
                Origem = "mercadolivre",
                PagoEm = mlOrder.CriadoEm,
                CriadoEm = DateTime.UtcNow
            };

            foreach (var item in mlOrder.Itens)
            {
                var produto = await _context.MarketplaceProdutos
                    .Where(m => m.IdExterno == item.MlItemId && m.Canal == "mercadolivre")
                    .Select(m => m.ProdutoId)
                    .FirstOrDefaultAsync();

                pedido.Itens.Add(new PedidoItem
                {
                    ProdutoId = produto,
                    ProdutoNome = item.Titulo,
                    Quantidade = item.Quantidade,
                    PrecoUnitario = item.PrecoUnitario,
                    Subtotal = item.Quantidade * item.PrecoUnitario
                });
            }

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Pedido ML {MlOrderId} importado como {NumeroPedido}", mlOrder.MlOrderId, pedido.NumeroPedido);
        }
    }
}
