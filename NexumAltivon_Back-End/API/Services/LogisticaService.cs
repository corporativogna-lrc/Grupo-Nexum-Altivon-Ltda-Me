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
    public interface ILogisticaService
    {
        Task<EtiquetaDto> GerarEtiquetaAsync(int pedidoId, int? transportadoraId, string servicoFrete);
        Task<RastreamentoDto> RastrearAsync(string codigoRastreio);
        Task<bool> AtualizarStatusEnvioAsync(int pedidoId, string status, string codigoRastreio, DateTime? dataEnvio, DateTime? dataEntrega);
        Task<DashboardLogisticaDto> ObterDashboardAsync();
        Task<List<TransportadoraDto>> ListarTransportadorasAsync();
        Task<bool> ImprimirEtiquetaAsync(int etiquetaId);
    }

    public class LogisticaService : ILogisticaService
    {
        private readonly NexumDbContext _context;
        private readonly IFreteService _frete;
        private readonly INotificacaoService _notificacao;
        private readonly ILogAuditoriaService _auditoria;
        private readonly ILogger<LogisticaService> _logger;

        public LogisticaService(
            NexumDbContext context,
            IFreteService frete,
            INotificacaoService notificacao,
            ILogAuditoriaService auditoria,
            ILogger<LogisticaService> logger)
        {
            _context = context;
            _frete = frete;
            _notificacao = notificacao;
            _auditoria = auditoria;
            _logger = logger;
        }

        public async Task<EtiquetaDto> GerarEtiquetaAsync(int pedidoId, int? transportadoraId, string servicoFrete)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .FirstOrDefaultAsync(p => p.PedidoId == pedidoId);

            if (pedido == null) throw new ArgumentException("Pedido não encontrado.");
            if (pedido.Status != "PAGO" && pedido.Status != "SEPARACAO")
                throw new InvalidOperationException("Pedido não está pronto para envio.");

            var transportadora = transportadoraId.HasValue
                ? await _context.Transportadoras.FindAsync(transportadoraId.Value)
                : await _context.Transportadoras.FirstOrDefaultAsync(t => t.Ativa);

            if (transportadora == null) throw new InvalidOperationException("Nenhuma transportadora disponível.");

            // Gerar código de rastreio fictício (em produção, virá da transportadora)
            var codigoRastreio = $"NX{DateTime.UtcNow:yyMMdd}{pedidoId:D5}{new Random().Next(100, 999)}";

            var etiqueta = new EtiquetaEnvio
            {
                PedidoId = pedidoId,
                TransportadoraId = transportadora.TransportadoraId,
                CodigoRastreio = codigoRastreio,
                UrlEtiquetaPdf = $"https://api.nexumaltivon.com/etiquetas/{pedidoId}.pdf",
                UrlEtiquetaZpl = $"https://api.nexumaltivon.com/etiquetas/{pedidoId}.zpl",
                Status = "GERADA",
                GeradaEm = DateTime.UtcNow
            };

            _context.Etiquetas.Add(etiqueta);

            pedido.Status = "SEPARACAO";
            pedido.CodigoRastreio = codigoRastreio;
            pedido.Transportadora = transportadora.Nome;

            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync("LOGISTICA", $"Etiqueta gerada pedido {pedidoId}, transportadora {transportadora.Nome}", null);

            return new EtiquetaDto
            {
                EtiquetaId = etiqueta.EtiquetaId,
                PedidoId = pedidoId,
                NumeroPedido = pedido.NumeroPedido,
                Transportadora = transportadora.Nome,
                CodigoRastreio = codigoRastreio,
                UrlEtiquetaPdf = etiqueta.UrlEtiquetaPdf,
                UrlEtiquetaZpl = etiqueta.UrlEtiquetaZpl,
                Status = "GERADA",
                GeradaEm = DateTime.UtcNow
            };
        }

        public async Task<RastreamentoDto> RastrearAsync(string codigoRastreio)
        {
            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(p => p.CodigoRastreio == codigoRastreio);

            if (pedido == null) return null;

            // Simulação de eventos (em produção, consultar API da transportadora)
            var eventos = new List<EventoRastreamentoDto>();
            var random = new Random();

            if (pedido.EnviadoEm.HasValue)
            {
                eventos.Add(new EventoRastreamentoDto
                {
                    DataHora = pedido.EnviadoEm.Value,
                    Status = "POSTADO",
                    Local = "Centro de Distribuição Nexum",
                    Descricao = "Objeto postado"
                });

                if (pedido.EntregueEm.HasValue)
                {
                    var meio = pedido.EnviadoEm.Value.AddHours(random.Next(12, 48));
                    eventos.Add(new EventoRastreamentoDto
                    {
                        DataHora = meio,
                        Status = "EM_TRANSITO",
                        Local = "Unidade de Tratamento",
                        Descricao = "Objeto em trânsito"
                    });

                    eventos.Add(new EventoRastreamentoDto
                    {
                        DataHora = pedido.EntregueEm.Value.AddHours(-2),
                        Status = "SAIU_ENTREGA",
                        Local = pedido.CidadeDestino ?? "Local de entrega",
                        Descricao = "Saiu para entrega ao destinatário"
                    });

                    eventos.Add(new EventoRastreamentoDto
                    {
                        DataHora = pedido.EntregueEm.Value,
                        Status = "ENTREGUE",
                        Local = pedido.CidadeDestino ?? "Local de entrega",
                        Descricao = "Objeto entregue ao destinatário"
                    });
                }
            }

            return new RastreamentoDto
            {
                CodigoRastreio = codigoRastreio,
                Transportadora = pedido.Transportadora,
                StatusAtual = pedido.Status,
                DescricaoStatus = ObterDescricaoStatus(pedido.Status),
                PrevisaoEntrega = pedido.EnviadoEm?.AddDays(5),
                Eventos = eventos.OrderByDescending(e => e.DataHora).ToList()
            };
        }

        public async Task<bool> AtualizarStatusEnvioAsync(int pedidoId, string status, string codigoRastreio, DateTime? dataEnvio, DateTime? dataEntrega)
        {
            var pedido = await _context.Pedidos.FindAsync(pedidoId);
            if (pedido == null) return false;

            pedido.Status = status;
            if (!string.IsNullOrEmpty(codigoRastreio)) pedido.CodigoRastreio = codigoRastreio;
            if (dataEnvio.HasValue) pedido.EnviadoEm = dataEnvio.Value;
            if (dataEntrega.HasValue) pedido.EntregueEm = dataEntrega.Value;

            await _context.SaveChangesAsync();

            var cliente = await _context.Clientes.FindAsync(pedido.ClienteId);
            if (cliente != null)
            {
                var msg = status switch
                {
                    "SEPARACAO" => "Seu pedido está em separação no nosso centro de distribuição.",
                    "ENVIADO" => $"Seu pedido foi enviado! Código de rastreio: {pedido.CodigoRastreio}",
                    "EM_TRANSITO" => "Seu pedido está a caminho da sua cidade.",
                    "ENTREGUE" => "Seu pedido foi entregue! Agradecemos a preferência.",
                    _ => $"Status atualizado: {status}"
                };
                await _notificacao.EnviarStatusPedidoAsync(cliente, pedido, msg);
            }

            await _auditoria.RegistrarAsync("LOGISTICA", $"Status envio pedido {pedidoId}: {status}", null);
            return true;
        }

        public async Task<DashboardLogisticaDto> ObterDashboardAsync()
        {
            var hoje = DateTime.UtcNow.Date;
            var pedidosHoje = await _context.Pedidos.Where(p => p.CriadoEm.Date == hoje).CountAsync();
            var separacao = await _context.Pedidos.Where(p => p.Status == "SEPARACAO").CountAsync();
            var transito = await _context.Pedidos.Where(p => p.Status == "ENVIADO" || p.Status == "EM_TRANSITO").CountAsync();
            var entreguesHoje = await _context.Pedidos.Where(p => p.EntregueEm.HasValue && p.EntregueEm.Value.Date == hoje).CountAsync();
            var atrasados = await _context.Pedidos
                .Where(p => p.Status == "ENVIADO" && p.EnviadoEm.HasValue && p.EnviadoEm.Value.AddDays(10) < DateTime.UtcNow)
                .CountAsync();

            var pendentes = await _context.Pedidos
                .Where(p => p.Status == "PAGO" || p.Status == "SEPARACAO")
                .OrderBy(p => p.CriadoEm)
                .Take(20)
                .Select(p => new PedidoLogisticaDto
                {
                    PedidoId = p.PedidoId,
                    NumeroPedido = p.NumeroPedido,
                    ClienteNome = p.Cliente.Nome,
                    Status = p.Status,
                    Transportadora = p.Transportadora,
                    CodigoRastreio = p.CodigoRastreio,
                    PrevisaoEntrega = p.EnviadoEm?.AddDays(5)
                })
                .ToListAsync();

            return new DashboardLogisticaDto
            {
                TotalPedidosHoje = pedidosHoje,
                PedidosSeparacao = separacao,
                PedidosTransito = transito,
                PedidosEntreguesHoje = entreguesHoje,
                PedidosAtrasados = atrasados,
                PedidosPendentes = pendentes
            };
        }

        public async Task<List<TransportadoraDto>> ListarTransportadorasAsync()
        {
            return await _context.Transportadoras
                .Select(t => new TransportadoraDto
                {
                    TransportadoraId = t.TransportadoraId,
                    Nome = t.Nome,
                    CodigoApi = t.CodigoApi,
                    Ativa = t.Ativa,
                    TaxaAdicional = t.TaxaAdicional
                })
                .ToListAsync();
        }

        public async Task<bool> ImprimirEtiquetaAsync(int etiquetaId)
        {
            var etiqueta = await _context.Etiquetas.FindAsync(etiquetaId);
            if (etiqueta == null) return false;
            etiqueta.Status = "IMPRESSA";
            await _context.SaveChangesAsync();
            return true;
        }

        private string ObterDescricaoStatus(string status)
        {
            return status switch
            {
                "PAGO" => "Pagamento confirmado, aguardando separação",
                "SEPARACAO" => "Em separação no centro de distribuição",
                "ENVIADO" => "Objeto enviado",
                "EM_TRANSITO" => "Em trânsito para cidade de destino",
                "ENTREGUE" => "Entregue ao destinatário",
                "DEVOLVIDO" => "Devolvido ao remetente",
                _ => status
            };
        }
    }
}
