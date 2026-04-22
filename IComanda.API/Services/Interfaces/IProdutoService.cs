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
    
    // Métodos CRUD completos
    Task<ProdutoCompletoDto?> GetProdutoCompletoAsync(int id);
    Task<IEnumerable<ProdutoCompletoDto>> BuscarProdutosCompletosAsync(string? termo, bool? ativo = null, int pagina = 1, int itensPorPagina = 50);
    Task<int> CriarProdutoAsync(CriarProdutoRequest request);
    Task<bool> AtualizarProdutoAsync(int id, AtualizarProdutoRequest request);
    Task<bool> ExcluirProdutoAsync(int id);
}
