using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;

namespace IComanda.API.Services.Interfaces;

/// <summary>
/// Interface para serviço de contas a receber
/// </summary>
public interface IReceberService
{
    /// <summary>
    /// Busca uma conta a receber por número e ordem
    /// </summary>
    Task<ReceberDto?> GetReceberPorNumeroOrdemAsync(string numero, string ordem);

    /// <summary>
    /// Busca contas a receber pendentes
    /// </summary>
    Task<IEnumerable<ReceberDto>> GetReceberPendentesAsync(int? codigoCliente = null, DateTime? dataVencimentoInicio = null, DateTime? dataVencimentoFim = null);

    /// <summary>
    /// Busca contas a receber por cliente
    /// </summary>
    Task<IEnumerable<ReceberDto>> GetReceberPorClienteAsync(int codigoCliente, bool apenasPendentes = true);

    /// <summary>
    /// Quita uma conta a receber
    /// </summary>
    Task<QuitarReceberResponseDto> QuitarReceberAsync(QuitarReceberRequest request);

    /// <summary>
    /// Verifica se o cliente tem contas a receber em aberto
    /// </summary>
    Task<ContasAbertoDto> VerificarContasAbertoAsync(int codigoCliente);
}

