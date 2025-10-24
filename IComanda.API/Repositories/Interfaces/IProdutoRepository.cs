using IComanda.API.Models.Entities;

namespace IComanda.API.Repositories.Interfaces;

public interface IProdutoRepository
{
    Task<IEnumerable<Produto>> BuscarProdutosAsync(string? termo, bool? ativo = true, int? grupo = null, int pagina = 1, int itensPorPagina = 50);
    Task<Produto?> GetProdutoAsync(int id);
    Task<Produto?> GetProdutoPorCodigoBarraAsync(string codigoBarra);
    Task<Produto?> GetProdutoPorCodigoInternoAsync(string codigoInterno);
    Task<int> ContarProdutosAsync(string? termo, bool? ativo = true, int? grupo = null);
}
