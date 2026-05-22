using Dapper;
using IComanda.API.Data;
using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;

namespace IComanda.API.Repositories.Implementations;

public class PizzaRepository : IPizzaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<PizzaRepository> _logger;
    private static bool _tablesChecked = false;

    public PizzaRepository(IDbConnectionFactory connectionFactory, ILogger<PizzaRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<bool> EnsureTablesAsync()
    {
        if (_tablesChecked) return true;
        using var conn = _connectionFactory.CreateConnection();
        var ddls = new[]
        {
            "ALTER TABLE GRUPO ADD TIPO VARCHAR(10) DEFAULT 'NORMAL'",
            "CREATE TABLE PIZZA_TAMANHOS (ID INTEGER NOT NULL PRIMARY KEY, GRUPO_ID INTEGER NOT NULL, DESCRICAO VARCHAR(30) NOT NULL, ORDEM INTEGER DEFAULT 0)",
            "CREATE GENERATOR PIZZA_TAMANHOS_GEN",
            "CREATE TABLE PIZZA_SABORES (ID INTEGER NOT NULL PRIMARY KEY, TAMANHO_ID INTEGER NOT NULL, DESCRICAO VARCHAR(100) NOT NULL, INGREDIENTES VARCHAR(300), PRECO DECIMAL(10,2) DEFAULT 0, ATIVO CHAR(1) DEFAULT '1')",
            "CREATE GENERATOR PIZZA_SABORES_GEN",
            "CREATE TABLE PIZZA_BORDAS (ID INTEGER NOT NULL PRIMARY KEY, DESCRICAO VARCHAR(50) NOT NULL, PRECO DECIMAL(10,2) DEFAULT 0, ATIVO CHAR(1) DEFAULT '1')",
            "CREATE GENERATOR PIZZA_BORDAS_GEN",
        };
        foreach (var ddl in ddls)
        {
            try { await conn.ExecuteAsync(ddl); }
            catch { /* already exists — ok */ }
        }
        _tablesChecked = true;
        return true;
    }

    // ── TAMANHOS ─────────────────────────────────────────────────────────────

    public async Task<IEnumerable<PizzaTamanho>> GetTamanhosPorGrupoAsync(int grupoId, bool comSabores = false)
    {
        await EnsureTablesAsync();
        using var conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT ID, GRUPO_ID, DESCRICAO, ORDEM FROM PIZZA_TAMANHOS WHERE GRUPO_ID = @GrupoId ORDER BY ORDEM, ID";
        var tamanhos = (await conn.QueryAsync<PizzaTamanho>(sql, new { GrupoId = grupoId })).ToList();

        if (comSabores && tamanhos.Any())
        {
            var ids = tamanhos.Select(t => t.Id).ToList();
            const string sqlSabores = "SELECT ID, TAMANHO_ID, DESCRICAO, INGREDIENTES, PRECO, CASE ATIVO WHEN '1' THEN 1 ELSE 0 END AS ATIVO FROM PIZZA_SABORES WHERE TAMANHO_ID IN @Ids AND ATIVO = '1' ORDER BY DESCRICAO";
            var sabores = (await conn.QueryAsync<PizzaSabor>(sqlSabores, new { Ids = ids })).ToList();
            foreach (var t in tamanhos)
                t.Sabores = sabores.Where(s => s.TamanhoId == t.Id).ToList();
        }
        return tamanhos;
    }

    public async Task<PizzaTamanho?> GetTamanhoAsync(int id)
    {
        await EnsureTablesAsync();
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<PizzaTamanho>(
            "SELECT ID, GRUPO_ID, DESCRICAO, ORDEM FROM PIZZA_TAMANHOS WHERE ID = @Id", new { Id = id });
    }

    public async Task<int> CriarTamanhoAsync(int grupoId, string descricao, int ordem)
    {
        await EnsureTablesAsync();
        using var conn = _connectionFactory.CreateConnection();
        var id = await conn.QuerySingleAsync<int>("SELECT GEN_ID(PIZZA_TAMANHOS_GEN, 1) FROM RDB$DATABASE");
        await conn.ExecuteAsync(
            "INSERT INTO PIZZA_TAMANHOS (ID, GRUPO_ID, DESCRICAO, ORDEM) VALUES (@Id, @GrupoId, @Descricao, @Ordem)",
            new { Id = id, GrupoId = grupoId, Descricao = descricao, Ordem = ordem });
        return id;
    }

    public async Task<bool> AtualizarTamanhoAsync(int id, string descricao, int ordem)
    {
        using var conn = _connectionFactory.CreateConnection();
        var rows = await conn.ExecuteAsync(
            "UPDATE PIZZA_TAMANHOS SET DESCRICAO = @Descricao, ORDEM = @Ordem WHERE ID = @Id",
            new { Id = id, Descricao = descricao, Ordem = ordem });
        return rows > 0;
    }

    public async Task<bool> ExcluirTamanhoAsync(int id)
    {
        using var conn = _connectionFactory.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM PIZZA_SABORES WHERE TAMANHO_ID = @Id", new { Id = id });
        var rows = await conn.ExecuteAsync("DELETE FROM PIZZA_TAMANHOS WHERE ID = @Id", new { Id = id });
        return rows > 0;
    }

    // ── SABORES ──────────────────────────────────────────────────────────────

    public async Task<IEnumerable<PizzaSabor>> GetSaboresPorTamanhoAsync(int tamanhoId, bool apenasAtivos = true)
    {
        await EnsureTablesAsync();
        using var conn = _connectionFactory.CreateConnection();
        var where = apenasAtivos ? "AND ATIVO = '1'" : "";
        var sql = $"SELECT ID, TAMANHO_ID, DESCRICAO, INGREDIENTES, PRECO, CASE ATIVO WHEN '1' THEN 1 ELSE 0 END AS ATIVO FROM PIZZA_SABORES WHERE TAMANHO_ID = @TamanhoId {where} ORDER BY DESCRICAO";
        return await conn.QueryAsync<PizzaSabor>(sql, new { TamanhoId = tamanhoId });
    }

    public async Task<int> CriarSaborAsync(int tamanhoId, string descricao, string? ingredientes, decimal preco)
    {
        await EnsureTablesAsync();
        using var conn = _connectionFactory.CreateConnection();
        var id = await conn.QuerySingleAsync<int>("SELECT GEN_ID(PIZZA_SABORES_GEN, 1) FROM RDB$DATABASE");
        await conn.ExecuteAsync(
            "INSERT INTO PIZZA_SABORES (ID, TAMANHO_ID, DESCRICAO, INGREDIENTES, PRECO, ATIVO) VALUES (@Id, @TamanhoId, @Descricao, @Ingredientes, @Preco, '1')",
            new { Id = id, TamanhoId = tamanhoId, Descricao = descricao, Ingredientes = ingredientes, Preco = preco });
        return id;
    }

    public async Task<bool> AtualizarSaborAsync(int id, string descricao, string? ingredientes, decimal preco, bool ativo)
    {
        using var conn = _connectionFactory.CreateConnection();
        var rows = await conn.ExecuteAsync(
            "UPDATE PIZZA_SABORES SET DESCRICAO = @Descricao, INGREDIENTES = @Ingredientes, PRECO = @Preco, ATIVO = @Ativo WHERE ID = @Id",
            new { Id = id, Descricao = descricao, Ingredientes = ingredientes, Preco = preco, Ativo = ativo ? "1" : "0" });
        return rows > 0;
    }

    public async Task<bool> ExcluirSaborAsync(int id)
    {
        using var conn = _connectionFactory.CreateConnection();
        var rows = await conn.ExecuteAsync("DELETE FROM PIZZA_SABORES WHERE ID = @Id", new { Id = id });
        return rows > 0;
    }

    // ── BORDAS ───────────────────────────────────────────────────────────────

    public async Task<IEnumerable<PizzaBorda>> GetBordasAsync(bool apenasAtivas = true)
    {
        await EnsureTablesAsync();
        using var conn = _connectionFactory.CreateConnection();
        var where = apenasAtivas ? "WHERE ATIVO = '1'" : "";
        var sql = $"SELECT ID, DESCRICAO, PRECO, CASE ATIVO WHEN '1' THEN 1 ELSE 0 END AS ATIVO FROM PIZZA_BORDAS {where} ORDER BY PRECO, DESCRICAO";
        return await conn.QueryAsync<PizzaBorda>(sql);
    }

    public async Task<int> CriarBordaAsync(string descricao, decimal preco)
    {
        await EnsureTablesAsync();
        using var conn = _connectionFactory.CreateConnection();
        var id = await conn.QuerySingleAsync<int>("SELECT GEN_ID(PIZZA_BORDAS_GEN, 1) FROM RDB$DATABASE");
        await conn.ExecuteAsync(
            "INSERT INTO PIZZA_BORDAS (ID, DESCRICAO, PRECO, ATIVO) VALUES (@Id, @Descricao, @Preco, '1')",
            new { Id = id, Descricao = descricao, Preco = preco });
        return id;
    }

    public async Task<bool> AtualizarBordaAsync(int id, string descricao, decimal preco, bool ativo)
    {
        using var conn = _connectionFactory.CreateConnection();
        var rows = await conn.ExecuteAsync(
            "UPDATE PIZZA_BORDAS SET DESCRICAO = @Descricao, PRECO = @Preco, ATIVO = @Ativo WHERE ID = @Id",
            new { Id = id, Descricao = descricao, Preco = preco, Ativo = ativo ? "1" : "0" });
        return rows > 0;
    }

    public async Task<bool> ExcluirBordaAsync(int id)
    {
        using var conn = _connectionFactory.CreateConnection();
        var rows = await conn.ExecuteAsync("DELETE FROM PIZZA_BORDAS WHERE ID = @Id", new { Id = id });
        return rows > 0;
    }

    public async Task<bool> AtualizarTipoGrupoAsync(int grupoId, string tipo)
    {
        await EnsureTablesAsync();
        using var conn = _connectionFactory.CreateConnection();
        var rows = await conn.ExecuteAsync(
            "UPDATE GRUPO SET TIPO = @Tipo WHERE ID = @Id",
            new { Tipo = tipo.ToUpperInvariant(), Id = grupoId });
        return rows > 0;
    }
}
