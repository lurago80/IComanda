using AutoMapper;
using IComanda.API.Models.DTOs;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Services.Interfaces;

namespace IComanda.API.Services.Implementations;

public class GrupoService : IGrupoService
{
    private readonly IGrupoRepository _grupoRepository;
    private readonly IMapper _mapper;

    public GrupoService(IGrupoRepository grupoRepository, IMapper mapper)
    {
        _grupoRepository = grupoRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<GrupoDto>> GetAllGruposAsync()
    {
        var grupos = await _grupoRepository.GetAllGruposAsync();
        return _mapper.Map<IEnumerable<GrupoDto>>(grupos);
    }

    public async Task<GrupoDto?> GetGrupoAsync(int id)
    {
        var grupo = await _grupoRepository.GetGrupoAsync(id);
        return grupo == null ? null : _mapper.Map<GrupoDto>(grupo);
    }

    public async Task<IEnumerable<GrupoDto>> GetGruposComQuantidadeAsync()
    {
        var grupos = await _grupoRepository.GetGruposComQuantidadeAsync();
        return _mapper.Map<IEnumerable<GrupoDto>>(grupos);
    }

    public async Task<IEnumerable<GrupoDto>> GetGruposComQuantidadeTodosAsync()
    {
        var grupos = await _grupoRepository.GetGruposComQuantidadeTodosAsync();

        return grupos.Select(g => new GrupoDto
        {
            Id = g.Id,
            Descricao = g.Descricao,
            QuantidadeProdutos = g.QuantidadeProdutos,
            ImprimirDuasVias = g.ImprimirDuasVias
        });
    }

    public async Task<GrupoDto> CriarGrupoAsync(string descricao, bool imprimirDuasVias = false)
    {
        if (string.IsNullOrWhiteSpace(descricao))
        {
            throw new ArgumentException("A descrição do grupo é obrigatória", nameof(descricao));
        }

        var id = await _grupoRepository.CriarGrupoAsync(descricao, imprimirDuasVias);
        var grupo = await _grupoRepository.GetGrupoAsync(id);
        
        return _mapper.Map<GrupoDto>(grupo!);
    }

    public async Task<GrupoDto> AtualizarGrupoAsync(int id, string descricao, bool imprimirDuasVias = false)
    {
        if (string.IsNullOrWhiteSpace(descricao))
        {
            throw new ArgumentException("A descrição do grupo é obrigatória", nameof(descricao));
        }

        var atualizado = await _grupoRepository.AtualizarGrupoAsync(id, descricao, imprimirDuasVias);
        
        if (!atualizado)
        {
            throw new KeyNotFoundException($"Grupo com ID {id} não encontrado");
        }

        var grupo = await _grupoRepository.GetGrupoAsync(id);
        return _mapper.Map<GrupoDto>(grupo!);
    }

    public async Task<bool> ExcluirGrupoAsync(int id)
    {
        return await _grupoRepository.ExcluirGrupoAsync(id);
    }
}
