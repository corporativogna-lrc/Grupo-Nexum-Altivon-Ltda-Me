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
    public interface IMarketplaceHubService
    {
        Task<SyncStatusDto> SincronizarProdutoAsync(int produtoId, string canal, bool forcar);
        Task<List<SyncStatusDto>> SincronizarLoteAsync(List<int> produtoIds, List<string> canais);
        Task<RelatorioSyncDto> ObterRelatorioSyncAsync(DateTime inicio, DateTime fim);
        Task<bool> PublicarShopeeAsync(int produtoId, long? categoryId, decimal? preco);
        Task<bool> PublicarAmazonAsync(int produtoId, string asin, string skuAmazon);
        Task<bool> AtualizarEstoqueMultiCanalAsync(int produtoId, int novoEstoque);
    }

    public class MarketplaceHubService : IMarketplaceHubService
    {
        private readonly NexumDbContext _context;
        private readonly IMercadoLivreService _ml;
        private readonly IConfiguration _config;
        private readonly ILogger<MarketplaceHubService> _logger;
        private readonly HttpClient _shopeeClient;
        private readonly HttpClient _amazonClient;

        public MarketplaceHubService(
            NexumDbContext context,
            IMercadoLivreService ml,
            IHttpClientFactory factory,
            IConfiguration config,
            ILogger<MarketplaceHubService> logger)
        {
            _context = context;
            _ml = ml;
            _config = config;
            _logger = logger;
            _shopeeClient = factory.CreateClient("Shopee");
            _amazonClient = factory.CreateClient("Amazon");
        }

        public async Task<SyncStatusDto> SincronizarProdutoAsync(int produtoId, string canal, bool forcar)
        {
            var produto = await _context.Produtos.FindAsync(produtoId);
            if (produto == null) throw new ArgumentException("Produto não encontrado.");

            var syncExistente = await _context.MarketplaceProdutos
                .FirstOrDefaultAsync(m => m.ProdutoId == produtoId && m.Canal == canal);

            if (syncExistente != null && !forcar)
            {
                // Apenas atualizar preço/estoque
                var preco = produto.PrecoPromocional > 0 ? produto.PrecoPromocional : produto.Preco;
                switch (canal.ToLower())
                {
                    case "mercadolivre":
                        await _ml.AtualizarPrecoEstoqueAsync(syncExistente.IdExterno, preco, produto.Estoque);
                        break;
                    case "shopee":
                        await AtualizarShopeeAsync(syncExistente.IdExterno, preco, produto.Estoque);
                        break;
                }
                syncExistente.PrecoExterno = preco;
                syncExistente.EstoqueExterno = produto.Estoque;
                syncExistente.SyncEm = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return await ObterStatusSync(produtoId, canal);
            }

            // Publicar novo
            switch (canal.ToLower())
            {
                case "mercadolivre":
                    var ml = await _ml.PublicarProdutoAsync(produtoId, null, null, null);
                    return await ObterStatusSync(produtoId, canal);
                case "shopee":
                    await PublicarShopeeAsync(produtoId, null, null);
                    return await ObterStatusSync(produtoId, canal);
                case "amazon":
                    await PublicarAmazonAsync(produtoId, null, null);
                    return await ObterStatusSync(produtoId, canal);
                default:
                    throw new ArgumentException($"Canal {canal} não suportado.");
            }
        }

        public async Task<List<SyncStatusDto>> SincronizarLoteAsync(List<int> produtoIds, List<string> canais)
        {
            var resultados = new List<SyncStatusDto>();
            foreach (var produtoId in produtoIds)
            {
                foreach (var canal in canais)
                {
                    try
                    {
                        var status = await SincronizarProdutoAsync(produtoId, canal, false);
                        resultados.Add(status);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro sync lote: Produto {ProdutoId}, Canal {Canal}", produtoId, canal);
                        resultados.Add(new SyncStatusDto
                        {
                            ProdutoId = produtoId,
                            Canais = new List<CanalSyncDto> { new() { Canal = canal, Status = "ERRO", Erro = ex.Message } }
                        });
                    }
                }
            }
            return resultados;
        }

        public async Task<RelatorioSyncDto> ObterRelatorioSyncAsync(DateTime inicio, DateTime fim)
        {
            var syncs = await _context.MarketplaceProdutos
                .Where(m => m.SyncEm >= inicio && m.SyncEm <= fim)
                .ToListAsync();

            var produtos = syncs.Select(s => s.ProdutoId).Distinct().Count();
            var ok = syncs.Count(s => s.Status == "active" || s.Status == "SINCRONIZADO");
            var erro = syncs.Count(s => s.Status == "ERRO" || s.Status == "paused");
            var pendentes = syncs.Count(s => string.IsNullOrEmpty(s.Status) || s.Status == "PENDENTE");

            return new RelatorioSyncDto
            {
                DataInicio = inicio,
                DataFim = fim,
                TotalProdutos = produtos,
                Sincronizados = ok,
                ComErro = erro,
                Pendentes = pendentes,
                Detalhes = syncs.GroupBy(s => s.ProdutoId)
                    .Select(g => new SyncStatusDto
                    {
                        ProdutoId = g.Key,
                        Canais = g.Select(m => new CanalSyncDto
                        {
                            Canal = m.Canal,
                            Status = m.Status,
                            IdExterno = m.IdExterno,
                            SyncEm = m.SyncEm
                        }).ToList()
                    }).ToList()
            };
        }

        public async Task<bool> PublicarShopeeAsync(int produtoId, long? categoryId, decimal? preco)
        {
            var produto = await _context.Produtos.FindAsync(produtoId);
            if (produto == null) return false;

            var shopeeBase = _config["Integracoes:Shopee:BaseUrl"];
            var partnerId = _config["Integracoes:Shopee:PartnerId"];
            var shopId = _config["Integracoes:Shopee:ShopId"];

            // Stub: Shopee exige assinatura HMAC específica
            _logger.LogInformation("[STUB] Publicar produto {ProdutoId} na Shopee", produtoId);

            var sync = new MarketplaceProduto
            {
                ProdutoId = produtoId,
                Canal = "shopee",
                IdExterno = $"SHP{produtoId}",
                Status = "active",
                PrecoExterno = preco ?? produto.Preco,
                EstoqueExterno = produto.Estoque,
                SyncEm = DateTime.UtcNow
            };
            _context.MarketplaceProdutos.Add(sync);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PublicarAmazonAsync(int produtoId, string asin, string skuAmazon)
        {
            var produto = await _context.Produtos.FindAsync(produtoId);
            if (produto == null) return false;

            // Stub: Amazon SP-API exige certificados e assinatura AWS4
            _logger.LogInformation("[STUB] Publicar produto {ProdutoId} na Amazon", produtoId);

            var sync = new MarketplaceProduto
            {
                ProdutoId = produtoId,
                Canal = "amazon",
                IdExterno = asin ?? $"AMZ{produtoId}",
                Status = "active",
                PrecoExterno = produto.Preco,
                EstoqueExterno = produto.Estoque,
                SyncEm = DateTime.UtcNow
            };
            _context.MarketplaceProdutos.Add(sync);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AtualizarEstoqueMultiCanalAsync(int produtoId, int novoEstoque)
        {
            var syncs = await _context.MarketplaceProdutos
                .Where(m => m.ProdutoId == produtoId)
                .ToListAsync();

            foreach (var sync in syncs)
            {
                try
                {
                    switch (sync.Canal.ToLower())
                    {
                        case "mercadolivre":
                            await _ml.AtualizarPrecoEstoqueAsync(sync.IdExterno, sync.PrecoExterno, novoEstoque);
                            break;
                        case "shopee":
                            await AtualizarShopeeAsync(sync.IdExterno, sync.PrecoExterno, novoEstoque);
                            break;
                        case "amazon":
                            await AtualizarAmazonAsync(sync.IdExterno, novoEstoque);
                            break;
                    }
                    sync.EstoqueExterno = novoEstoque;
                    sync.SyncEm = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro atualizar estoque {Canal} produto {ProdutoId}", sync.Canal, produtoId);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task AtualizarShopeeAsync(string itemId, decimal preco, int estoque)
        {
            _logger.LogInformation("[STUB] Atualizar Shopee item {ItemId}: preco {Preco}, estoque {Estoque}", itemId, preco, estoque);
            await Task.CompletedTask;
        }

        private async Task AtualizarAmazonAsync(string asin, int estoque)
        {
            _logger.LogInformation("[STUB] Atualizar Amazon ASIN {Asin}: estoque {Estoque}", asin, estoque);
            await Task.CompletedTask;
        }

        private async Task<SyncStatusDto> ObterStatusSync(int produtoId, string canal)
        {
            var syncs = await _context.MarketplaceProdutos
                .Where(m => m.ProdutoId == produtoId && m.Canal == canal)
                .ToListAsync();

            var produto = await _context.Produtos.FindAsync(produtoId);
            return new SyncStatusDto
            {
                ProdutoId = produtoId,
                ProdutoNome = produto?.Nome,
                Canais = syncs.Select(s => new CanalSyncDto
                {
                    Canal = s.Canal,
                    Status = s.Status,
                    IdExterno = s.IdExterno,
                    Url = s.Url,
                    SyncEm = s.SyncEm
                }).ToList()
            };
        }
    }
}
