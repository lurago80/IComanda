using AutoMapper;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Services.Interfaces;

namespace IComanda.API.Services.Implementations;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IMapper _mapper;

    public ClienteService(IClienteRepository clienteRepository, IMapper mapper)
    {
        _clienteRepository = clienteRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ClienteDto>> BuscarClientesAsync(BuscarClienteRequest request)
    {
        var clientes = await _clienteRepository.BuscarClientesAsync(request);
        return _mapper.Map<IEnumerable<ClienteDto>>(clientes);
    }

    public async Task<ClienteDto?> GetByIdAsync(int id)
    {
        var cliente = await _clienteRepository.GetByIdAsync(id);
        return cliente != null ? _mapper.Map<ClienteDto>(cliente) : null;
    }

    public async Task<ClienteDto?> GetByCpfCnpjAsync(string cpfCnpj)
    {
        var cliente = await _clienteRepository.GetByCpfCnpjAsync(cpfCnpj);
        return cliente != null ? _mapper.Map<ClienteDto>(cliente) : null;
    }

    public async Task<IEnumerable<ClienteDto>> GetByVendedorAsync(int idVendedor)
    {
        var clientes = await _clienteRepository.GetByVendedorAsync(idVendedor);
        return _mapper.Map<IEnumerable<ClienteDto>>(clientes);
    }

    public async Task<int> ContarClientesAsync(BuscarClienteRequest request)
    {
        return await _clienteRepository.ContarClientesAsync(request);
    }
}
