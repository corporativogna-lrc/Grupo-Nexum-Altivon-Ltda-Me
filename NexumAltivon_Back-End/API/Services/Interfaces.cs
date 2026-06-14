using NexumAltivon.API.DTOs;
using NexumAltivon.API.Models;

namespace NexumAltivon.API.Services;

public interface IAuthService
{
    Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto dto);
    Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto dto);
    Task<ApiResponse<UsuarioDto>> RegistrarAsync(RegistrarUsuarioDto dto);
    Task<ApiResponse<bool>> AlterarSenhaAsync(int usuarioId, AlterarSenhaDto dto);
    string GerarTokenJWT(Usuario usuario);
    string GerarRefreshToken();
}

public interface IUsuarioService
{
    Task<ApiResponse<UsuarioDto>> ObterPorIdAsync(int id);
    Task<ApiResponse<List<UsuarioDto>>> ListarAsync(PaginacaoDto paginacao);
    Task<ApiResponse<UsuarioDto>> CriarAsync(RegistrarUsuarioDto dto);
    Task<ApiResponse<UsuarioDto>> AtualizarAsync(int id, UsuarioDto dto);
    Task<ApiResponse<bool>> ExcluirAsync(int id);
}

public interface IClienteService
{
    Task<ApiResponse<ClienteDto>> ObterPorIdAsync(int id);
    Task<ApiResponse<ClienteDto>> ObterPorEmailAsync(string email);
    Task<ApiResponse<List<ClienteDto>>> ListarAsync(PaginacaoDto paginacao);
    Task<ApiResponse<ClienteDto>> CriarAsync(CriarClienteDto dto);
    Task<ApiResponse<ClienteDto>> ConfirmarEmailAsync(string token);
    Task<ApiResponse<ClienteDto>> AtualizarAsync(int id, AtualizarClienteDto dto);
    Task<ApiResponse<bool>> ExcluirAsync(int id);
    Task<ApiResponse<EnderecoDto>> AdicionarEnderecoAsync(int clienteId, CriarEnderecoDto dto);
    Task<ApiResponse<bool>> RemoverEnderecoAsync(int clienteId, int enderecoId);
}

public interface IProdutoService
{
    Task<ApiResponse<ProdutoDto>> ObterPorIdAsync(int id);
    Task<ApiResponse<ProdutoDto>> ObterPorSlugAsync(string slug);
    Task<ApiResponse<List<ProdutoListagemDto>>> ListarAsync(PaginacaoDto paginacao, int? lojaId = null, int? categoriaId = null);
    Task<ApiResponse<List<ProdutoListagemDto>>> ListarDestaquesAsync(int? lojaId = null);
    Task<ApiResponse<ProdutoDto>> CriarAsync(CriarProdutoDto dto);
    Task<ApiResponse<ProdutoDto>> AtualizarAsync(int id, CriarProdutoDto dto);
    Task<ApiResponse<bool>> ExcluirAsync(int id);
    Task<ApiResponse<bool>> AtualizarEstoqueAsync(int id, int quantidade);
}

public interface IPedidoService
{
    Task<ApiResponse<PedidoDto>> ObterPorIdAsync(int id);
    Task<ApiResponse<PedidoDto>> ObterPorNumeroAsync(string numero);
    Task<ApiResponse<List<PedidoDto>>> ListarAsync(PaginacaoDto paginacao, int? clienteId = null, string? status = null);
    Task<ApiResponse<PedidoDto>> CriarAsync(CriarPedidoDto dto);
    Task<ApiResponse<PedidoDto>> AtualizarStatusAsync(int id, AtualizarStatusPedidoDto dto);
}

public interface ICarrinhoService
{
    Task<ApiResponse<List<CarrinhoItemDto>>> ObterCarrinhoAsync(int? clienteId, string? sessaoId);
    Task<ApiResponse<CarrinhoItemDto>> AdicionarItemAsync(int? clienteId, string? sessaoId, AdicionarCarrinhoDto dto);
    Task<ApiResponse<bool>> RemoverItemAsync(int itemId);
    Task<ApiResponse<CarrinhoItemDto>> AtualizarQuantidadeAsync(int itemId, AtualizarQuantidadeCarrinhoDto dto);
    Task<ApiResponse<bool>> LimparCarrinhoAsync(int? clienteId, string? sessaoId);
    Task<ApiResponse<bool>> MigrarCarrinhoAsync(string sessaoId, int clienteId);
}

public interface ILojaService
{
    Task<ApiResponse<LojaDto>> ObterPorIdAsync(int id);
    Task<ApiResponse<LojaDto>> ObterPorSlugAsync(string slug);
    Task<ApiResponse<List<LojaDto>>> ListarTodasAsync();
    Task<ApiResponse<LojaDto>> CriarAsync(CriarLojaDto dto);
    Task<ApiResponse<LojaDto>> AtualizarAsync(int id, CriarLojaDto dto);
    Task<ApiResponse<bool>> ExcluirAsync(int id);
}

public interface ICrmService
{
    Task<ApiResponse<CrmLeadDto>> ObterPorIdAsync(int id);
    Task<ApiResponse<List<CrmLeadDto>>> ListarAsync(PaginacaoDto paginacao, string? tipo = null, string? status = null);
    Task<ApiResponse<CrmLeadDto>> CriarAsync(CriarLeadDto dto);
    Task<ApiResponse<CrmLeadDto>> AtualizarAsync(int id, AtualizarLeadDto dto);
    Task<ApiResponse<bool>> ExcluirAsync(int id);
}

public interface IFinanceiroService
{
    Task<ApiResponse<decimal>> ObterFaturamentoAsync(DateTime inicio, DateTime fim, int? lojaId = null);
    Task<ApiResponse<Dictionary<string, decimal>>> ObterFaturamentoPorLojaAsync(DateTime inicio, DateTime fim);
}

public interface ILogAuditoriaService
{
    Task RegistrarAsync(string tabela, int registroId, string acao, int? usuarioId, string usuarioTipo,
        string? ip, string? userAgent, string? dadosAnteriores, string? dadosNovos, string? endpoint);
}

public interface INotificacaoService
{
    Task CriarNotificacaoAsync(string tipo, string titulo, string mensagem, string destinatarioTipo, int? destinatarioId = null, string? linkAcao = null);
    Task EnviarConfirmacaoCadastroAsync(Cliente cliente, string linkConfirmacao);
}

public interface IConfiguracaoService
{
    Task<string?> ObterValorAsync(string chave);
    Task<T?> ObterValorAsync<T>(string chave);
    Task AtualizarValorAsync(string chave, string valor);
}

// Interfaces de Integração
public interface IMercadoPagoService
{
    Task<string> CriarPreferenciaAsync(PedidoDto pedido);
    Task<bool> ProcessarWebhookAsync(string payload);
}

public interface IMelhorEnvioService
{
    Task<string?> CalcularFreteAsync(string cepOrigem, string cepDestino, decimal peso, decimal altura, decimal largura, decimal comprimento);
    Task<string?> GerarEtiquetaAsync(int pedidoId);
}

public interface IMarketplaceHubService
{
    Task<bool> SincronizarProdutosAsync(int marketplaceId);
    Task<bool> ImportarPedidosAsync(int marketplaceId);
}

public interface IDropshippingService
{
    Task<bool> EnviarPedidoFornecedorAsync(int pedidoId);
    Task<bool> SincronizarEstoqueAsync(int fornecedorId);
}
