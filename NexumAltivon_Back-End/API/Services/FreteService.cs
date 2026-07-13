/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Models;

namespace NexumAltivon.API.Services
{
    public interface IFreteService
    {
        Task<List<OpcaoFreteDto>> CalcularFreteAsync(string cepDestino, List<ItemCarrinho> itens);
        Task<List<OpcaoFreteDto>> CalcularFreteMelhorEnvioAsync(string cepDestino, List<ItemCarrinho> itens);
        Task<string> GerarEtiquetaAsync(int pedidoId);
        Task RastrearEnvioAsync(string codigoRastreio);
    }

    public class FreteService : IFreteService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<FreteService> _logger;
        private readonly bool _usarMelhorEnvio;

        public FreteService(IHttpClientFactory factory, IConfiguration config, ILogger<FreteService> logger)
        {
            _httpClient = factory.CreateClient("MelhorEnvio");
            _config = config;
            _logger = logger;
            _usarMelhorEnvio = bool.Parse(_config["Integracoes:MelhorEnvio:Ativo"] ?? "false");
            var token = _config["Integracoes:MelhorEnvio:Token"];
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<OpcaoFreteDto>> CalcularFreteAsync(string cepDestino, List<ItemCarrinho> itens)
        {
            if (_usarMelhorEnvio)
                return await CalcularFreteMelhorEnvioAsync(cepDestino, itens);

            return CalcularFreteTabelaPropria(cepDestino, itens);
        }

        private List<OpcaoFreteDto> CalcularFreteTabelaPropria(string cepDestino, List<ItemCarrinho> itens)
        {
            var pesoTotal = itens.Sum(i => i.Quantidade * 0.5); // estimativa 500g por item
            var regiao = ObterRegiao(cepDestino);
            var baseFrete = regiao switch
            {
                "SP" => 12.90m,
                "SUDESTE" => 18.50m,
                "SUL" => 22.00m,
                "NORDESTE" => 28.00m,
                "NORTE" => 32.00m,
                "CENTRO_OESTE" => 25.00m,
                _ => 20.00m
            };

            var adicionalPeso = pesoTotal > 5 ? (decimal)(pesoTotal - 5) * 1.5m : 0;
            var valor = baseFrete + adicionalPeso;

            return new List<OpcaoFreteDto>
            {
                new() {
                    Codigo = "ECONOMICA",
                    Nome = "Econômica Nexum",
                    Descricao = "Entrega em até 8 dias úteis",
                    Valor = valor,
                    PrazoDias = 8,
                    SeguroIncluso = true
                },
                new() {
                    Codigo = "EXPRESSA",
                    Nome = "Expressa Nexum",
                    Descricao = "Entrega em até 4 dias úteis",
                    Valor = valor * 1.6m,
                    PrazoDias = 4,
                    SeguroIncluso = true
                },
                new() {
                    Codigo = "PREMIUM",
                    Nome = "Premium Nexum",
                    Descricao = "Entrega em até 2 dias úteis",
                    Valor = valor * 2.2m,
                    PrazoDias = 2,
                    SeguroIncluso = true
                }
            };
        }

        public async Task<List<OpcaoFreteDto>> CalcularFreteMelhorEnvioAsync(string cepDestino, List<ItemCarrinho> itens)
        {
            try
            {
                var products = itens.Select(i => new
                {
                    id = i.ProdutoId.ToString(),
                    width = 11,
                    height = 17,
                    length = 11,
                    weight = 0.5,
                    insurance_value = (double)i.PrecoUnitario,
                    quantity = i.Quantidade
                }).ToList();

                var request = new
                {
                    from = new { postal_code = _config["Integracoes:MelhorEnvio:CepOrigem"] ?? "01001000" },
                    to = new { postal_code = cepDestino.Replace("-", "") },
                    products = products,
                    options = new { receipt = false, own_hand = false }
                };

                var response = await _httpClient.PostAsJsonAsync("/api/v2/me/shipment/calculate", request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Melhor Envio recusou cotacao: {Content}.", content);
                    throw new InvalidOperationException("Melhor Envio recusou a cotacao. Revise token, origem, destino e dados dos produtos.");
                }

                using var doc = JsonDocument.Parse(content);
                var opcoes = new List<OpcaoFreteDto>();
                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    if (element.GetProperty("error").GetString() != null) continue;
                    opcoes.Add(new OpcaoFreteDto
                    {
                        Codigo = element.GetProperty("id").GetInt32().ToString(),
                        Nome = element.GetProperty("name").GetString(),
                        Descricao = $"{element.GetProperty("company").GetProperty("name").GetString()} - {element.GetProperty("name").GetString()}",
                        Valor = (decimal)element.GetProperty("price").GetDouble(),
                        PrazoDias = element.GetProperty("delivery_time").GetProperty("days").GetInt32(),
                        SeguroIncluso = true
                    });
                }

                return opcoes.OrderBy(o => o.Valor).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar Melhor Envio");
                throw;
            }
        }

        public async Task<string> GerarEtiquetaAsync(int pedidoId)
        {
            if (!_usarMelhorEnvio)
            {
                throw new InvalidOperationException("Geracao de etiqueta requer Melhor Envio ativo com token real.");
            }

            var endpoint = _config["Integracoes:MelhorEnvio:EtiquetaEndpoint"] ?? _config["MelhorEnvio:EtiquetaEndpoint"];
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new InvalidOperationException("Geracao de etiqueta requer endpoint real de etiqueta do Melhor Envio.");
            }

            var response = await _httpClient.PostAsJsonAsync(endpoint, new { pedido_id = pedidoId });
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Melhor Envio recusou a etiqueta do pedido {pedidoId}. HTTP {(int)response.StatusCode}.");
            }

            using var document = JsonDocument.Parse(content);
            foreach (var propertyName in new[] { "tracking_code", "codigo_rastreio", "tracking_number", "code" })
            {
                if (document.RootElement.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
                {
                    var codigo = property.GetString();
                    if (!string.IsNullOrWhiteSpace(codigo))
                    {
                        return codigo;
                    }
                }
            }

            throw new InvalidOperationException("Melhor Envio respondeu sem codigo de rastreio da etiqueta.");
        }

        public async Task RastrearEnvioAsync(string codigoRastreio)
        {
            if (string.IsNullOrWhiteSpace(codigoRastreio))
            {
                throw new ArgumentException("Codigo de rastreio obrigatorio.", nameof(codigoRastreio));
            }

            var endpointTemplate = _config["Integracoes:MelhorEnvio:RastreamentoEndpointTemplate"] ?? _config["MelhorEnvio:RastreamentoEndpointTemplate"];
            if (string.IsNullOrWhiteSpace(endpointTemplate))
            {
                throw new InvalidOperationException("Rastreamento requer endpoint real do provedor logistico.");
            }

            var endpoint = endpointTemplate.Replace("{codigo}", Uri.EscapeDataString(codigoRastreio), StringComparison.OrdinalIgnoreCase);
            using var response = await _httpClient.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Provedor logistico recusou rastreamento. HTTP {(int)response.StatusCode}.");
            }
        }

        private string ObterRegiao(string cep)
        {
            var prefixo = int.Parse(cep.Replace("-", "").Substring(0, 3));
            return prefixo switch
            {
                >= 10 and <= 199 => "SP",
                >= 200 and <= 289 => "SUDESTE", // RJ
                >= 290 and <= 299 => "SUDESTE", // ES
                >= 300 and <= 399 => "SUDESTE", // MG
                >= 400 and <= 499 => "NORDESTE", // BA
                >= 500 and <= 599 => "NORDESTE", // PE, AL, PB, RN
                >= 600 and <= 639 => "NORDESTE", // CE
                >= 640 and <= 699 => "NORDESTE", // PI, MA
                >= 700 and <= 799 => "CENTRO_OESTE", // DF, GO
                >= 800 and <= 879 => "SUL", // PR
                >= 880 and <= 899 => "SUL", // SC
                >= 900 and <= 999 => "SUL", // RS
                >= 650 and <= 699 => "NORTE", // PA, AP (overlap tratado pela ordem)
                >= 768 and <= 769 => "NORTE", // RO
                >= 770 and <= 779 => "NORTE", // TO (mas overlap, simplificar)
                _ => "OUTROS"
            };
        }
    }
}
