using AutoMapper;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Entities;
using IComanda.API.Models.Requests;

namespace IComanda.API.Mappings;

public class ProdutoMappingProfile : Profile
{
    public ProdutoMappingProfile()
    {
        CreateMap<Produto, ProdutoDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CodigoBarra, opt => opt.MapFrom(src => src.CodigoBarra))
            .ForMember(dest => dest.CodigoInterno, opt => opt.MapFrom(src => src.CodigoInterno))
            .ForMember(dest => dest.Descricao, opt => opt.MapFrom(src => src.Descricao))
            .ForMember(dest => dest.Caracteristica, opt => opt.MapFrom(src => src.Caracteristica))
            .ForMember(dest => dest.Quantidade, opt => opt.MapFrom(src => src.Quantidade))
            .ForMember(dest => dest.PrecoVenda, opt => opt.MapFrom(src => src.PrecoVenda))
            .ForMember(dest => dest.Atacado, opt => opt.MapFrom(src => src.Atacado))
            .ForMember(dest => dest.UnMedida, opt => opt.MapFrom(src => src.UnMedida))
            .ForMember(dest => dest.Ativo, opt => opt.MapFrom(src => src.Ativo))
            .ForMember(dest => dest.Grupo, opt => opt.MapFrom(src => src.Grupo))
            .ForMember(dest => dest.Pesavel, opt => opt.MapFrom(src => src.Pesavel))
            .ForMember(dest => dest.Marca, opt => opt.MapFrom(src => src.Marca))
            .ForMember(dest => dest.Categoria, opt => opt.MapFrom(src => src.Categoria))
            .ForMember(dest => dest.Cor, opt => opt.MapFrom(src => src.Cor))
            .ForMember(dest => dest.Tamanho, opt => opt.MapFrom(src => src.Tamanho));

        CreateMap<ItemCarrinho, ItemCarrinhoDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Cupom, opt => opt.MapFrom(src => src.Cupom))
            .ForMember(dest => dest.Operador, opt => opt.MapFrom(src => src.Operador))
            .ForMember(dest => dest.Item, opt => opt.MapFrom(src => src.Item))
            .ForMember(dest => dest.Codigo, opt => opt.MapFrom(src => src.Codigo))
            .ForMember(dest => dest.Barras, opt => opt.MapFrom(src => src.Barras))
            .ForMember(dest => dest.Descricao, opt => opt.MapFrom(src => src.Descricao))
            .ForMember(dest => dest.Qtd, opt => opt.MapFrom(src => src.Qtd))
            .ForMember(dest => dest.Preco, opt => opt.MapFrom(src => src.Preco))
            .ForMember(dest => dest.Desconto, opt => opt.MapFrom(src => src.Desconto + src.Desconto1))
            .ForMember(dest => dest.Acrescimo, opt => opt.MapFrom(src => src.Acrescimo))
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total))
            .ForMember(dest => dest.Und, opt => opt.MapFrom(src => src.Und))
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.Data))
            .ForMember(dest => dest.Hora, opt => opt.MapFrom(src => src.Hora));

        CreateMap<Grupo, GrupoDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Descricao, opt => opt.MapFrom(src => src.Descricao))
            .ForMember(dest => dest.CodGrupo, opt => opt.MapFrom(src => src.CodGrupo))
            .ForMember(dest => dest.QuantidadeProdutos, opt => opt.MapFrom(src => src.QuantidadeProdutos));

        // Mapeamentos de Venda
        CreateMap<Venda, VendaDto>();
        CreateMap<ItemVenda, ItemVendaDto>();

        // Mapeamentos de Request para Entity
        CreateMap<CriarVendaRequest, Venda>()
            .ForMember(dest => dest.Nota, opt => opt.Ignore())
            .ForMember(dest => dest.Sequencia, opt => opt.Ignore())
            .ForMember(dest => dest.Itens, opt => opt.Ignore());

        CreateMap<CriarItemVendaRequest, ItemVenda>()
            .ForMember(dest => dest.Nota, opt => opt.Ignore())
            .ForMember(dest => dest.Item, opt => opt.Ignore())
            .ForMember(dest => dest.Sequencia, opt => opt.Ignore());
    }
}
