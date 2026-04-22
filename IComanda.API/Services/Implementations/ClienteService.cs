using AutoMapper;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Services.Interfaces;
using System.Globalization;
using System.Text;

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
        // Limpar CPF/CNPJ e Telefone (se informados)
        var cpfCnpjLimpo = string.IsNullOrWhiteSpace(request.CpfCnpj) 
            ? string.Empty 
            : request.CpfCnpj.Replace(".", "").Replace("-", "").Replace("/", "")
                            .Replace("(", "").Replace(")", "").Replace(" ", "");
        
        var telefoneLimpo = string.IsNullOrWhiteSpace(request.Telefone) 
            ? string.Empty 
            : request.Telefone.Replace(".", "").Replace("-", "").Replace("/", "")
                             .Replace("(", "").Replace(")", "").Replace(" ", "");

        // Verificar duplicidade apenas quando for gravar no cadastro (evitar duplicados na lista de clientes)
        if (request.GravarNoCadastro)
        {
            if (!string.IsNullOrWhiteSpace(cpfCnpjLimpo))
            {
                var existePorCpf = await _clienteRepository.ExistePorCpfCnpjAsync(cpfCnpjLimpo);
                if (existePorCpf)
                    throw new InvalidOperationException("Já existe um cliente cadastrado com este CPF/CNPJ.");
            }
            if (!string.IsNullOrWhiteSpace(telefoneLimpo))
            {
                // ExistePorTelefoneAsync já verifica as colunas TELEFONE e CELULAR
                var existePorTelefone = await _clienteRepository.ExistePorTelefoneAsync(telefoneLimpo);
                if (existePorTelefone)
                    throw new InvalidOperationException(
                        "Já existe um cliente cadastrado com este telefone. " +
                        "Você pode buscar pelo telefone na lista de clientes.");
            }
        }

        // Criar novo cliente (com endereço para delivery e opção de não gravar no cadastro)
        var novoCliente = new Models.Entities.Cliente
        {
            Nome = RemoverAcentos(request.Nome),
            // CORREÇÃO: não usar string vazia (causa violação de UNIQUE no Firebird);
            // usar null quando não informado — NULL é aceito múltiplas vezes em colunas UNIQUE.
            CpfCnpj = string.IsNullOrWhiteSpace(cpfCnpjLimpo) ? null : cpfCnpjLimpo,
            Telefone = telefoneLimpo ?? string.Empty,
            Celular = request.Celular,
            Fantasia = RemoverAcentos(request.Fantasia),
            Endereco1 = RemoverAcentos(request.Endereco1),
            Numero1 = request.Numero1,
            Complemento1 = RemoverAcentos(request.Complemento1),
            Bairro1 = RemoverAcentos(request.Bairro1),
            Cidade1 = RemoverAcentos(request.Cidade1),
            Uf1 = request.Uf1,
            Cep1 = request.Cep1,
            DataCadastro = DateTime.Now,
            Ativo = request.GravarNoCadastro ? 1 : 0,
            Bloqueado = 0
        };

        var sucesso = await _clienteRepository.CriarClienteAsync(novoCliente);
        if (!sucesso)
        {
            throw new InvalidOperationException("Não foi possível cadastrar o cliente. Verifique os dados e tente novamente.");
        }

        // Buscar cliente recém-cadastrado pelo ID (que foi definido antes de inserir)
        Models.Entities.Cliente clienteCadastrado;
        
        var clientePorId = await _clienteRepository.GetByIdAsync(novoCliente.Id);
        if (clientePorId != null)
        {
            clienteCadastrado = clientePorId;
        }
        // Se não encontrou por ID, tentar por CPF/CNPJ (se informado)
        else if (!string.IsNullOrWhiteSpace(cpfCnpjLimpo))
        {
            var clientePorCpf = await _clienteRepository.GetByCpfCnpjAsync(cpfCnpjLimpo);
            if (clientePorCpf != null)
            {
                clienteCadastrado = clientePorCpf;
            }
            else
            {
                throw new Exception("Cliente cadastrado mas não foi possível recuperá-lo por ID ou CPF/CNPJ");
            }
        }
        // Se ainda não encontrou, buscar pelo nome (mais recente)
        else
        {
            var clientes = await _clienteRepository.BuscarClientesAsync(new Models.Requests.BuscarClienteRequest 
            { 
                Q = request.Nome,
                Pagina = 1,
                ItensPorPagina = 10
            });
            
            // Pegar o primeiro que corresponde exatamente ao nome (case-insensitive)
            var clientePorNome = clientes.FirstOrDefault(c =>
                c.Nome != null && c.Nome.Equals(request.Nome, StringComparison.OrdinalIgnoreCase));
            
            if (clientePorNome == null)
            {
                throw new Exception("Cliente cadastrado mas não foi possível recuperá-lo");
            }
            
            clienteCadastrado = clientePorNome;
        }

        return _mapper.Map<ClienteDto>(clienteCadastrado);
    }

    public async Task<ClienteDto> CriarClienteAsync(CriarClienteRequest request)
    {
        // Validar campos obrigatórios
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            throw new ArgumentException("Nome é obrigatório");
        }

        // Verificar se CPF/CNPJ já existe (se informado)
        if (!string.IsNullOrWhiteSpace(request.CpfCnpj))
        {
            var cpfCnpjLimpo = request.CpfCnpj.Replace(".", "").Replace("-", "").Replace("/", "")
                                              .Replace("(", "").Replace(")", "").Replace(" ", "");
            var existePorCpf = await _clienteRepository.ExistePorCpfCnpjAsync(cpfCnpjLimpo);
            if (existePorCpf)
            {
                throw new InvalidOperationException("CPF/CNPJ já cadastrado");
            }
        }

        // Mapear request para entidade
        var cliente = new Models.Entities.Cliente
        {
            Nome = RemoverAcentos(request.Nome),
            CpfCnpj = request.CpfCnpj?.Replace(".", "").Replace("-", "").Replace("/", "")
                                      .Replace("(", "").Replace(")", "").Replace(" ", ""),
            RgIe = request.RgIe,
            Telefone = request.Telefone?.Replace(".", "").Replace("-", "").Replace("/", "")
                               .Replace("(", "").Replace(")", "").Replace(" ", ""),
            Celular = request.Celular?.Replace(".", "").Replace("-", "").Replace("/", "")
                                .Replace("(", "").Replace(")", "").Replace(" ", ""),
            Obs = RemoverAcentos(request.Obs),
            DataNascimento = request.DataNascimento,
            Endereco1 = RemoverAcentos(request.Endereco1),
            Numero1 = request.Numero1,
            Complemento1 = RemoverAcentos(request.Complemento1),
            Bairro1 = RemoverAcentos(request.Bairro1),
            Cidade1 = RemoverAcentos(request.Cidade1),
            Uf1 = request.Uf1,
            Email = request.Email,
            Endereco2 = RemoverAcentos(request.Endereco2),
            Numero2 = request.Numero2,
            Complemento2 = RemoverAcentos(request.Complemento2),
            Bairro2 = RemoverAcentos(request.Bairro2),
            Cidade2 = RemoverAcentos(request.Cidade2),
            Uf2 = request.Uf2,
            TabelaPreco = request.TabelaPreco,
            Ativo = request.Ativo ?? 1,
            Email1 = request.Email1,
            Cep1 = request.Cep1,
            Cep2 = request.Cep2,
            Classificacao = request.Classificacao,
            Acesso = request.Acesso,
            Limite = request.Limite,
            RgOrgao = request.RgOrgao,
            RgEstado = request.RgEstado,
            RgEmissao = request.RgEmissao,
            TelFax = request.TelFax,
            DataCadastro = DateTime.Now,
            TipoProdutor = request.TipoProdutor,
            Ip = request.Ip,
            Incra = request.Incra,
            Nirf = request.Nirf,
            DeclaItr = request.DeclaItr,
            Carro = request.Carro,
            PlacaCarro1 = request.PlacaCarro1,
            PlacaCarro2 = request.PlacaCarro2,
            PlacaCarro3 = request.PlacaCarro3,
            Moto = request.Moto,
            PlacaMoto1 = request.PlacaMoto1,
            PlacaMoto2 = request.PlacaMoto2,
            PlacaMoto3 = request.PlacaMoto3,
            Cei = request.Cei,
            NumProprio = request.NumProprio,
            Fantasia = request.Fantasia,
            Cnae = request.Cnae,
            IdVendedor = request.IdVendedor ?? 0,
            IbgeUf = request.IbgeUf ?? 0,
            IbgeMunicipio = request.IbgeMunicipio ?? 0,
            Contribuinte = request.Contribuinte,
            Mensal = request.Mensal,
            Site = request.Site,
            Bloqueado = request.Bloqueado ?? 0,
            IdRota = request.IdRota ?? 0,
            ValorAluguel = request.ValorAluguel,
            VencAluguel = request.VencAluguel,
            Comissao = request.Comissao
        };

        var id = await _clienteRepository.CriarClienteCompletoAsync(cliente);
        var clienteCriado = await _clienteRepository.GetByIdAsync(id);
        
        if (clienteCriado == null)
        {
            throw new Exception("Erro ao recuperar cliente criado");
        }

        return _mapper.Map<ClienteDto>(clienteCriado);
    }

    public async Task<ClienteDto> AtualizarClienteAsync(int id, CriarClienteRequest request)
    {
        // Verificar se cliente existe
        var clienteExistente = await _clienteRepository.GetByIdAsync(id);
        if (clienteExistente == null)
        {
            throw new InvalidOperationException("Cliente não encontrado");
        }

        // Verificar se CPF/CNPJ já existe em outro cliente (se informado)
        if (!string.IsNullOrWhiteSpace(request.CpfCnpj))
        {
            var cpfCnpjLimpo = request.CpfCnpj.Replace(".", "").Replace("-", "").Replace("/", "")
                                              .Replace("(", "").Replace(")", "").Replace(" ", "");
            var clientePorCpf = await _clienteRepository.GetByCpfCnpjAsync(cpfCnpjLimpo);
            if (clientePorCpf != null && clientePorCpf.Id != id)
            {
                throw new InvalidOperationException("CPF/CNPJ já cadastrado para outro cliente");
            }
        }

        // Mapear request para entidade
        var cliente = new Models.Entities.Cliente
        {
            Id = id,
            Nome = request.Nome,
            CpfCnpj = request.CpfCnpj?.Replace(".", "").Replace("-", "").Replace("/", "")
                                      .Replace("(", "").Replace(")", "").Replace(" ", ""),
            RgIe = request.RgIe,
            Telefone = request.Telefone?.Replace(".", "").Replace("-", "").Replace("/", "")
                               .Replace("(", "").Replace(")", "").Replace(" ", ""),
            Celular = request.Celular?.Replace(".", "").Replace("-", "").Replace("/", "")
                                .Replace("(", "").Replace(")", "").Replace(" ", ""),
            Obs = request.Obs,
            DataNascimento = request.DataNascimento,
            Endereco1 = request.Endereco1,
            Numero1 = request.Numero1,
            Complemento1 = request.Complemento1,
            Bairro1 = request.Bairro1,
            Cidade1 = request.Cidade1,
            Uf1 = request.Uf1,
            Email = request.Email,
            Endereco2 = request.Endereco2,
            Numero2 = request.Numero2,
            Complemento2 = request.Complemento2,
            Bairro2 = request.Bairro2,
            Cidade2 = request.Cidade2,
            Uf2 = request.Uf2,
            TabelaPreco = request.TabelaPreco,
            Ativo = request.Ativo ?? clienteExistente.Ativo ?? 1,
            Email1 = request.Email1,
            Cep1 = request.Cep1,
            Cep2 = request.Cep2,
            Classificacao = request.Classificacao,
            Acesso = request.Acesso,
            Limite = request.Limite,
            RgOrgao = request.RgOrgao,
            RgEstado = request.RgEstado,
            RgEmissao = request.RgEmissao,
            TelFax = request.TelFax,
            DataCadastro = clienteExistente.DataCadastro,
            TipoProdutor = request.TipoProdutor,
            Ip = request.Ip,
            Incra = request.Incra,
            Nirf = request.Nirf,
            DeclaItr = request.DeclaItr,
            Carro = request.Carro,
            PlacaCarro1 = request.PlacaCarro1,
            PlacaCarro2 = request.PlacaCarro2,
            PlacaCarro3 = request.PlacaCarro3,
            Moto = request.Moto,
            PlacaMoto1 = request.PlacaMoto1,
            PlacaMoto2 = request.PlacaMoto2,
            PlacaMoto3 = request.PlacaMoto3,
            Cei = request.Cei,
            NumProprio = request.NumProprio,
            Fantasia = request.Fantasia,
            Cnae = request.Cnae,
            IdVendedor = request.IdVendedor ?? clienteExistente.IdVendedor,
            IbgeUf = request.IbgeUf ?? clienteExistente.IbgeUf,
            IbgeMunicipio = request.IbgeMunicipio ?? clienteExistente.IbgeMunicipio,
            Contribuinte = request.Contribuinte ?? clienteExistente.Contribuinte,
            Mensal = request.Mensal ?? clienteExistente.Mensal,
            Site = request.Site,
            Bloqueado = request.Bloqueado ?? clienteExistente.Bloqueado,
            IdRota = request.IdRota ?? clienteExistente.IdRota,
            ValorAluguel = request.ValorAluguel ?? clienteExistente.ValorAluguel,
            VencAluguel = request.VencAluguel ?? clienteExistente.VencAluguel,
            Comissao = request.Comissao ?? clienteExistente.Comissao
        };

        var sucesso = await _clienteRepository.AtualizarClienteAsync(cliente);
        if (!sucesso)
        {
            throw new Exception("Erro ao atualizar cliente");
        }

        var clienteAtualizado = await _clienteRepository.GetByIdAsync(id);
        if (clienteAtualizado == null)
        {
            throw new Exception("Erro ao recuperar cliente atualizado");
        }

        return _mapper.Map<ClienteDto>(clienteAtualizado);
    }

    public async Task ExcluirClienteAsync(int id)
    {
        var cliente = await _clienteRepository.GetByIdAsync(id);
        if (cliente == null)
            throw new InvalidOperationException("Cliente não encontrado.");

        await _clienteRepository.ExcluirClienteAsync(id);
    }

    /// <summary>
    /// Remove acentos e diacríticos de um texto para evitar erros no banco de dados (Firebird).
    /// Exemplo: "João" → "Joao", "Rua São João" → "Rua Sao Joao"
    /// </summary>
    private static string RemoverAcentos(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return texto ?? string.Empty;

        var normalizado = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalizado.Length);

        foreach (var c in normalizado)
        {
            var categoria = CharUnicodeInfo.GetUnicodeCategory(c);
            if (categoria != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
