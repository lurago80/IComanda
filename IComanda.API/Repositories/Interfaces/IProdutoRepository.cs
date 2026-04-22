using IComanda.API.Models.Entities;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;

namespace IComanda.API.Repositories.Interfaces;

public interface IProdutoRepository
{
    // Métodos de busca (mantidos para compatibilidade)
    Task<IEnumerable<Produto>> BuscarProdutosAsync(string? termo, bool? ativo = null, int? grupo = null, int pagina = 1, int itensPorPagina = 50);
    Task<Produto?> GetProdutoAsync(int id);
    Task<Dictionary<int, string>> GetDescricoesPorCodigosAsync(IEnumerable<int> codigos);
    Task<Produto?> GetProdutoPorCodigoBarraAsync(string codigoBarra);
    Task<Produto?> GetProdutoPorCodigoInternoAsync(string codigoInterno);
    Task<int> ContarProdutosAsync(string? termo, bool? ativo = null, int? grupo = null);

    // Métodos CRUD completos
    Task<ProdutoCompletoDto?> GetProdutoCompletoAsync(int id);
    Task<IEnumerable<ProdutoCompletoDto>> BuscarProdutosCompletosAsync(string? termo, bool? ativo = null, int pagina = 1, int itensPorPagina = 50);
    Task<int> CriarProdutoCompletoAsync(CriarProdutoRequest request);
    Task<bool> AtualizarProdutoCompletoAsync(int id, AtualizarProdutoRequest request);
    Task<bool> ExcluirProdutoAsync(int id);
}
