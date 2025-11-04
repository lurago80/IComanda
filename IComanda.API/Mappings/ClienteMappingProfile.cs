using AutoMapper;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Entities;

namespace IComanda.API.Mappings;

public class ClienteMappingProfile : Profile
{
    public ClienteMappingProfile()
    {
        CreateMap<Cliente, ClienteDto>()
            .ForMember(dest => dest.Ativo, opt => opt.MapFrom(src => src.Ativo == 1))
            .ForMember(dest => dest.Bloqueado, opt => opt.MapFrom(src => src.Bloqueado == 1))
            .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.Nome ?? ""))
            .ForMember(dest => dest.CpfCnpj, opt => opt.MapFrom(src => src.CpfCnpj))
            .ForMember(dest => dest.Telefone, opt => opt.MapFrom(src => src.Telefone))
            .ForMember(dest => dest.Celular, opt => opt.MapFrom(src => src.Celular))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Endereco1, opt => opt.MapFrom(src => src.Endereco1))
            .ForMember(dest => dest.Cidade1, opt => opt.MapFrom(src => src.Cidade1))
            .ForMember(dest => dest.Uf1, opt => opt.MapFrom(src => src.Uf1))
            .ForMember(dest => dest.Cep1, opt => opt.MapFrom(src => src.Cep1))
            .ForMember(dest => dest.Fantasia, opt => opt.MapFrom(src => src.Fantasia))
            .ForMember(dest => dest.DataCadastro, opt => opt.MapFrom(src => src.DataCadastro))
            .ForMember(dest => dest.Limite, opt => opt.MapFrom(src => src.Limite))
            .ForMember(dest => dest.Classificacao, opt => opt.MapFrom(src => src.Classificacao))
            .ForMember(dest => dest.IdVendedor, opt => opt.MapFrom(src => src.IdVendedor));
    }
}
