using AutoMapper;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Entities;
using IComanda.API.Models.Enums;

namespace IComanda.API.Mappings;

public class ForcaVendasMappingProfile : Profile
{
    public ForcaVendasMappingProfile()
    {
        // Vendedor → VendedorDto
        CreateMap<Vendedor, VendedorDto>()
            .ForMember(dest => dest.Ativo, opt => opt.MapFrom(src => src.Ativo == "S"))
            .ForMember(dest => dest.VendasMesAtual, opt => opt.Ignore())       // calculado no service
            .ForMember(dest => dest.PercentualMeta, opt => opt.Ignore());      // calculado no service

        // VendedorDto → Vendedor
        CreateMap<VendedorDto, Vendedor>()
            .ForMember(dest => dest.Ativo, opt => opt.MapFrom(src => src.Ativo ? "S" : "N"));

        // PedidoFV → PedidoFVDto
        CreateMap<PedidoFV, PedidoFVDto>()
            .ForMember(dest => dest.Status,      opt => opt.MapFrom(src => ((StatusPedidoFV)src.Status).ToString()))
            .ForMember(dest => dest.StatusCodigo, opt => opt.MapFrom(src => (int)src.Status))
            .ForMember(dest => dest.NomeAprovador, opt => opt.Ignore()); // enriquecido no service

        // ItemPedidoFV → ItemPedidoFVDto
        CreateMap<ItemPedidoFV, ItemPedidoFVDto>();

        // VisitaFV → VisitaFVDto
        CreateMap<VisitaFV, VisitaFVDto>()
            .ForMember(dest => dest.Status,      opt => opt.MapFrom(src => ((StatusVisitaFV)src.Status).ToString()))
            .ForMember(dest => dest.StatusCodigo, opt => opt.MapFrom(src => (int)src.Status));

        // MetaFV → MetaFVDto
        CreateMap<MetaFV, MetaFVDto>()
            .ForMember(dest => dest.PercentualAtingido, opt => opt.MapFrom(src => src.PercentualAtingido))
            .ForMember(dest => dest.MetaAtingida,       opt => opt.MapFrom(src => src.MetaAtingida));
    }
}
