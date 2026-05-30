using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
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
    public interface IErpSyncService
    {
        Task<ErpSyncResultDto> SincronizarProdutosAsync(DateTime? inicio, DateTime? fim, bool forcaCompleta);
        Task<ErpSyncResultDto> SincronizarClientesAsync(DateTime? inicio, DateTime? fim, bool forcaCompleta);
        Task<ErpSyncResultDto> SincronizarPedidosAsync(DateTime? inicio, DateTime? fim, bool forcaCompleta);
        Task<ErpSyncResultDto> SincronizarEstoqueAsync();
        Task<ErpStatusConexaoDto> TestarConexaoAsync();
        Task<ErpConfiguracaoDto> ObterConfiguracaoAsync();
        Task<bool> SalvarConfiguracaoAsync(ErpConfiguracaoDto config);
    }

    public class ErpSyncService : IErpSyncService
    {
        private readonly HttpClient _httpClient;
        private readonly NexumDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<ErpSyncService> _logger;
        private readonly string _erpBaseUrl;
        private readonly string _erpToken;

        public ErpSyncService(IHttpClientFactory factory, NexumDbContext context, IConfiguration config, ILogger<ErpSyncService> logger)
        {
            _httpClient = factory.CreateClient("GenesisGest");
            _context = context;
            _config = config;
            _logger = logger;
            _erpBaseUrl = _config["Integracoes:GenesisGest:UrlBase"]?.TrimEnd('/');
            _erpToken = _config["Integracoes:GenesisGest:TokenApi"];
            if (!string.IsNullOrEmpty(_erpToken))
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _erpToken);
        }

        public async Task<ErpSyncResultDto> SincronizarProdutosAsync(DateTime? inicio, DateTime? fim, bool forcaCompleta)
        {
            try
            {
                var query = _context.Produtos.AsQueryable();
                if (!forcaCompleta && inicio.HasValue)
                    query = query.Where(p => p.AtualizadoEm >= inicio.Value);

                var produtos = await query.Take(500).ToListAsync();
                var erpProdutos = produtos.Select(p => new ErpProdutoDto
                {
                    ProdutoId = p.ProdutoId,
                    Nome = p.Nome,
                    Sku = p.Sku,
                    CodigoErp = p.CodigoErp,
                    PrecoCusto = p.PrecoCusto,
                    PrecoVenda = p.Preco,
                    Estoque = p.Estoque,
                    Unidade = p.Unidade ?? "UN",
                    Ncm = p.Ncm,
                    Cest = p.Cest,
                    PesoKg = p.PesoKg,
                    Ativo = p.Ativo
                }).ToList();

                // Enviar para ERP
                var response = await _httpClient.PostAsJsonAsync($"{_erpBaseUrl}/api/produtos/sync", erpProdutos);
                var enviados = erpProdutos.Count;
                var recebidos = 0;
                var erros = 0;

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Sync produtos ERP: {Content}", content);
                    // Atualizar flag de sync
                    foreach (var p in produtos)
                    {
                        p.SincronizadoErpEm = DateTime.UtcNow;
                    }
                    await _context.SaveChangesAsync();
                }
                else
                {
                    erros = enviados;
                    _logger.LogError("Falha sync produtos ERP: {Status}", response.StatusCode);
                }

                return new ErpSyncResultDto
                {
                    Sucesso = response.IsSuccessStatusCode,
                    Entidade = "PRODUTOS",
                    RegistrosEnviados = enviados,
                    RegistrosRecebidos = recebidos,
                    RegistrosComErro = erros,
                    Mensagem = response.IsSuccessStatusCode ? "Sincronizado com sucesso" : $"Erro: {response.StatusCode}",
                    ExecutadoEm = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro sync produtos ERP");
                return new ErpSyncResultDto { Sucesso = false, Entidade = "PRODUTOS", Mensagem = ex.Message, ExecutadoEm = DateTime.UtcNow };
            }
        }

        public async Task<ErpSyncResultDto> SincronizarClientesAsync(DateTime? inicio, DateTime? fim, bool forcaCompleta)
        {
            try
            {
                var query = _context.Clientes.AsQueryable();
                if (!forcaCompleta && inicio.HasValue)
                    query = query.Where(c => c.CriadoEm >= inicio.Value);

                var clientes = await query.Take(500).ToListAsync();
                var erpClientes = clientes.Select(c => new ErpClienteDto
                {
                    ClienteId = c.ClienteId,
                    Nome = c.Nome,
                    CpfCnpj = c.Cpf,
                    Email = c.Email,
                    Telefone = c.Telefone,
                    CodigoErp = c.CodigoErp
                }).ToList();

                var response = await _httpClient.PostAsJsonAsync($"{_erpBaseUrl}/api/clientes/sync", erpClientes);
                if (response.IsSuccessStatusCode)
                {
                    foreach (var c in clientes) c.SincronizadoErpEm = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return new ErpSyncResultDto
                {
                    Sucesso = response.IsSuccessStatusCode,
                    Entidade = "CLIENTES",
                    RegistrosEnviados = erpClientes.Count,
                    RegistrosComErro = response.IsSuccessStatusCode ? 0 : erpClientes.Count,
                    Mensagem = response.IsSuccessStatusCode ? "OK" : $"Erro: {response.StatusCode}",
                    ExecutadoEm = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new ErpSyncResultDto { Sucesso = false, Entidade = "CLIENTES", Mensagem = ex.Message, ExecutadoEm = DateTime.UtcNow };
            }
        }

        public async Task<ErpSyncResultDto> SincronizarPedidosAsync(DateTime? inicio, DateTime? fim, bool forcaCompleta)
        {
            try
            {
                var query = _context.Pedidos
                    .Include(p => p.Itens)
                    .Include(p => p.Cliente)
                    .AsQueryable();

                if (!forcaCompleta && inicio.HasValue)
                    query = query.Where(p => p.CriadoEm >= inicio.Value);

                var pedidos = await query.Where(p => p.Status == "PAGO" || p.Status == "ENTREGUE").Take(200).ToListAsync();
                var erpPedidos = pedidos.Select(p => new ErpPedidoDto
                {
                    NumeroPedido = p.NumeroPedido,
                    CodigoErp = p.CodigoErp,
                    ClienteId = p.ClienteId,
                    ClienteNome = p.Cliente?.Nome,
                    CpfCnpj = p.Cliente?.Cpf,
                    DataEmissao = p.CriadoEm,
                    Total = p.Total,
                    Status = p.Status,
                    Itens = p.Itens.Select(i => new ErpPedidoItemDto
                    {
                        Sku = i.Sku,
                        Descricao = i.ProdutoNome,
                        Quantidade = i.Quantidade,
                        PrecoUnitario = i.PrecoUnitario,
                        Subtotal = i.Subtotal,
                        Ncm = i.Ncm
                    }).ToList()
                }).ToList();

                var response = await _httpClient.PostAsJsonAsync($"{_erpBaseUrl}/api/pedidos/sync", erpPedidos);
                if (response.IsSuccessStatusCode)
                {
                    foreach (var p in pedidos) p.SincronizadoErpEm = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return new ErpSyncResultDto
                {
                    Sucesso = response.IsSuccessStatusCode,
                    Entidade = "PEDIDOS",
                    RegistrosEnviados = erpPedidos.Count,
                    RegistrosComErro = response.IsSuccessStatusCode ? 0 : erpPedidos.Count,
                    Mensagem = response.IsSuccessStatusCode ? "OK" : $"Erro: {response.StatusCode}",
                    ExecutadoEm = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new ErpSyncResultDto { Sucesso = false, Entidade = "PEDIDOS", Mensagem = ex.Message, ExecutadoEm = DateTime.UtcNow };
            }
        }

        public async Task<ErpSyncResultDto> SincronizarEstoqueAsync()
        {
            try
            {
                var produtos = await _context.Produtos.Where(p => p.Ativo).ToListAsync();
                var estoqueData = produtos.Select(p => new { p.CodigoErp, p.Sku, p.Estoque, p.EstoqueMinimo }).ToList();

                var response = await _httpClient.PostAsJsonAsync($"{_erpBaseUrl}/api/estoque/sync", estoqueData);
                return new ErpSyncResultDto
                {
                    Sucesso = response.IsSuccessStatusCode,
                    Entidade = "ESTOQUE",
                    RegistrosEnviados = estoqueData.Count,
                    Mensagem = response.IsSuccessStatusCode ? "Estoque sincronizado" : $"Erro: {response.StatusCode}",
                    ExecutadoEm = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new ErpSyncResultDto { Sucesso = false, Entidade = "ESTOQUE", Mensagem = ex.Message, ExecutadoEm = DateTime.UtcNow };
            }
        }

        public async Task<ErpStatusConexaoDto> TestarConexaoAsync()
        {
            try
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var response = await _httpClient.GetAsync($"{_erpBaseUrl}/api/health");
                sw.Stop();

                var content = response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : null;
                var versao = content != null && JsonDocument.Parse(content).RootElement.TryGetProperty("versao", out var v)
                    ? v.GetString() : "desconhecida";

                return new ErpStatusConexaoDto
                {
                    Conectado = response.IsSuccessStatusCode,
                    VersaoErp = versao,
                    Mensagem = response.IsSuccessStatusCode ? "Conectado" : $"Erro: {response.StatusCode}",
                    LatenciaMs = (int)sw.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                return new ErpStatusConexaoDto { Conectado = false, Mensagem = ex.Message };
            }
        }

        public async Task<ErpConfiguracaoDto> ObterConfiguracaoAsync()
        {
            return new ErpConfiguracaoDto
            {
                UrlBase = _erpBaseUrl,
                TokenApi = _erpToken?.Substring(0, Math.Min(10, _erpToken?.Length ?? 0)) + "...",
                IntervaloSyncMinutos = int.Parse(_config["Integracoes:GenesisGest:IntervaloMinutos"] ?? "60"),
                SyncAutomatico = bool.Parse(_config["Integracoes:GenesisGest:AutoSync"] ?? "true"),
                EntidadesAtivas = _config.GetSection("Integracoes:GenesisGest:Entidades").Get<List<string>>() ?? new List<string>()
            };
        }

        public async Task<bool> SalvarConfiguracaoAsync(ErpConfiguracaoDto config)
        {
            // Em produção: salvar no banco de configurações criptografadas
            _logger.LogInformation("Configuração ERP atualizada: {Url}", config.UrlBase);
            await Task.CompletedTask;
            return true;
        }
    }
}
