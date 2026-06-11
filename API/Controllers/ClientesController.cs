using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Services;

namespace NexumAltivon.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    /// <summary>
    /// Lista todos os clientes com paginação (requer autenticação)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "Vendedor")]
    [ProducesResponseType(typeof(ApiResponse<List<ClienteDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ClienteDto>>>> Listar([FromQuery] PaginacaoDto paginacao)
    {
        var resultado = await _clienteService.ListarAsync(paginacao);
        Response.Headers.Append("X-Total-Count", resultado.TotalRegistros?.ToString() ?? "0");
        Response.Headers.Append("X-Page-Count", resultado.TotalPaginas?.ToString() ?? "0");
        return Ok(resultado);
    }

    /// <summary>
    /// Obtém um cliente pelo ID
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ClienteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ClienteDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ClienteDto>>> ObterPorId(int id)
    {
        var resultado = await _clienteService.ObterPorIdAsync(id);
        if (!resultado.Sucesso)
            return NotFound(resultado);
        return Ok(resultado);
    }

    /// <summary>
    /// Obtém um cliente pelo e-mail
    /// </summary>
    [HttpGet("email/{email}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ClienteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ClienteDto>>> ObterPorEmail(string email)
    {
        var resultado = await _clienteService.ObterPorEmailAsync(email);
        if (!resultado.Sucesso)
            return NotFound(resultado);
        return Ok(resultado);
    }

    /// <summary>
    /// Cria um novo cliente (público - cadastro no site)
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ClienteDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<ClienteDto>>> Criar([FromBody] CriarClienteDto dto)
    {
        var resultado = await _clienteService.CriarAsync(dto);
        if (!resultado.Sucesso)
            return BadRequest(resultado);
        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Dados?.Id }, resultado);
    }

    /// <summary>
    /// Atualiza um cliente
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ClienteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ClienteDto>>> Atualizar(int id, [FromBody] AtualizarClienteDto dto)
    {
        var resultado = await _clienteService.AtualizarAsync(id, dto);
        if (!resultado.Sucesso)
            return NotFound(resultado);
        return Ok(resultado);
    }

    /// <summary>
    /// Remove um cliente (requer Admin)
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> Excluir(int id)
    {
        var resultado = await _clienteService.ExcluirAsync(id);
        if (!resultado.Sucesso)
            return NotFound(resultado);
        return Ok(resultado);
    }

    /// <summary>
    /// Adiciona um endereço ao cliente
    /// </summary>
    [HttpPost("{id:int}/enderecos")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<EnderecoDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<EnderecoDto>>> AdicionarEndereco(int id, [FromBody] CriarEnderecoDto dto)
    {
        var resultado = await _clienteService.AdicionarEnderecoAsync(id, dto);
        if (!resultado.Sucesso)
            return BadRequest(resultado);
        return CreatedAtAction(nameof(ObterPorId), new { id }, resultado);
    }

    /// <summary>
    /// Remove um endereço do cliente
    /// </summary>
    [HttpDelete("{clienteId:int}/enderecos/{enderecoId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> RemoverEndereco(int clienteId, int enderecoId)
    {
        var resultado = await _clienteService.RemoverEnderecoAsync(clienteId, enderecoId);
        if (!resultado.Sucesso)
            return NotFound(resultado);
        return Ok(resultado);
    }
}
