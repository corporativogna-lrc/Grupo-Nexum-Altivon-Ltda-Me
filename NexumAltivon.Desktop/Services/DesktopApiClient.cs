/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7258 Data: 30/10/2026
 */

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NexumAltivon.Desktop.Models;

namespace NexumAltivon.Desktop.Services;

public sealed class DesktopApiClient
{
    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(12)
    };

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<DesktopLoginResult> AuthenticateAsync(
        TerminalProfile profile,
        string email,
        string password,
        string? mfaCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return new DesktopLoginResult(false, null, null, "E-mail e senha são obrigatórios.");
        }

        foreach (var endpoint in GetEndpoints(profile))
        {
            try
            {
                using var response = await Http.PostAsJsonAsync(
                    $"{endpoint}/api/auth/login",
                    new DesktopLoginRequest(email.Trim(), password, string.IsNullOrWhiteSpace(mfaCode) ? null : mfaCode.Trim()),
                    JsonOptions,
                    cancellationToken);

                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode is 400 or 401 or 403)
                    {
                        return new DesktopLoginResult(false, null, null, ExtractErrorMessage(content, response.ReasonPhrase));
                    }
                    continue;
                }

                var envelope = JsonSerializer.Deserialize<DesktopApiEnvelope<DesktopLoginResponse>>(content, JsonOptions);
                var login = envelope?.Dados;

                if (envelope is null || !envelope.Sucesso || login is null || string.IsNullOrWhiteSpace(login.Token) || login.ExpiraEm <= DateTime.UtcNow)
                {
                    return new DesktopLoginResult(false, null, null, envelope?.Mensagem ?? "A API não confirmou uma sessão JWT válida.");
                }

                return new DesktopLoginResult(true, login.Token, login.Usuario, $"Sessão autenticada em {endpoint}.");
            }
            catch (HttpRequestException) { continue; }
            catch (TaskCanceledException) { continue; }
        }

        return new DesktopLoginResult(false, null, null, "API local e pública indisponíveis para autenticação.");
    }

    public async Task<DesktopHealthResult> CheckHealthAsync(TerminalProfile profile, CancellationToken cancellationToken = default)
    {
        bool localHealthy = false;
        bool publicHealthy = false;
        string detail;

        if (!string.IsNullOrWhiteSpace(profile.ApiBaseUrl))
        {
            localHealthy = await PingEndpointAsync($"{profile.ApiBaseUrl.TrimEnd('/')}/health", cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(profile.PublicApiUrl))
        {
            publicHealthy = await PingEndpointAsync($"{profile.PublicApiUrl.TrimEnd('/')}/health", cancellationToken);
        }

        string status = (localHealthy || publicHealthy) ? "Conectado ao servidor" : "Operando em Contingência";

        if (!localHealthy && !publicHealthy)
            detail = "Nenhum endpoint respondeu. O sistema registrará as operações localmente na pasta de contingência (Outbox).";
        else if (localHealthy && !publicHealthy)
            detail = "Conectado apenas à rede local. Sincronização externa indisponível.";
        else if (!localHealthy && publicHealthy)
            detail = "Servidor local inacessível. Operando via nuvem (Cloudflare).";
        else
            detail = "Todos os sistemas operacionais e sincronizados perfeitamente.";

        return new DesktopHealthResult(status, detail, DateTime.Now, localHealthy, publicHealthy);
    }

    public Task<DesktopApiDataResult<DesktopComprasPainel>> GetComprasPainelAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopComprasPainel>(
            profile, token, HttpMethod.Get, "/api/compras/painel", null, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopGenesisSaldoEstoque>>> GetGenesisPhysicalStockAsync(
        TerminalProfile profile,
        string token,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = string.IsNullOrWhiteSpace(search)
            ? string.Empty
            : $"?termo={Uri.EscapeDataString(search.Trim())}";
        return SendAuthorizedAsync<List<DesktopGenesisSaldoEstoque>>(
            profile, token, HttpMethod.Get, $"/api/erp/estoque-fisico{query}", null, cancellationToken);
    }

    public Task<DesktopApiDataResult<List<DesktopGenesisMovimentacaoEstoque>>> GetGenesisStockMovementsAsync(
        TerminalProfile profile,
        string token,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = string.IsNullOrWhiteSpace(search)
            ? string.Empty
            : $"?termo={Uri.EscapeDataString(search.Trim())}";
        return SendAuthorizedAsync<List<DesktopGenesisMovimentacaoEstoque>>(
            profile, token, HttpMethod.Get, $"/api/erp/estoque-movimentacoes{query}", null, cancellationToken);
    }

    public Task<DesktopApiDataResult<List<DesktopProdutoIdentificador>>> GetProductIdentifiersAsync(
        TerminalProfile profile, string token, string? search, int page = 1,
        CancellationToken cancellationToken = default)
    {
        var query = $"?pagina={Math.Max(1, page)}";
        if (!string.IsNullOrWhiteSpace(search))
            query += $"&termo={Uri.EscapeDataString(search.Trim())}";
        return SendAuthorizedAsync<List<DesktopProdutoIdentificador>>(
            profile, token, HttpMethod.Get, $"/api/admin/produtos/identificadores{query}", null, cancellationToken);
    }

    public Task<DesktopApiDataResult<DesktopProdutoIdentificador>> UpdateProductIdentifiersAsync(
        TerminalProfile profile, string token, int productId, DesktopProdutoIdentificadorUpdateRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopProdutoIdentificador>(
            profile, token, HttpMethod.Put, $"/api/admin/produtos/{productId}/identificadores", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopPdvSaleResult>> RegisterPdvSaleAsync(
        TerminalProfile profile, string token, DesktopPdvSaleRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopPdvSaleResult>(profile, token, HttpMethod.Post,
            "/api/erp/genesis/pdv/vendas", request, cancellationToken);

    public async Task<DesktopApiDataResult<List<DesktopPdvCatalogProduct>>> SearchPdvCatalogAsync(
        TerminalProfile profile, string search, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(search) || search.Trim().Length < 2)
        {
            return new DesktopApiDataResult<List<DesktopPdvCatalogProduct>>(
                false, null, "Digite ao menos dois caracteres para pesquisar o catálogo.");
        }
        if (search.Trim().Length > 256)
        {
            return new DesktopApiDataResult<List<DesktopPdvCatalogProduct>>(
                false, null, "A pesquisa do catálogo aceita no máximo 256 caracteres.");
        }

        var path = $"/api/produtos?busca={Uri.EscapeDataString(search.Trim())}&pagina=1&itensPorPagina=30";
        foreach (var endpoint in GetEndpoints(profile))
        {
            try
            {
                using var response = await Http.GetAsync($"{endpoint}{path}", cancellationToken);
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode >= 500) continue;
                    return new DesktopApiDataResult<List<DesktopPdvCatalogProduct>>(
                        false, null, ExtractErrorMessage(content, response.ReasonPhrase));
                }

                var envelope = JsonSerializer.Deserialize<DesktopApiEnvelope<List<DesktopPdvCatalogProduct>>>(content, JsonOptions);
                if (envelope is null || !envelope.Sucesso || envelope.Dados is null)
                {
                    return new DesktopApiDataResult<List<DesktopPdvCatalogProduct>>(
                        false, null, envelope?.Mensagem ?? "A API não retornou um catálogo válido.");
                }

                return new DesktopApiDataResult<List<DesktopPdvCatalogProduct>>(true, envelope.Dados, envelope.Mensagem);
            }
            catch (HttpRequestException) { continue; }
            catch (TaskCanceledException) { continue; }
        }

        return new DesktopApiDataResult<List<DesktopPdvCatalogProduct>>(
            false, null, "API local e pública indisponíveis para consultar o catálogo.");
    }

    public async Task<DesktopApiDataResult<JsonElement>> GetPublicOrderTrackingAsync(
        TerminalProfile profile, string orderNumber, string customerEmailOrDocument,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderNumber) || string.IsNullOrWhiteSpace(customerEmailOrDocument))
        {
            return new DesktopApiDataResult<JsonElement>(false, default,
                "Número do pedido e e-mail ou CPF/CNPJ do comprador são obrigatórios.");
        }

        var identity = customerEmailOrDocument.Trim();
        var identityParameter = identity.Contains('@') ? "email" : "documento";
        var path = $"/api/pedidos/acompanhar?numero={Uri.EscapeDataString(orderNumber.Trim())}&{identityParameter}={Uri.EscapeDataString(identity)}";
        foreach (var endpoint in GetEndpoints(profile))
        {
            try
            {
                using var response = await Http.GetAsync($"{endpoint}{path}", cancellationToken);
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode >= 500) continue;
                    return new DesktopApiDataResult<JsonElement>(false, default,
                        ExtractErrorMessage(content, response.ReasonPhrase));
                }

                var envelope = JsonSerializer.Deserialize<DesktopApiEnvelope<JsonElement>>(content, JsonOptions);
                if (envelope is null || !envelope.Sucesso || envelope.Dados.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
                {
                    return new DesktopApiDataResult<JsonElement>(false, default,
                        envelope?.Mensagem ?? "A API não retornou os dados do pedido.");
                }

                return new DesktopApiDataResult<JsonElement>(true, envelope.Dados, envelope.Mensagem);
            }
            catch (HttpRequestException) { continue; }
            catch (TaskCanceledException) { continue; }
        }

        return new DesktopApiDataResult<JsonElement>(false, default,
            "API local e pública indisponíveis para consultar o status do pedido.");
    }

    public Task<DesktopApiDataResult<List<DesktopAuditoriaOperacional>>> GetAuditoriaAsync(
        TerminalProfile profile,
        string token,
        string tabela,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tabela))
        {
            return Task.FromResult(new DesktopApiDataResult<List<DesktopAuditoriaOperacional>>(
                false, null, "A tabela é obrigatória para consultar a auditoria."));
        }

        var path = $"/api/auditoria?tabela={Uri.EscapeDataString(tabela.Trim())}";
        return SendAuthorizedAsync<List<DesktopAuditoriaOperacional>>(
            profile, token, HttpMethod.Get, path, null, cancellationToken);
    }

    public Task<DesktopApiDataResult<List<DesktopAuditoriaOperacional>>> GetAuditoriaFiltradaAsync(
        TerminalProfile profile,
        string token,
        string? modulo,
        string? tabela,
        int? usuario,
        DateTime? dataInicio,
        DateTime? dataFim,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(modulo))
            query.Add($"modulo={Uri.EscapeDataString(modulo.Trim())}");
        if (!string.IsNullOrWhiteSpace(tabela))
            query.Add($"tabela={Uri.EscapeDataString(tabela.Trim())}");
        if (usuario.HasValue)
            query.Add($"usuario={usuario.Value}");
        if (dataInicio.HasValue)
            query.Add($"dataInicio={Uri.EscapeDataString(dataInicio.Value.ToString("O"))}");
        if (dataFim.HasValue)
            query.Add($"dataFim={Uri.EscapeDataString(dataFim.Value.ToString("O"))}");

        var path = "/api/auditoria" + (query.Count == 0 ? string.Empty : "?" + string.Join("&", query));
        return SendAuthorizedAsync<List<DesktopAuditoriaOperacional>>(
            profile, token, HttpMethod.Get, path, null, cancellationToken);
    }

    public Task<DesktopApiDataResult<List<DesktopSiteConfiguracao>>> GetSiteConfigurationsAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopSiteConfiguracao>>(
            profile, token, HttpMethod.Get, "/api/site/configuracoes", null, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopSiteConfiguracao>>> SaveSiteConfigurationsAsync(
        TerminalProfile profile,
        string token,
        DesktopSiteConfiguracaoUpdateRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopSiteConfiguracao>>(
            profile, token, HttpMethod.Put, "/api/site/configuracoes", request, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopCliente>>> GetClientesAsync(
        TerminalProfile profile, string token, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopCliente>>(
            profile, token, HttpMethod.Get, "/api/clientes", null, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopCliente>>> SearchClientesAsync(
        TerminalProfile profile, string token, string? search, CancellationToken cancellationToken = default)
    {
        var term = search?.Trim();
        if (!string.IsNullOrEmpty(term) && term.Length < 2)
        {
            return Task.FromResult(new DesktopApiDataResult<List<DesktopCliente>>(
                false, null, "Informe pelo menos dois caracteres para filtrar clientes."));
        }

        var path = string.IsNullOrEmpty(term)
            ? "/api/clientes"
            : $"/api/clientes?busca={Uri.EscapeDataString(term)}";
        return SendAuthorizedAsync<List<DesktopCliente>>(
            profile, token, HttpMethod.Get, path, null, cancellationToken);
    }

    public Task<DesktopApiDataResult<DesktopCliente>> GetClienteAsync(
        TerminalProfile profile, string token, int clienteId, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopCliente>(
            profile, token, HttpMethod.Get, $"/api/clientes/{clienteId}", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopCliente>> CreateClienteAsync(
        TerminalProfile profile, string token, DesktopClienteRequest request, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopCliente>(
            profile, token, HttpMethod.Post, "/api/clientes/admin", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopCliente>> UpdateClienteAsync(
        TerminalProfile profile, string token, int clienteId, DesktopClienteRequest request, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopCliente>(
            profile, token, HttpMethod.Put, $"/api/clientes/{clienteId}", request, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopClienteEndereco>>> GetClienteEnderecosAsync(
        TerminalProfile profile, string token, int clienteId, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopClienteEndereco>>(
            profile, token, HttpMethod.Get, $"/api/clientes/{clienteId}/enderecos", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopClienteEndereco>> CreateClienteEnderecoAsync(
        TerminalProfile profile, string token, int clienteId, DesktopClienteEnderecoRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopClienteEndereco>(
            profile, token, HttpMethod.Post, $"/api/clientes/{clienteId}/enderecos", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopClienteEndereco>> UpdateClienteEnderecoAsync(
        TerminalProfile profile, string token, int clienteId, int enderecoId, DesktopClienteEnderecoRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopClienteEndereco>(
            profile, token, HttpMethod.Put, $"/api/clientes/{clienteId}/enderecos/{enderecoId}", request, cancellationToken);

    public Task<DesktopApiDataResult<bool>> ArchiveClienteEnderecoAsync(
        TerminalProfile profile, string token, int clienteId, int enderecoId, string? rowVersion,
        CancellationToken cancellationToken = default)
    {
        var path = $"/api/clientes/{clienteId}/enderecos/{enderecoId}?rowVersion={Uri.EscapeDataString(rowVersion ?? string.Empty)}";
        return SendAuthorizedAsync<bool>(profile, token, HttpMethod.Delete, path, null, cancellationToken);
    }

    public Task<DesktopApiDataResult<List<DesktopFornecedorContato>>> GetFornecedorContatosAsync(
        TerminalProfile profile, string token, int fornecedorId, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopFornecedorContato>>(
            profile, token, HttpMethod.Get, $"/api/fornecedores/{fornecedorId}/contatos", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopFornecedorContato>> CreateFornecedorContatoAsync(
        TerminalProfile profile, string token, int fornecedorId, DesktopFornecedorContatoRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopFornecedorContato>(
            profile, token, HttpMethod.Post, $"/api/fornecedores/{fornecedorId}/contatos", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopFornecedorContato>> UpdateFornecedorContatoAsync(
        TerminalProfile profile, string token, int fornecedorId, int contatoId, DesktopFornecedorContatoRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopFornecedorContato>(
            profile, token, HttpMethod.Put, $"/api/fornecedores/{fornecedorId}/contatos/{contatoId}", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopFornecedorContato>> DeactivateFornecedorContatoAsync(
        TerminalProfile profile, string token, int fornecedorId, int contatoId, string? rowVersion,
        CancellationToken cancellationToken = default)
    {
        var path = $"/api/fornecedores/{fornecedorId}/contatos/{contatoId}?rowVersion={Uri.EscapeDataString(rowVersion ?? string.Empty)}";
        return SendAuthorizedAsync<DesktopFornecedorContato>(profile, token, HttpMethod.Delete, path, null, cancellationToken);
    }

    public Task<DesktopApiDataResult<List<DesktopContaPagar>>> GetContasPagarAsync(
        TerminalProfile profile, string token, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopContaPagar>>(
            profile, token, HttpMethod.Get, "/api/erp/genesis/financeiro/contas-pagar", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopContaPagar>> CreateContaPagarAsync(
        TerminalProfile profile, string token, DesktopContaPagarCreateRequest request, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopContaPagar>(
            profile, token, HttpMethod.Post, "/api/erp/genesis/financeiro/contas-pagar", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopContaPagar>> BaixarContaPagarAsync(
        TerminalProfile profile, string token, int contaId, DesktopBaixaPagarRequest request, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopContaPagar>(
            profile, token, HttpMethod.Post, $"/api/erp/genesis/financeiro/contas-pagar/{contaId}/baixa", request, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopContaReceber>>> GetContasReceberAsync(
        TerminalProfile profile, string token, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopContaReceber>>(
            profile, token, HttpMethod.Get, "/api/erp/genesis/financeiro/contas-receber", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopContaReceber>> CreateContaReceberAsync(
        TerminalProfile profile, string token, DesktopContaReceberCreateRequest request, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopContaReceber>(
            profile, token, HttpMethod.Post, "/api/erp/genesis/financeiro/contas-receber", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopContaReceber>> BaixarContaReceberAsync(
        TerminalProfile profile, string token, int contaId, DesktopBaixaReceberRequest request, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopContaReceber>(
            profile, token, HttpMethod.Post, $"/api/erp/genesis/financeiro/contas-receber/{contaId}/baixa", request, cancellationToken);

    public Task<DesktopFileDownloadResult> GetContasPagarPdfAsync(
        TerminalProfile profile, string token, CancellationToken cancellationToken = default) =>
        SendAuthorizedFileAsync(profile, token, "/api/erp/genesis/financeiro/contas-pagar/relatorio.pdf", cancellationToken);

    public Task<DesktopFileDownloadResult> GetContasReceberPdfAsync(
        TerminalProfile profile, string token, CancellationToken cancellationToken = default) =>
        SendAuthorizedFileAsync(profile, token, "/api/erp/genesis/financeiro/contas-receber/relatorio.pdf", cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopEmpresaGrupo>>> GetEmpresasGrupoAsync(
        TerminalProfile profile, string token, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopEmpresaGrupo>>(
            profile, token, HttpMethod.Get, "/api/erp/empresas", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopEmpresaGrupo>> CreateEmpresaGrupoAsync(
        TerminalProfile profile, string token, DesktopEmpresaGrupoRequest request, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopEmpresaGrupo>(
            profile, token, HttpMethod.Post, "/api/erp/empresas", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopEmpresaGrupo>> UpdateEmpresaGrupoAsync(
        TerminalProfile profile, string token, int empresaId, DesktopEmpresaGrupoRequest request, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopEmpresaGrupo>(
            profile, token, HttpMethod.Put, $"/api/erp/empresas/{empresaId}", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopEmpresaGrupo>> DeleteEmpresaGrupoAsync(
        TerminalProfile profile, string token, int empresaId, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopEmpresaGrupo>(
            profile, token, HttpMethod.Delete, $"/api/erp/empresas/{empresaId}", null, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopOpsAtivo>>> GetOpsAtivosAsync(
        TerminalProfile profile, string token, string? status, CancellationToken cancellationToken = default)
    {
        var query = string.IsNullOrWhiteSpace(status) ? string.Empty : $"?status={Uri.EscapeDataString(status.Trim())}";
        return SendAuthorizedAsync<List<DesktopOpsAtivo>>(
            profile, token, HttpMethod.Get, $"/api/ops/ativos{query}", null, cancellationToken);
    }

    public Task<DesktopApiDataResult<List<DesktopOpsOrdemServico>>> GetOpsOrdensServicoAsync(
        TerminalProfile profile, string token, string? status, CancellationToken cancellationToken = default)
    {
        var query = string.IsNullOrWhiteSpace(status) ? string.Empty : $"?status={Uri.EscapeDataString(status.Trim())}";
        return SendAuthorizedAsync<List<DesktopOpsOrdemServico>>(
            profile, token, HttpMethod.Get, $"/api/ops/ordens-servico{query}", null, cancellationToken);
    }

    public Task<DesktopApiDataResult<DesktopOpsOrdemServicoDetalhe>> GetOpsOrdemServicoAsync(
        TerminalProfile profile, string token, int ordemId, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopOpsOrdemServicoDetalhe>(
            profile, token, HttpMethod.Get, $"/api/ops/ordens-servico/{ordemId}", null, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopOpsProducaoApontamento>>> GetOpsProducaoApontamentosAsync(
        TerminalProfile profile, string token, DateTime? inicio, DateTime? fim, CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (inicio.HasValue)
            query.Add($"inicio={Uri.EscapeDataString(inicio.Value.ToString("O"))}");
        if (fim.HasValue)
            query.Add($"fim={Uri.EscapeDataString(fim.Value.ToString("O"))}");
        var path = "/api/ops/producao/apontamentos" + (query.Count == 0 ? string.Empty : "?" + string.Join("&", query));
        return SendAuthorizedAsync<List<DesktopOpsProducaoApontamento>>(
            profile, token, HttpMethod.Get, path, null, cancellationToken);
    }

    public Task<DesktopApiDataResult<DesktopOpsAtivo>> SaveOpsAtivoAsync(
        TerminalProfile profile, string token, int? ativoId, DesktopOpsAtivoRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopOpsAtivo>(
            profile, token,
            ativoId.HasValue ? HttpMethod.Put : HttpMethod.Post,
            ativoId.HasValue ? $"/api/ops/ativos/{ativoId.Value}" : "/api/ops/ativos",
            request, cancellationToken);

    public Task<DesktopApiDataResult<bool>> DeleteOpsAtivoAsync(
        TerminalProfile profile, string token, int ativoId, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<bool>(profile, token, HttpMethod.Delete, $"/api/ops/ativos/{ativoId}", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopOpsOrdemServico>> SaveOpsOrdemServicoAsync(
        TerminalProfile profile, string token, int? ordemId, DesktopOpsOrdemServicoRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopOpsOrdemServico>(
            profile, token,
            ordemId.HasValue ? HttpMethod.Put : HttpMethod.Post,
            ordemId.HasValue ? $"/api/ops/ordens-servico/{ordemId.Value}" : "/api/ops/ordens-servico",
            request, cancellationToken);

    public Task<DesktopApiDataResult<bool>> DeleteOpsOrdemServicoAsync(
        TerminalProfile profile, string token, int ordemId, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<bool>(profile, token, HttpMethod.Delete, $"/api/ops/ordens-servico/{ordemId}", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopOpsProducaoApontamento>> CreateOpsProducaoApontamentoAsync(
        TerminalProfile profile, string token, DesktopOpsProducaoApontamentoRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopOpsProducaoApontamento>(
            profile, token, HttpMethod.Post, "/api/ops/producao/apontamentos", request, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopSiteMidia>>> GetSiteMidiasAsync(
        TerminalProfile profile, string token, string? tipo, CancellationToken cancellationToken = default)
    {
        var query = string.IsNullOrWhiteSpace(tipo) ? string.Empty : $"?tipo={Uri.EscapeDataString(tipo.Trim())}";
        return SendAuthorizedAsync<List<DesktopSiteMidia>>(
            profile, token, HttpMethod.Get, $"/api/site/midias{query}", null, cancellationToken);
    }

    public Task<DesktopApiDataResult<DesktopFiscalManualEmissaoResponse>> PrepareManualNfeAsync(
        TerminalProfile profile, string token, DesktopFiscalManualEmissaoRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopFiscalManualEmissaoResponse>(
            profile, token, HttpMethod.Post, "/api/fiscal/preparar-emissao-manual", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopFiscalDraftSaveResult>> SaveManualNfeDraftAsync(
        TerminalProfile profile, string token, DesktopFiscalManualEmissaoRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopFiscalDraftSaveResult>(
            profile, token, HttpMethod.Post, "/api/fiscal/rascunho-manual", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopFiscalDraftResult>> GetManualNfeDraftAsync(
        TerminalProfile profile, string token, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopFiscalDraftResult>(
            profile, token, HttpMethod.Get, "/api/fiscal/rascunho-manual", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopSiteMidia>> UploadSiteMidiaAsync(
        TerminalProfile profile, string token, DesktopSiteMidiaUploadRequest request, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopSiteMidia>(
            profile, token, HttpMethod.Post, "/api/site/midias", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopSiteMidia>> UpdateSiteMidiaAsync(
        TerminalProfile profile, string token, Guid midiaId, DesktopSiteMidiaUpdateRequest request, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopSiteMidia>(
            profile, token, HttpMethod.Put, $"/api/site/midias/{midiaId:D}", request, cancellationToken);

    public Task<DesktopApiDataResult<bool>> DeleteSiteMidiaAsync(
        TerminalProfile profile, string token, Guid midiaId, string? rowVersion, CancellationToken cancellationToken = default)
    {
        var path = $"/api/site/midias/{midiaId:D}?rowVersion={Uri.EscapeDataString(rowVersion ?? string.Empty)}";
        return SendAuthorizedAsync<bool>(profile, token, HttpMethod.Delete, path, null, cancellationToken);
    }

    public Task<DesktopApiDataResult<List<DesktopSitePublicProfile>>> GetSitePublicProfilesAsync(
        TerminalProfile profile, string token, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopSitePublicProfile>>(
            profile, token, HttpMethod.Get, "/api/site/perfis-publicos", null, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopConciliacaoFinanceira>>> GetConciliacoesFinanceirasAsync(
        TerminalProfile profile, string token, DateTime? inicio, DateTime? fim, string? status,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (inicio.HasValue)
            query.Add($"inicio={Uri.EscapeDataString(inicio.Value.ToString("O"))}");
        if (fim.HasValue)
            query.Add($"fim={Uri.EscapeDataString(fim.Value.ToString("O"))}");
        if (!string.IsNullOrWhiteSpace(status))
            query.Add($"status={Uri.EscapeDataString(status.Trim())}");
        var path = "/api/financeiro/conciliacao" + (query.Count == 0 ? string.Empty : "?" + string.Join("&", query));
        return SendAuthorizedAsync<List<DesktopConciliacaoFinanceira>>(
            profile, token, HttpMethod.Get, path, null, cancellationToken);
    }

    public Task<DesktopApiDataResult<DesktopConciliacaoFinanceira>> SaveConciliacaoFinanceiraAsync(
        TerminalProfile profile, string token, DesktopConciliacaoFinanceiraRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopConciliacaoFinanceira>(
            profile, token, HttpMethod.Post, "/api/financeiro/conciliacao", request, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopFechamentoContabil>>> GetFechamentosContabeisAsync(
        TerminalProfile profile, string token, int? empresaId, DateTime? periodo,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (empresaId.HasValue)
            query.Add($"empresaId={empresaId.Value}");
        if (periodo.HasValue)
            query.Add($"periodo={Uri.EscapeDataString(periodo.Value.ToString("O"))}");
        var path = "/api/financeiro/fechamento" + (query.Count == 0 ? string.Empty : "?" + string.Join("&", query));
        return SendAuthorizedAsync<List<DesktopFechamentoContabil>>(
            profile, token, HttpMethod.Get, path, null, cancellationToken);
    }

    public Task<DesktopApiDataResult<List<DesktopCrmSegmento>>> GetMarketingSegmentsAsync(
        TerminalProfile profile, string token, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopCrmSegmento>>(
            profile, token, HttpMethod.Get, "/api/crm/segmentos", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopCrmSegmento>> SaveMarketingSegmentAsync(
        TerminalProfile profile, string token, DesktopCrmSegmentoRequest request, Guid? segmentId,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopCrmSegmento>(
            profile, token,
            segmentId.HasValue ? HttpMethod.Put : HttpMethod.Post,
            segmentId.HasValue ? $"/api/crm/segmentos/{segmentId.Value:D}" : "/api/crm/segmentos",
            request, cancellationToken);

    public Task<DesktopApiDataResult<bool>> DeleteMarketingSegmentAsync(
        TerminalProfile profile, string token, Guid segmentId, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<bool>(profile, token, HttpMethod.Delete, $"/api/crm/segmentos/{segmentId:D}", null, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopCrmCampanha>>> GetMarketingCampaignsAsync(
        TerminalProfile profile, string token, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopCrmCampanha>>(
            profile, token, HttpMethod.Get, "/api/crm/campanhas", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopCrmCampanha>> SaveMarketingCampaignAsync(
        TerminalProfile profile, string token, DesktopCrmCampanhaRequest request, Guid? campaignId,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopCrmCampanha>(
            profile, token,
            campaignId.HasValue ? HttpMethod.Put : HttpMethod.Post,
            campaignId.HasValue ? $"/api/crm/campanhas/{campaignId.Value:D}" : "/api/crm/campanhas",
            request, cancellationToken);

    public Task<DesktopApiDataResult<bool>> DeleteMarketingCampaignAsync(
        TerminalProfile profile, string token, Guid campaignId, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<bool>(profile, token, HttpMethod.Delete, $"/api/crm/campanhas/{campaignId:D}", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopFechamentoContabil>> SaveFechamentoContabilAsync(
        TerminalProfile profile, string token, DesktopFechamentoContabilRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopFechamentoContabil>(
            profile, token, HttpMethod.Post, "/api/financeiro/fechamento", request, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopSiteProfileOrigin>>> GetSiteProfileOriginsAsync(
        TerminalProfile profile, string token, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopSiteProfileOrigin>>(
            profile, token, HttpMethod.Get, "/api/site/perfis-publicos/origens", null, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopSiteProfileProduct>>> GetSiteProfileProductsAsync(
        TerminalProfile profile, string token, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopSiteProfileProduct>>(
            profile, token, HttpMethod.Get, "/api/site/perfis-publicos/produtos-disponiveis", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopSitePublicProfile>> SaveSitePublicProfileAsync(
        TerminalProfile profile, string token, Guid? profileId, DesktopSitePublicProfileRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopSitePublicProfile>(
            profile, token,
            profileId.HasValue ? HttpMethod.Put : HttpMethod.Post,
            profileId.HasValue ? $"/api/site/perfis-publicos/{profileId.Value:D}" : "/api/site/perfis-publicos",
            request, cancellationToken);

    public Task<DesktopApiDataResult<bool>> DeleteSitePublicProfileAsync(
        TerminalProfile profile, string token, Guid profileId, string? rowVersion,
        CancellationToken cancellationToken = default)
    {
        var path = $"/api/site/perfis-publicos/{profileId:D}?rowVersion={Uri.EscapeDataString(rowVersion ?? string.Empty)}";
        return SendAuthorizedAsync<bool>(profile, token, HttpMethod.Delete, path, null, cancellationToken);
    }

    public Task<DesktopApiDataResult<JsonElement>> GetOperationalJsonAsync(
        TerminalProfile profile,
        string token,
        string path,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path) || !path.StartsWith("/api/", StringComparison.Ordinal))
        {
            return Task.FromResult(new DesktopApiDataResult<JsonElement>(
                false,
                default,
                "O caminho da consulta operacional precisa ser uma rota /api/ oficial."));
        }

        return SendAuthorizedAsync<JsonElement>(
            profile, token, HttpMethod.Get, path, null, cancellationToken);
    }

    public Task<DesktopApiDataResult<List<DesktopTenant>>> GetTenantsAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopTenant>>(
            profile, token, HttpMethod.Get, "/api/tenants", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopTenant>> SaveTenantAsync(
        TerminalProfile profile,
        string token,
        Guid? id,
        DesktopTenantRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopTenant>(
            profile,
            token,
            id.HasValue ? HttpMethod.Put : HttpMethod.Post,
            id.HasValue ? $"/api/tenants/{id.Value:D}" : "/api/tenants",
            request,
            cancellationToken);

    public Task<DesktopApiDataResult<bool>> DeleteTenantAsync(
        TerminalProfile profile,
        string token,
        Guid id,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<bool>(
            profile, token, HttpMethod.Delete, $"/api/tenants/{id:D}", null, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopWorkflowDefinition>>> GetWorkflowDefinitionsAsync(
        TerminalProfile profile,
        string token,
        string? entidade = null,
        CancellationToken cancellationToken = default)
    {
        var path = string.IsNullOrWhiteSpace(entidade)
            ? "/api/workflows/definicoes"
            : $"/api/workflows/definicoes?entidade={Uri.EscapeDataString(entidade.Trim())}";
        return SendAuthorizedAsync<List<DesktopWorkflowDefinition>>(
            profile, token, HttpMethod.Get, path, null, cancellationToken);
    }

    public Task<DesktopApiDataResult<DesktopWorkflowDefinition>> SaveWorkflowDefinitionAsync(
        TerminalProfile profile,
        string token,
        Guid? id,
        DesktopWorkflowDefinitionRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopWorkflowDefinition>(
            profile,
            token,
            id.HasValue ? HttpMethod.Put : HttpMethod.Post,
            id.HasValue ? $"/api/workflows/definicoes/{id.Value:D}" : "/api/workflows/definicoes",
            request,
            cancellationToken);

    public Task<DesktopApiDataResult<bool>> DeleteWorkflowDefinitionAsync(
        TerminalProfile profile,
        string token,
        Guid id,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<bool>(
            profile, token, HttpMethod.Delete, $"/api/workflows/definicoes/{id:D}", null, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopWorkflowInstance>>> GetWorkflowInstancesAsync(
        TerminalProfile profile,
        string token,
        string? entidade = null,
        string? estado = null,
        string? registroChave = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(entidade)) query.Add($"entidade={Uri.EscapeDataString(entidade.Trim())}");
        if (!string.IsNullOrWhiteSpace(estado)) query.Add($"estado={Uri.EscapeDataString(estado.Trim())}");
        if (!string.IsNullOrWhiteSpace(registroChave)) query.Add($"registroChave={Uri.EscapeDataString(registroChave.Trim())}");

        var path = query.Count == 0
            ? "/api/workflows/instancias"
            : $"/api/workflows/instancias?{string.Join("&", query)}";

        return SendAuthorizedAsync<List<DesktopWorkflowInstance>>(
            profile, token, HttpMethod.Get, path, null, cancellationToken);
    }

    public Task<DesktopApiDataResult<DesktopWorkflowInstance>> CreateWorkflowInstanceAsync(
        TerminalProfile profile,
        string token,
        DesktopWorkflowInstanceRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopWorkflowInstance>(
            profile, token, HttpMethod.Post, "/api/workflows/instancias", request, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopWorkflowTransition>>> GetWorkflowTransitionsAsync(
        TerminalProfile profile,
        string token,
        Guid instanceId,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopWorkflowTransition>>(
            profile, token, HttpMethod.Get, $"/api/workflows/instancias/{instanceId:D}/transicoes", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopWorkflowTransition>> TransitionWorkflowInstanceAsync(
        TerminalProfile profile,
        string token,
        Guid instanceId,
        DesktopWorkflowTransitionRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopWorkflowTransition>(
            profile, token, HttpMethod.Post, $"/api/workflows/instancias/{instanceId:D}/transicoes", request, cancellationToken);

    public Task<DesktopApiDataResult<bool>> DeleteWorkflowInstanceAsync(
        TerminalProfile profile,
        string token,
        Guid instanceId,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<bool>(
            profile, token, HttpMethod.Delete, $"/api/workflows/instancias/{instanceId:D}", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopComprasPainel>> CreateCompraSolicitacaoAsync(
        TerminalProfile profile,
        string token,
        DesktopCompraSolicitacaoRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopComprasPainel>(
            profile, token, HttpMethod.Post, "/api/compras/solicitacoes", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopComprasPainel>> CreateCompraCotacaoAsync(
        TerminalProfile profile,
        string token,
        DesktopCompraCotacaoRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopComprasPainel>(
            profile, token, HttpMethod.Post, "/api/compras/cotacoes", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopComprasPainel>> CreateCompraPedidoAsync(
        TerminalProfile profile,
        string token,
        DesktopCompraPedidoRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopComprasPainel>(
            profile, token, HttpMethod.Post, "/api/compras/pedidos", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopComprasPainel>> CreateCompraEntradaAsync(
        TerminalProfile profile,
        string token,
        int pedidoId,
        DesktopCompraEntradaRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopComprasPainel>(
            profile, token, HttpMethod.Post, $"/api/compras/pedidos/{pedidoId}/entradas", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopComprasPainel>> UpdateCompraSolicitacaoStatusAsync(
        TerminalProfile profile,
        string token,
        int solicitacaoId,
        DesktopCompraStatusRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopComprasPainel>(
            profile, token, HttpMethod.Patch, $"/api/compras/solicitacoes/{solicitacaoId}/status", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopComprasPainel>> UpdateCompraPedidoStatusAsync(
        TerminalProfile profile,
        string token,
        int pedidoId,
        DesktopCompraStatusRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopComprasPainel>(
            profile, token, HttpMethod.Patch, $"/api/compras/pedidos/{pedidoId}/status", request, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopFornecedor>>> GetFornecedoresAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopFornecedor>>(
            profile, token, HttpMethod.Get, "/api/fornecedores", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopFornecedor>> GetFornecedorAsync(
        TerminalProfile profile,
        string token,
        int fornecedorId,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopFornecedor>(
            profile, token, HttpMethod.Get, $"/api/fornecedores/{fornecedorId}", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopFornecedor>> CreateFornecedorAsync(
        TerminalProfile profile,
        string token,
        DesktopFornecedorRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopFornecedor>(
            profile, token, HttpMethod.Post, "/api/fornecedores", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopFornecedor>> UpdateFornecedorAsync(
        TerminalProfile profile,
        string token,
        int fornecedorId,
        DesktopFornecedorRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopFornecedor>(
            profile, token, HttpMethod.Put, $"/api/fornecedores/{fornecedorId}", request, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopCategoria>>> GetCategoriasAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopCategoria>>(
            profile, token, HttpMethod.Get, "/api/categorias", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopCategoria>> CreateCategoriaAsync(
        TerminalProfile profile,
        string token,
        DesktopCategoriaRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopCategoria>(
            profile, token, HttpMethod.Post, "/api/categorias", request, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopCentroCusto>>> GetCentrosCustoAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopCentroCusto>>(
            profile, token, HttpMethod.Get, "/api/centros-custo", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopCentroCusto>> CreateCentroCustoAsync(
        TerminalProfile profile,
        string token,
        DesktopCentroCustoRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopCentroCusto>(
            profile, token, HttpMethod.Post, "/api/centros-custo", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopCentroCusto>> UpdateCentroCustoAsync(
        TerminalProfile profile,
        string token,
        int id,
        DesktopCentroCustoRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopCentroCusto>(
            profile, token, HttpMethod.Put, $"/api/centros-custo/{id}", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopCentroCusto>> DeleteCentroCustoAsync(
        TerminalProfile profile,
        string token,
        int id,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopCentroCusto>(
            profile, token, HttpMethod.Delete, $"/api/centros-custo/{id}", null, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopItemServico>>> GetItensServicoAsync(
        TerminalProfile profile,
        string token,
        string? termo = null,
        string? tipo = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(termo)) query.Add($"termo={Uri.EscapeDataString(termo.Trim())}");
        if (!string.IsNullOrWhiteSpace(tipo)) query.Add($"tipo={Uri.EscapeDataString(tipo.Trim())}");

        var path = query.Count == 0
            ? "/api/itens-servico"
            : $"/api/itens-servico?{string.Join('&', query)}";

        return SendAuthorizedAsync<List<DesktopItemServico>>(
            profile, token, HttpMethod.Get, path, null, cancellationToken);
    }

    public Task<DesktopApiDataResult<DesktopItemServico>> SaveItemServicoAsync(
        TerminalProfile profile,
        string token,
        int? id,
        DesktopItemServicoRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopItemServico>(
            profile,
            token,
            id.HasValue ? HttpMethod.Put : HttpMethod.Post,
            id.HasValue ? $"/api/itens-servico/{id.Value}" : "/api/itens-servico",
            request,
            cancellationToken);

    public Task<DesktopApiDataResult<DesktopItemServico>> DeleteItemServicoAsync(
        TerminalProfile profile,
        string token,
        int id,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopItemServico>(
            profile, token, HttpMethod.Delete, $"/api/itens-servico/{id}", null, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopDropshippingCanal>>> GetDropshippingChannelsAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopDropshippingCanal>>(
            profile, token, HttpMethod.Get, "/api/dropshipping/canais", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopDropshippingCanal>> SaveDropshippingChannelAsync(
        TerminalProfile profile,
        string token,
        DesktopDropshippingCanalRequest request,
        int? id = null,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopDropshippingCanal>(
            profile,
            token,
            id.HasValue ? HttpMethod.Put : HttpMethod.Post,
            id.HasValue ? $"/api/dropshipping/canais/{id.Value}" : "/api/dropshipping/canais",
            request,
            cancellationToken);

    public Task<DesktopApiDataResult<JsonElement>> DeleteDropshippingChannelAsync(
        TerminalProfile profile,
        string token,
        int id,
        string rowVersion,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<JsonElement>(
            profile,
            token,
            HttpMethod.Delete,
            $"/api/dropshipping/canais/{id}?rowVersion={Uri.EscapeDataString(rowVersion)}",
            null,
            cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopDreGerencial>>> GetDreAsync(
        TerminalProfile profile,
        string token,
        int? empresaId,
        string? competencia,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (empresaId.HasValue) query.Add($"empresaId={empresaId.Value}");
        if (!string.IsNullOrWhiteSpace(competencia)) query.Add($"competencia={Uri.EscapeDataString(competencia.Trim())}");
        var path = query.Count == 0 ? "/api/financeiro/dre" : $"/api/financeiro/dre?{string.Join('&', query)}";
        return SendAuthorizedAsync<List<DesktopDreGerencial>>(
            profile, token, HttpMethod.Get, path, null, cancellationToken);
    }

    public Task<DesktopApiDataResult<List<DesktopContabilLancamento>>> GetContabilLancamentosAsync(
        TerminalProfile profile,
        string token,
        int? empresaId,
        DateTime? inicio,
        DateTime? fim,
        string? lote,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (empresaId.HasValue) query.Add($"empresaId={empresaId.Value}");
        if (inicio.HasValue) query.Add($"inicio={Uri.EscapeDataString(inicio.Value.ToString("O"))}");
        if (fim.HasValue) query.Add($"fim={Uri.EscapeDataString(fim.Value.ToString("O"))}");
        if (!string.IsNullOrWhiteSpace(lote)) query.Add($"lote={Uri.EscapeDataString(lote.Trim())}");
        var path = query.Count == 0
            ? "/api/financeiro/contabil/lancamentos"
            : $"/api/financeiro/contabil/lancamentos?{string.Join('&', query)}";
        return SendAuthorizedAsync<List<DesktopContabilLancamento>>(
            profile, token, HttpMethod.Get, path, null, cancellationToken);
    }

    public Task<DesktopApiDataResult<DesktopContabilLancamento>> CreateContabilLancamentoAsync(
        TerminalProfile profile,
        string token,
        DesktopContabilLancamentoRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopContabilLancamento>(
            profile, token, HttpMethod.Post, "/api/financeiro/contabil/lancamentos", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopFiscalRoutingSimulation>> SimulateFiscalRoutingAsync(
        TerminalProfile profile,
        string token,
        DesktopFiscalRoutingSimulationRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopFiscalRoutingSimulation>(
            profile, token, HttpMethod.Post, "/api/fiscal/simular-roteamento", request, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopLogisticaExpedicao>>> GetLogisticaExpedicoesAsync(
        TerminalProfile profile,
        string token,
        string? status = null,
        int? limite = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(status)) query.Add($"status={Uri.EscapeDataString(status.Trim())}");
        if (limite.HasValue) query.Add($"limite={limite.Value}");
        var path = query.Count == 0 ? "/api/logistica/expedicoes" : $"/api/logistica/expedicoes?{string.Join('&', query)}";
        return SendAuthorizedAsync<List<DesktopLogisticaExpedicao>>(
            profile, token, HttpMethod.Get, path, null, cancellationToken);
    }

    public Task<DesktopApiDataResult<DesktopLogisticaRastreamento>> GetLogisticaRastreamentoAsync(
        TerminalProfile profile,
        string token,
        string codigo,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopLogisticaRastreamento>(
            profile,
            token,
            HttpMethod.Get,
            $"/api/logistica/rastreamento/{Uri.EscapeDataString(codigo.Trim())}",
            null,
            cancellationToken);

    public Task<DesktopApiDataResult<DesktopPedidoLoja>> UpdatePedidoLogisticaAsync(
        TerminalProfile profile,
        string token,
        int pedidoId,
        DesktopPedidoLogisticaRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopPedidoLoja>(
            profile, token, HttpMethod.Put, $"/api/pedidos/{pedidoId}/logistica", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopPedidoLoja>> AdvancePedidoFluxoAsync(
        TerminalProfile profile,
        string token,
        int pedidoId,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopPedidoLoja>(
            profile, token, HttpMethod.Post, $"/api/pedidos/{pedidoId}/fluxo-operacional", null, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopLead>>> GetLeadsAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopLead>>(
            profile, token, HttpMethod.Get, "/api/crm/leads", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopLead>> UpdateLeadStatusAsync(
        TerminalProfile profile,
        string token,
        int leadId,
        DesktopLeadStatusUpdateRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopLead>(
            profile, token, HttpMethod.Put, $"/api/crm/leads/{leadId}/status", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopOpenAiAssistentesStatus>> GetOpenAiAssistantsStatusAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopOpenAiAssistentesStatus>(
            profile, token, HttpMethod.Get, "/api/admin/integracoes/openai", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopOpenAiAssistentesStatus>> SaveOpenAiAssistantsAsync(
        TerminalProfile profile,
        string token,
        DesktopOpenAiAssistentesConfiguracaoRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopOpenAiAssistentesStatus>(
            profile, token, HttpMethod.Put, "/api/admin/integracoes/openai", request, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopCupomAdmin>>> GetCuponsAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopCupomAdmin>>(
            profile, token, HttpMethod.Get, "/api/admin/cupons", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopCupomAdmin>> SaveCupomAsync(
        TerminalProfile profile,
        string token,
        int? id,
        DesktopCupomRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopCupomAdmin>(
            profile,
            token,
            id.HasValue ? HttpMethod.Put : HttpMethod.Post,
            id.HasValue ? $"/api/admin/cupons/{id.Value}" : "/api/admin/cupons",
            request,
            cancellationToken);

    public Task<DesktopApiDataResult<DesktopCupomAdmin>> DeactivateCupomAsync(
        TerminalProfile profile,
        string token,
        int id,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopCupomAdmin>(
            profile, token, HttpMethod.Delete, $"/api/admin/cupons/{id}", null, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopUsuarioAcesso>>> GetAccessUsersAsync(
        TerminalProfile profile, string token, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopUsuarioAcesso>>(
            profile, token, HttpMethod.Get, "/api/admin/usuarios", null, cancellationToken);

    public Task<DesktopApiDataResult<List<string>>> GetAdministrativeRolesAsync(
        TerminalProfile profile, string token, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<string>>(
            profile, token, HttpMethod.Get, "/api/admin/usuarios/perfis", null, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopPerfilAcesso>>> GetAccessProfilesAsync(
        TerminalProfile profile, string token, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopPerfilAcesso>>(
            profile, token, HttpMethod.Get, "/api/perfis", null, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopPermissaoAcesso>>> GetPermissionsAsync(
        TerminalProfile profile, string token, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopPermissaoAcesso>>(
            profile, token, HttpMethod.Get, "/api/permissoes", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopUsuarioAcesso>> SaveAccessUserAsync(
        TerminalProfile profile, string token, DesktopUsuarioAcessoRequest request, int? id,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopUsuarioAcesso>(profile, token,
            id.HasValue ? HttpMethod.Put : HttpMethod.Post,
            id.HasValue ? $"/api/admin/usuarios/{id.Value}" : "/api/admin/usuarios",
            request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopPerfilAcesso>> SaveAccessProfileAsync(
        TerminalProfile profile, string token, DesktopPerfilAcessoRequest request, int? id,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopPerfilAcesso>(profile, token,
            id.HasValue ? HttpMethod.Put : HttpMethod.Post,
            id.HasValue ? $"/api/perfis/{id.Value}" : "/api/perfis",
            request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopPerfilAcesso>> DeactivateAccessProfileAsync(
        TerminalProfile profile, string token, int id, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopPerfilAcesso>(
            profile, token, HttpMethod.Delete, $"/api/perfis/{id}", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopPermissaoAcesso>> SavePermissionAsync(
        TerminalProfile profile, string token, DesktopPermissaoAcessoRequest request, int? id,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopPermissaoAcesso>(profile, token,
            id.HasValue ? HttpMethod.Put : HttpMethod.Post,
            id.HasValue ? $"/api/permissoes/{id.Value}" : "/api/permissoes",
            request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopPermissaoAcesso>> DeactivatePermissionAsync(
        TerminalProfile profile, string token, int id, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopPermissaoAcesso>(
            profile, token, HttpMethod.Delete, $"/api/permissoes/{id}", null, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopPerfilPermissao>>> GetProfilePermissionsAsync(
        TerminalProfile profile, string token, int profileId, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopPerfilPermissao>>(
            profile, token, HttpMethod.Get, $"/api/perfis/{profileId}/permissoes", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopPerfilPermissao>> SaveProfilePermissionAsync(
        TerminalProfile profile, string token, int profileId, DesktopPerfilPermissaoRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopPerfilPermissao>(
            profile, token, HttpMethod.Post, $"/api/perfis/{profileId}/permissoes", request, cancellationToken);

    public Task<DesktopApiDataResult<object>> RemoveProfilePermissionAsync(
        TerminalProfile profile, string token, int profileId, int permissionId,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<object>(profile, token, HttpMethod.Delete,
            $"/api/perfis/{profileId}/permissoes/{permissionId}", null, cancellationToken);

    // ============================================================
    // MÉTODOS PRIVADOS DE INFRAESTRUTURA E ROTEAMENTO (Failover)
    // ============================================================

    private async Task<DesktopFileDownloadResult> SendAuthorizedFileAsync(
        TerminalProfile profile,
        string token,
        string path,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
            return new DesktopFileDownloadResult(false, null, null, "Token de autenticação ausente ou inválido.");

        foreach (var endpoint in GetEndpoints(profile))
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"{endpoint}{path}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using var response = await Http.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    if ((int)response.StatusCode is 401 or 403)
                        return new DesktopFileDownloadResult(false, null, null, "Sessão expirada ou acesso não autorizado (401/403).");
                    if ((int)response.StatusCode >= 500)
                        continue;
                    return new DesktopFileDownloadResult(false, null, null, ExtractErrorMessage(errorContent, response.ReasonPhrase));
                }

                var mediaType = response.Content.Headers.ContentType?.MediaType;
                var disposition = response.Content.Headers.ContentDisposition;
                var fileName = disposition?.FileNameStar ?? disposition?.FileName;
                fileName = fileName?.Trim().Trim('"');
                if (!string.Equals(mediaType, "application/pdf", StringComparison.OrdinalIgnoreCase))
                    return new DesktopFileDownloadResult(false, null, null, $"A API retornou o tipo de conteúdo inesperado: {mediaType ?? "ausente"}.");
                if (string.IsNullOrWhiteSpace(fileName))
                    return new DesktopFileDownloadResult(false, null, null, "A API não informou o nome do arquivo PDF.");

                var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                if (bytes.Length == 0)
                    return new DesktopFileDownloadResult(false, null, null, "A API retornou um relatório PDF vazio.");

                return new DesktopFileDownloadResult(true, bytes, fileName, "Relatório PDF recebido da API oficial.");
            }
            catch (HttpRequestException)
            {
                continue;
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                continue;
            }
        }

        return new DesktopFileDownloadResult(false, null, null, "Nenhum servidor de API disponível para gerar o relatório.");
    }

    private async Task<DesktopApiDataResult<T>> SendAuthorizedAsync<T>(
        TerminalProfile profile,
        string token,
        HttpMethod method,
        string path,
        object? payload,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return new DesktopApiDataResult<T>(false, default, "Token de autenticação ausente ou inválido.");
        }

        using var baseRequest = new HttpRequestMessage(method, string.Empty);
        baseRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (payload != null)
        {
            baseRequest.Content = JsonContent.Create(payload, options: JsonOptions);
        }

        foreach (var endpoint in GetEndpoints(profile))
        {
            try
            {
                var requestClone = await CloneRequestAsync(baseRequest, $"{endpoint}{path}");
                using var response = await Http.SendAsync(requestClone, cancellationToken);
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode is 401 or 403)
                    {
                        return new DesktopApiDataResult<T>(false, default, "Sessão expirada ou acesso não autorizado (401/403).");
                    }
                    if ((int)response.StatusCode >= 500)
                    {
                        continue; // Tenta o endpoint secundário (público) se o principal (local) falhar com erro de servidor.
                    }
                    return new DesktopApiDataResult<T>(false, default, ExtractErrorMessage(content, response.ReasonPhrase));
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    return new DesktopApiDataResult<T>(true, default, "Operação confirmada pela API sem conteúdo de resposta.");
                }

                var envelope = JsonSerializer.Deserialize<DesktopApiEnvelope<T>>(content, JsonOptions);
                if (envelope == null || !envelope.Sucesso)
                {
                    return new DesktopApiDataResult<T>(false, default, envelope?.Mensagem ?? "Erro no processamento da requisição pela API.");
                }

                return new DesktopApiDataResult<T>(
                    true,
                    envelope.Dados,
                    envelope.Mensagem,
                    envelope.TotalRegistros,
                    envelope.PaginaAtual,
                    envelope.TotalPaginas);
            }
            catch (HttpRequestException) { continue; }
            catch (TaskCanceledException) { continue; }
        }

        return new DesktopApiDataResult<T>(false, default, "Nenhum servidor de API disponível no momento. Transação cancelada.");
    }

    private static IEnumerable<string> GetEndpoints(TerminalProfile profile)
    {
        if (!string.IsNullOrWhiteSpace(profile.ApiBaseUrl))
            yield return profile.ApiBaseUrl.TrimEnd('/');

        if (!string.IsNullOrWhiteSpace(profile.PublicApiUrl))
            yield return profile.PublicApiUrl.TrimEnd('/');
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage req, string newUri)
    {
        var clone = new HttpRequestMessage(req.Method, newUri);

        if (req.Content != null)
        {
            var bytes = await req.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(bytes);

            if (req.Content.Headers != null)
            {
                foreach (var header in req.Content.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        clone.Headers.Authorization = req.Headers.Authorization;
        return clone;
    }

    private static async Task<bool> PingEndpointAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await Http.GetAsync(url, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static string ExtractErrorMessage(string content, string? defaultPhrase)
    {
        try
        {
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("mensagem", out var msgProp) && msgProp.ValueKind == JsonValueKind.String)
            {
                return msgProp.GetString() ?? defaultPhrase ?? "Erro desconhecido";
            }
            if (doc.RootElement.TryGetProperty("title", out var titleProp) && titleProp.ValueKind == JsonValueKind.String)
            {
                return titleProp.GetString() ?? defaultPhrase ?? "Erro de validação";
            }
        }
        catch
        {
            // Ignora falha no parse do JSON e usa o fallback
        }
        return defaultPhrase ?? "Erro na comunicação com a API.";
    }
}
