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

    public async Task<IEnumerable<FormaPagamento>> GetAllAsync()
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
            ORDER BY INDICE, DESCRICAO";

        _logger.LogInformation("🔍 Buscando todas as formas de pagamento");
        return await connection.QueryAsync<FormaPagamento>(sql);
    }

    public async Task<bool> UpdateAsync(FormaPagamento forma)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            UPDATE FORMA_PAGAMENTO SET
                DESCRICAO     = @Descricao,
                ATIVO         = @Ativo,
                INDICE        = @Indice,
                MEIO_PAGTO    = @MeioPagto,
                PERMITE_TROCO = @PermiteTroco,
                TIPO          = @Tipo
            WHERE ID = @Id";

        _logger.LogInformation("✏️ Atualizando forma de pagamento ID {Id}", forma.Id);
        var rows = await connection.ExecuteAsync(sql, new
        {
            forma.Id,
            forma.Descricao,
            forma.Ativo,
            forma.Indice,
            forma.MeioPagto,
            forma.PermiteTroco,
            forma.Tipo
        });
        return rows > 0;
    }

    public async Task<bool> ToggleAtivoAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            UPDATE FORMA_PAGAMENTO
            SET ATIVO = CASE WHEN ATIVO = 1 THEN 0 ELSE 1 END
            WHERE ID = @Id";

        _logger.LogInformation("🔄 Alternando status ativo da forma de pagamento ID {Id}", id);
        var rows = await connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }

    public async Task<int> CreateAsync(FormaPagamento forma)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Gera próximo ID manualmente (Firebird pode não ter IDENTITY padrão)
        var maxIdSql = "SELECT COALESCE(MAX(ID), 0) + 1 FROM FORMA_PAGAMENTO";
        var novoId = await connection.ExecuteScalarAsync<int>(maxIdSql);

        var sql = @"
            INSERT INTO FORMA_PAGAMENTO (ID, DESCRICAO, ATIVO, INDICE, MOEDA, MEIO_PAGTO, PERMITE_TROCO, TIPO)
            VALUES (@Id, @Descricao, @Ativo, @Indice, @Moeda, @MeioPagto, @PermiteTroco, @Tipo)";

        _logger.LogInformation("➕ Criando nova forma de pagamento: {Descricao}", forma.Descricao);
        await connection.ExecuteAsync(sql, new
        {
            Id        = novoId,
            forma.Descricao,
            Ativo     = (short)1,
            forma.Indice,
            Moeda     = forma.Moeda,
            forma.MeioPagto,
            forma.PermiteTroco,
            forma.Tipo
        });
        return novoId;
    }
}

