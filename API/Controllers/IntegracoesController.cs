using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Services;

namespace NexumAltivon.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Gerente")]
    public class IntegracoesController : ControllerBase
    {
        private readonly IMarketplaceHubService _hub;
        private readonly IMercadoLivreService _ml;
        private readonly IDropshippingService _dropshipping;
        private readonly ILogisticaService _logistica;
        private readonly IErpSyncService _erp;
        private readonly IMarketplaceSyncService _sync;

        public IntegracoesController(
            IMarketplaceHubService hub,
            IMercadoLivreService ml,
            IDropshippingService dropshipping,
            ILogisticaService logistica,
            IErpSyncService erp,
            IMarketplaceSyncService sync)
        {
            _hub = hub;
            _ml = ml;
            _dropshipping = dropshipping;
            _logistica = logistica;
            _erp = erp;
            _sync = sync;
        }

        // ========== MARKETPLACES ==========
        [HttpPost("marketplaces/sync")]
        public async Task<ActionResult<List<SyncStatusDto>>> SincronizarLote([FromBody] SincronizarProdutoRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var resultado = await _hub.SincronizarLoteAsync(new List<int> { request.ProdutoId }, request.Canais);
            return Ok(resultado);
        }

        [HttpPost("marketplaces/sync-lote")]
        public async Task<ActionResult<List<SyncStatusDto>>> SincronizarLoteMultiplo([FromBody] List<SincronizarProdutoRequest> requests)
        {
            var produtoIds = requests.Select(r => r.ProdutoId).ToList();
            var canais = requests.SelectMany(r => r.Canais).Distinct().ToList();
            var resultado = await _hub.SincronizarLoteAsync(produtoIds, canais);
            return Ok(resultado);
        }

        [HttpGet("marketplaces/relatorio")]
        public async Task<ActionResult<RelatorioSyncDto>> RelatorioSync([FromQuery] DateTime inicio, [FromQuery] DateTime fim)
        {
            var relatorio = await _hub.ObterRelatorioSyncAsync(inicio, fim);
            return Ok(relatorio);
        }

        [HttpGet("marketplaces/status/{produtoId}")]
        public async Task<ActionResult<SyncStatusDto>> StatusSync(int produtoId, [FromQuery] string canal = "mercadolivre")
        {
            var status = canal.ToLower() == "mercadolivre"
                ? await _ml.ObterStatusSyncAsync(produtoId)
                : await _hub.SincronizarProdutoAsync(produtoId, canal, false);
            return Ok(status);
        }

        // ========== MERCADO LIVRE ==========
        [HttpPost("mercadolivre/publicar/{produtoId}")]
        public async Task<ActionResult<MlProdutoPublicadoDto>> PublicarMl(int produtoId, [FromBody] MlPublicarProdutoRequest request)
        {
            var resultado = await _ml.PublicarProdutoAsync(produtoId, request?.CategoriaMl, request?.PrecoEspecifico, request?.EstoqueEspecifico);
            return Ok(resultado);
        }

        [HttpPost("mercadolivre/importar-pedidos")]
        public async Task<ActionResult<List<MlPedidoRecebidoDto>>> ImportarPedidosMl()
        {
            var pedidos = await _ml.ImportarPedidosPendentesAsync();
            return Ok(pedidos);
        }

        [HttpPost("mercadolivre/marcar-enviado/{mlOrderId}")]
        public async Task<ActionResult<bool>> MarcarEnviadoMl(string mlOrderId, [FromQuery] string codigoRastreio)
        {
            var resultado = await _ml.MarcarEnviadoAsync(mlOrderId, codigoRastreio);
            return Ok(resultado);
        }

        // ========== DROPSHIPPING ==========
        [HttpPost("dropshipping/roteiar")]
        public async Task<ActionResult<DropshippingPedidoDto>> RoteiarPedido([FromBody] RoteiarPedidoRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var resultado = await _dropshipping.RoteiarPedidoAsync(request.PedidoId, request.AutoSelecionarFornecedor);
            return Ok(resultado);
        }

        [HttpGet("dropshipping/pendentes")]
        public async Task<ActionResult<List<DropshippingPedidoDto>>> PedidosPendentesDropshipping()
        {
            var pendentes = await _dropshipping.ObterPedidosPendentesAsync();
            return Ok(pendentes);
        }

        [HttpPut("dropshipping/{dropshippingId}/status")]
        public async Task<ActionResult<bool>> AtualizarStatusDropshipping(int dropshippingId, [FromBody] AtualizarStatusDropshippingRequest request)
        {
            var resultado = await _dropshipping.AtualizarStatusAsync(dropshippingId, request.Status, request.CodigoRastreio, request.UrlRastreio);
            return Ok(resultado);
        }

        [HttpGet("dropshipping/fornecedores")]
        public async Task<ActionResult<List<FornecedorDropshippingDto>>> ListarFornecedores()
        {
            var fornecedores = await _dropshipping.ListarFornecedoresAsync();
            return Ok(fornecedores);
        }

        [HttpGet("dropshipping/comissao/{fornecedorId}")]
        public async Task<ActionResult<ComissaoDropshippingDto>> ComissaoFornecedor(int fornecedorId, [FromQuery] DateTime inicio, [FromQuery] DateTime fim)
        {
            var comissao = await _dropshipping.ObterComissaoFornecedorAsync(fornecedorId, inicio, fim);
            return Ok(comissao);
        }

        // ========== LOGÍSTICA ==========
        [HttpPost("logistica/etiqueta")]
        public async Task<ActionResult<EtiquetaDto>> GerarEtiqueta([FromBody] GerarEtiquetaRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var etiqueta = await _logistica.GerarEtiquetaAsync(request.PedidoId, request.TransportadoraId, request.ServicoFrete);
            return Ok(etiqueta);
        }

        [HttpGet("logistica/rastrear/{codigoRastreio}")]
        [AllowAnonymous]
        public async Task<ActionResult<RastreamentoDto>> Rastrear(string codigoRastreio)
        {
            var rastreamento = await _logistica.RastrearAsync(codigoRastreio);
            if (rastreamento == null) return NotFound();
            return Ok(rastreamento);
        }

        [HttpPut("logistica/status-envio")]
        public async Task<ActionResult<bool>> AtualizarStatusEnvio([FromBody] AtualizarStatusEnvioRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var resultado = await _logistica.AtualizarStatusEnvioAsync(request.PedidoId, request.Status, request.CodigoRastreio, request.DataEnvio, request.DataEntrega);
            return Ok(resultado);
        }

        [HttpGet("logistica/dashboard")]
        public async Task<ActionResult<DashboardLogisticaDto>> DashboardLogistica()
        {
            var dashboard = await _logistica.ObterDashboardAsync();
            return Ok(dashboard);
        }

        [HttpGet("logistica/transportadoras")]
        public async Task<ActionResult<List<TransportadoraDto>>> ListarTransportadoras()
        {
            var transportadoras = await _logistica.ListarTransportadorasAsync();
            return Ok(transportadoras);
        }

        // ========== ERP GenesisGest ==========
        [HttpPost("erp/sync")]
        public async Task<ActionResult<List<ErpSyncResultDto>>> SyncErp([FromBody] ErpSyncRequest request)
        {
            var resultados = new List<ErpSyncResultDto>();
            if (request.Entidade == "PRODUTOS" || string.IsNullOrEmpty(request.Entidade))
                resultados.Add(await _erp.SincronizarProdutosAsync(request.DataInicio, request.DataFim, request.ForcaCompleta));
            if (request.Entidade == "CLIENTES" || string.IsNullOrEmpty(request.Entidade))
                resultados.Add(await _erp.SincronizarClientesAsync(request.DataInicio, request.DataFim, request.ForcaCompleta));
            if (request.Entidade == "PEDIDOS" || string.IsNullOrEmpty(request.Entidade))
                resultados.Add(await _erp.SincronizarPedidosAsync(request.DataInicio, request.DataFim, request.ForcaCompleta));
            if (request.Entidade == "ESTOQUE" || string.IsNullOrEmpty(request.Entidade))
                resultados.Add(await _erp.SincronizarEstoqueAsync());
            return Ok(resultados);
        }

        [HttpGet("erp/status")]
        public async Task<ActionResult<ErpStatusConexaoDto>> StatusErp()
        {
            var status = await _erp.TestarConexaoAsync();
            return Ok(status);
        }

        [HttpGet("erp/configuracao")]
        public async Task<ActionResult<ErpConfiguracaoDto>> ConfiguracaoErp()
        {
            var config = await _erp.ObterConfiguracaoAsync();
            return Ok(config);
        }

        [HttpPut("erp/configuracao")]
        public async Task<ActionResult<bool>> SalvarConfiguracaoErp([FromBody] ErpConfiguracaoDto config)
        {
            var resultado = await _erp.SalvarConfiguracaoAsync(config);
            return Ok(resultado);
        }

        // ========== SYNC AUTOMÁTICO ==========
        [HttpPost("sync/executar-agendado")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ExecutarSyncAgendado()
        {
            await _sync.ExecutarSyncAgendadoAsync();
            return Ok(new { mensagem = "Sincronização agendada executada com sucesso." });
        }

        [HttpGet("sync/logs")]
        public async Task<ActionResult<List<SyncLogDto>>> LogsSync([FromQuery] DateTime inicio, [FromQuery] DateTime fim)
        {
            var logs = await _sync.ObterLogsSyncAsync(inicio, fim);
            return Ok(logs);
        }
    }
}
