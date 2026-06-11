using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Services;

namespace NexumAltivon.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarrinhoController : ControllerBase
    {
        private readonly ICarrinhoService _carrinhoService;
        private readonly IAuthService _auth;
        private const string SESSAO_COOKIE = "nx_session_id";

        public CarrinhoController(ICarrinhoService carrinhoService, IAuthService auth)
        {
            _carrinhoService = carrinhoService;
            _auth = auth;
        }

        private string ObterSessaoId()
        {
            var sessao = Request.Cookies[SESSAO_COOKIE];
            if (string.IsNullOrEmpty(sessao))
            {
                sessao = Guid.NewGuid().ToString("N");
                Response.Cookies.Append(SESSAO_COOKIE, sessao, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(30),
                    MaxAge = TimeSpan.FromDays(30)
                });
            }
            return sessao;
        }

        private int? ObterClienteId()
        {
            var userId = User.Identity?.IsAuthenticated == true ? User.FindFirst("sub")?.Value : null;
            return int.TryParse(userId, out var id) ? id : null;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<CarrinhoDto>> ObterCarrinho()
        {
            var sessaoId = ObterSessaoId();
            var clienteId = ObterClienteId();
            var carrinho = await _carrinhoService.ObterCarrinhoAsync(sessaoId, clienteId);
            return Ok(carrinho);
        }

        [HttpPost("itens")]
        [AllowAnonymous]
        public async Task<ActionResult<CarrinhoDto>> AdicionarItem([FromBody] AdicionarItemCarrinhoRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var sessaoId = ObterSessaoId();
            var clienteId = ObterClienteId();
            var carrinho = await _carrinhoService.AdicionarItemAsync(sessaoId, clienteId, request);
            return Ok(carrinho);
        }

        [HttpPut("itens/{itemId}")]
        [AllowAnonymous]
        public async Task<ActionResult<CarrinhoDto>> AtualizarQuantidade(int itemId, [FromBody] AtualizarQuantidadeRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var sessaoId = ObterSessaoId();
            var carrinho = await _carrinhoService.AtualizarQuantidadeAsync(sessaoId, itemId, request.Quantidade);
            return Ok(carrinho);
        }

        [HttpDelete("itens/{itemId}")]
        [AllowAnonymous]
        public async Task<ActionResult<CarrinhoDto>> RemoverItem(int itemId)
        {
            var sessaoId = ObterSessaoId();
            var carrinho = await _carrinhoService.RemoverItemAsync(sessaoId, itemId);
            return Ok(carrinho);
        }

        [HttpPost("cupom")]
        [AllowAnonymous]
        public async Task<ActionResult<CarrinhoDto>> AplicarCupom([FromBody] AplicarCupomRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var sessaoId = ObterSessaoId();
            var carrinho = await _carrinhoService.AplicarCupomAsync(sessaoId, request.CodigoCupom);
            return Ok(carrinho);
        }

        [HttpDelete("cupom")]
        [AllowAnonymous]
        public async Task<ActionResult<CarrinhoDto>> RemoverCupom()
        {
            var sessaoId = ObterSessaoId();
            var carrinho = await _carrinhoService.RemoverCupomAsync(sessaoId);
            return Ok(carrinho);
        }

        [HttpPost("migrar")]
        [Authorize]
        public async Task<ActionResult<bool>> MigrarCarrinho()
        {
            var sessaoId = ObterSessaoId();
            var clienteId = ObterClienteId();
            if (!clienteId.HasValue) return Unauthorized();

            var resultado = await _carrinhoService.MigrarCarrinhoSessaoParaClienteAsync(sessaoId, clienteId.Value);
            if (resultado)
            {
                Response.Cookies.Delete(SESSAO_COOKIE);
            }
            return Ok(new { sucesso = resultado });
        }

        [HttpDelete]
        [AllowAnonymous]
        public async Task<IActionResult> LimparCarrinho()
        {
            var sessaoId = ObterSessaoId();
            await _carrinhoService.LimparCarrinhoAsync(sessaoId);
            return NoContent();
        }

        [HttpGet("resumo")]
        [AllowAnonymous]
        public async Task<ActionResult<ResumoCarrinhoDto>> ObterResumo()
        {
            var sessaoId = ObterSessaoId();
            var resumo = await _carrinhoService.ObterResumoAsync(sessaoId);
            return Ok(resumo);
        }
    }
}
