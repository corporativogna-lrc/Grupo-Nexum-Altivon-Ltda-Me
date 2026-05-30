using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NexumAltivon.ERP.Data;
using NexumAltivon.ERP.Models;

namespace NexumAltivon.ERP.Services
{
    /// <summary>
    /// Bridge de sincronização bidirecional entre ERP GenesisGest.Net e E-Commerce Nexum Altivon
    /// Mantém produtos, clientes, pedidos e estoque sincronizados entre os sistemas
    /// </summary>
    public interface ISyncErpService
    {
        Task<SincronizacaoResultado> SincronizarProdutosAsync();
        Task<SincronizacaoResultado> SincronizarClientesAsync();
        Task<SincronizacaoResultado> SincronizarPedidosAsync();
        Task<SincronizacaoResultado> SincronizarEstoqueAsync();
        Task<SincronizacaoResultado> ExecutarSyncCompletoAsync();
        Task<SincronizacaoResultado> ExecutarSyncAgendadoAsync();
        Task<IEnumerable<LogSincronizacao>> ObterLogsAsync(DateTime? inicio = null, DateTime? fim = null);
    }

    public class SincronizacaoResultado
    {
        public string Entidade { get; set; } = string.Empty;
        public int Inseridos { get; set; }
        public int Atualizados { get; set; }
        public int Ignorados { get; set; }
        public int Erros { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }
        public TimeSpan Duracao => Fim - Inicio;
        public List<string> Mensagens { get; set; } = new();
        public bool Sucesso => Erros == 0;
    }

    public class LogSincronizacao
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public string Entidade { get; set; } = string.Empty;
        public string Operacao { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Detalhes { get; set; }
        public string? Erro { get; set; }
    }

    public class SyncErpService : ISyncErpService
    {
        private readonly NexumDbContext _context;
        private readonly ILogger<SyncErpService> _logger;
        private readonly IEstoqueService _estoqueService;

        public SyncErpService(
            NexumDbContext context,
            ILogger<SyncErpService> logger,
            IEstoqueService estoqueService)
        {
            _context = context;
            _logger = logger;
            _estoqueService = estoqueService;
        }

        /// <summary>
        /// Sincroniza produtos do ERP para o E-Commerce
        /// Atualiza preços, estoque e status de ativação
        /// </summary>
        public async Task<SincronizacaoResultado> SincronizarProdutosAsync()
        {
            var resultado = new SincronizacaoResultado
            {
                Entidade = "Produtos",
                Inicio = DateTime.Now
            };

            try
            {
                var produtosErp = await _context.Produtos
                    .Where(p => p.Ativo)
                    .ToListAsync();

                foreach (var produto in produtosErp)
                {
                    try
                    {
                        // Verifica se produto existe no e-commerce (simulação)
                        // Em produção: chamar API do e-commerce ou atualizar tabela compartilhada
                        var produtoEcommerce = await _context.Produtos
                            .FirstOrDefaultAsync(p => p.Id == produto.Id);

                        if (produtoEcommerce != null)
                        {
                            // Atualiza preço e estoque
                            produtoEcommerce.Preco = produto.Preco;
                            produtoEcommerce.PrecoPromocional = produto.PrecoPromocional;
                            produtoEcommerce.EstoqueAtual = produto.EstoqueAtual;
                            produtoEcommerce.UltimaAtualizacao = DateTime.Now;
                            resultado.Atualizados++;
                        }
                        else
                        {
                            // Produto novo — em produção, replicar para e-commerce
                            resultado.Inseridos++;
                        }

                        _logger.LogInformation("Produto {Sku} sincronizado", produto.Sku);
                    }
                    catch (Exception ex)
                    {
                        resultado.Erros++;
                        resultado.Mensagens.Add($"Erro no produto {produto.Sku}: {ex.Message}");
                        _logger.LogError(ex, "Erro sincronizando produto {Sku}", produto.Sku);
                    }
                }

                await _context.SaveChangesAsync();
                resultado.Mensagens.Add($"{produtosErp.Count} produtos processados");
            }
            catch (Exception ex)
            {
                resultado.Erros++;
                resultado.Mensagens.Add($"Erro geral: {ex.Message}");
                _logger.LogError(ex, "Erro na sincronização de produtos");
            }

            resultado.Fim = DateTime.Now;
            await RegistrarLogAsync(resultado);
            return resultado;
        }

        /// <summary>
        /// Sincroniza clientes do E-Commerce para o ERP (CRM)
        /// Converte clientes em leads ou atualiza cadastro existente
        /// </summary>
        public async Task<SincronizacaoResultado> SincronizarClientesAsync()
        {
            var resultado = new SincronizacaoResultado
            {
                Entidade = "Clientes",
                Inicio = DateTime.Now
            };

            try
            {
                var clientesNovos = await _context.Clientes
                    .Where(c => c.CriadoEm >= DateTime.Now.AddDays(-1))
                    .ToListAsync();

                foreach (var cliente in clientesNovos)
                {
                    try
                    {
                        // Verifica se já existe como lead
                        var leadExistente = await _context.LeadsCRM
                            .FirstOrDefaultAsync(l => l.Email == cliente.Email || l.Cpf == cliente.Cpf);

                        if (leadExistente == null)
                        {
                            // Cria lead automaticamente
                            var lead = new LeadCRM
                            {
                                Nome = cliente.Nome,
                                Email = cliente.Email,
                                Telefone = cliente.Telefone,
                                WhatsApp = cliente.Celular,
                                Origem = "E-Commerce",
                                Status = "Convertido",
                                Tipo = "Cliente",
                                ClienteConvertidoId = cliente.Id,
                                DataConversao = cliente.CriadoEm,
                                CriadoEm = DateTime.Now,
                                CriadoPor = "SYNC_AUTO"
                            };

                            _context.LeadsCRM.Add(lead);
                            resultado.Inseridos++;
                        }
                        else
                        {
                            leadExistente.Status = "Convertido";
                            leadExistente.ClienteConvertidoId = cliente.Id;
                            leadExistente.DataConversao = cliente.CriadoEm;
                            leadExistente.AtualizadoEm = DateTime.Now;
                            resultado.Atualizados++;
                        }

                        _logger.LogInformation("Cliente {Nome} sincronizado para CRM", cliente.Nome);
                    }
                    catch (Exception ex)
                    {
                        resultado.Erros++;
                        resultado.Mensagens.Add($"Erro no cliente {cliente.Nome}: {ex.Message}");
                        _logger.LogError(ex, "Erro sincronizando cliente {Nome}", cliente.Nome);
                    }
                }

                await _context.SaveChangesAsync();
                resultado.Mensagens.Add($"{clientesNovos.Count} clientes processados");
            }
            catch (Exception ex)
            {
                resultado.Erros++;
                resultado.Mensagens.Add($"Erro geral: {ex.Message}");
                _logger.LogError(ex, "Erro na sincronização de clientes");
            }

            resultado.Fim = DateTime.Now;
            await RegistrarLogAsync(resultado);
            return resultado;
        }

        /// <summary>
        /// Sincroniza pedidos do E-Commerce para o ERP
        /// Gera contas a receber e movimenta estoque
        /// </summary>
        public async Task<SincronizacaoResultado> SincronizarPedidosAsync()
        {
            var resultado = new SincronizacaoResultado
            {
                Entidade = "Pedidos",
                Inicio = DateTime.Now
            };

            try
            {
                var pedidosNovos = await _context.Pedidos
                    .Where(p => p.Status == "Pago" && p.CriadoEm >= DateTime.Now.AddDays(-1))
                    .Include(p => p.Itens)
                    .ToListAsync();

                foreach (var pedido in pedidosNovos)
                {
                    try
                    {
                        // Verifica se já gerou conta a receber
                        var contaExistente = await _context.ContasReceber
                            .AnyAsync(c => c.NumeroPedidoReferencia == pedido.Numero);

                        if (!contaExistente)
                        {
                            // Gera conta a receber
                            var conta = new ContaReceber
                            {
                                NumeroDocumento = $"PED-{pedido.Numero}",
                                ClienteId = pedido.ClienteId,
                                Descricao = $"Pedido {pedido.Numero} — {pedido.Itens?.Count ?? 0} itens",
                                ValorOriginal = pedido.ValorTotal,
                                DataEmissao = DateTime.Now,
                                DataVencimento = DateTime.Now.AddDays(1), // PIX = D+1
                                Status = "Pendente",
                                NumeroPedidoReferencia = pedido.Numero,
                                LojaId = pedido.LojaId,
                                CentroCustoId = 9, // 3.01 Vendas Online
                                CriadoEm = DateTime.Now,
                                CriadoPor = "SYNC_AUTO"
                            };

                            _context.ContasReceber.Add(conta);
                            resultado.Inseridos++;
                        }

                        // Movimenta estoque
                        if (pedido.Itens != null)
                        {
                            foreach (var item in pedido.Itens)
                            {
                                await _estoqueService.RegistrarSaidaAsync(
                                    item.ProdutoId,
                                    item.Quantidade,
                                    pedido.Id,
                                    pedido.Numero,
                                    "Venda E-Commerce",
                                    pedido.LojaId,
                                    "SYNC_AUTO"
                                );
                            }
                        }

                        _logger.LogInformation("Pedido {Numero} sincronizado", pedido.Numero);
                    }
                    catch (Exception ex)
                    {
                        resultado.Erros++;
                        resultado.Mensagens.Add($"Erro no pedido {pedido.Numero}: {ex.Message}");
                        _logger.LogError(ex, "Erro sincronizando pedido {Numero}", pedido.Numero);
                    }
                }

                await _context.SaveChangesAsync();
                resultado.Mensagens.Add($"{pedidosNovos.Count} pedidos processados");
            }
            catch (Exception ex)
            {
                resultado.Erros++;
                resultado.Mensagens.Add($"Erro geral: {ex.Message}");
                _logger.LogError(ex, "Erro na sincronização de pedidos");
            }

            resultado.Fim = DateTime.Now;
            await RegistrarLogAsync(resultado);
            return resultado;
        }

        /// <summary>
        /// Sincroniza estoque entre todas as lojas e o centro de distribuição
        /// </summary>
        public async Task<SincronizacaoResultado> SincronizarEstoqueAsync()
        {
            var resultado = new SincronizacaoResultado
            {
                Entidade = "Estoque",
                Inicio = DateTime.Now
            };

            try
            {
                var produtos = await _context.Produtos
                    .Where(p => p.Ativo)
                    .ToListAsync();

                foreach (var produto in produtos)
                {
                    try
                    {
                        // Recalcula estoque baseado em movimentações
                        var entradas = await _context.MovimentacoesEstoque
                            .Where(m => m.ProdutoId == produto.Id && m.Tipo == "Entrada")
                            .SumAsync(m => m.Quantidade);

                        var saidas = await _context.MovimentacoesEstoque
                            .Where(m => m.ProdutoId == produto.Id && m.Tipo == "Saida")
                            .SumAsync(m => m.Quantidade);

                        var estoqueCalculado = entradas - saidas;

                        if (produto.EstoqueAtual != estoqueCalculado)
                        {
                            produto.EstoqueAtual = estoqueCalculado;
                            produto.UltimaAtualizacao = DateTime.Now;
                            resultado.Atualizados++;
                        }
                        else
                        {
                            resultado.Ignorados++;
                        }

                        _logger.LogInformation("Estoque do produto {Sku} sincronizado: {Estoque}",
                            produto.Sku, estoqueCalculado);
                    }
                    catch (Exception ex)
                    {
                        resultado.Erros++;
                        resultado.Mensagens.Add($"Erro no produto {produto.Sku}: {ex.Message}");
                        _logger.LogError(ex, "Erro sincronizando estoque {Sku}", produto.Sku);
                    }
                }

                await _context.SaveChangesAsync();
                resultado.Mensagens.Add($"{produtos.Count} produtos processados");
            }
            catch (Exception ex)
            {
                resultado.Erros++;
                resultado.Mensagens.Add($"Erro geral: {ex.Message}");
                _logger.LogError(ex, "Erro na sincronização de estoque");
            }

            resultado.Fim = DateTime.Now;
            await RegistrarLogAsync(resultado);
            return resultado;
        }

        /// <summary>
        /// Executa sincronização completa de todas as entidades
        /// </summary>
        public async Task<SincronizacaoResultado> ExecutarSyncCompletoAsync()
        {
            var resultadoGeral = new SincronizacaoResultado
            {
                Entidade = "Sync Completo",
                Inicio = DateTime.Now
            };

            var produtos = await SincronizarProdutosAsync();
            var clientes = await SincronizarClientesAsync();
            var pedidos = await SincronizarPedidosAsync();
            var estoque = await SincronizarEstoqueAsync();

            resultadoGeral.Inseridos = produtos.Inseridos + clientes.Inseridos + pedidos.Inseridos + estoque.Inseridos;
            resultadoGeral.Atualizados = produtos.Atualizados + clientes.Atualizados + pedidos.Atualizados + estoque.Atualizados;
            resultadoGeral.Erros = produtos.Erros + clientes.Erros + pedidos.Erros + estoque.Erros;
            resultadoGeral.Mensagens.AddRange(produtos.Mensagens);
            resultadoGeral.Mensagens.AddRange(clientes.Mensagens);
            resultadoGeral.Mensagens.AddRange(pedidos.Mensagens);
            resultadoGeral.Mensagens.AddRange(estoque.Mensagens);
            resultadoGeral.Fim = DateTime.Now;

            _logger.LogInformation(
                "Sincronização completa finalizada. Inseridos: {Inseridos}, Atualizados: {Atualizados}, Erros: {Erros}",
                resultadoGeral.Inseridos, resultadoGeral.Atualizados, resultadoGeral.Erros);

            return resultadoGeral;
        }

        /// <summary>
        /// Executa sincronização agendada (para uso com Hangfire/Quartz)
        /// </summary>
        public async Task<SincronizacaoResultado> ExecutarSyncAgendadoAsync()
        {
            _logger.LogInformation("Iniciando sincronização agendada em {Data}", DateTime.Now);
            return await ExecutarSyncCompletoAsync();
        }

        public async Task<IEnumerable<LogSincronizacao>> ObterLogsAsync(DateTime? inicio = null, DateTime? fim = null)
        {
            // Em produção: implementar tabela de logs persistente
            return new List<LogSincronizacao>();
        }

        private async Task RegistrarLogAsync(SincronizacaoResultado resultado)
        {
            // Em produção: persistir em tabela de logs
            _logger.LogInformation(
                "[{Entidade}] Sync finalizado em {Duracao}. Inseridos: {Inseridos}, Atualizados: {Atualizados}, Erros: {Erros}",
                resultado.Entidade, resultado.Duracao, resultado.Inseridos, resultado.Atualizados, resultado.Erros);
        }
    }
}
