using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NexumAltivon.ERP.Data;
using NexumAltivon.ERP.DTOs;
using NexumAltivon.ERP.Models;

namespace NexumAltivon.ERP.Services
{
    /// <summary>
    /// Serviço de CRM completo — leads, pipeline, interações e tarefas
    /// </summary>
    public interface ICrmService
    {
        Task<LeadCRMDto> CriarLeadAsync(CriarLeadCRMDto dto, string responsavel);
        Task<LeadCRMDto> AtualizarStatusLeadAsync(int id, AtualizarStatusLeadDto dto);
        Task<LeadCRMDto> AtualizarStatusLeadAsync(AtualizarStatusLeadDto dto, string responsavel);
        Task<LeadCRMDto> ConverterLeadAsync(int id, int? clienteId);
        Task<bool> ConverterLeadAsync(int id, int? clienteId, string responsavel);
        Task<IEnumerable<LeadCRMDto>> ListarLeadsAsync(string? status = null, string? origem = null, string? tipo = null);
        Task<IEnumerable<LeadCRMDto>> ListarLeadsAsync(string? status, string? origem, string? tipo, string? responsavel);
        Task<LeadCRMDto?> ObterLeadPorIdAsync(int id);
        Task<IEnumerable<PipelineCRMDto>> ObterPipelineAsync();
        Task<InteracaoCRMDto> RegistrarInteracaoAsync(CriarInteracaoCRMDto dto);
        Task<InteracaoCRMDto> RegistrarInteracaoAsync(CriarInteracaoCRMDto dto, string responsavel);
        Task<IEnumerable<InteracaoCRMDto>> ListarInteracoesAsync(int leadId);
        Task<TarefaCRMDto> CriarTarefaAsync(CriarTarefaCRMDto dto);
        Task<TarefaCRMDto> CriarTarefaAsync(CriarTarefaCRMDto dto, string responsavel);
        Task<TarefaCRMDto> ConcluirTarefaAsync(int id);
        Task<bool> ConcluirTarefaAsync(int id, string responsavel);
        Task<IEnumerable<TarefaCRMDto>> ListarTarefasAsync(string? status = null, string? responsavel = null);
        Task<IEnumerable<TarefaCRMDto>> ListarTarefasAsync(string? status, string? responsavel, bool? atrasadas);
        Task<ResumoCrmDto> ObterResumoCrmAsync();
    }

    public class CrmService : ICrmService
    {
        private readonly NexumDbContext _context;

        public CrmService(NexumDbContext context)
        {
            _context = context;
        }

        public async Task<LeadCRMDto> CriarLeadAsync(CriarLeadCRMDto dto, string responsavel)
        {
            var lead = new LeadCRM
            {
                Nome = dto.Nome,
                Email = dto.Email,
                Telefone = dto.Telefone,
                WhatsApp = dto.WhatsApp,
                Origem = dto.Origem,
                Status = "Novo",
                Tipo = dto.Tipo,
                Observacoes = dto.Observacoes,
                Empresa = dto.Empresa,
                Cargo = dto.Cargo,
                Cnpj = dto.Cnpj,
                Cpf = dto.Cpf,
                ResponsavelNome = responsavel,
                ValorEstimado = dto.ValorEstimado,
                Probabilidade = dto.Probabilidade ?? 10,
                DataPrevisaoFechamento = dto.DataPrevisaoFechamento,
                CriadoEm = DateTime.Now,
                CriadoPor = responsavel
            };

            _context.LeadsCRM.Add(lead);
            await _context.SaveChangesAsync();

            // Registra interação automática de criação
            var interacao = new InteracaoCRM
            {
                LeadId = lead.Id,
                Tipo = "Nota",
                Descricao = "Lead criado no sistema",
                DataInteracao = DateTime.Now,
                Responsavel = responsavel,
                CriadoEm = DateTime.Now
            };
            _context.InteracoesCRM.Add(interacao);
            await _context.SaveChangesAsync();

            return await ObterLeadDtoPorIdAsync(lead.Id);
        }

        public async Task<LeadCRMDto> AtualizarStatusLeadAsync(int id, AtualizarStatusLeadDto dto)
        {
            var lead = await _context.LeadsCRM.FindAsync(id)
                ?? throw new Exception("Lead não encontrado.");

            var statusAnterior = lead.Status;
            lead.Status = dto.NovoStatus;
            lead.AtualizadoEm = DateTime.Now;
            lead.DataUltimoContato = DateTime.Now;

            // Se convertido, registra data
            if (dto.NovoStatus == "Convertido")
                lead.DataConversao = DateTime.Now;

            // Registra interação de mudança de status
            var interacao = new InteracaoCRM
            {
                LeadId = lead.Id,
                Tipo = "Nota",
                Descricao = $"Status alterado de '{statusAnterior}' para '{dto.NovoStatus}'. Motivo: {dto.Motivo}",
                DataInteracao = DateTime.Now,
                Responsavel = lead.ResponsavelNome,
                CriadoEm = DateTime.Now
            };
            _context.InteracoesCRM.Add(interacao);

            await _context.SaveChangesAsync();
            return await ObterLeadDtoPorIdAsync(lead.Id);
        }

        public Task<LeadCRMDto> AtualizarStatusLeadAsync(AtualizarStatusLeadDto dto, string responsavel)
        {
            return AtualizarStatusLeadAsync(dto.LeadId, dto);
        }

        public async Task<LeadCRMDto> ConverterLeadAsync(int id, int? clienteId)
        {
            var lead = await _context.LeadsCRM.FindAsync(id)
                ?? throw new Exception("Lead não encontrado.");

            lead.Status = "Convertido";
            lead.DataConversao = DateTime.Now;
            lead.DataUltimoContato = DateTime.Now;
            lead.ClienteConvertidoId = clienteId;
            lead.AtualizadoEm = DateTime.Now;

            var interacao = new InteracaoCRM
            {
                LeadId = lead.Id,
                Tipo = "Nota",
                Descricao = clienteId.HasValue
                    ? $"Lead convertido em cliente (ID: {clienteId})"
                    : "Lead convertido (cliente ainda não vinculado)",
                DataInteracao = DateTime.Now,
                Responsavel = lead.ResponsavelNome,
                CriadoEm = DateTime.Now
            };
            _context.InteracoesCRM.Add(interacao);

            await _context.SaveChangesAsync();
            return await ObterLeadDtoPorIdAsync(lead.Id);
        }

        public async Task<bool> ConverterLeadAsync(int id, int? clienteId, string responsavel)
        {
            await ConverterLeadAsync(id, clienteId);
            return true;
        }

        public async Task<IEnumerable<LeadCRMDto>> ListarLeadsAsync(string? status = null, string? origem = null, string? tipo = null)
        {
            var query = _context.LeadsCRM.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(l => l.Status == status);

            if (!string.IsNullOrEmpty(origem))
                query = query.Where(l => l.Origem == origem);

            if (!string.IsNullOrEmpty(tipo))
                query = query.Where(l => l.Tipo == tipo);

            return await query.OrderByDescending(l => l.CriadoEm)
                .Select(l => new LeadCRMDto
                {
                    Id = l.Id,
                    Nome = l.Nome,
                    Email = l.Email,
                    Telefone = l.Telefone,
                    WhatsApp = l.WhatsApp,
                    Origem = l.Origem,
                    Status = l.Status,
                    Tipo = l.Tipo,
                    Observacoes = l.Observacoes,
                    Empresa = l.Empresa,
                    Cargo = l.Cargo,
                    Cnpj = l.Cnpj,
                    Cpf = l.Cpf,
                    ResponsavelNome = l.ResponsavelNome,
                    ValorEstimado = l.ValorEstimado,
                    Probabilidade = l.Probabilidade,
                    DataPrevisaoFechamento = l.DataPrevisaoFechamento,
                    DataUltimoContato = l.DataUltimoContato,
                    DataConversao = l.DataConversao,
                    CriadoEm = l.CriadoEm
                }).ToListAsync();
        }

        public async Task<IEnumerable<LeadCRMDto>> ListarLeadsAsync(string? status, string? origem, string? tipo, string? responsavel)
        {
            var leads = await ListarLeadsAsync(status, origem, tipo);
            return string.IsNullOrEmpty(responsavel)
                ? leads
                : leads.Where(l => l.ResponsavelNome == responsavel);
        }

        public async Task<LeadCRMDto?> ObterLeadPorIdAsync(int id)
        {
            return await ObterLeadDtoPorIdAsync(id);
        }

        public async Task<IEnumerable<PipelineCRMDto>> ObterPipelineAsync()
        {
            var leads = await _context.LeadsCRM
                .Where(l => l.Status != "Convertido" && l.Status != "Perdido")
                .ToListAsync();

            return leads
                .GroupBy(l => l.Status)
                .Select(g => new PipelineCRMDto
                {
                    Status = g.Key,
                    Quantidade = g.Count(),
                    ValorTotal = g.Sum(l => l.ValorEstimado ?? 0),
                    ProbabilidadeMedia = g.Any(l => l.Probabilidade.HasValue)
                        ? (int)g.Average(l => l.Probabilidade ?? 0)
                        : 0
                })
                .OrderBy(p =>
                    p.Status == "Novo" ? 1 :
                    p.Status == "EmAtendimento" ? 2 :
                    p.Status == "Qualificado" ? 3 :
                    p.Status == "Proposta" ? 4 : 5)
                .ToList();
        }

        public async Task<InteracaoCRMDto> RegistrarInteracaoAsync(CriarInteracaoCRMDto dto)
        {
            var interacao = new InteracaoCRM
            {
                LeadId = dto.LeadId,
                Tipo = dto.Tipo,
                Descricao = dto.Descricao,
                DataInteracao = DateTime.Now,
                Responsavel = dto.Responsavel,
                Anotacoes = dto.Anotacoes,
                CriadoEm = DateTime.Now
            };

            // Atualiza data do último contato no lead
            var lead = await _context.LeadsCRM.FindAsync(dto.LeadId);
            if (lead != null)
            {
                lead.DataUltimoContato = DateTime.Now;
                lead.AtualizadoEm = DateTime.Now;
            }

            _context.InteracoesCRM.Add(interacao);
            await _context.SaveChangesAsync();

            return new InteracaoCRMDto
            {
                Id = interacao.Id,
                LeadId = interacao.LeadId,
                Tipo = interacao.Tipo,
                Descricao = interacao.Descricao,
                DataInteracao = interacao.DataInteracao,
                Responsavel = interacao.Responsavel
            };
        }

        public Task<InteracaoCRMDto> RegistrarInteracaoAsync(CriarInteracaoCRMDto dto, string responsavel)
        {
            dto.Responsavel ??= responsavel;
            return RegistrarInteracaoAsync(dto);
        }

        public async Task<IEnumerable<InteracaoCRMDto>> ListarInteracoesAsync(int leadId)
        {
            return await _context.InteracoesCRM
                .Where(i => i.LeadId == leadId)
                .OrderByDescending(i => i.DataInteracao)
                .Select(i => new InteracaoCRMDto
                {
                    Id = i.Id,
                    LeadId = i.LeadId,
                    Tipo = i.Tipo,
                    Descricao = i.Descricao,
                    DataInteracao = i.DataInteracao,
                    Responsavel = i.Responsavel
                }).ToListAsync();
        }

        public async Task<TarefaCRMDto> CriarTarefaAsync(CriarTarefaCRMDto dto)
        {
            var tarefa = new TarefaCRM
            {
                Titulo = dto.Titulo,
                Descricao = dto.Descricao,
                Tipo = dto.Tipo,
                Prioridade = dto.Prioridade,
                Status = "Pendente",
                LeadId = dto.LeadId,
                ClienteId = dto.ClienteId,
                DataVencimento = dto.DataVencimento,
                Responsavel = dto.Responsavel,
                CriadoEm = DateTime.Now
            };

            _context.TarefasCRM.Add(tarefa);
            await _context.SaveChangesAsync();

            return new TarefaCRMDto
            {
                Id = tarefa.Id,
                Titulo = tarefa.Titulo,
                Descricao = tarefa.Descricao,
                Tipo = tarefa.Tipo,
                Prioridade = tarefa.Prioridade,
                Status = tarefa.Status,
                LeadId = tarefa.LeadId,
                DataVencimento = tarefa.DataVencimento,
                Responsavel = tarefa.Responsavel,
                CriadoEm = tarefa.CriadoEm
            };
        }

        public Task<TarefaCRMDto> CriarTarefaAsync(CriarTarefaCRMDto dto, string responsavel)
        {
            dto.Responsavel ??= responsavel;
            return CriarTarefaAsync(dto);
        }

        public async Task<TarefaCRMDto> ConcluirTarefaAsync(int id)
        {
            var tarefa = await _context.TarefasCRM.FindAsync(id)
                ?? throw new Exception("Tarefa não encontrada.");

            tarefa.Status = "Concluida";
            tarefa.DataConclusao = DateTime.Now;
            tarefa.AtualizadoEm = DateTime.Now;

            await _context.SaveChangesAsync();

            return new TarefaCRMDto
            {
                Id = tarefa.Id,
                Titulo = tarefa.Titulo,
                Descricao = tarefa.Descricao,
                Tipo = tarefa.Tipo,
                Prioridade = tarefa.Prioridade,
                Status = tarefa.Status,
                DataVencimento = tarefa.DataVencimento,
                DataConclusao = tarefa.DataConclusao,
                Responsavel = tarefa.Responsavel,
                CriadoEm = tarefa.CriadoEm
            };
        }

        public async Task<bool> ConcluirTarefaAsync(int id, string responsavel)
        {
            await ConcluirTarefaAsync(id);
            return true;
        }

        public async Task<IEnumerable<TarefaCRMDto>> ListarTarefasAsync(string? status = null, string? responsavel = null)
        {
            var query = _context.TarefasCRM.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.Status == status);

            if (!string.IsNullOrEmpty(responsavel))
                query = query.Where(t => t.Responsavel == responsavel);

            return await query.OrderBy(t => t.DataVencimento)
                .Select(t => new TarefaCRMDto
                {
                    Id = t.Id,
                    Titulo = t.Titulo,
                    Descricao = t.Descricao,
                    Tipo = t.Tipo,
                    Prioridade = t.Prioridade,
                    Status = t.Status,
                    LeadId = t.LeadId,
                    DataVencimento = t.DataVencimento,
                    DataConclusao = t.DataConclusao,
                    Responsavel = t.Responsavel,
                    CriadoEm = t.CriadoEm
                }).ToListAsync();
        }

        public async Task<IEnumerable<TarefaCRMDto>> ListarTarefasAsync(string? status, string? responsavel, bool? atrasadas)
        {
            var tarefas = await ListarTarefasAsync(status, responsavel);
            return atrasadas == true ? tarefas.Where(t => t.Atrasada) : tarefas;
        }

        public async Task<ResumoCrmDto> ObterResumoCrmAsync()
        {
            var leadsAtivos = await _context.LeadsCRM.CountAsync(l => l.Status != "Convertido" && l.Status != "Perdido");
            var leadsConvertidos = await _context.LeadsCRM.CountAsync(l => l.Status == "Convertido");
            var tarefasPendentes = await _context.TarefasCRM.CountAsync(t => t.Status != "Concluida");
            var valorPipeline = await _context.LeadsCRM
                .Where(l => l.Status != "Convertido" && l.Status != "Perdido")
                .SumAsync(l => l.ValorEstimado ?? 0);

            return new ResumoCrmDto
            {
                LeadsAtivos = leadsAtivos,
                LeadsConvertidos = leadsConvertidos,
                TarefasPendentes = tarefasPendentes,
                ValorPipeline = valorPipeline
            };
        }

        private async Task<LeadCRMDto?> ObterLeadDtoPorIdAsync(int id)
        {
            return await _context.LeadsCRM
                .Where(l => l.Id == id)
                .Select(l => new LeadCRMDto
                {
                    Id = l.Id,
                    Nome = l.Nome,
                    Email = l.Email,
                    Telefone = l.Telefone,
                    WhatsApp = l.WhatsApp,
                    Origem = l.Origem,
                    Status = l.Status,
                    Tipo = l.Tipo,
                    Observacoes = l.Observacoes,
                    Empresa = l.Empresa,
                    Cargo = l.Cargo,
                    Cnpj = l.Cnpj,
                    Cpf = l.Cpf,
                    ResponsavelNome = l.ResponsavelNome,
                    ValorEstimado = l.ValorEstimado,
                    Probabilidade = l.Probabilidade,
                    DataPrevisaoFechamento = l.DataPrevisaoFechamento,
                    DataUltimoContato = l.DataUltimoContato,
                    DataConversao = l.DataConversao,
                    CriadoEm = l.CriadoEm
                }).FirstOrDefaultAsync();
        }
    }
}
