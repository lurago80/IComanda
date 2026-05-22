using IComanda.API.Models.Entities;

namespace IComanda.API.Repositories.Interfaces;

public interface IPizzaRepository
{
    // Tamanhos
    Task<IEnumerable<PizzaTamanho>> GetTamanhosPorGrupoAsync(int grupoId, bool comSabores = false);
    Task<PizzaTamanho?> GetTamanhoAsync(int id);
    Task<int> CriarTamanhoAsync(int grupoId, string descricao, int ordem);
    Task<bool> AtualizarTamanhoAsync(int id, string descricao, int ordem);
    Task<bool> ExcluirTamanhoAsync(int id);

    // Sabores
    Task<IEnumerable<PizzaSabor>> GetSaboresPorTamanhoAsync(int tamanhoId, bool apenasAtivos = true);
    Task<int> CriarSaborAsync(int tamanhoId, string descricao, string? ingredientes, decimal preco);
    Task<bool> AtualizarSaborAsync(int id, string descricao, string? ingredientes, decimal preco, bool ativo);
    Task<bool> ExcluirSaborAsync(int id);

    // Bordas
    Task<IEnumerable<PizzaBorda>> GetBordasAsync(bool apenasAtivas = true);
    Task<int> CriarBordaAsync(string descricao, decimal preco);
    Task<bool> AtualizarBordaAsync(int id, string descricao, decimal preco, bool ativo);
    Task<bool> ExcluirBordaAsync(int id);

    // Utilitário
    Task<bool> EnsureTablesAsync();
    Task<bool> AtualizarTipoGrupoAsync(int grupoId, string tipo);
}
