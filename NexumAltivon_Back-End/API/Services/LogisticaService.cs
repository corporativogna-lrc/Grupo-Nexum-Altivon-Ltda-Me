/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NexumAltivon.API.Data;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Models;

namespace NexumAltivon.API.Services;

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
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly INotificacaoService _notificacao;
    private readonly ILogAuditoriaService _auditoria;
    private readonly ILogger<LogisticaService> _logger;

    public LogisticaService(
        NexumDbContext context,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        INotificacaoService notificacao,
        ILogAuditoriaService auditoria,
        ILogger<LogisticaService> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _notificacao = notificacao;
        _auditoria = auditoria;
        _logger = logger;
    }

    public async Task<EtiquetaDto> GerarEtiquetaAsync(int pedidoId, int? transportadoraId, string servicoFrete)
    {
        var pedido = await _context.Pedidos
            .Include(item => item.Cliente)
            .FirstOrDefaultAsync(item => item.Id == pedidoId);

        if (pedido is null)
        {
            throw new ArgumentException("Pedido nao encontrado.", nameof(pedidoId));
        }

        if (pedido.Status is not (StatusPedido.Pago or StatusPedido.EmSeparacao or StatusPedido.Enviado))
        {
            throw new InvalidOperationException("Pedido nao esta pronto para emissao real de etiqueta.");
        }

        var transportadora = transportadoraId.HasValue
            ? await _context.Transportadoras.FirstOrDefaultAsync(item => item.Id == transportadoraId.Value && item.Ativa)
            : await _context.Transportadoras.OrderBy(item => item.Id).FirstOrDefaultAsync(item => item.Ativa);

        if (transportadora is null)
        {
            throw new InvalidOperationException("Nenhuma transportadora ativa cadastrada para emissao de etiqueta.");
        }

        var endpointTemplate = FirstConfigured(
            transportadora.ApiEndpoint,
            _configuration["Logistica:EtiquetaEndpointTemplate"],
            _configuration["Integracoes:Logistica:EtiquetaEndpointTemplate"]);
        var token = FirstConfigured(
            transportadora.ApiToken,
            _configuration["Logistica:EtiquetaToken"],
            _configuration["Integracoes:Logistica:EtiquetaToken"],
            _configuration["MelhorEnvio:Token"],
            _configuration["Integracoes:MelhorEnvio:Token"]);

        if (!IsConfigured(endpointTemplate) || !IsConfigured(token))
        {
            throw new InvalidOperationException("Emissao de etiqueta externa requer endpoint e token reais da transportadora/hub logistico.");
        }

        var endpoint = BuildEndpoint(endpointTemplate!, pedido.Id, pedido.NumeroPedido, servicoFrete, null);
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = JsonContent.Create(new
        {
            pedido_id = pedido.Id,
            numero_pedido = pedido.NumeroPedido,
            servico_frete = servicoFrete,
            transportadora = transportadora.Slug
        });

        using var client = _httpClientFactory.CreateClient("melhor-envio");
        using var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Transportadora recusou emissao de etiqueta. HTTP {(int)response.StatusCode}.");
        }

        var codigoRastreio = ExtractFirstString(body, "tracking_code", "codigo_rastreio", "tracking_number", "code");
        var etiquetaUrl = ExtractFirstString(body, "url", "label_url", "etiqueta_url", "pdf", "print_url");
        if (!IsConfigured(codigoRastreio) || !IsConfigured(etiquetaUrl))
        {
            throw new InvalidOperationException("Transportadora respondeu sem codigo de rastreio ou URL de etiqueta.");
        }

        var envio = new Envio
        {
            PedidoId = pedido.Id,
            TransportadoraId = transportadora.Id,
            CodigoRastreio = codigoRastreio,
            EtiquetaUrl = etiquetaUrl,
            StatusEnvio = StatusEnvio.EtiquetaGerada,
            PrazoDias = pedido.FretePrazoDias,
            DataEntregaEstimada = pedido.FretePrazoDias > 0 ? DateTime.UtcNow.AddDays(pedido.FretePrazoDias) : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Envios.Add(envio);
        pedido.FreteCodigoRastreio = codigoRastreio;
        pedido.FreteTransportadora = transportadora.Nome;
        pedido.FreteMetodo = string.IsNullOrWhiteSpace(servicoFrete) ? pedido.FreteMetodo : servicoFrete;
        pedido.Status = StatusPedido.EmSeparacao;
        pedido.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _auditoria.RegistrarAsync("LOGISTICA", $"Etiqueta externa emitida para pedido {pedido.Id}, transportadora {transportadora.Nome}", null);

        return new EtiquetaDto
        {
            EtiquetaId = envio.Id,
            PedidoId = pedido.Id,
            NumeroPedido = pedido.NumeroPedido,
            Transportadora = transportadora.Nome,
            CodigoRastreio = codigoRastreio!,
            UrlEtiquetaPdf = etiquetaUrl!,
            UrlEtiquetaZpl = string.Empty,
            Status = envio.StatusEnvio.ToString(),
            GeradaEm = envio.CreatedAt
        };
    }

    public async Task<RastreamentoDto> RastrearAsync(string codigoRastreio)
    {
        if (string.IsNullOrWhiteSpace(codigoRastreio))
        {
            throw new ArgumentException("Codigo de rastreio obrigatorio.", nameof(codigoRastreio));
        }

        var envio = await _context.Envios
            .Include(item => item.Pedido)
            .Include(item => item.Transportadora)
            .FirstOrDefaultAsync(item => item.CodigoRastreio == codigoRastreio);

        if (envio is null)
        {
            throw new KeyNotFoundException("Codigo de rastreio nao localizado em envio real.");
        }

        var endpointTemplate = FirstConfigured(
            _configuration["Logistica:RastreamentoEndpointTemplate"],
            _configuration["Integracoes:Logistica:RastreamentoEndpointTemplate"],
            _configuration["MelhorEnvio:RastreamentoEndpointTemplate"],
            _configuration["Integracoes:MelhorEnvio:RastreamentoEndpointTemplate"]);
        var token = FirstConfigured(
            envio.Transportadora?.ApiToken,
            _configuration["Logistica:RastreamentoToken"],
            _configuration["Integracoes:Logistica:RastreamentoToken"],
            _configuration["MelhorEnvio:Token"],
            _configuration["Integracoes:MelhorEnvio:Token"]);

        if (!IsConfigured(endpointTemplate) || !IsConfigured(token))
        {
            throw new InvalidOperationException("Rastreamento externo requer endpoint e token reais da transportadora/hub logistico.");
        }

        var endpoint = BuildEndpoint(endpointTemplate!, envio.PedidoId, envio.Pedido?.NumeroPedido, null, codigoRastreio);
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var client = _httpClientFactory.CreateClient("melhor-envio");
        using var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Transportadora recusou rastreamento. HTTP {(int)response.StatusCode}.");
        }

        var eventos = ExtractEventos(body);
        envio.EventosRastreamento = body;
        envio.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new RastreamentoDto
        {
            CodigoRastreio = codigoRastreio,
            Transportadora = envio.Transportadora?.Nome ?? envio.Pedido?.FreteTransportadora ?? string.Empty,
            StatusAtual = ExtractFirstString(body, "status", "status_name", "situacao", "state") ?? envio.StatusEnvio.ToString(),
            DescricaoStatus = ExtractFirstString(body, "description", "descricao", "message") ?? envio.StatusEnvio.ToString(),
            PrevisaoEntrega = envio.DataEntregaEstimada,
            Eventos = eventos
        };
    }

    public async Task<bool> AtualizarStatusEnvioAsync(int pedidoId, string status, string codigoRastreio, DateTime? dataEnvio, DateTime? dataEntrega)
    {
        var pedido = await _context.Pedidos
            .Include(item => item.Cliente)
            .FirstOrDefaultAsync(item => item.Id == pedidoId);
        if (pedido is null)
        {
            return false;
        }

        if (!Enum.TryParse<StatusPedido>(status, true, out var statusPedido))
        {
            throw new ArgumentException("Status de pedido invalido.", nameof(status));
        }

        pedido.Status = statusPedido;
        if (!string.IsNullOrWhiteSpace(codigoRastreio))
        {
            pedido.FreteCodigoRastreio = codigoRastreio.Trim();
        }

        if (dataEnvio.HasValue)
        {
            pedido.DataEnvio = dataEnvio.Value;
        }

        if (dataEntrega.HasValue)
        {
            pedido.DataEntrega = dataEntrega.Value;
        }

        pedido.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        if (pedido.Cliente is not null)
        {
            await _notificacao.EnviarStatusPedidoAsync(pedido.Cliente, pedido, $"Status logistico atualizado: {statusPedido}");
        }

        await _auditoria.RegistrarAsync("LOGISTICA", $"Status logistico do pedido {pedidoId}: {statusPedido}", null);
        return true;
    }

    public async Task<DashboardLogisticaDto> ObterDashboardAsync()
    {
        var hoje = DateTime.UtcNow.Date;
        var pedidosHoje = await _context.Pedidos.CountAsync(item => item.CreatedAt.Date == hoje);
        var separacao = await _context.Pedidos.CountAsync(item => item.Status == StatusPedido.EmSeparacao);
        var transito = await _context.Pedidos.CountAsync(item => item.Status == StatusPedido.Enviado);
        var entreguesHoje = await _context.Pedidos.CountAsync(item => item.DataEntrega.HasValue && item.DataEntrega.Value.Date == hoje);
        var atrasados = await _context.Pedidos.CountAsync(item =>
            item.Status == StatusPedido.Enviado &&
            item.DataEnvio.HasValue &&
            item.FretePrazoDias > 0 &&
            item.DataEnvio.Value.AddDays(item.FretePrazoDias) < DateTime.UtcNow);

        var pendentes = await _context.Pedidos
            .AsNoTracking()
            .Include(item => item.Cliente)
            .Where(item => item.Status == StatusPedido.Pago || item.Status == StatusPedido.EmSeparacao)
            .OrderBy(item => item.CreatedAt)
            .Take(20)
            .Select(item => new PedidoLogisticaDto
            {
                PedidoId = item.Id,
                NumeroPedido = item.NumeroPedido,
                ClienteNome = item.Cliente == null ? string.Empty : item.Cliente.Nome,
                Status = item.Status.ToString(),
                Transportadora = item.FreteTransportadora ?? string.Empty,
                CodigoRastreio = item.FreteCodigoRastreio ?? string.Empty,
                PrevisaoEntrega = item.DataEnvio.HasValue && item.FretePrazoDias > 0 ? item.DataEnvio.Value.AddDays(item.FretePrazoDias) : null,
                DiasAtraso = item.DataEnvio.HasValue && item.FretePrazoDias > 0 && item.DataEnvio.Value.AddDays(item.FretePrazoDias) < DateTime.UtcNow
                    ? (DateTime.UtcNow - item.DataEnvio.Value.AddDays(item.FretePrazoDias)).Days
                    : 0
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
            .AsNoTracking()
            .OrderBy(item => item.Nome)
            .Select(item => new TransportadoraDto
            {
                TransportadoraId = item.Id,
                Nome = item.Nome,
                CodigoApi = item.Slug,
                Ativa = item.Ativa,
                TaxaAdicional = null
            })
            .ToListAsync();
    }

    public Task<bool> ImprimirEtiquetaAsync(int etiquetaId)
    {
        throw new InvalidOperationException("Impressao direta de etiqueta requer spooler/impressora configurados; o sistema nao marca etiqueta como impressa sem comprovacao do processo real.");
    }

    private static Uri BuildEndpoint(string template, int pedidoId, string? numeroPedido, string? servicoFrete, string? codigoRastreio)
    {
        var endpoint = template
            .Replace("{pedidoId}", Uri.EscapeDataString(pedidoId.ToString()), StringComparison.OrdinalIgnoreCase)
            .Replace("{numeroPedido}", Uri.EscapeDataString(numeroPedido ?? string.Empty), StringComparison.OrdinalIgnoreCase)
            .Replace("{servico}", Uri.EscapeDataString(servicoFrete ?? string.Empty), StringComparison.OrdinalIgnoreCase)
            .Replace("{codigo}", Uri.EscapeDataString(codigoRastreio ?? string.Empty), StringComparison.OrdinalIgnoreCase);

        if (Uri.TryCreate(endpoint, UriKind.Absolute, out var absolute))
        {
            return absolute;
        }

        return new Uri(new Uri("https://www.melhorenvio.com.br/"), endpoint.TrimStart('/'));
    }

    private static string? FirstConfigured(params string?[] values)
    {
        return values.FirstOrDefault(IsConfigured);
    }

    private static bool IsConfigured(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && !value.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase)
            && !value.Contains("USE_ENV", StringComparison.OrdinalIgnoreCase)
            && !value.Equals("null", StringComparison.OrdinalIgnoreCase);
    }

    private static string? ExtractFirstString(string json, params string[] names)
    {
        using var document = JsonDocument.Parse(json);
        return ExtractFirstString(document.RootElement, names);
    }

    private static string? ExtractFirstString(JsonElement element, params string[] names)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var name in names)
            {
                foreach (var property in element.EnumerateObject())
                {
                    if (property.NameEquals(name) && property.Value.ValueKind is JsonValueKind.String or JsonValueKind.Number)
                    {
                        var value = property.Value.ToString();
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            return value.Trim();
                        }
                    }
                }
            }

            foreach (var property in element.EnumerateObject())
            {
                var nested = ExtractFirstString(property.Value, names);
                if (!string.IsNullOrWhiteSpace(nested))
                {
                    return nested;
                }
            }
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var nested = ExtractFirstString(item, names);
                if (!string.IsNullOrWhiteSpace(nested))
                {
                    return nested;
                }
            }
        }

        return null;
    }

    private static List<EventoRastreamentoDto> ExtractEventos(string json)
    {
        using var document = JsonDocument.Parse(json);
        var eventos = new List<EventoRastreamentoDto>();
        CollectEventos(document.RootElement, eventos);
        return eventos
            .OrderByDescending(item => item.DataHora)
            .ToList();
    }

    private static void CollectEventos(JsonElement element, List<EventoRastreamentoDto> eventos)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object)
                {
                    var status = ExtractFirstString(item, "status", "status_name", "name", "title", "message");
                    var descricao = ExtractFirstString(item, "description", "descricao", "detail", "detalhe", "message");
                    if (!string.IsNullOrWhiteSpace(status) || !string.IsNullOrWhiteSpace(descricao))
                    {
                        eventos.Add(new EventoRastreamentoDto
                        {
                            DataHora = ExtractFirstDateTime(item) ?? DateTime.MinValue,
                            Status = status ?? "Evento",
                            Local = ExtractFirstString(item, "location", "local", "city", "cidade", "unit") ?? string.Empty,
                            Descricao = descricao ?? string.Empty
                        });
                    }

                    CollectEventos(item, eventos);
                }
            }

            return;
        }

        if (element.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (property.Value.ValueKind is JsonValueKind.Array or JsonValueKind.Object)
            {
                CollectEventos(property.Value, eventos);
            }
        }
    }

    private static DateTime? ExtractFirstDateTime(JsonElement element)
    {
        var value = ExtractFirstString(element, "date", "datetime", "data", "data_hora", "created_at", "updated_at", "timestamp");
        return DateTimeOffset.TryParse(value, out var parsed) ? parsed.UtcDateTime : null;
    }
}
