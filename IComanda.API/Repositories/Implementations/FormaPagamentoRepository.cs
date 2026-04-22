using Dapper;
using IComanda.API.Data;
using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Repositories.Implementations;

public class FormaPagamentoRepository : IFormaPagamentoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<FormaPagamentoRepository> _logger;

    public FormaPagamentoRepository(IDbConnectionFactory connectionFactory, ILogger<FormaPagamentoRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<FormaPagamento>> GetFormasPagamentoAtivasAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT 
                ID AS Id,
                DESCRICAO AS Descricao,
                ATIVO AS Ativo,
                INDICE AS Indice,
                MOEDA AS Moeda,
                MEIO_PAGTO AS MeioPagto,
                PERMITE_TROCO AS PermiteTroco,
                TIPO AS Tipo
            FROM FORMA_PAGAMENTO
            WHERE ATIVO = 1
            ORDER BY INDICE, DESCRICAO";

        _logger.LogInformation("🔍 Buscando formas de pagamento ativas");

        var formas = await connection.QueryAsync<FormaPagamento>(sql);
        return formas;
    }

    public async Task<FormaPagamento?> GetFormaPagamentoByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT 
                ID AS Id,
                DESCRICAO AS Descricao,
                ATIVO AS Ativo,
                INDICE AS Indice,
                MOEDA AS Moeda,
                MEIO_PAGTO AS MeioPagto,
                PERMITE_TROCO AS PermiteTroco,
                TIPO AS Tipo
            FROM FORMA_PAGAMENTO
            WHERE ID = @Id";

        _logger.LogInformation("🔍 Buscando forma de pagamento por ID: {Id}", id);

        var forma = await connection.QueryFirstOrDefaultAsync<FormaPagamento>(sql, new { Id = id });
        return forma;
    }

    public async Task<FormaPagamento?> GetFormaPagamentoPorDescricaoAsync(string descricao)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT 
                ID AS Id,
                DESCRICAO AS Descricao,
                ATIVO AS Ativo,
                INDICE AS Indice,
                MOEDA AS Moeda,
                MEIO_PAGTO AS MeioPagto,
                PERMITE_TROCO AS PermiteTroco,
                TIPO AS Tipo
            FROM FORMA_PAGAMENTO
            WHERE UPPER(TRIM(DESCRICAO)) = UPPER(TRIM(@Descricao))";

        _logger.LogInformation("🔍 Buscando forma de pagamento por descrição: {Descricao}", descricao);

        var forma = await connection.QueryFirstOrDefaultAsync<FormaPagamento>(sql, new { Descricao = descricao });
        return forma;
    }
}

