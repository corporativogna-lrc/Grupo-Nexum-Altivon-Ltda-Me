using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NexumAltivon.API.Data;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Models;

namespace NexumAltivon.API.Services
{
    public interface IMarketplaceSyncService
    {
        Task SincronizarEstoqueAutomaticoAsync(int produtoId, int novoEstoque);
        Task SincronizarPrecoAutomaticoAsync(int produtoId, decimal novoPreco);
        Task ExecutarSyncAgendadoAsync();
        Task<List<SyncLogDto>> ObterLogsSyncAsync(DateTime inicio, DateTime fim);
    }

    public class MarketplaceSyncService : IMarketplaceSyncService
    {
        private readonly NexumDbContext _context;
        private readonly IMarketplaceHubService _hub;
        private readonly IMercadoLivreService _ml;
        private readonly ILogger<MarketplaceSyncService> _logger;

        public MarketplaceSyncService(
            NexumDbContext context,
            IMarketplaceHubService hub,
            IMercadoLivreService ml,
            ILogger<MarketplaceSyncService> logger)
        {
            _context = context;
            _hub = hub;
            _ml = ml;
            _logger = logger;
        }

        public async Task SincronizarEstoqueAutomaticoAsync(int produtoId, int novoEstoque)
        {
            var syncs = await _context.MarketplaceProdutos.Where(m => m.ProdutoId == produtoId).ToListAsync();
            if (!syncs.Any()) return;

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
                        case "amazon":
                            await _hub.AtualizarEstoqueMultiCanalAsync(produtoId, novoEstoque);
                            break;
                    }
                    sync.EstoqueExterno = novoEstoque;
                    sync.SyncEm = DateTime.UtcNow;
                    await RegistrarLog("ESTOQUE", produtoId, sync.Canal, true, null);
                }
                catch (Exception ex)
                {
                    await RegistrarLog("ESTOQUE", produtoId, sync.Canal, false, ex.Message);
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task SincronizarPrecoAutomaticoAsync(int produtoId, decimal novoPreco)
        {
            var syncs = await _context.MarketplaceProdutos.Where(m => m.ProdutoId == produtoId).ToListAsync();
            foreach (var sync in syncs)
            {
                try
                {
                    if (sync.Canal == "mercadolivre")
                        await _ml.AtualizarPrecoEstoqueAsync(sync.IdExterno, novoPreco, sync.EstoqueExterno);
                    sync.PrecoExterno = novoPreco;
                    sync.SyncEm = DateTime.UtcNow;
                    await RegistrarLog("PRECO", produtoId, sync.Canal, true, null);
                }
                catch (Exception ex)
                {
                    await RegistrarLog("PRECO", produtoId, sync.Canal, false, ex.Message);
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task ExecutarSyncAgendadoAsync()
        {
            _logger.LogInformation("Iniciando sync agendado de marketplaces...");
            var produtos = await _context.Produtos.Where(p => p.Ativo).ToListAsync();
            foreach (var produto in produtos)
            {
                await SincronizarEstoqueAutomaticoAsync(produto.ProdutoId, produto.Estoque);
            }
            _logger.LogInformation("Sync agendado concluído.");
        }

        public async Task<List<SyncLogDto>> ObterLogsSyncAsync(DateTime inicio, DateTime fim)
        {
            return await _context.SyncLogs
                .Where(l => l.DataHora >= inicio && l.DataHora <= fim)
                .OrderByDescending(l => l.DataHora)
                .Select(l => new SyncLogDto
                {
                    SyncLogId = l.SyncLogId,
                    Tipo = l.Tipo,
                    ProdutoId = l.ProdutoId,
                    Canal = l.Canal,
                    Sucesso = l.Sucesso,
                    Mensagem = l.Mensagem,
                    DataHora = l.DataHora
                })
                .ToListAsync();
        }

        private async Task RegistrarLog(string tipo, int produtoId, string canal, bool sucesso, string erro)
        {
            _context.SyncLogs.Add(new SyncLog
            {
                Tipo = tipo,
                ProdutoId = produtoId,
                Canal = canal,
                Sucesso = sucesso,
                Mensagem = erro,
                DataHora = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
    }

    public class SyncLogDto
    {
        public int SyncLogId { get; set; }
        public string Tipo { get; set; }
        public int ProdutoId { get; set; }
        public string Canal { get; set; }
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; }
        public DateTime DataHora { get; set; }
    }
}
