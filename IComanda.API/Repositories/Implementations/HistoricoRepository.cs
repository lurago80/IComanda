using Dapper;
using IComanda.API.Data;
using IComanda.API.Models.DTOs;
using IComanda.API.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System.Data;
using FirebirdSql.Data.FirebirdClient;

namespace IComanda.API.Repositories.Implementations;

public class HistoricoRepository : IHistoricoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<HistoricoRepository> _logger;

    public HistoricoRepository(IDbConnectionFactory connectionFactory, ILogger<HistoricoRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    /// <summary>Verifica se a tabela HISTORICO_ALTERACOES existe; se não, cria tabela, generator e trigger.</summary>
    private async Task EnsureHistoricoTableExistsAsync(IDbConnection connection)
    {
        if (connection is not FbConnection fbConn) return;
        if (connection.State != ConnectionState.Open)
            connection.Open();

        var exists = await connection.ExecuteScalarAsync<int?>(
            "SELECT 1 FROM RDB$RELATIONS WHERE RDB$RELATION_NAME = 'HISTORICO_ALTERACOES'");
        if (exists.HasValue && exists.Value == 1) return;

        _logger.LogInformation("Criando tabela HISTORICO_ALTERACOES (histórico de alterações) no Firebird...");
        var cmd = fbConn.CreateCommand();

        cmd.CommandText = @"
CREATE TABLE HISTORICO_ALTERACOES (
    ID INTEGER NOT NULL PRIMARY KEY,
    TIPO VARCHAR(50) NOT NULL,
    ENTIDADE_ID VARCHAR(50) NOT NULL,
    ACAO VARCHAR(50) NOT NULL,
    OPERADOR INTEGER NOT NULL,
    NOME_OPERADOR VARCHAR(100),
    DATA_HORA TIMESTAMP NOT NULL,
    DESCRICAO VARCHAR(500),
    DADOS_ANTIGOS BLOB SUB_TYPE TEXT,
    DADOS_NOVOS BLOB SUB_TYPE TEXT,
    OBSERVACOES VARCHAR(500)
)";
        cmd.ExecuteNonQuery();

        cmd.CommandText = "CREATE GENERATOR GEN_HISTORICO_ALTERACOES_ID";
        try { cmd.ExecuteNonQuery(); } catch { /* generator pode já existir */ }

        // Trigger: atribuir ID automaticamente se NULL (Firebird aceita sem ';' no final da linha dentro do BEGIN/END)
        cmd.CommandText = @"
CREATE TRIGGER TR_HISTORICO_ALTERACOES_BI FOR HISTORICO_ALTERACOES
ACTIVE BEFORE INSERT POSITION 0
AS
BEGIN
 IF (NEW.ID IS NULL) THEN NEW.ID = GEN_ID(GEN_HISTORICO_ALTERACOES_ID, 1);
END";
        try { cmd.ExecuteNonQuery(); } catch (Exception ex) { _logger.LogWarning(ex, "Trigger HISTORICO_ALTERACOES pode já existir."); }

        _logger.LogInformation("Tabela HISTORICO_ALTERACOES criada com sucesso.");
    }

    public async Task EnsureTableExistsAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        await EnsureHistoricoTableExistsAsync(connection);
    }

    public async Task<int> CriarHistoricoAsync(HistoricoAlteracaoDto historico)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            await EnsureHistoricoTableExistsAsync(connection);
            const string sql = @"
                INSERT INTO HISTORICO_ALTERACOES (TIPO, ENTIDADE_ID, ACAO, OPERADOR, NOME_OPERADOR, DATA_HORA, DESCRICAO, DADOS_ANTIGOS, DADOS_NOVOS, OBSERVACOES)
                VALUES (@Tipo, @EntidadeId, @Acao, @Operador, @NomeOperador, @DataHora, @Descricao, @DadosAntigos, @DadosNovos, @Observacoes)";
            await connection.ExecuteAsync(sql, new
            {
                historico.Tipo,
                historico.EntidadeId,
                historico.Acao,
                historico.Operador,
                historico.NomeOperador,
                DataHora = historico.DataHora,
                historico.Descricao,
                historico.DadosAntigos,
                historico.DadosNovos,
                historico.Observacoes
            });
            _logger.LogInformation(
                "📝 [HISTÓRICO] {Tipo} | {Acao} | Entidade: {EntidadeId} | Operador: {Operador} | {Descricao}",
                historico.Tipo, historico.Acao, historico.EntidadeId, historico.Operador, historico.Descricao ?? "");
            return 1;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Tabela HISTORICO_ALTERACOES pode não existir. Execute Scripts/criar_tabela_historico_alteracoes.sql");
            _logger.LogInformation(
                "📝 [HISTÓRICO] {Tipo} | {Acao} | Entidade: {EntidadeId} | Operador: {Operador} | {Descricao}",
                historico.Tipo, historico.Acao, historico.EntidadeId, historico.Operador, historico.Descricao ?? "");
            return 0;
        }
    }

    public async Task<IEnumerable<HistoricoAlteracaoDto>> GetHistoricoAsync(string? tipo = null, string? entidadeId = null, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            await EnsureHistoricoTableExistsAsync(connection);
            var sql = @"
                SELECT ID AS Id, TIPO AS Tipo, ENTIDADE_ID AS EntidadeId, ACAO AS Acao, OPERADOR AS Operador,
                       NOME_OPERADOR AS NomeOperador, DATA_HORA AS DataHora, DESCRICAO AS Descricao,
                       DADOS_ANTIGOS AS DadosAntigos, DADOS_NOVOS AS DadosNovos, OBSERVACOES AS Observacoes
                FROM HISTORICO_ALTERACOES WHERE 1=1";
            if (!string.IsNullOrWhiteSpace(tipo))
                sql += " AND UPPER(TRIM(TIPO)) = UPPER(TRIM(@Tipo))";
            if (!string.IsNullOrWhiteSpace(entidadeId))
                sql += " AND TRIM(ENTIDADE_ID) = TRIM(@EntidadeId)";
            if (dataInicio.HasValue)
                sql += " AND DATA_HORA >= @DataInicio";
            if (dataFim.HasValue)
                sql += " AND CAST(DATA_HORA AS DATE) <= @DataFim";
            sql += " ORDER BY DATA_HORA DESC";

            var list = await connection.QueryAsync<HistoricoAlteracaoDto>(sql, new { Tipo = tipo, EntidadeId = entidadeId, DataInicio = dataInicio, DataFim = dataFim });
            return list;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao buscar histórico. Tabela HISTORICO_ALTERACOES pode não existir.");
            throw;
        }
    }

    public async Task<IEnumerable<HistoricoAlteracaoDto>> GetHistoricoPorOperadorAsync(int operador, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            await EnsureHistoricoTableExistsAsync(connection);
            var sql = @"
                SELECT ID AS Id, TIPO AS Tipo, ENTIDADE_ID AS EntidadeId, ACAO AS Acao, OPERADOR AS Operador,
                       NOME_OPERADOR AS NomeOperador, DATA_HORA AS DataHora, DESCRICAO AS Descricao,
                       DADOS_ANTIGOS AS DadosAntigos, DADOS_NOVOS AS DadosNovos, OBSERVACOES AS Observacoes
                FROM HISTORICO_ALTERACOES WHERE OPERADOR = @Operador";
            if (dataInicio.HasValue)
                sql += " AND DATA_HORA >= @DataInicio";
            if (dataFim.HasValue)
                sql += " AND CAST(DATA_HORA AS DATE) <= @DataFim";
            sql += " ORDER BY DATA_HORA DESC";

            var list = await connection.QueryAsync<HistoricoAlteracaoDto>(sql, new { Operador = operador, DataInicio = dataInicio, DataFim = dataFim });
            return list;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao buscar histórico do operador.");
            return new List<HistoricoAlteracaoDto>();
        }
    }
}

