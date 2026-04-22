using IComanda.API.Models.Entities;
using System.Data;

namespace IComanda.API.Repositories.Interfaces;

/// <summary>
/// Interface para repositório de contas a receber
/// </summary>
public interface IReceberRepository
{
    /// <summary>
    /// Cria um novo registro em RECEBER
    /// </summary>
    Task<bool> CriarReceberAsync(Receber receber, IDbTransaction? transaction = null);

    /// <summary>
    /// Gera o próximo número para RECEBER
    /// </summary>
    Task<string> GerarProximoNumeroAsync(IDbTransaction? transaction = null);

    /// <summary>
    /// Gera a próxima ordem para um número
    /// </summary>
    Task<string> GerarProximaOrdemAsync(string numero, IDbTransaction? transaction = null);

    /// <summary>
    /// Busca uma conta a receber por número e ordem
    /// </summary>
    Task<Receber?> GetReceberPorNumeroOrdemAsync(string numero, string ordem);

    /// <summary>
    /// Busca contas a receber pendentes (não quitadas)
    /// </summary>
    Task<IEnumerable<Receber>> GetReceberPendentesAsync(int? codigoCliente = null, DateTime? dataVencimentoInicio = null, DateTime? dataVencimentoFim = null);

    /// <summary>
    /// Busca contas a receber por cliente
    /// </summary>
    Task<IEnumerable<Receber>> GetReceberPorClienteAsync(int codigoCliente, bool apenasPendentes = true);

    /// <summary>
    /// Quita uma conta a receber
    /// </summary>
    Task<bool> QuitarReceberAsync(string numero, string ordem, decimal valorRecebido, DateTime dataRecebimento, int? operador = null, IDbTransaction? transaction = null);

    /// <summary>
    /// Verifica se o cliente tem contas a receber em aberto e retorna o total pendente
    /// </summary>
    Task<(bool TemContasAberto, decimal ValorTotalPendente, int QuantidadeContas)> VerificarContasAbertoAsync(int codigoCliente);
}

