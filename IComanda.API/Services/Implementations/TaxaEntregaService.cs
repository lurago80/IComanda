using IComanda.API.Models.DTOs;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Services.Interfaces;

namespace IComanda.API.Services.Implementations;

public class TaxaEntregaService : ITaxaEntregaService
{
    private readonly ITaxaEntregaRepository _repository;

    public TaxaEntregaService(ITaxaEntregaRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<TaxaEntregaDto>> GetAllAsync()
    {
        var list = await _repository.GetAllAsync();
        return list.Select(e => new TaxaEntregaDto { Id = e.Id, Descricao = e.Descricao, Valor = e.Valor });
    }

    public async Task<TaxaEntregaDto?> GetByIdAsync(int id)
    {
        var e = await _repository.GetByIdAsync(id);
        return e == null ? null : new TaxaEntregaDto { Id = e.Id, Descricao = e.Descricao, Valor = e.Valor };
    }

    public async Task<TaxaEntregaDto> CriarAsync(string descricao, decimal valor)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("A descrição é obrigatória.", nameof(descricao));
        var id = await _repository.CriarAsync(descricao, valor);
        var created = await _repository.GetByIdAsync(id);
        return new TaxaEntregaDto { Id = created!.Id, Descricao = created.Descricao, Valor = created.Valor };
    }

    public async Task<TaxaEntregaDto?> AtualizarAsync(int id, string descricao, decimal valor)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("A descrição é obrigatória.", nameof(descricao));
        var ok = await _repository.AtualizarAsync(id, descricao, valor);
        if (!ok) return null;
        return await GetByIdAsync(id);
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        return await _repository.ExcluirAsync(id);
    }
}
