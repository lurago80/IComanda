using IComanda.API.Models.DTOs;
using IComanda.API.Models.Entities;
using IComanda.API.Models.Requests;

namespace IComanda.API.Services.Interfaces;

public interface IProdutoService
{
    Task<IEnumerable<ProdutoDto>> BuscarProdutosAsync(BuscarProdutoRequest request);
    Task<ProdutoDto?> GetProdutoAsync(int id);
    Task<ProdutoDto?> GetProdutoPorCodigoBarraAsync(string codigoBarra);
    Task<ProdutoDto?> GetProdutoPorCodigoInternoAsync(string codigoInterno);
    Task<(IEnumerable<ProdutoDto> Produtos, int Total)> BuscarProdutosComPaginacaoAsync(BuscarProdutoRequest request);
}
