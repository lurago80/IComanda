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
            .ForMember(dest => dest.RgIe, opt => opt.MapFrom(src => src.RgIe))
            .ForMember(dest => dest.Telefone, opt => opt.MapFrom(src => src.Telefone))
            .ForMember(dest => dest.Celular, opt => opt.MapFrom(src => src.Celular))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Email1, opt => opt.MapFrom(src => src.Email1))
            .ForMember(dest => dest.Endereco1, opt => opt.MapFrom(src => src.Endereco1))
            .ForMember(dest => dest.Numero1, opt => opt.MapFrom(src => src.Numero1))
            .ForMember(dest => dest.Complemento1, opt => opt.MapFrom(src => src.Complemento1))
            .ForMember(dest => dest.Bairro1, opt => opt.MapFrom(src => src.Bairro1))
            .ForMember(dest => dest.Cidade1, opt => opt.MapFrom(src => src.Cidade1))
            .ForMember(dest => dest.Uf1, opt => opt.MapFrom(src => src.Uf1))
            .ForMember(dest => dest.Cep1, opt => opt.MapFrom(src => src.Cep1))
            .ForMember(dest => dest.Endereco2, opt => opt.MapFrom(src => src.Endereco2))
            .ForMember(dest => dest.Numero2, opt => opt.MapFrom(src => src.Numero2))
            .ForMember(dest => dest.Complemento2, opt => opt.MapFrom(src => src.Complemento2))
            .ForMember(dest => dest.Bairro2, opt => opt.MapFrom(src => src.Bairro2))
            .ForMember(dest => dest.Cidade2, opt => opt.MapFrom(src => src.Cidade2))
            .ForMember(dest => dest.Uf2, opt => opt.MapFrom(src => src.Uf2))
            .ForMember(dest => dest.Cep2, opt => opt.MapFrom(src => src.Cep2))
            .ForMember(dest => dest.Fantasia, opt => opt.MapFrom(src => src.Fantasia))
            .ForMember(dest => dest.DataCadastro, opt => opt.MapFrom(src => src.DataCadastro))
            .ForMember(dest => dest.DataNascimento, opt => opt.MapFrom(src => src.DataNascimento))
            .ForMember(dest => dest.Limite, opt => opt.MapFrom(src => src.Limite))
            .ForMember(dest => dest.Classificacao, opt => opt.MapFrom(src => src.Classificacao))
            .ForMember(dest => dest.Obs, opt => opt.MapFrom(src => src.Obs))
            .ForMember(dest => dest.IdVendedor, opt => opt.MapFrom(src => src.IdVendedor));
    }
}
