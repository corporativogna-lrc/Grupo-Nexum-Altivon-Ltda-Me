using Microsoft.EntityFrameworkCore;
using NexumAltivon_ERP.Data;
using NexumAltivon_ERP.Models.Financeiro;
using NexumAltivon_ERP.DTOs.Financeiro;
using AutoMapper;

namespace NexumAltivon_ERP.Services.Financeiro
{
    public interface IPlanoContasService
    {
        Task<List<PlanoContasDto>> ListarPlanoContasAsync();
        Task<List<CentroCustoDto>> ListarCentrosCustoAsync();
        Task<PlanoContasDto> CriarPlanoContasAsync(PlanoContasDto dto);
        Task<CentroCustoDto> CriarCentroCustoAsync(CentroCustoDto dto);
    }

    public class PlanoContasService : IPlanoContasService
    {
        private readonly GenesisDbContext _context;
        private readonly IMapper _mapper;

        public PlanoContasService(GenesisDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<PlanoContasDto>> ListarPlanoContasAsync()
        {
            var planos = await _context.PlanosContas
                .AsNoTracking()
                .Include(p => p.Pai)
                .Where(p => p.Ativo)
                .OrderBy(p => p.Codigo)
                .ToListAsync();

            return planos.Select(p =>
            {
                var dto = _mapper.Map<PlanoContasDto>(p);
                dto.PaiNome = p.Pai?.Nome ?? "";
                return dto;
            }).ToList();
        }

        public async Task<List<CentroCustoDto>> ListarCentrosCustoAsync()
        {
            var centros = await _context.CentrosCusto
                .AsNoTracking()
                .Where(c => c.Ativo)
                .OrderBy(c => c.Codigo)
                .ToListAsync();

            return _mapper.Map<List<CentroCustoDto>>(centros);
        }

        public async Task<PlanoContasDto> CriarPlanoContasAsync(PlanoContasDto dto)
        {
            var entity = _mapper.Map<PlanoContas>(dto);
            entity.Ativo = true;
            _context.PlanosContas.Add(entity);
            await _context.SaveChangesAsync();
            return _mapper.Map<PlanoContasDto>(entity);
        }

        public async Task<CentroCustoDto> CriarCentroCustoAsync(CentroCustoDto dto)
        {
            var entity = _mapper.Map<CentroCusto>(dto);
            entity.Ativo = true;
            _context.CentrosCusto.Add(entity);
            await _context.SaveChangesAsync();
            return _mapper.Map<CentroCustoDto>(entity);
        }
    }
}