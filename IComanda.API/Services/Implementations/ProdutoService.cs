using AutoMapper;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Entities;
using IComanda.API.Models.Requests;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Services.Interfaces;

namespace IComanda.API.Services.Implementations;

public class ProdutoService : IProdutoService
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly IMapper _mapper;

    public ProdutoService(IProdutoRepository produtoRepository, IMapper mapper)
    {
        _produtoRepository = produtoRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProdutoDto>> BuscarProdutosAsync(BuscarProdutoRequest request)
    {
        var produtos = await _produtoRepository.BuscarProdutosAsync(
            request.Q,
            request.Ativo,
            request.Grupo,
            request.Pagina,
            request.ItensPorPagina);

        return _mapper.Map<IEnumerable<ProdutoDto>>(produtos);
    }

    public async Task<ProdutoDto?> GetProdutoAsync(int id)
    {
        var produto = await _produtoRepository.GetProdutoAsync(id);
        return produto == null ? null : _mapper.Map<ProdutoDto>(produto);
    }

    public async Task<ProdutoDto?> GetProdutoPorCodigoBarraAsync(string codigoBarra)
    {
        var produto = await _produtoRepository.GetProdutoPorCodigoBarraAsync(codigoBarra);
        return produto == null ? null : _mapper.Map<ProdutoDto>(produto);
    }

    public async Task<ProdutoDto?> GetProdutoPorCodigoInternoAsync(string codigoInterno)
    {
        var produto = await _produtoRepository.GetProdutoPorCodigoInternoAsync(codigoInterno);
        return produto == null ? null : _mapper.Map<ProdutoDto>(produto);
    }

    public async Task<(IEnumerable<ProdutoDto> Produtos, int Total)> BuscarProdutosComPaginacaoAsync(BuscarProdutoRequest request)
    {
        var produtos = await _produtoRepository.BuscarProdutosAsync(
            request.Q,
            request.Ativo,
            request.Grupo,
            request.Pagina,
            request.ItensPorPagina);

        var total = await _produtoRepository.ContarProdutosAsync(
            request.Q,
            request.Ativo,
            request.Grupo);

        var produtosDto = _mapper.Map<IEnumerable<ProdutoDto>>(produtos);

        return (produtosDto, total);
    }

    public async Task<ProdutoCompletoDto?> GetProdutoCompletoAsync(int id)
    {
        return await _produtoRepository.GetProdutoCompletoAsync(id);
    }

    public async Task<IEnumerable<ProdutoCompletoDto>> BuscarProdutosCompletosAsync(string? termo, bool? ativo = null, int pagina = 1, int itensPorPagina = 50)
    {
        return await _produtoRepository.BuscarProdutosCompletosAsync(termo, ativo, pagina, itensPorPagina);
    }

    public async Task<int> CriarProdutoAsync(CriarProdutoRequest request)
    {
        // Validações básicas
        if (string.IsNullOrWhiteSpace(request.Descricao))
        {
            throw new ArgumentException("A descrição do produto é obrigatória");
        }

        if (request.TipoItemId <= 0)
        {
            throw new ArgumentException("O tipo de item é obrigatório");
        }

        if (request.UnidadeMedidaId <= 0)
        {
            throw new ArgumentException("A unidade de medida é obrigatória");
        }

        if (request.PessoaId <= 0)
        {
            throw new ArgumentException("O ID da pessoa é obrigatório");
        }

        return await _produtoRepository.CriarProdutoCompletoAsync(request);
    }

    public async Task<bool> AtualizarProdutoAsync(int id, AtualizarProdutoRequest request)
    {
        // Verificar se o produto existe
        var produtoExistente = await _produtoRepository.GetProdutoCompletoAsync(id);
        if (produtoExistente == null)
        {
            throw new KeyNotFoundException($"Produto com ID {id} não encontrado");
        }

        return await _produtoRepository.AtualizarProdutoCompletoAsync(id, request);
    }

    public async Task<bool> ExcluirProdutoAsync(int id)
    {
        // Verificar se o produto existe
        var produtoExistente = await _produtoRepository.GetProdutoCompletoAsync(id);
        if (produtoExistente == null)
        {
            throw new KeyNotFoundException($"Produto com ID {id} não encontrado");
        }

        return await _produtoRepository.ExcluirProdutoAsync(id);
    }
}
