using AutoMapper;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Models;

namespace NexumAltivon.API.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Usuario
        CreateMap<Usuario, UsuarioDto>();
        CreateMap<RegistrarUsuarioDto, Usuario>();

        // Cliente
        CreateMap<Cliente, ClienteDto>();
        CreateMap<CriarClienteDto, Cliente>();
        CreateMap<AtualizarClienteDto, Cliente>();

        // Endereco
        CreateMap<Endereco, EnderecoDto>();
        CreateMap<CriarEnderecoDto, Endereco>();

        // Loja
        CreateMap<Loja, LojaDto>();
        CreateMap<CriarLojaDto, Loja>();

        // Produto
        CreateMap<Produto, ProdutoDto>()
            .ForMember(dest => dest.LojaNome, opt => opt.MapFrom(src => src.Loja != null ? src.Loja.Nome : ""))
            .ForMember(dest => dest.CategoriaNome, opt => opt.MapFrom(src => src.Categoria != null ? src.Categoria.Nome : ""));
        CreateMap<Produto, ProdutoListagemDto>()
            .ForMember(dest => dest.LojaNome, opt => opt.MapFrom(src => src.Loja != null ? src.Loja.Nome : ""));
        CreateMap<CriarProdutoDto, Produto>();

        // Pedido
        CreateMap<Pedido, PedidoDto>()
            .ForMember(dest => dest.ClienteNome, opt => opt.MapFrom(src => src.Cliente != null ? src.Cliente.Nome : ""));
        CreateMap<PedidoItem, PedidoItemDto>();
        CreateMap<CriarPedidoDto, Pedido>();
        CreateMap<CriarPedidoItemDto, PedidoItem>();

        // Carrinho
        CreateMap<Carrinho, CarrinhoItemDto>()
            .ForMember(dest => dest.ProdutoNome, opt => opt.MapFrom(src => src.Produto != null ? src.Produto.Nome : ""))
            .ForMember(dest => dest.ProdutoImagem, opt => opt.MapFrom(src => src.Produto != null ? src.Produto.ImagemPrincipal : ""))
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Quantidade * src.PrecoUnitario));

        // CRM
        CreateMap<CrmLead, CrmLeadDto>();
        CreateMap<CriarLeadDto, CrmLead>();
        CreateMap<AtualizarLeadDto, CrmLead>();
    }
}
