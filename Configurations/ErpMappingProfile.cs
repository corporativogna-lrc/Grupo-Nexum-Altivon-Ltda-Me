using AutoMapper;
using NexumAltivon.ERP.DTOs;
using NexumAltivon.ERP.Models;

namespace NexumAltivon.ERP.Configurations;

public class ErpMappingProfile : Profile
{
    public ErpMappingProfile()
    {
        CreateMap<ContaPagar, ContaPagarDto>();
        CreateMap<ContaReceber, ContaReceberDto>();
        CreateMap<NotaFiscal, NotaFiscalDto>();
        CreateMap<ConfiguracaoFiscal, ConfiguracaoFiscalDto>();
        CreateMap<MovimentacaoEstoque, MovimentacaoEstoqueDto>();
        CreateMap<Inventario, InventarioDto>();
        CreateMap<LocalEstoque, LocalEstoqueDto>();
        CreateMap<Fornecedor, FornecedorDto>();
        CreateMap<Compra, CompraDto>();
        CreateMap<Oportunidade, OportunidadeDto>();
        CreateMap<Atendimento, AtendimentoDto>();
        CreateMap<CampanhaMarketing, CampanhaMarketingDto>();
        CreateMap<LeadScore, LeadScoreDto>();
        CreateMap<DrePeriodo, DrePeriodoDto>();
    }
}
