using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexumAltivon.ERP.DTOs;
using NexumAltivon.ERP.Services;
using System.Security.Claims;

namespace NexumAltivon.ERP.Controllers
{
    [ApiController]
    [Route("api/erp/[controller]")]
    [Authorize(Roles = "SuperAdmin,Admin,Gerente,Vendedor,Suporte")]
    public class CrmController : ControllerBase
    {
        private readonly ICrmService _crmService;

        public CrmController(ICrmService crmService)
        {
            _crmService = crmService;
        }

        private string UsuarioAtual => User.FindFirst(ClaimTypes.Email)?.Value ?? "sistema";

        [HttpGet("leads")]
        public async Task<ActionResult<List<LeadCRMDto>>> ListarLeads(
            [FromQuery] string? status,
            [FromQuery] string? origem,
            [FromQuery] string? tipo,
            [FromQuery] string? responsavel)
        {
            var leads = await _crmService.ListarLeadsAsync(status, origem, tipo, responsavel);
            return Ok(leads);
        }

        [HttpGet("leads/{id}")]
        public async Task<ActionResult<LeadCRMDto>> ObterLead(int id)
        {
            var lead = await _crmService.ObterLeadPorIdAsync(id);
            if (lead == null) return NotFound();
            return Ok(lead);
        }

        [HttpPost("leads")]
        public async Task<ActionResult<LeadCRMDto>> CriarLead([FromBody] CriarLeadCRMDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var lead = await _crmService.CriarLeadAsync(dto, UsuarioAtual);
            return CreatedAtAction(nameof(ObterLead), new { id = lead.Id }, lead);
        }

        [HttpPut("leads/status")]
        public async Task<ActionResult<LeadCRMDto>> AtualizarStatus([FromBody] AtualizarStatusLeadDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var lead = await _crmService.AtualizarStatusLeadAsync(dto, UsuarioAtual);
            return Ok(lead);
        }

        [HttpPost("leads/{leadId}/converter")]
        public async Task<IActionResult> ConverterLead(int leadId, [FromQuery] int clienteId)
        {
            var resultado = await _crmService.ConverterLeadAsync(leadId, clienteId, UsuarioAtual);
            if (!resultado) return BadRequest("Erro ao converter lead");
            return Ok(new { sucesso = true });
        }

        [HttpGet("leads/{leadId}/interacoes")]
        public async Task<ActionResult<List<InteracaoCRMDto>>> ListarInteracoes(int leadId)
        {
            var interacoes = await _crmService.ListarInteracoesAsync(leadId);
            return Ok(interacoes);
        }

        [HttpPost("interacoes")]
        public async Task<ActionResult<InteracaoCRMDto>> RegistrarInteracao([FromBody] CriarInteracaoCRMDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var interacao = await _crmService.RegistrarInteracaoAsync(dto, UsuarioAtual);
            return Ok(interacao);
        }

        [HttpGet("tarefas")]
        public async Task<ActionResult<List<TarefaCRMDto>>> ListarTarefas(
            [FromQuery] string? status,
            [FromQuery] string? responsavel,
            [FromQuery] bool? atrasadas)
        {
            var tarefas = await _crmService.ListarTarefasAsync(status, responsavel, atrasadas);
            return Ok(tarefas);
        }

        [HttpPost("tarefas")]
        public async Task<ActionResult<TarefaCRMDto>> CriarTarefa([FromBody] CriarTarefaCRMDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var tarefa = await _crmService.CriarTarefaAsync(dto, UsuarioAtual);
            return Ok(tarefa);
        }

        [HttpPut("tarefas/{tarefaId}/concluir")]
        public async Task<IActionResult> ConcluirTarefa(int tarefaId)
        {
            var resultado = await _crmService.ConcluirTarefaAsync(tarefaId, UsuarioAtual);
            if (!resultado) return NotFound();
            return Ok(new { sucesso = true });
        }

        [HttpGet("pipeline")]
        public async Task<ActionResult<List<PipelineCRMDto>>> ObterPipeline()
        {
            var pipeline = await _crmService.ObterPipelineAsync();
            return Ok(pipeline);
        }

        [HttpGet("resumo")]
        public async Task<ActionResult<ResumoCrmDto>> ObterResumo()
        {
            var resumo = await _crmService.ObterResumoCrmAsync();
            return Ok(resumo);
        }
    }
}
