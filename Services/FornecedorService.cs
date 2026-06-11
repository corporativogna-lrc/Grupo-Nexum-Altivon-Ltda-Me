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
    public interface IFornecedorService
    {
        Task<IEnumerable<FornecedorDto>> ListarAsync(string? status = null, string? segmento = null, bool? dropshipping = null);
        Task<FornecedorDto?> ObterPorIdAsync(int id);
        Task<FornecedorDto> CriarAsync(CriarFornecedorDto dto, string usuario);
        Task<FornecedorDto> AtualizarAsync(int id, CriarFornecedorDto dto);
        Task AvaliarAsync(int fornecedorId, object dto, string usuario);
        Task<IEnumerable<object>> ListarAvaliacoesAsync(int fornecedorId);
    }

    public class FornecedorService : IFornecedorService
    {
        private readonly NexumDbContext _context;

        public FornecedorService(NexumDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FornecedorDto>> ListarAsync(string? status = null, string? segmento = null, bool? dropshipping = null)
        {
            var query = _context.Fornecedores.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(f => f.Status == status);

            if (!string.IsNullOrEmpty(segmento))
                query = query.Where(f => f.Segmento == segmento);

            if (dropshipping.HasValue)
                query = query.Where(f => f.Dropshipping == dropshipping.Value);

            return await query
                .Select(f => new FornecedorDto
                {
                    Id = f.Id,
                    RazaoSocial = f.RazaoSocial,
                    NomeFantasia = f.NomeFantasia,
                    Cnpj = f.Cnpj,
                    Email = f.Email,
                    Telefone = f.Telefone,
                    Celular = f.Celular,
                    Cidade = f.Cidade,
                    Uf = f.Uf,
                    Segmento = f.Segmento,
                    Status = f.Status,
                    LimiteCredito = f.LimiteCredito,
                    PrazoPagamentoDias = f.PrazoPagamentoDias,
                    Dropshipping = f.Dropshipping,
                    ComissaoDropshipping = f.ComissaoDropshipping,
                    MediaAvaliacao = _context.AvaliacoesFornecedor
                        .Where(a => a.FornecedorId == f.Id)
                        .Average(a => (double?)a.Nota),
                    CriadoEm = f.CriadoEm
                }).ToListAsync();
        }

        public async Task<FornecedorDto?> ObterPorIdAsync(int id)
        {
            var f = await _context.Fornecedores.FindAsync(id);
            if (f == null) return null;

            return new FornecedorDto
            {
                Id = f.Id,
                RazaoSocial = f.RazaoSocial,
                NomeFantasia = f.NomeFantasia,
                Cnpj = f.Cnpj,
                Email = f.Email,
                Telefone = f.Telefone,
                Celular = f.Celular,
                Cidade = f.Cidade,
                Uf = f.Uf,
                Segmento = f.Segmento,
                Status = f.Status,
                LimiteCredito = f.LimiteCredito,
                PrazoPagamentoDias = f.PrazoPagamentoDias,
                Dropshipping = f.Dropshipping,
                ComissaoDropshipping = f.ComissaoDropshipping,
                CriadoEm = f.CriadoEm
            };
        }

        public async Task<FornecedorDto> CriarAsync(CriarFornecedorDto dto, string usuario)
        {
            var fornecedor = new Fornecedor
            {
                RazaoSocial = dto.RazaoSocial,
                NomeFantasia = dto.NomeFantasia,
                Cnpj = dto.Cnpj,
                Email = dto.Email,
                Telefone = dto.Telefone,
                Celular = dto.Celular,
                Endereco = dto.Endereco,
                Cidade = dto.Cidade,
                Uf = dto.Uf,
                Cep = dto.Cep,
                Segmento = dto.Segmento,
                LimiteCredito = dto.LimiteCredito,
                PrazoPagamentoDias = dto.PrazoPagamentoDias,
                Dropshipping = dto.Dropshipping,
                ComissaoDropshipping = dto.ComissaoDropshipping,
                Observacoes = dto.Observacoes,
                Status = "Ativo",
                CriadoEm = DateTime.Now,
                CriadoPor = usuario
            };

            _context.Fornecedores.Add(fornecedor);
            await _context.SaveChangesAsync();

            return await ObterPorIdAsync(fornecedor.Id) ?? throw new Exception("Erro ao recuperar fornecedor criado.");
        }

        public async Task<FornecedorDto> AtualizarAsync(int id, CriarFornecedorDto dto)
        {
            var fornecedor = await _context.Fornecedores.FindAsync(id)
                ?? throw new Exception("Fornecedor não encontrado.");

            fornecedor.RazaoSocial = dto.RazaoSocial;
            fornecedor.NomeFantasia = dto.NomeFantasia;
            fornecedor.Email = dto.Email;
            fornecedor.Telefone = dto.Telefone;
            fornecedor.Celular = dto.Celular;
            fornecedor.Endereco = dto.Endereco;
            fornecedor.Cidade = dto.Cidade;
            fornecedor.Uf = dto.Uf;
            fornecedor.Cep = dto.Cep;
            fornecedor.Segmento = dto.Segmento;
            fornecedor.LimiteCredito = dto.LimiteCredito;
            fornecedor.PrazoPagamentoDias = dto.PrazoPagamentoDias;
            fornecedor.Dropshipping = dto.Dropshipping;
            fornecedor.ComissaoDropshipping = dto.ComissaoDropshipping;
            fornecedor.Observacoes = dto.Observacoes;
            fornecedor.AtualizadoEm = DateTime.Now;

            await _context.SaveChangesAsync();
            return await ObterPorIdAsync(id) ?? throw new Exception("Erro ao recuperar fornecedor atualizado.");
        }

        public async Task AvaliarAsync(int fornecedorId, object dto, string usuario)
        {
            // Implementação simplificada — em produção, usar DTO tipado
            var avaliacao = new AvaliacaoFornecedor
            {
                FornecedorId = fornecedorId,
                Nota = 5, // Placeholder
                Comentario = "Avaliação registrada",
                CriadoEm = DateTime.Now,
                CriadoPor = usuario
            };

            _context.AvaliacoesFornecedor.Add(avaliacao);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<object>> ListarAvaliacoesAsync(int fornecedorId)
        {
            return await _context.AvaliacoesFornecedor
                .Where(a => a.FornecedorId == fornecedorId)
                .OrderByDescending(a => a.CriadoEm)
                .Select(a => new
                {
                    a.Id,
                    a.Nota,
                    a.Comentario,
                    a.CategoriaAvaliacao,
                    a.CriadoEm,
                    a.CriadoPor
                }).ToListAsync<object>();
        }
    }
}
