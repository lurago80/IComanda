using IComanda.API.Models.Entities;

namespace IComanda.API.Repositories.Interfaces;

public interface IEmitenteRepository
{
    /// <summary>
    /// Obtém os dados do emitente (geralmente há apenas um registro)
    /// </summary>
    /// <returns>Dados do emitente ou null se não encontrado</returns>
    Task<Emitente?> GetEmitenteAsync();

    /// <summary>
    /// Insere ou atualiza os dados do emitente (upsert no ID = 1)
    /// </summary>
    Task SaveEmitenteAsync(Emitente emitente);
}

