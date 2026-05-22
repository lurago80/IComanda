using IComanda.API.Models.Entities;

namespace IComanda.API.Repositories.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> BuscarPorNomeAsync(string nome);
    Task<Usuario?> BuscarPorIdAsync(int id);
    Task<IEnumerable<Usuario>> ListarAtivosAsync();
    Task<IEnumerable<Usuario>> ListarTodosAsync();
    Task AtualizarSenhaAsync(int usuarioId, string novaSenhaHash);
    Task<int> CriarAsync(Usuario usuario);
    Task AtualizarAsync(Usuario usuario);
    Task ExcluirAsync(int id);
}

