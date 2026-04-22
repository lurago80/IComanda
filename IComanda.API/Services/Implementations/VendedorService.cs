using AutoMapper;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Entities;
using IComanda.API.Models.Requests;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Services.Implementations;

public class VendedorService : IVendedorService
{
    private readonly IVendedorRepository _vendedorRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<VendedorService> _logger;
    private readonly IPasswordHasher _passwordHasher;

    public VendedorService(IVendedorRepository vendedorRepo, IMapper mapper, ILogger<VendedorService> logger, IPasswordHasher passwordHasher)
    {
        _vendedorRepo    = vendedorRepo;
        _mapper          = mapper;
        _logger          = logger;
        _passwordHasher  = passwordHasher;
    }

    public async Task<IEnumerable<VendedorDto>> BuscarAsync(BuscarVendedorRequest request)
    {
        var vendedores = await _vendedorRepo.BuscarVendedoresAsync(request);
        var dtos = _mapper.Map<IEnumerable<VendedorDto>>(vendedores).ToList();
        return dtos;
    }

    public async Task<VendedorDto?> GetByIdAsync(int id)
    {
        var vendedor = await _vendedorRepo.GetByIdAsync(id);
        if (vendedor == null) return null;

        var dto = _mapper.Map<VendedorDto>(vendedor);

        // Enriquecer com vendas do mês atual
        var agora = DateTime.Now;
        dto.VendasMesAtual = await _vendedorRepo.GetVendasMesAsync(id, agora.Month, agora.Year);
        dto.PercentualMeta = dto.MetaMensal > 0
            ? Math.Round((dto.VendasMesAtual / dto.MetaMensal) * 100, 2)
            : 0;

        return dto;
    }

    public async Task<IEnumerable<VendedorDto>> GetAtivosAsync()
    {
        var vendedores = await _vendedorRepo.GetAtivosAsync();
        return _mapper.Map<IEnumerable<VendedorDto>>(vendedores);
    }

    public async Task<VendedorDto> CriarAsync(VendedorDto dto)
    {
        var entidade = _mapper.Map<Vendedor>(dto);
        entidade.Ativo = "S";
        var id = await _vendedorRepo.CriarAsync(entidade);
        entidade.Id = id;
        _logger.LogInformation("Vendedor '{Nome}' criado com ID {Id}", dto.Nome, id);
        return _mapper.Map<VendedorDto>(entidade);
    }

    public async Task<VendedorDto> AtualizarAsync(int id, VendedorDto dto)
    {
        var existente = await _vendedorRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Vendedor {id} não encontrado");

        _mapper.Map(dto, existente);
        existente.Id = id;
        await _vendedorRepo.AtualizarAsync(existente);
        _logger.LogInformation("Vendedor {Id} atualizado", id);
        return _mapper.Map<VendedorDto>(existente);
    }

    public async Task AlterarStatusAsync(int id, bool ativo)
    {
        var ok = await _vendedorRepo.AlterarStatusAsync(id, ativo);
        if (!ok) throw new KeyNotFoundException($"Vendedor {id} não encontrado");
        _logger.LogInformation("Vendedor {Id} {Status}", id, ativo ? "ativado" : "desativado");
    }

    public async Task AlterarSenhaAsync(int id, string novaSenha)
    {
        if (string.IsNullOrWhiteSpace(novaSenha))
            throw new ArgumentException("Senha não pode ser vazia");

        var senhaHash = _passwordHasher.HashPassword(novaSenha);
        var ok = await _vendedorRepo.AlterarSenhaAsync(id, senhaHash);
        if (!ok) throw new KeyNotFoundException($"Vendedor {id} não encontrado");
        _logger.LogInformation("Senha do vendedor {Id} alterada", id);
    }
}
