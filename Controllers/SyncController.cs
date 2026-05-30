using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexumAltivon.ERP.Services;

namespace NexumAltivon.ERP.Controllers
{
    /// <summary>
    /// Controle de sincronização ERP ↔ E-Commerce
    /// </summary>
    [ApiController]
    [Route("api/erp/sync")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class SyncController : ControllerBase
    {
        private readonly ISyncErpService _syncService;

        public SyncController(ISyncErpService syncService)
        {
            _syncService = syncService;
        }

        [HttpPost("completo")]
        public async Task<IActionResult> ExecutarSyncCompleto()
        {
            var resultado = await _syncService.ExecutarSyncCompletoAsync();
            return Ok(resultado);
        }

        [HttpPost("agendado")]
        public async Task<IActionResult> ExecutarSyncAgendado()
        {
            var resultado = await _syncService.ExecutarSyncAgendadoAsync();
            return Ok(resultado);
        }

        [HttpPost("produtos")]
        public async Task<IActionResult> SincronizarProdutos()
        {
            var resultado = await _syncService.SincronizarProdutosAsync();
            return Ok(resultado);
        }

        [HttpPost("clientes")]
        public async Task<IActionResult> SincronizarClientes()
        {
            var resultado = await _syncService.SincronizarClientesAsync();
            return Ok(resultado);
        }

        [HttpPost("pedidos")]
        public async Task<IActionResult> SincronizarPedidos()
        {
            var resultado = await _syncService.SincronizarPedidosAsync();
            return Ok(resultado);
        }

        [HttpPost("estoque")]
        public async Task<IActionResult> SincronizarEstoque()
        {
            var resultado = await _syncService.SincronizarEstoqueAsync();
            return Ok(resultado);
        }
    }
}
