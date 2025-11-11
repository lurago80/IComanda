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

    public async Task<VerificarClienteResponse> VerificarClienteAsync(string cpfCnpjOuTelefone)
    {
        // Limpar entrada (remover caracteres especiais)
        var valorLimpo = cpfCnpjOuTelefone.Replace(".", "").Replace("-", "").Replace("/", "")
                                           .Replace("(", "").Replace(")", "").Replace(" ", "");

        // Tentar buscar por CPF/CNPJ primeiro
        var clientePorCpf = await _clienteRepository.GetByCpfCnpjAsync(valorLimpo);
        if (clientePorCpf != null)
        {
            return new VerificarClienteResponse
            {
                Existe = true,
                Cliente = _mapper.Map<ClienteDto>(clientePorCpf),
                Mensagem = "Cliente encontrado por CPF/CNPJ"
            };
        }

        // Tentar buscar por telefone
        var clientePorTelefone = await _clienteRepository.GetByTelefoneAsync(valorLimpo);
        if (clientePorTelefone != null)
        {
            return new VerificarClienteResponse
            {
                Existe = true,
                Cliente = _mapper.Map<ClienteDto>(clientePorTelefone),
                Mensagem = "Cliente encontrado por telefone"
            };
        }

        // Cliente não encontrado
        return new VerificarClienteResponse
        {
            Existe = false,
            Cliente = null,
            Mensagem = "Cliente não encontrado"
        };
    }

    public async Task<ClienteDto> CadastroRapidoAsync(CadastroRapidoClienteRequest request)
    {
        // Limpar CPF/CNPJ e Telefone
        var cpfCnpjLimpo = request.CpfCnpj.Replace(".", "").Replace("-", "").Replace("/", "")
                                          .Replace("(", "").Replace(")", "").Replace(" ", "");
        var telefoneLimpo = request.Telefone.Replace(".", "").Replace("-", "").Replace("/", "")
                                            .Replace("(", "").Replace(")", "").Replace(" ", "");

        // Verificar se já existe
        var existePorCpf = await _clienteRepository.ExistePorCpfCnpjAsync(cpfCnpjLimpo);
        if (existePorCpf)
        {
            throw new InvalidOperationException("CPF/CNPJ já cadastrado");
        }

        var existePorTelefone = await _clienteRepository.ExistePorTelefoneAsync(telefoneLimpo);
        if (existePorTelefone)
        {
            throw new InvalidOperationException("Telefone já cadastrado");
        }

        // Criar novo cliente
        var novoCliente = new Models.Entities.Cliente
        {
            Nome = request.Nome,
            CpfCnpj = cpfCnpjLimpo,
            Telefone = telefoneLimpo,
            Celular = request.Celular,
            Fantasia = request.Fantasia,
            DataCadastro = DateTime.Now,
            Ativo = 1,
            Bloqueado = 0
        };

        var sucesso = await _clienteRepository.CriarClienteAsync(novoCliente);
        if (!sucesso)
        {
            throw new Exception("Erro ao cadastrar cliente");
        }

        // Buscar cliente recém-cadastrado
        var clienteCadastrado = await _clienteRepository.GetByCpfCnpjAsync(cpfCnpjLimpo);
        if (clienteCadastrado == null)
        {
            throw new Exception("Cliente cadastrado mas não foi possível recuperá-lo");
        }

        return _mapper.Map<ClienteDto>(clienteCadastrado);
    }
}
