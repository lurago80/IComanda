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
            .ForMember(dest => dest.CodigoBarra, opt => opt.MapFrom(src => src.CodigoBarra ?? string.Empty))
            .ForMember(dest => dest.CodigoInterno, opt => opt.MapFrom(src => src.CodigoInterno ?? string.Empty))
            .ForMember(dest => dest.Descricao, opt => opt.MapFrom(src => src.Descricao ?? string.Empty))
            .ForMember(dest => dest.Caracteristica, opt => opt.MapFrom(src => src.Caracteristica ?? string.Empty))
            .ForMember(dest => dest.Quantidade, opt => opt.MapFrom(src => src.Quantidade ?? 0m))
            .ForMember(dest => dest.PrecoVenda, opt => opt.MapFrom(src => src.PrecoVenda ?? 0))
            .ForMember(dest => dest.Atacado, opt => opt.MapFrom(src => src.Atacado ?? 0))
            .ForMember(dest => dest.UnMedida, opt => opt.MapFrom(src => src.UnMedida ?? string.Empty))
            .ForMember(dest => dest.Ativo, opt => opt.MapFrom(src => src.Ativo == 1))
            .ForMember(dest => dest.Grupo, opt => opt.MapFrom(src => src.Grupo ?? 0))
            .ForMember(dest => dest.Pesavel, opt => opt.MapFrom(src => src.Pesavel == 1))
            .ForMember(dest => dest.Marca, opt => opt.MapFrom(src => src.Marca ?? string.Empty))
            .ForMember(dest => dest.Categoria, opt => opt.MapFrom(src => src.Categoria ?? string.Empty))
            .ForMember(dest => dest.Cor, opt => opt.MapFrom(src => src.Cor ?? string.Empty))
            .ForMember(dest => dest.Tamanho, opt => opt.MapFrom(src => src.Tamanho ?? string.Empty));

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
            .ForMember(dest => dest.QuantidadeProdutos, opt => opt.MapFrom(src => src.QuantidadeProdutos));

        // Mapeamentos de Venda
        CreateMap<Venda, VendaDto>()
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.TotProdutos));
        CreateMap<ItemVenda, ItemVendaDto>()
            .ForMember(dest => dest.Descricao, opt => opt.MapFrom(src => src.Descricao ?? string.Empty));

        // Mapeamentos de Request para Entity
        CreateMap<CriarVendaRequest, Venda>()
            .ForMember(dest => dest.Nota, opt => opt.Ignore())
            .ForMember(dest => dest.Sequencia, opt => opt.Ignore())
            .ForMember(dest => dest.Itens, opt => opt.Ignore())
            .ForMember(dest => dest.Cfops, opt => opt.MapFrom(src => "5.102"))
            .ForMember(dest => dest.Natureza, opt => opt.MapFrom(src => "Venda de Mercadoria"))
            .ForMember(dest => dest.Loja, opt => opt.MapFrom(src => ""))
            .ForMember(dest => dest.Modelo, opt => opt.MapFrom(src => "D2"))
            .ForMember(dest => dest.Serie, opt => opt.MapFrom(src => "001"))
            .ForMember(dest => dest.Subserie, opt => opt.MapFrom(src => "01"))
            .ForMember(dest => dest.Origem, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.Origem) ? "BA" : src.Origem.Trim().ToUpperInvariant()))
            .ForMember(dest => dest.Emissao, opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.Hora, opt => opt.MapFrom(src => DateTime.Now.TimeOfDay))
            .ForMember(dest => dest.DataSaida, opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.HoraSaida, opt => opt.MapFrom(src => DateTime.Now.TimeOfDay))
            .ForMember(dest => dest.Saida, opt => opt.MapFrom(src => 'X'))
            .ForMember(dest => dest.Avista, opt => opt.MapFrom(src => "1"))
            .ForMember(dest => dest.Lancado, opt => opt.MapFrom(src => "EFETIVADO"))
            .ForMember(dest => dest.Quantidade, opt => opt.MapFrom(src => string.Empty))
            .ForMember(dest => dest.FormasPgto, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.FormasPgto) ? "À VISTA" : src.FormasPgto))
            .ForMember(dest => dest.Especie, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Especie) ? "DINHEIRO" : src.Especie))
            .ForMember(dest => dest.Operador, opt => opt.MapFrom(src => src.Operador == 0 ? 1 : src.Operador))
            .ForMember(dest => dest.Caixa, opt => opt.MapFrom(src => src.Caixa));

        CreateMap<CriarItemVendaRequest, ItemVenda>()
            .ForMember(dest => dest.Nota, opt => opt.Ignore())
            .ForMember(dest => dest.Item, opt => opt.Ignore())
            .ForMember(dest => dest.Sequencia, opt => opt.Ignore());
    }
}
