using Dapper;
using IComanda.API.Data;
using IComanda.API.Models.Entities;
using IComanda.API.Models.Requests;
using IComanda.API.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Repositories.Implementations;

public class VendedorRepository : IVendedorRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<VendedorRepository> _logger;

    public VendedorRepository(IDbConnectionFactory connectionFactory, ILogger<VendedorRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<Vendedor>> BuscarVendedoresAsync(BuscarVendedorRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereConditions = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            whereConditions.Add("(UPPER(NOME) LIKE UPPER(@Q) OR UPPER(COALESCE(EMAIL, '')) LIKE UPPER(@Q))");
            parameters.Add("@Q", $"%{request.Q.Trim()}%");
        }

        if (request.Ativo.HasValue)
        {
            whereConditions.Add("ATIVO = @Ativo");
            parameters.Add("@Ativo", request.Ativo.Value ? "S" : "N");
        }

        if (!string.IsNullOrWhiteSpace(request.Regiao))
        {
            whereConditions.Add("UPPER(COALESCE(REGIAO, '')) LIKE UPPER(@Regiao)");
            parameters.Add("@Regiao", $"%{request.Regiao}%");
        }

        var whereClause = whereConditions.Any() ? $"WHERE {string.Join(" AND ", whereConditions)}" : "";

        var limit = request.ItensPorPagina ?? 50;
        var offset = ((request.Pagina ?? 1) - 1) * limit;

        var sql = $@"
            SELECT FIRST {limit} SKIP {offset}
                ID          AS Id,
                NOME        AS Nome,
                EMAIL       AS Email,
                CELULAR     AS Celular,
                ATIVO       AS Ativo,
                COALESCE(COMISSAO, 0)   AS ComissaoPerc,
                COALESCE(META, 0)       AS MetaMensal,
                REGIAO      AS Regiao,
                OBS         AS Obs,
                DATACADASTRO AS DataCadastro
            FROM VENDEDOR
            {whereClause}
            ORDER BY NOME";

        _logger.LogDebug("Buscando vendedores com filtros");
        return await connection.QueryAsync<Vendedor>(sql, parameters);
    }

    public async Task<Vendedor?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT
                ID          AS Id,
                NOME        AS Nome,
                EMAIL       AS Email,
                CELULAR     AS Celular,
                ATIVO       AS Ativo,
                COALESCE(COMISSAO, 0)   AS ComissaoPerc,
                COALESCE(META, 0)       AS MetaMensal,
                REGIAO      AS Regiao,
                OBS         AS Obs,
                DATACADASTRO AS DataCadastro
            FROM VENDEDOR
            WHERE ID = @Id";

        return await connection.QuerySingleOrDefaultAsync<Vendedor>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Vendedor>> GetAtivosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT
                ID          AS Id,
                NOME        AS Nome,
                EMAIL       AS Email,
                CELULAR     AS Celular,
                ATIVO       AS Ativo,
                COALESCE(COMISSAO, 0)   AS ComissaoPerc,
                COALESCE(META, 0)       AS MetaMensal,
                REGIAO      AS Regiao,
                OBS         AS Obs,
                DATACADASTRO AS DataCadastro
            FROM VENDEDOR
            WHERE ATIVO = 'S'
            ORDER BY NOME";

        return await connection.QueryAsync<Vendedor>(sql);
    }

    public async Task<int> CriarAsync(Vendedor vendedor)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO VENDEDOR (NOME, EMAIL, CELULAR, ATIVO, COMISSAO, META, REGIAO, OBS, DATACADASTRO)
            VALUES (@Nome, @Email, @Celular, @Ativo, @ComissaoPerc, @MetaMensal, @Regiao, @Obs, @DataCadastro)
            RETURNING ID";

        vendedor.DataCadastro = DateTime.Now;
        var id = await connection.ExecuteScalarAsync<int>(sql, new
        {
            vendedor.Nome,
            vendedor.Email,
            vendedor.Celular,
            vendedor.Ativo,
            vendedor.ComissaoPerc,
            vendedor.MetaMensal,
            vendedor.Regiao,
            vendedor.Obs,
            vendedor.DataCadastro
        });

        _logger.LogInformation("Vendedor criado com ID: {Id}", id);
        return id;
    }

    public async Task<bool> AtualizarAsync(Vendedor vendedor)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE VENDEDOR SET
                NOME        = @Nome,
                EMAIL       = @Email,
                CELULAR     = @Celular,
                COMISSAO    = @ComissaoPerc,
                META        = @MetaMensal,
                REGIAO      = @Regiao,
                OBS         = @Obs
            WHERE ID = @Id";

        var rows = await connection.ExecuteAsync(sql, new
        {
            vendedor.Id,
            vendedor.Nome,
            vendedor.Email,
            vendedor.Celular,
            vendedor.ComissaoPerc,
            vendedor.MetaMensal,
            vendedor.Regiao,
            vendedor.Obs
        });

        return rows > 0;
    }

    public async Task<bool> AlterarStatusAsync(int id, bool ativo)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE VENDEDOR SET ATIVO = @Ativo WHERE ID = @Id";
        var rows = await connection.ExecuteAsync(sql, new { Id = id, Ativo = ativo ? "S" : "N" });
        return rows > 0;
    }

    public async Task<bool> AlterarSenhaAsync(int id, string senhaHash)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE VENDEDOR SET SENHA = @Senha WHERE ID = @Id";
        var rows = await connection.ExecuteAsync(sql, new { Id = id, Senha = senhaHash });
        return rows > 0;
    }

    public async Task<decimal> GetVendasMesAsync(int idVendedor, int mes, int ano)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT COALESCE(SUM(TOTAL), 0)
            FROM VENDAS
            WHERE VENDEDOR = @IdVendedor
              AND EXTRACT(MONTH FROM EMISSAO) = @Mes
              AND EXTRACT(YEAR  FROM EMISSAO) = @Ano
              AND LANCADO = 'EFETIVADO'";

        return await connection.ExecuteScalarAsync<decimal>(sql, new { IdVendedor = idVendedor, Mes = mes, Ano = ano });
    }
}
