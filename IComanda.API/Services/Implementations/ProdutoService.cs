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
}
