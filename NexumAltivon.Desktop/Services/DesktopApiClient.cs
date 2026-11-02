/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7195
 */

using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
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
            return new DesktopLoginResult(false, null, null, "E-mail e senha sao obrigatorios.");
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
                    return new DesktopLoginResult(false, null, null, envelope?.Mensagem ?? "A API nao confirmou uma sessao JWT valida.");
                }

                return new DesktopLoginResult(true, login.Token, login.Usuario, $"Sessao autenticada em {endpoint}.");
            }
            catch (HttpRequestException)
            {
                continue;
            }
            catch (TaskCanceledException)
            {
                continue;
            }
        }

        return new DesktopLoginResult(false, null, null, "API local e publica indisponiveis para autenticacao.");
    }

    public Task<DesktopApiDataResult<DesktopComprasPainel>> GetComprasPainelAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopComprasPainel>(
            profile, token, HttpMethod.Get, "/api/compras/painel", null, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopFiscalEmpresa>>> GetFiscalCompaniesAsync(
        TerminalProfile profile, string token, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopFiscalEmpresa>>(
            profile, token, HttpMethod.Get, "/api/erp/empresas", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopFiscalRoutingResult>> SimulateFiscalRoutingAsync(
        TerminalProfile profile, string token, DesktopFiscalRoutingRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopFiscalRoutingResult>(
            profile, token, HttpMethod.Post, "/api/fiscal/simular-roteamento", request, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopPedidoConsulta>>> GetOrdersAsync(
        TerminalProfile profile, string token, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopPedidoConsulta>>(
            profile, token, HttpMethod.Get, "/api/pedidos", null, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopEmpresaLojaVinculo>>> GetEmpresasGrupoLojasAsync(
        TerminalProfile profile, string token, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopEmpresaLojaVinculo>>(
            profile, token, HttpMethod.Get, "/api/erp/empresas/lojas", null, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopCategoriaAdmin>>> GetAdminCategoriesAsync(
        TerminalProfile profile, string token, string? search, CancellationToken cancellationToken = default)
    {
        var query = string.IsNullOrWhiteSpace(search) ? string.Empty : $"?termo={Uri.EscapeDataString(search.Trim())}";
        return SendAuthorizedAsync<List<DesktopCategoriaAdmin>>(
            profile, token, HttpMethod.Get, $"/api/admin/categorias{query}", null, cancellationToken);
    }

    public Task<DesktopApiDataResult<DesktopCategoriaAdmin>> CreateAdminCategoryAsync(
        TerminalProfile profile, string token, DesktopCategoriaCreateRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopCategoriaAdmin>(
            profile, token, HttpMethod.Post, "/api/admin/categorias", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopCategoriaAdmin>> UpdateAdminCategoryAsync(
        TerminalProfile profile, string token, int id, DesktopCategoriaUpdateRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopCategoriaAdmin>(
            profile, token, HttpMethod.Put, $"/api/admin/categorias/{id}", request, cancellationToken);

    public Task<DesktopApiDataResult<int>> CreateEmpresaGrupoDraftAsync(
        TerminalProfile profile, string token, int lojaId, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<int>(
            profile, token, HttpMethod.Post, $"/api/erp/empresas/lojas/{lojaId}/rascunho", new { }, cancellationToken);

    public Task<DesktopApiDataResult<string>> UpdateEmpresaGrupoLegalDataAsync(
        TerminalProfile profile, string token, int empresaId, DesktopEmpresaDadosLegaisRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<string>(
            profile, token, HttpMethod.Put, $"/api/erp/empresas/{empresaId}/dados-legais", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopProdutoIdentificadorPagina>> GetProductIdentifiersAsync(
        TerminalProfile profile,
        string token,
        string? search,
        int page,
        CancellationToken cancellationToken = default)
    {
        var query = $"?pagina={Math.Max(1, page)}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            query += $"&termo={Uri.EscapeDataString(search.Trim())}";
        }

        return SendAuthorizedAsync<DesktopProdutoIdentificadorPagina>(
            profile, token, HttpMethod.Get, $"/api/admin/produtos/identificadores{query}", null, cancellationToken);
    }

    public Task<DesktopApiDataResult<DesktopProdutoIdentificador>> UpdateProductIdentifiersAsync(
        TerminalProfile profile,
        string token,
        int productId,
        DesktopProdutoIdentificadorUpdateRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopProdutoIdentificador>(
            profile, token, HttpMethod.Put, $"/api/admin/produtos/{productId}/identificadores", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopProdutoAdminPagina>> GetAdminProductsAsync(
        TerminalProfile profile, string token, string? search, int page = 1,
        CancellationToken cancellationToken = default)
    {
        var query = $"?pagina={Math.Max(1, page)}";
        if (!string.IsNullOrWhiteSpace(search)) query += $"&termo={Uri.EscapeDataString(search.Trim())}";
        return SendAuthorizedAsync<DesktopProdutoAdminPagina>(
            profile, token, HttpMethod.Get, $"/api/admin/produtos{query}", null, cancellationToken);
    }

    public async Task<DesktopApiDataResult<List<DesktopProdutoLoja>>> GetPublicProductsAsync(
        TerminalProfile profile,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 60;
        var failures = new List<string>();

        foreach (var endpoint in GetEndpoints(profile))
        {
            var products = new List<DesktopProdutoLoja>();
            var endpointFailed = false;

            for (var page = 1; page <= 100; page++)
            {
                try
                {
                    var path = $"/api/produtos?pagina={page}&itensPorPagina={pageSize}";
                    using var response = await Http.GetAsync($"{endpoint}{path}", cancellationToken);
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    if (!response.IsSuccessStatusCode)
                    {
                        failures.Add($"{endpoint}: {(int)response.StatusCode} {ExtractErrorMessage(content, response.ReasonPhrase)}");
                        endpointFailed = true;
                        break;
                    }

                    var envelope = JsonSerializer.Deserialize<DesktopApiEnvelope<List<DesktopProdutoLoja>>>(content, JsonOptions);
                    if (envelope is null || !envelope.Sucesso || envelope.Dados is null)
                    {
                        failures.Add($"{endpoint}: {envelope?.Mensagem ?? "A API não confirmou o catálogo público."}");
                        endpointFailed = true;
                        break;
                    }

                    products.AddRange(envelope.Dados);
                    if (envelope.Dados.Count < pageSize)
                    {
                        return new DesktopApiDataResult<List<DesktopProdutoLoja>>(
                            true,
                            products,
                            $"Catálogo carregado da API com {products.Count} produtos.");
                    }
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
                {
                    failures.Add($"{endpoint}: {ex.Message}");
                    endpointFailed = true;
                    break;
                }
            }

            if (!endpointFailed)
            {
                failures.Add($"{endpoint}: o catálogo excedeu o limite seguro de 100 páginas.");
            }
        }

        return new DesktopApiDataResult<List<DesktopProdutoLoja>>(
            false,
            null,
            string.Join(" | ", failures));
    }

    public Task<DesktopApiDataResult<DesktopProdutoLoja>> UpdateProductAsync(
        TerminalProfile profile, string token, string slug, DesktopProdutoUpdateRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopProdutoLoja>(
            profile, token, HttpMethod.Put, $"/api/produtos/{Uri.EscapeDataString(slug)}", request, cancellationToken);

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

    public Task<DesktopApiDataResult<List<DesktopFornecedorContato>>> GetFornecedorContatosAsync(
        TerminalProfile profile,
        string token,
        int fornecedorId,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopFornecedorContato>>(
            profile, token, HttpMethod.Get, $"/api/fornecedores/{fornecedorId}/contatos", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopFornecedorContato>> CreateFornecedorContatoAsync(
        TerminalProfile profile,
        string token,
        int fornecedorId,
        DesktopFornecedorContatoRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopFornecedorContato>(
            profile, token, HttpMethod.Post, $"/api/fornecedores/{fornecedorId}/contatos", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopFornecedorContato>> UpdateFornecedorContatoAsync(
        TerminalProfile profile,
        string token,
        int fornecedorId,
        int contatoId,
        DesktopFornecedorContatoRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopFornecedorContato>(
            profile, token, HttpMethod.Put, $"/api/fornecedores/{fornecedorId}/contatos/{contatoId}", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopFornecedorContato>> DeactivateFornecedorContatoAsync(
        TerminalProfile profile,
        string token,
        int fornecedorId,
        int contatoId,
        string rowVersion,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopFornecedorContato>(
            profile,
            token,
            HttpMethod.Delete,
            $"/api/fornecedores/{fornecedorId}/contatos/{contatoId}?rowVersion={Uri.EscapeDataString(rowVersion)}",
            null,
            cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopCliente>>> GetClientesAsync(
        TerminalProfile profile,
        string token,
        string? busca = null,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopCliente>>(
            profile, token, HttpMethod.Get,
            string.IsNullOrWhiteSpace(busca) ? "/api/clientes" : $"/api/clientes?busca={Uri.EscapeDataString(busca.Trim())}",
            null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopCliente>> GetClienteAsync(
        TerminalProfile profile,
        string token,
        int clienteId,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopCliente>(
            profile, token, HttpMethod.Get, $"/api/clientes/{clienteId}", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopCliente>> CreateClienteAsync(
        TerminalProfile profile,
        string token,
        DesktopClienteRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopCliente>(
            profile, token, HttpMethod.Post, "/api/clientes/admin", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopCliente>> UpdateClienteAsync(
        TerminalProfile profile,
        string token,
        int clienteId,
        DesktopClienteRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopCliente>(
            profile, token, HttpMethod.Put, $"/api/clientes/{clienteId}", request, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopClienteEndereco>>> GetClienteEnderecosAsync(
        TerminalProfile profile,
        string token,
        int clienteId,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopClienteEndereco>>(
            profile, token, HttpMethod.Get, $"/api/clientes/{clienteId}/enderecos", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopClienteEndereco>> CreateClienteEnderecoAsync(
        TerminalProfile profile,
        string token,
        int clienteId,
        DesktopClienteEnderecoRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopClienteEndereco>(
            profile, token, HttpMethod.Post, $"/api/clientes/{clienteId}/enderecos", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopClienteEndereco>> UpdateClienteEnderecoAsync(
        TerminalProfile profile,
        string token,
        int clienteId,
        int enderecoId,
        DesktopClienteEnderecoRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopClienteEndereco>(
            profile, token, HttpMethod.Put, $"/api/clientes/{clienteId}/enderecos/{enderecoId}", request, cancellationToken);

    public Task<DesktopApiDataResult<bool>> ArchiveClienteEnderecoAsync(
        TerminalProfile profile,
        string token,
        int clienteId,
        int enderecoId,
        string rowVersion,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<bool>(
            profile,
            token,
            HttpMethod.Delete,
            $"/api/clientes/{clienteId}/enderecos/{enderecoId}?rowVersion={Uri.EscapeDataString(rowVersion)}",
            null,
            cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopLogisticaExpedicao>>> GetLogisticaExpedicoesAsync(
        TerminalProfile profile,
        string token,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var path = "/api/logistica/expedicoes?limite=200";
        if (!string.IsNullOrWhiteSpace(status))
        {
            path += $"&status={Uri.EscapeDataString(status.Trim())}";
        }

        return SendAuthorizedAsync<List<DesktopLogisticaExpedicao>>(
            profile, token, HttpMethod.Get, path, null, cancellationToken);
    }

    public Task<DesktopApiDataResult<DesktopPedidoLogisticaAtualizada>> UpdatePedidoLogisticaAsync(
        TerminalProfile profile,
        string token,
        int pedidoId,
        DesktopPedidoLogisticaRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopPedidoLogisticaAtualizada>(
            profile,
            token,
            HttpMethod.Put,
            $"/api/pedidos/{pedidoId}/logistica",
            request,
            cancellationToken);

    public Task<DesktopApiDataResult<DesktopPedidoLogisticaAtualizada>> AdvancePedidoFluxoAsync(
        TerminalProfile profile,
        string token,
        int pedidoId,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopPedidoLogisticaAtualizada>(
            profile,
            token,
            HttpMethod.Post,
            $"/api/pedidos/{pedidoId}/fluxo-operacional",
            null,
            cancellationToken);

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

    public Task<DesktopApiDataResult<DesktopFiscalManualEmissaoResponse>> PrepareManualNfeAsync(
        TerminalProfile profile,
        string token,
        DesktopFiscalManualEmissaoRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopFiscalManualEmissaoResponse>(
            profile,
            token,
            HttpMethod.Post,
            "/api/fiscal/preparar-emissao-manual",
            request,
            cancellationToken);

    public Task<DesktopApiDataResult<DesktopFiscalManualDraftSaved>> SaveManualNfeDraftAsync(
        TerminalProfile profile,
        string token,
        DesktopFiscalManualEmissaoRequest request,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopFiscalManualDraftSaved>(
            profile,
            token,
            HttpMethod.Post,
            "/api/fiscal/rascunho-manual",
            request,
            cancellationToken);

    public Task<DesktopApiDataResult<DesktopFiscalManualDraftRead>> GetManualNfeDraftAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopFiscalManualDraftRead>(
            profile,
            token,
            HttpMethod.Get,
            "/api/fiscal/rascunho-manual",
            null,
            cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopContaPagar>>> GetContasPagarAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<List<DesktopContaPagar>>(
            profile, token, HttpMethod.Get, "/api/erp/genesis/financeiro/contas-pagar", null, cancellationToken);

    public Task<DesktopFileDownloadResult> GetContasPagarPdfAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default)
        => DownloadAuthorizedPdfAsync(
            profile, token, "/api/erp/genesis/financeiro/contas-pagar/relatorio.pdf", cancellationToken);

    public Task<DesktopApiDataResult<DesktopContaPagar>> CreateContaPagarAsync(
        TerminalProfile profile,
        string token,
        DesktopContaPagarCreateRequest payload,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopContaPagar>(
            profile, token, HttpMethod.Post, "/api/erp/genesis/financeiro/contas-pagar", payload, cancellationToken);

    public Task<DesktopApiDataResult<DesktopContaPagar>> BaixarContaPagarAsync(
        TerminalProfile profile,
        string token,
        int id,
        DesktopBaixaPagarRequest payload,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopContaPagar>(
            profile, token, HttpMethod.Post, $"/api/erp/genesis/financeiro/contas-pagar/{id}/baixa", payload, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopContaReceber>>> GetContasReceberAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<List<DesktopContaReceber>>(
            profile, token, HttpMethod.Get, "/api/erp/genesis/financeiro/contas-receber", null, cancellationToken);

    public Task<DesktopFileDownloadResult> GetContasReceberPdfAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default)
        => DownloadAuthorizedPdfAsync(
            profile, token, "/api/erp/genesis/financeiro/contas-receber/relatorio.pdf", cancellationToken);

    public Task<DesktopApiDataResult<DesktopContaReceber>> CreateContaReceberAsync(
        TerminalProfile profile,
        string token,
        DesktopContaReceberCreateRequest payload,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopContaReceber>(
            profile, token, HttpMethod.Post, "/api/erp/genesis/financeiro/contas-receber", payload, cancellationToken);

    public Task<DesktopApiDataResult<DesktopContaReceber>> BaixarContaReceberAsync(
        TerminalProfile profile,
        string token,
        int id,
        DesktopBaixaReceberRequest payload,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopContaReceber>(
            profile, token, HttpMethod.Post, $"/api/erp/genesis/financeiro/contas-receber/{id}/baixa", payload, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopGenesisRhColaborador>>> GetGenesisRhColaboradoresAsync(
        TerminalProfile profile, string token, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopGenesisRhColaborador>>(
            profile, token, HttpMethod.Get, "/api/erp/genesis/rh/colaboradores", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopGenesisRhColaborador>> CreateGenesisRhColaboradorAsync(
        TerminalProfile profile, string token, DesktopGenesisRhColaboradorRequest request, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopGenesisRhColaborador>(
            profile, token, HttpMethod.Post, "/api/erp/genesis/rh/colaboradores", request, cancellationToken);

    public Task<DesktopApiDataResult<DesktopGenesisRhColaborador>> UpdateGenesisRhColaboradorAsync(
        TerminalProfile profile, string token, int id, DesktopGenesisRhColaboradorRequest request, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopGenesisRhColaborador>(
            profile, token, HttpMethod.Put, $"/api/erp/genesis/rh/colaboradores/{id}", request, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopGenesisRhReference>>> GetGenesisRhReferencesAsync(
        TerminalProfile profile, string token, string type, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopGenesisRhReference>>(
            profile, token, HttpMethod.Get, $"/api/erp/genesis/rh/referencias?tipo={Uri.EscapeDataString(type)}", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopGenesisRhReference>> CreateGenesisRhReferenceAsync(
        TerminalProfile profile, string token, DesktopGenesisRhReferenceRequest request, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopGenesisRhReference>(
            profile, token, HttpMethod.Post, "/api/erp/genesis/rh/referencias", request, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopGenesisFinanceReference>>> GetGenesisFinanceReferencesAsync(
        TerminalProfile profile, string token, string type, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<List<DesktopGenesisFinanceReference>>(
            profile, token, HttpMethod.Get, $"/api/erp/genesis/financeiro/referencias?tipo={Uri.EscapeDataString(type)}", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopGenesisFinanceReference>> CreateGenesisFinanceReferenceAsync(
        TerminalProfile profile, string token, DesktopGenesisFinanceReferenceRequest request, CancellationToken cancellationToken = default) =>
        SendAuthorizedAsync<DesktopGenesisFinanceReference>(
            profile, token, HttpMethod.Post, "/api/erp/genesis/financeiro/referencias", request, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopAuditoriaOperacional>>> GetAuditoriaAsync(
        TerminalProfile profile,
        string token,
        string tabela,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<List<DesktopAuditoriaOperacional>>(
            profile,
            token,
            HttpMethod.Get,
            $"/api/auditoria?tabela={Uri.EscapeDataString(tabela)}",
            null,
            cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopConciliacaoFinanceira>>> GetConciliacoesFinanceirasAsync(
        TerminalProfile profile,
        string token,
        DateTime? inicio,
        DateTime? fim,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (inicio.HasValue)
        {
            query.Add($"inicio={Uri.EscapeDataString(inicio.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))}");
        }

        if (fim.HasValue)
        {
            query.Add($"fim={Uri.EscapeDataString(fim.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))}");
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query.Add($"status={Uri.EscapeDataString(status.Trim())}");
        }

        var suffix = query.Count == 0 ? string.Empty : $"?{string.Join("&", query)}";
        return SendAuthorizedAsync<List<DesktopConciliacaoFinanceira>>(
            profile,
            token,
            HttpMethod.Get,
            $"/api/financeiro/conciliacao{suffix}",
            null,
            cancellationToken);
    }

    public Task<DesktopApiDataResult<DesktopConciliacaoFinanceira>> SaveConciliacaoFinanceiraAsync(
        TerminalProfile profile,
        string token,
        DesktopConciliacaoFinanceiraRequest payload,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopConciliacaoFinanceira>(
            profile,
            token,
            HttpMethod.Post,
            "/api/financeiro/conciliacao",
            payload,
            cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopCrmSegmento>>> GetMarketingSegmentsAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<List<DesktopCrmSegmento>>(profile, token, HttpMethod.Get, "/api/crm/segmentos", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopOpenAiAssistentesStatus>> GetOpenAiAssistantsStatusAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopOpenAiAssistentesStatus>(
            profile,
            token,
            HttpMethod.Get,
            "/api/admin/integracoes/openai",
            null,
            cancellationToken);

    public Task<DesktopApiDataResult<DesktopOpenAiAssistentesStatus>> SaveOpenAiAssistantsAsync(
        TerminalProfile profile,
        string token,
        DesktopOpenAiAssistentesConfiguracaoRequest payload,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopOpenAiAssistentesStatus>(
            profile,
            token,
            HttpMethod.Put,
            "/api/admin/integracoes/openai",
            payload,
            cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopSiteConfiguracao>>> GetSiteConfigurationsAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<List<DesktopSiteConfiguracao>>(
            profile,
            token,
            HttpMethod.Get,
            "/api/site/configuracoes",
            null,
            cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopSiteConfiguracao>>> SaveSiteConfigurationsAsync(
        TerminalProfile profile,
        string token,
        DesktopSiteConfiguracaoUpdateRequest payload,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<List<DesktopSiteConfiguracao>>(
            profile,
            token,
            HttpMethod.Put,
            "/api/site/configuracoes",
            payload,
            cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopUsuarioAcesso>>> GetAccessUsersAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<List<DesktopUsuarioAcesso>>(
            profile, token, HttpMethod.Get, "/api/admin/usuarios", null, cancellationToken);

    public Task<DesktopApiDataResult<List<string>>> GetAdministrativeRolesAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<List<string>>(
            profile, token, HttpMethod.Get, "/api/admin/usuarios/perfis", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopUsuarioAcesso>> SaveAccessUserAsync(
        TerminalProfile profile,
        string token,
        DesktopUsuarioAcessoRequest payload,
        int? id,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopUsuarioAcesso>(
            profile,
            token,
            id.HasValue ? HttpMethod.Put : HttpMethod.Post,
            id.HasValue ? $"/api/admin/usuarios/{id.Value}" : "/api/admin/usuarios",
            payload,
            cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopPerfilAcesso>>> GetAccessProfilesAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<List<DesktopPerfilAcesso>>(
            profile, token, HttpMethod.Get, "/api/perfis", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopPerfilAcesso>> SaveAccessProfileAsync(
        TerminalProfile profile,
        string token,
        DesktopPerfilAcessoRequest payload,
        int? id,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopPerfilAcesso>(
            profile,
            token,
            id.HasValue ? HttpMethod.Put : HttpMethod.Post,
            id.HasValue ? $"/api/perfis/{id.Value}" : "/api/perfis",
            payload,
            cancellationToken);

    public Task<DesktopApiDataResult<DesktopPerfilAcesso>> DeactivateAccessProfileAsync(
        TerminalProfile profile,
        string token,
        int id,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopPerfilAcesso>(
            profile, token, HttpMethod.Delete, $"/api/perfis/{id}", null, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopPermissaoAcesso>>> GetPermissionsAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<List<DesktopPermissaoAcesso>>(
            profile, token, HttpMethod.Get, "/api/permissoes", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopPermissaoAcesso>> SavePermissionAsync(
        TerminalProfile profile,
        string token,
        DesktopPermissaoAcessoRequest payload,
        int? id,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopPermissaoAcesso>(
            profile,
            token,
            id.HasValue ? HttpMethod.Put : HttpMethod.Post,
            id.HasValue ? $"/api/permissoes/{id.Value}" : "/api/permissoes",
            payload,
            cancellationToken);

    public Task<DesktopApiDataResult<DesktopPermissaoAcesso>> DeactivatePermissionAsync(
        TerminalProfile profile,
        string token,
        int id,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopPermissaoAcesso>(
            profile, token, HttpMethod.Delete, $"/api/permissoes/{id}", null, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopPerfilPermissao>>> GetProfilePermissionsAsync(
        TerminalProfile profile,
        string token,
        int profileId,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<List<DesktopPerfilPermissao>>(
            profile, token, HttpMethod.Get, $"/api/perfis/{profileId}/permissoes", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopPerfilPermissao>> SaveProfilePermissionAsync(
        TerminalProfile profile,
        string token,
        int profileId,
        DesktopPerfilPermissaoRequest payload,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopPerfilPermissao>(
            profile, token, HttpMethod.Post, $"/api/perfis/{profileId}/permissoes", payload, cancellationToken);

    public Task<DesktopApiDataResult<JsonElement>> RemoveProfilePermissionAsync(
        TerminalProfile profile,
        string token,
        int profileId,
        int permissionId,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<JsonElement>(
            profile,
            token,
            HttpMethod.Delete,
            $"/api/perfis/{profileId}/permissoes/{permissionId}",
            null,
            cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopCrmCampanha>>> GetMarketingCampaignsAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<List<DesktopCrmCampanha>>(profile, token, HttpMethod.Get, "/api/crm/campanhas", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopCrmSegmento>> SaveMarketingSegmentAsync(
        TerminalProfile profile,
        string token,
        DesktopCrmSegmentoRequest payload,
        Guid? id,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopCrmSegmento>(
            profile,
            token,
            id.HasValue ? HttpMethod.Put : HttpMethod.Post,
            id.HasValue ? $"/api/crm/segmentos/{id.Value}" : "/api/crm/segmentos",
            payload,
            cancellationToken);

    public Task<DesktopApiDataResult<DesktopCrmCampanha>> SaveMarketingCampaignAsync(
        TerminalProfile profile,
        string token,
        DesktopCrmCampanhaRequest payload,
        Guid? id,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopCrmCampanha>(
            profile,
            token,
            id.HasValue ? HttpMethod.Put : HttpMethod.Post,
            id.HasValue ? $"/api/crm/campanhas/{id.Value}" : "/api/crm/campanhas",
            payload,
            cancellationToken);

    public Task<DesktopApiDataResult<bool>> DeleteMarketingSegmentAsync(
        TerminalProfile profile,
        string token,
        Guid id,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<bool>(profile, token, HttpMethod.Delete, $"/api/crm/segmentos/{id}", null, cancellationToken);

    public Task<DesktopApiDataResult<bool>> DeleteMarketingCampaignAsync(
        TerminalProfile profile,
        string token,
        Guid id,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<bool>(profile, token, HttpMethod.Delete, $"/api/crm/campanhas/{id}", null, cancellationToken);

    public Task<DesktopApiDataResult<List<DesktopDropshippingCanal>>> GetDropshippingChannelsAsync(
        TerminalProfile profile,
        string token,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<List<DesktopDropshippingCanal>>(profile, token, HttpMethod.Get, "/api/dropshipping/canais", null, cancellationToken);

    public Task<DesktopApiDataResult<DesktopDropshippingCanal>> SaveDropshippingChannelAsync(
        TerminalProfile profile,
        string token,
        DesktopDropshippingCanalRequest payload,
        int? id,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<DesktopDropshippingCanal>(
            profile,
            token,
            id.HasValue ? HttpMethod.Put : HttpMethod.Post,
            id.HasValue ? $"/api/dropshipping/canais/{id.Value}" : "/api/dropshipping/canais",
            payload,
            cancellationToken);

    public Task<DesktopApiDataResult<bool>> DeleteDropshippingChannelAsync(
        TerminalProfile profile,
        string token,
        int id,
        string rowVersion,
        CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<bool>(
            profile,
            token,
            HttpMethod.Delete,
            $"/api/dropshipping/canais/{id}?rowVersion={Uri.EscapeDataString(rowVersion)}",
            null,
            cancellationToken);

    public async Task<DesktopApiHealthResult> CheckHealthAsync(TerminalProfile profile, CancellationToken cancellationToken = default)
    {
        var local = await TryHealthAsync(profile.ApiBaseUrl, cancellationToken);
        var publicApi = await TryHealthAsync(profile.PublicApiUrl, cancellationToken);

        var status = local.Success
            ? "Servidor principal operacional"
            : publicApi.Success
                ? "API pública operacional; origem local indisponível"
                : "API indisponível";

        var detail = local.Success
            ? $"Origem {profile.ApiBaseUrl} respondeu Healthy. Terminal pronto para ERP/PDV."
            : publicApi.Success
                ? $"Cloudflare respondeu em {profile.PublicApiUrl}, mas o terminal não alcançou {profile.ApiBaseUrl}."
                : $"Sem resposta local nem pública. Contingência local: {(profile.OfflineContingencyEnabled ? "ativa" : "inativa")}.";

        return new DesktopApiHealthResult(status, detail, local.Success, publicApi.Success, DateTime.Now);
    }

    public async Task<DesktopApiSubmitResult> SubmitSaleAsync(
        TerminalProfile profile,
        PdvSaleDraft sale,
        CancellationToken cancellationToken = default)
    {
        var payload = new GenesisPdvVendaApiRequest(
            sale.CodigoVenda,
            sale.EmpresaEmissora.StartsWith("Empresa matriz ativa", StringComparison.OrdinalIgnoreCase)
                ? "MATRIZ_ATIVA"
                : sale.EmpresaEmissora,
            null,
            sale.ClienteNome,
            sale.ClienteDocumento,
            null,
            null,
            sale.Terminal,
            profile.TerminalCode,
            sale.Operador,
            sale.Desconto,
            0m,
            $"Canal={sale.Canal}; entrega={sale.TipoEntrega}; fiscal={sale.StatusFiscal}; financeiro={sale.StatusFinanceiro}; logistica={sale.StatusLogistico}; decisao={sale.DecisaoEmpresaEmissora}",
            sale.Itens.Select(item => new GenesisPdvVendaItemApiRequest(
                null,
                item.Codigo,
                item.Codigo,
                item.Descricao,
                item.Quantidade,
                item.ValorUnitario,
                item.CustoEstimado,
                item.Desconto,
                item.OrigemAquisicao)).ToList(),
            sale.Pagamentos.Select(payment => new GenesisPdvPagamentoApiRequest(
                payment.Forma,
                payment.Valor,
                1,
                payment.Autorizacao,
                null,
                null)).ToList());

        var reachableEndpoints = new List<string>();
        var healthFailures = new List<string>();
        foreach (var endpoint in GetEndpoints(profile))
        {
            var health = await TryPdvHealthAsync(endpoint, cancellationToken);
            if (health.Success)
            {
                reachableEndpoints.Add(endpoint);
                break;
            }

            healthFailures.Add($"{endpoint}: {health.Detail}");
        }

        if (reachableEndpoints.Count == 0)
        {
            return new DesktopApiSubmitResult(
                false,
                false,
                string.Empty,
                $"Nenhuma API respondeu antes do envio. A venda não foi submetida. {string.Join(" | ", healthFailures)}",
                null,
                false,
                false,
                false);
        }

        return await SubmitJsonWithFallbackAsync(
            profile,
            "/api/desktop/genesis/pdv/vendas",
            payload,
            "venda PDV",
            cancellationToken,
            requireJwt: true,
            endpointOverride: reachableEndpoints.ToArray());
    }

    public async Task<DesktopApiSubmitResult> SubmitOperationAsync<TPayload>(
        TerminalProfile profile,
        string module,
        string operationCode,
        TPayload payload,
        CancellationToken cancellationToken = default)
    {
        var request = new GenesisDesktopOperationApiRequest(
            operationCode,
            profile.TerminalCode,
            profile.StoreCode,
            "GenesisGest.Net Desktop",
            "Recebido",
            JsonSerializer.SerializeToElement(payload, JsonOptions),
            $"Operação {module} enviada pelo terminal {profile.TerminalCode}.");

        return await SubmitJsonWithFallbackAsync(
            profile,
            $"/api/desktop/genesis/operacoes/{Uri.EscapeDataString(module)}",
            request,
            $"operação {module}",
            cancellationToken);
    }

    private static async Task<DesktopApiSubmitResult> SubmitJsonWithFallbackAsync<TPayload>(
        TerminalProfile profile,
        string path,
        TPayload payload,
        string operationLabel,
        CancellationToken cancellationToken,
        bool requireJwt = false,
        string[]? endpointOverride = null)
    {
        var endpoints = (endpointOverride ?? new[]
        {
            profile.ApiBaseUrl,
            profile.PublicApiUrl
        }
        .Where(url => !string.IsNullOrWhiteSpace(url))
        .Select(url => url.TrimEnd('/'))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray());

        var failures = new List<string>();

        foreach (var endpoint in endpoints)
        {
            var result = await TrySubmitAsync(profile, endpoint, path, payload, operationLabel, cancellationToken, requireJwt);
            if (result.Success)
            {
                return result;
            }

            failures.Add($"{endpoint}: {result.Detail}");
            if (result.EndpointResponded || result.PersistedOnServer || result.StopRetry)
            {
                return result with { Detail = string.Join(" | ", failures) };
            }
        }

        return new DesktopApiSubmitResult(
            false,
            false,
            string.Empty,
            string.Join(" | ", failures),
            null);
    }

    private static async Task<DesktopApiDataResult<T>> SendAuthorizedAsync<T>(
        TerminalProfile profile,
        string token,
        HttpMethod method,
        string path,
        object? payload,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return new DesktopApiDataResult<T>(false, default, "Sessao administrativa ausente.");
        }

        var failures = new List<string>();
        foreach (var endpoint in GetEndpoints(profile))
        {
            try
            {
                using var request = new HttpRequestMessage(method, $"{endpoint}{path}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                if (payload is not null)
                {
                    request.Content = JsonContent.Create(payload, options: JsonOptions);
                }

                using var response = await Http.SendAsync(request, cancellationToken);
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var detail = $"{(int)response.StatusCode} {ExtractErrorMessage(content, response.ReasonPhrase)}";
                    return new DesktopApiDataResult<T>(false, default, $"{endpoint}: {detail}");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    object deleted = true;
                    return new DesktopApiDataResult<T>(true, (T)deleted, $"Exclusao confirmada em {endpoint}.");
                }

                var envelope = JsonSerializer.Deserialize<DesktopApiEnvelope<T>>(content, JsonOptions);
                if (envelope is null || !envelope.Sucesso || envelope.Dados is null)
                {
                    return new DesktopApiDataResult<T>(false, default, envelope?.Mensagem ?? "A API nao confirmou os dados persistidos.");
                }

                return new DesktopApiDataResult<T>(true, envelope.Dados, envelope.Mensagem);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                failures.Add($"{endpoint}: {ex.Message}");
            }
        }

        return new DesktopApiDataResult<T>(false, default, string.Join(" | ", failures));
    }

    private static async Task<DesktopFileDownloadResult> DownloadAuthorizedPdfAsync(
        TerminalProfile profile,
        string token,
        string path,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return new DesktopFileDownloadResult(false, null, null, string.Empty, "Sessao administrativa ausente.");
        }

        var failures = new List<string>();
        foreach (var endpoint in GetEndpoints(profile))
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"{endpoint}{path}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using var response = await Http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var detail = $"{(int)response.StatusCode} {ExtractErrorMessage(content, response.ReasonPhrase)}";
                    return new DesktopFileDownloadResult(false, null, null, string.Empty, $"{endpoint}: {detail}");
                }

                var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
                if (!string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return new DesktopFileDownloadResult(
                        false,
                        null,
                        null,
                        contentType,
                        $"{endpoint}: a API respondeu sem um documento PDF valido.");
                }

                var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                if (bytes.Length < 5
                    || bytes[0] != (byte)'%'
                    || bytes[1] != (byte)'P'
                    || bytes[2] != (byte)'D'
                    || bytes[3] != (byte)'F'
                    || bytes[4] != (byte)'-')
                {
                    return new DesktopFileDownloadResult(false, null, null, contentType, $"{endpoint}: assinatura PDF invalida.");
                }

                var responseName = response.Content.Headers.ContentDisposition?.FileNameStar
                    ?? response.Content.Headers.ContentDisposition?.FileName;
                if (string.IsNullOrWhiteSpace(responseName))
                {
                    return new DesktopFileDownloadResult(false, null, null, contentType, $"{endpoint}: nome do arquivo ausente na resposta.");
                }

                var fileName = Path.GetFileName(responseName.Trim().Trim('"'));
                if (string.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return new DesktopFileDownloadResult(false, null, null, contentType, $"{endpoint}: nome de arquivo PDF invalido.");
                }

                return new DesktopFileDownloadResult(
                    true,
                    bytes,
                    fileName,
                    contentType,
                    $"Documento de {bytes.Length:N0} bytes recebido de {endpoint}.");
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException)
            {
                failures.Add($"{endpoint}: {ex.Message}");
            }
        }

        return new DesktopFileDownloadResult(false, null, null, string.Empty, string.Join(" | ", failures));
    }

    private static string[] GetEndpoints(TerminalProfile profile)
        => new[] { profile.ApiBaseUrl, profile.PublicApiUrl }
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Select(url => url.TrimEnd('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static string ExtractErrorMessage(string content, string? reasonPhrase)
    {
        if (!string.IsNullOrWhiteSpace(content))
        {
            try
            {
                using var document = JsonDocument.Parse(content);
                foreach (var property in new[] { "mensagem", "detail", "erro", "message" })
                {
                    if (document.RootElement.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String)
                    {
                        return value.GetString() ?? reasonPhrase ?? "Falha na API.";
                    }
                }
            }
            catch (JsonException)
            {
                return TrimContent(content);
            }
        }

        return reasonPhrase ?? "Falha na API.";
    }

    private static async Task<DesktopApiSubmitResult> TrySubmitAsync<TPayload>(
        TerminalProfile profile,
        string endpoint,
        string path,
        TPayload payload,
        string operationLabel,
        CancellationToken cancellationToken,
        bool requireJwt)
    {
        var requestMayHaveBeenProcessed = false;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{endpoint}{path}")
            {
                Content = JsonContent.Create(payload, options: JsonOptions)
            };

            request.Headers.TryAddWithoutValidation("X-Nexum-Terminal", profile.TerminalCode);
            request.Headers.TryAddWithoutValidation("X-Nexum-Store", profile.StoreCode);
            if (requireJwt)
            {
                if (string.IsNullOrWhiteSpace(profile.DesktopAccessToken))
                {
                    return new DesktopApiSubmitResult(false, false, endpoint,
                        "Sessão administrativa ausente. Autentique o operador antes de registrar a venda.", null,
                        false, false, true);
                }

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", profile.DesktopAccessToken.Trim());
            }
            if (!string.IsNullOrWhiteSpace(profile.DesktopAccessToken))
            {
                request.Headers.TryAddWithoutValidation("X-Nexum-Desktop-Token", profile.DesktopAccessToken.Trim());
            }

            requestMayHaveBeenProcessed = true;
            using var response = await Http.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new DesktopApiSubmitResult(
                    false,
                    false,
                    endpoint,
                    $"{(int)response.StatusCode} {response.ReasonPhrase}: {TrimContent(content)}",
                    null,
                    true,
                    false);
            }

            if (operationLabel.Equals("venda PDV", StringComparison.OrdinalIgnoreCase)
                && HasPartialPdvPersistence(content))
            {
                return new DesktopApiSubmitResult(
                    false,
                    true,
                    endpoint,
                    "A venda foi gravada no Genesis, mas a API não confirmou o espelho Nexum. Não repita a venda; reconciliação é necessária.",
                    ExtractServerReference(content),
                    true,
                    true);
            }

            return new DesktopApiSubmitResult(
                true,
                true,
                endpoint,
                $"{operationLabel} gravada no servidor",
                ExtractServerReference(content),
                true,
                false);
        }
        catch (Exception ex)
        {
            return new DesktopApiSubmitResult(false, false, endpoint, ex.Message, null, false, requestMayHaveBeenProcessed);
        }
    }

    private static bool HasPartialPdvPersistence(string content)
    {
        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;
            return root.TryGetProperty("gravadoNoServidor", out var persisted)
                && persisted.ValueKind == JsonValueKind.True
                && root.TryGetProperty("sincronizadoNexum", out var synchronized)
                && synchronized.ValueKind == JsonValueKind.False;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static async Task<(bool Success, string Detail)> TryHealthAsync(string baseUrl, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await Http.GetAsync($"{baseUrl.TrimEnd('/')}/health", cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var success = response.IsSuccessStatusCode && content.Contains("Healthy", StringComparison.OrdinalIgnoreCase);
            return (success, $"{(int)response.StatusCode} {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private static async Task<(bool Success, string Detail)> TryPdvHealthAsync(string baseUrl, CancellationToken cancellationToken)
    {
        foreach (var path in new[] { "/health", "/health/db", "/health/db/genesis" })
        {
            try
            {
                using var response = await Http.GetAsync($"{baseUrl.TrimEnd('/')}{path}", cancellationToken);
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return (false, $"{path}: HTTP {(int)response.StatusCode} {ExtractErrorMessage(content, response.ReasonPhrase)}");
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return (false, $"{path}: {ex.Message}");
            }
        }

        return (true, "API, Nexum e Genesis respondem.");
    }

    private static string? ExtractServerReference(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("id", out var id))
            {
                return $"Id={id}";
            }

            if (doc.RootElement.TryGetProperty("venda", out var venda) && venda.TryGetProperty("id", out var vendaId))
            {
                return $"VendaId={vendaId}";
            }
        }
        catch (JsonException)
        {
            return TrimContent(content);
        }

        return TrimContent(content);
    }

    private static string TrimContent(string content)
    {
        var value = content.ReplaceLineEndings(" ").Trim();
        return value.Length <= 180 ? value : value[..180];
    }
}

public sealed record DesktopApiHealthResult(
    string Status,
    string Detail,
    bool LocalHealthy,
    bool PublicHealthy,
    DateTime CheckedAt);

public sealed record DesktopApiSubmitResult(
    bool Success,
    bool PersistedOnServer,
    string Endpoint,
    string Detail,
    string? ServerReference,
    bool EndpointResponded = false,
    bool RequestMayHaveBeenProcessed = false,
    bool StopRetry = false);

internal sealed record GenesisPdvVendaApiRequest(
    string? Numero,
    string? EmpresaCodigo,
    int? EmpresaNexumId,
    string ClienteNome,
    string? ClienteDocumento,
    string? ClienteEmail,
    int? ClienteNexumId,
    string? Terminal,
    string? CaixaCodigo,
    string? Operador,
    decimal Desconto,
    decimal Frete,
    string? Observacoes,
    List<GenesisPdvVendaItemApiRequest> Itens,
    List<GenesisPdvPagamentoApiRequest> Pagamentos);

internal sealed record GenesisPdvVendaItemApiRequest(
    int? ProdutoNexumId,
    string? ProdutoCodigo,
    string? Sku,
    string Descricao,
    decimal Quantidade,
    decimal PrecoUnitario,
    decimal CustoUnitario,
    decimal Desconto,
    string? OrigemAquisicao);

internal sealed record GenesisPdvPagamentoApiRequest(
    string Forma,
    decimal Valor,
    int Parcelas,
    string? Autorizacao,
    string? Nsu,
    string? Bandeira);

internal sealed record GenesisDesktopOperationApiRequest(
    string Codigo,
    string? Terminal,
    string? Loja,
    string? Origem,
    string? Status,
    JsonElement Payload,
    string? Observacoes);

public sealed record DesktopLoginResult(bool Success, string? Token, DesktopUser? User, string Detail);
public sealed record DesktopLoginRequest(string Email, string Senha, string? MfaCode);
public sealed record DesktopLoginResponse(string Token, string RefreshToken, DateTime ExpiraEm, DesktopUser Usuario);
public sealed record DesktopUser(int Id, string Nome, string Email, string Perfil);
public sealed record DesktopApiEnvelope<T>(bool Sucesso, string Mensagem, T? Dados, List<string>? Erros);
public sealed record DesktopApiDataResult<T>(bool Success, T? Data, string Detail);

public sealed record DesktopGenesisRhColaborador(
    int Id, string Nome, string? Cargo, string? Departamento, string? Status, DateTime? DataAdmissao);

public sealed record DesktopGenesisRhColaboradorRequest(
    string Nome, string? Cargo, string? Departamento, string? Status, DateTime? DataAdmissao);

public sealed record DesktopGenesisRhReference(
    int Id, string Tipo, string Codigo, string Descricao, int Ordem);

public sealed record DesktopGenesisRhReferenceRequest(
    string Tipo, string Codigo, string Descricao, int Ordem);

public sealed record DesktopGenesisFinanceReference(
    int Id, string Tipo, string Codigo, string Descricao, int Ordem);

public sealed record DesktopGenesisFinanceReferenceRequest(
    string Tipo, string Codigo, string Descricao, int Ordem);
public sealed record DesktopFiscalEmpresa(
    int Id, string? CodigoEmpresa, string? RazaoSocial, string? Cnpj, string? Estado,
    bool Ativa, string? CategoriaFiscal, string? SubcategoriaFiscal, string? NaturezaOperacaoPadrao);
public sealed record DesktopFiscalRoutingRequest(
    int TipoOperacao, decimal ValorProdutos, decimal ValorFrete, string EstadoOrigem,
    string EstadoDestino, string? CategoriaFiscal, string? SubcategoriaFiscal,
    string? NaturezaOperacao, bool ExigeMarketplace, bool ExigeDropshipping,
    bool RequerSaidaNfe, bool RequerEntradaNfe);
public sealed record DesktopFiscalRoutingResult(
    bool Sucesso, string Resumo, string? CodigoEmpresaSelecionada,
    string? RazaoSocialSelecionada, string? CnpjSelecionado, string? EstadoSelecionado,
    List<DesktopFiscalRoutingRank> Ranking);
public sealed record DesktopFiscalRoutingRank(
    string CodigoEmpresa, string RazaoSocial, string Cnpj, string? RegimeTributario,
    string? CategoriaFiscal, string? SubcategoriaFiscal, decimal CustoTributarioEstimado,
    decimal CustoOperacionalEstimado, decimal LucroEstimado,
    decimal MargemEstimadaPercentual, decimal Score, List<string> Justificativas);
public sealed record DesktopPedidoConsulta(
    int Id, string NumeroPedido, decimal Total, string Status, DateTime CreatedAt,
    string StatusPagamento, string? MeioPagamento, decimal FreteValor, string? FreteMetodo,
    string? ClienteNome = null, int? LojaId = null, string? LojaNome = null,
    List<DesktopPedidoItemResumo>? Itens = null);
public sealed record DesktopPedidoItemResumo(
    int? ProdutoId, string NomeProduto, string? SkuProduto,
    int Quantidade, decimal PrecoUnitario, decimal PrecoTotal);
public sealed record DesktopEmpresaLojaVinculo(
    int LojaId, string NomeLoja, string Slug, string? Segmento, bool LojaAtiva,
    int? EmpresaId, string? NomeFantasia, string? RazaoSocial, string? Cnpj,
    bool? EmpresaAtiva, string? RowVersion);
public sealed record DesktopLoja(int Id, string Nome, string Slug, string Segmento,
    string? Descricao, string CorPrimaria, string CorSecundaria, bool Ativa, int OrdemExibicao);
public sealed record DesktopCategoriaAdmin(int Id, int LojaId, string LojaNome, string Slug,
    string Nome, string? Descricao, int? CategoriaPaiId, string? CategoriaPaiSlug,
    int Ordem, bool Ativa, DateTime UpdatedAt, string RowVersion);
public sealed record DesktopCategoriaCreateRequest(int LojaId, string? Slug, string Nome,
    string? Descricao, int? CategoriaPaiId, int Ordem);
public sealed record DesktopCategoriaUpdateRequest(string Nome, string? Descricao,
    int? CategoriaPaiId, int Ordem, bool Ativa, string RowVersion);
public sealed record DesktopEmpresaDadosLegaisRequest(
    string? RazaoSocial, string? NomeFantasia, string? Cnpj,
    string? ResponsavelLegal, string? ResponsavelFiscal,
    string? EmailFiscal, string? EmailComercial, string? Telefone, string? Whatsapp,
    string? Cep, string? Logradouro, string? Numero, string? Complemento,
    string? Bairro, string? Cidade, string? Estado, string? RegimeTributario,
    bool Ativa, string? RowVersion);
public sealed record DesktopProdutoIdentificador(
    int Id,
    int LojaId,
    string Nome,
    string Sku,
    string? CodigoBarras,
    string? QrCode,
    bool Ativo,
    string RowVersion);
public sealed record DesktopProdutoIdentificadorPagina(
    List<DesktopProdutoIdentificador> Itens,
    int Total,
    int Pagina,
    int TotalPaginas);
public sealed record DesktopProdutoIdentificadorUpdateRequest(
    string? CodigoBarras,
    string? QrCode,
    string RowVersion);
public sealed record DesktopProdutoAdminPagina(
    List<DesktopProdutoAdmin> Itens, int Total, int Pagina, int TotalPaginas);
public sealed record DesktopProdutoAdmin(
    int Id, string Slug, string Nome, string Sku, string? DescricaoCurta, string? Descricao,
    decimal Preco, decimal? PrecoPromocional, decimal Custo, int Estoque, int EstoqueMinimo,
    int EstoqueReservado, string? ImagemUrl, string? CategoriaId, bool Destaque, bool Ativo,
    string? Marca, string? Tags, decimal Peso, decimal Altura, decimal Largura, decimal Comprimento,
    string TipoProduto, int? FornecedorId, string? SeoTitulo, string? SeoDescricao,
    string? SeoKeywords, string? ImagensGaleria);
public sealed record DesktopProdutoLoja(
    string Id, string Nome, string Descricao, string? DescricaoCurta, decimal Preco,
    decimal? PrecoPromocional, string ImagemUrl, int Estoque, int EstoqueMinimo,
    int EstoqueReservado, bool Destaque, string Sku, string CategoriaId, decimal? Avaliacao,
    decimal Custo, decimal Peso, decimal Altura, decimal Largura, decimal Comprimento,
    string TipoProduto, int? FornecedorId, string? Marca, string? Tags, string? SeoTitulo,
    string? SeoDescricao, string? SeoKeywords, string? ImagensGaleria);
public sealed record DesktopProdutoUpdateRequest(
    string? Id, string Nome, string? Descricao, string? DescricaoCurta, decimal Preco,
    decimal? PrecoPromocional, string? ImagemUrl, int Estoque, int? EstoqueMinimo,
    int? EstoqueReservado, bool Destaque, string? Sku, string? CategoriaId,
    string? SubcategoriaId, decimal? Avaliacao, decimal? Custo, decimal? Peso,
    decimal? Altura, decimal? Largura, decimal? Comprimento, string? TipoProduto,
    int? FornecedorId, string? Marca, string? Tags, string? SeoTitulo,
    string? SeoDescricao, string? SeoKeywords, string? ImagensGaleria);
public sealed record DesktopFileDownloadResult(
    bool Success,
    byte[]? Content,
    string? FileName,
    string ContentType,
    string Detail);

public sealed record DesktopComprasPainel(
    DesktopComprasKpi Kpis,
    List<string> Alertas,
    List<DesktopCompraSolicitacao> Solicitacoes,
    List<DesktopCompraCotacao> Cotacoes,
    List<DesktopCompraPedido> Pedidos,
    List<DesktopCompraEntrada> Entradas,
    List<DesktopCompraProdutoReposicao> ProdutosReposicao,
    List<DesktopCompraFornecedor> Fornecedores);

public sealed record DesktopComprasKpi(
    int SolicitacoesAbertas,
    int PedidosAbertos,
    int EntradasMes,
    decimal ValorComprasAbertas,
    int ContasAPagarCompras,
    int FornecedoresAtivos,
    int ProdutosReposicao);

public sealed record DesktopCompraSolicitacao(
    int Id,
    int? ProdutoId,
    string ProdutoNome,
    int Quantidade,
    string Finalidade,
    string Origem,
    string Status,
    string Prioridade,
    DateTime CreatedAt);

public sealed record DesktopCompraCotacao(
    int Id,
    int SolicitacaoId,
    int FornecedorId,
    string FornecedorNome,
    int? ProdutoId,
    string ProdutoNome,
    int Quantidade,
    decimal CustoUnitario,
    decimal ValorTotal,
    int PrazoEntregaDias,
    string Origem,
    string Status,
    DateTime CreatedAt);

public sealed record DesktopCompraPedido(
    int Id,
    string Numero,
    int FornecedorId,
    string FornecedorNome,
    string Origem,
    string Finalidade,
    string Status,
    string StatusFiscal,
    decimal ValorTotal,
    DateTime? DataPrevistaEntrega,
    DateTime CreatedAt,
    List<DesktopCompraPedidoItem> Itens);

public sealed record DesktopCompraPedidoItem(
    int Id,
    int? ProdutoId,
    string ProdutoNome,
    string? Sku,
    int Quantidade,
    int QuantidadeRecebida,
    int QuantidadePendente,
    decimal CustoUnitario,
    decimal ValorTotal,
    string Origem,
    string Finalidade);

public sealed record DesktopCompraEntrada(
    int Id,
    int CompraPedidoId,
    string PedidoNumero,
    string FornecedorNome,
    string? NumeroDocumento,
    string? ChaveNfeEntrada,
    string TipoEntrada,
    string StatusFiscal,
    decimal ValorTotal,
    DateTime CreatedAt);

public sealed record DesktopCompraProdutoReposicao(
    int ProdutoId,
    string ProdutoNome,
    string Sku,
    int EstoqueAtual,
    int EstoqueMinimo,
    string TipoProduto,
    int? FornecedorId,
    decimal CustoAtual);

public sealed record DesktopCompraFornecedor(
    int Id,
    string Nome,
    string? Documento,
    string? Segmento,
    int PrazoEntregaDias,
    int PrazoPagamentoDias);

public sealed record DesktopCompraSolicitacaoRequest(
    int? ProdutoId,
    string? ProdutoNome,
    int Quantidade,
    string? Origem,
    string? Finalidade,
    string? Prioridade,
    string? Observacoes);

public sealed record DesktopCompraCotacaoRequest(
    int FornecedorId,
    int? SolicitacaoId,
    int? ProdutoId,
    string? ProdutoNome,
    int Quantidade,
    decimal CustoUnitario,
    string? Origem,
    string? Finalidade,
    string? Prioridade,
    int? PrazoEntregaDias,
    string? Observacoes);

public sealed record DesktopCompraPedidoRequest(
    int FornecedorId,
    int? SolicitacaoId,
    string? Origem,
    string? Finalidade,
    DateTime? DataPrevistaEntrega,
    DateTime? DataVencimento,
    string? MeioPagamento,
    string? Observacoes,
    List<DesktopCompraPedidoItemRequest> Itens);

public sealed record DesktopCompraPedidoItemRequest(
    int? ProdutoId,
    string? ProdutoNome,
    string? Sku,
    int Quantidade,
    decimal CustoUnitario);

public sealed record DesktopCompraEntradaRequest(
    string? NumeroDocumento,
    string? ChaveNfeEntrada,
    string? TipoEntrada,
    string? RecebidoPor,
    string? Observacoes,
    List<DesktopCompraEntradaItemRequest> Itens);

public sealed record DesktopCompraEntradaItemRequest(int ItemId, int QuantidadeRecebida);

public sealed record DesktopCompraStatusRequest(string Status, string? Observacoes);

public sealed record DesktopLogisticaExpedicao(
    int PedidoId,
    string NumeroPedido,
    string ClienteNome,
    string Status,
    string? FreteMetodo,
    string? FreteTransportadora,
    int FretePrazoDias,
    string? FreteCodigoRastreio,
    DateTime? DataEnvio,
    DateTime? DataEntrega,
    DateTime? PrevisaoEntrega,
    bool UltimoRastreamentoOperacional,
    string? UltimoStatusExterno,
    DateTime? UltimaConsultaEm,
    string RowVersion,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record DesktopPedidoLogisticaRequest(
    [property: JsonPropertyName("frete_metodo")] string FreteMetodo,
    [property: JsonPropertyName("frete_transportadora")] string FreteTransportadora,
    [property: JsonPropertyName("frete_prazo_dias")] int FretePrazoDias,
    [property: JsonPropertyName("frete_codigo_rastreio")] string FreteCodigoRastreio,
    [property: JsonPropertyName("row_version")] string RowVersion);

public sealed record DesktopPedidoLogisticaAtualizada(
    int Id,
    string NumeroPedido,
    decimal Total,
    string Status,
    DateTime CreatedAt,
    string StatusPagamento,
    string? MeioPagamento,
    string? GatewayPagamento,
    string? GatewayTransacaoId,
    decimal FreteValor,
    string? FreteMetodo,
    string? FreteTransportadora,
    int FretePrazoDias,
    string? FreteCodigoRastreio);

public sealed record DesktopLogisticaRastreamento(
    Guid ConsultaId,
    string CodigoRastreio,
    int PedidoId,
    string NumeroPedido,
    string? Transportadora,
    string? MetodoFrete,
    string StatusInterno,
    DateTime? DataEnvio,
    DateTime? DataEntrega,
    DateTime? PrevisaoEntrega,
    bool RastreamentoExternoConfigurado,
    bool RastreamentoExternoOperacional,
    string FonteExterna,
    int? HttpStatusCode,
    string? StatusExterno,
    List<DesktopLogisticaRastreamentoEvento> Eventos,
    List<string> Pendencias,
    string? RespostaSha256,
    DateTime ConsultadoEm);

public sealed record DesktopLogisticaRastreamentoEvento(
    DateTime? DataHora,
    string Status,
    string? Local,
    string? Descricao);

public sealed record DesktopFiscalManualEmissaoRequest(
    string EmpresaEmissora,
    string CnpjEmissor,
    string ClienteDestinatario,
    string DocumentoDestinatario,
    string NaturezaOperacao,
    string Cfop,
    decimal Subtotal,
    decimal Frete,
    decimal ImpostosEstimados,
    decimal MargemMinima,
    string? Observacoes,
    string TipoOperacao,
    string EstadoOrigem,
    string EstadoDestino,
    string? CategoriaFiscal,
    string? SubcategoriaFiscal,
    bool ExigeMarketplace,
    bool ExigeDropshipping,
    bool RequerSaidaNfe,
    bool RequerEntradaNfe);

public sealed record DesktopFiscalManualEmissaoResponse(
    bool CertificadoOperacional,
    string CertificadoStatus,
    string? CertificadoReferencia,
    string RoteamentoResumo,
    string? CodigoEmpresaSelecionada,
    string? RazaoSocialSelecionada,
    string? CnpjSelecionado,
    string? EstadoSelecionado,
    List<string> Pendencias,
    DateTime GeradoEm);

public sealed record DesktopFiscalManualDraftSaved(
    bool Salvo,
    string Chave,
    DateTime SalvoEm);

public sealed record DesktopFiscalManualDraftRead(
    bool Existe,
    string? Valor,
    DateTime? UpdatedAt);

public sealed record DesktopUsuarioAcesso(
    int Id,
    string Nome,
    string Email,
    string Perfil,
    bool Ativo,
    string? Telefone,
    DateTime? UltimoLogin,
    DateTime UpdatedAt);

public sealed record DesktopUsuarioAcessoRequest(
    string Nome,
    string Email,
    string Perfil,
    bool Ativo,
    string? Telefone,
    string? Senha);

public sealed record DesktopPerfilAcesso(
    int Id,
    string Nome,
    string? Descricao,
    decimal AlcadaMaxima,
    int NivelHierarquico,
    bool Ativo,
    DateTime CriadoEm);

public sealed record DesktopPerfilAcessoRequest(
    string Nome,
    string? Descricao,
    decimal AlcadaMaxima,
    int NivelHierarquico,
    bool Ativo);

public sealed record DesktopPermissaoAcesso(
    int Id,
    string Modulo,
    string Funcionalidade,
    string Chave,
    string? Descricao,
    bool Ativo);

public sealed record DesktopPermissaoAcessoRequest(
    string Modulo,
    string Funcionalidade,
    string Chave,
    string? Descricao,
    bool Ativo);

public sealed record DesktopPerfilPermissao(
    int Id,
    int PerfilId,
    int PermissaoId,
    string Modulo,
    string Funcionalidade,
    string Chave,
    bool Leitura,
    bool Escrita,
    bool Exclusao,
    bool Impressao);

public sealed record DesktopPerfilPermissaoRequest(
    int PermissaoId,
    bool Leitura,
    bool Escrita,
    bool Exclusao,
    bool Impressao);

public sealed record DesktopCrmSegmento(
    Guid Id,
    string Nome,
    string? Descricao,
    string Cor,
    int Prioridade,
    decimal? TicketMedioMinimo,
    decimal? TicketMedioMaximo,
    int? FrequenciaMinimaDias,
    int? FrequenciaMaximaDias,
    bool Ativo,
    string RowVersion,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record DesktopCrmSegmentoRequest(
    string Nome,
    string? Descricao,
    string Cor,
    int Prioridade,
    decimal? TicketMedioMinimo,
    decimal? TicketMedioMaximo,
    int? FrequenciaMinimaDias,
    int? FrequenciaMaximaDias,
    bool Ativo,
    string? RowVersion);

public sealed record DesktopCrmCampanha(
    Guid Id,
    string Nome,
    string? Descricao,
    string Tipo,
    string Status,
    Guid? SegmentoId,
    string? SegmentoNome,
    DateTime DataInicio,
    DateTime? DataFim,
    decimal Orcamento,
    decimal CustoAtual,
    int Alcance,
    int Cliques,
    int LeadsGerados,
    int OportunidadesGeradas,
    int VendasGeradas,
    decimal ReceitaGerada,
    decimal Roas,
    decimal Cpc,
    decimal Cpl,
    decimal Cpa,
    string? PublicoAlvo,
    string? Conteudo,
    string RowVersion,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record DesktopCrmCampanhaRequest(
    string Nome,
    string? Descricao,
    string Tipo,
    string Status,
    Guid? SegmentoId,
    DateTime DataInicio,
    DateTime? DataFim,
    decimal Orcamento,
    decimal CustoAtual,
    int Alcance,
    int Cliques,
    int LeadsGerados,
    int OportunidadesGeradas,
    int VendasGeradas,
    decimal ReceitaGerada,
    string? PublicoAlvo,
    string? Conteudo,
    string? RowVersion);

public sealed record DesktopDropshippingCanal(
    int Id,
    string Nome,
    string Slug,
    string Tipo,
    string? ApiEndpoint,
    bool Ativo,
    bool CredenciaisConfiguradas,
    string StatusCredenciais,
    string DetalheCredenciais,
    string RowVersion,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record DesktopDropshippingCanalRequest(
    string Nome,
    string Slug,
    string Tipo,
    string? ApiEndpoint,
    bool Ativo,
    string? RowVersion);

public sealed record DesktopOpenAiAssistentesConfiguracaoRequest(
    string? YaraApiKey,
    string YaraModel,
    string? SophiaApiKey,
    string SophiaModel);

public sealed record DesktopOpenAiAssistentesStatus(
    DesktopOpenAiAssistenteStatus Yara,
    DesktopOpenAiAssistenteStatus Sophia);

public sealed record DesktopOpenAiAssistenteStatus(
    string Assistente,
    bool Configurada,
    string Modelo,
    string Origem,
    DateTime? AtualizadoEm);

public sealed record DesktopSiteConfiguracao(
    int Id,
    string Chave,
    string? Valor,
    string Tipo,
    string? Descricao,
    string? Grupo,
    bool Editavel,
    DateTime UpdatedAt);

public sealed record DesktopSiteConfiguracaoUpdate(
    string Chave,
    string? Valor,
    string? Tipo,
    string? Descricao,
    string? Grupo,
    bool? Editavel);

public sealed record DesktopSiteConfiguracaoUpdateRequest(List<DesktopSiteConfiguracaoUpdate> Itens);
