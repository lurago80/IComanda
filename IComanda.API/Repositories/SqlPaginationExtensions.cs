namespace IComanda.API.Repositories;

/// <summary>
/// Extensões para geração de SQL com paginação (Firebird)
/// </summary>
public static class SqlPaginationExtensions
{
    /// <summary>
    /// Gerar SQL com paginação para Firebird
    /// Firebird usa ROWS e TO para paginação
    /// Sintaxe: SELECT ... FROM ... ROWS n TO m
    /// </summary>
    public static string AddPaginationToSql(this string sql, int offset, int limit)
    {
        // Firebird pagination: ROWS offset TO (offset + limit)
        // ROWS é 1-based: ROWS 1 TO 10 = primeiro 10 registros
        var start = offset + 1; // Converter de 0-based para 1-based
        var end = offset + limit;

        return $"{sql} ROWS {start} TO {end}";
    }

    /// <summary>
    /// Gerar SQL com SKIP/OFFSET para OFFSET/FETCH (para bancos que suportam)
    /// Nota: Firebird usa ROWS ... TO ..., não OFFSET/FETCH
    /// </summary>
    public static string AddOffsetFetchToSql(this string sql, int offset, int limit)
    {
        return $"{sql} OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY";
    }

    /// <summary>
    /// Adicionar ordenação ao SQL
    /// </summary>
    public static string AddOrderBy(this string sql, string? orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
            return sql;

        // Validar ordenação para evitar SQL injection
        // Aceitar: "CAMPO ASC", "CAMPO DESC", "CAMPO", "CAMPO1 ASC, CAMPO2 DESC"
        var validOrder = ValidateOrderBy(orderBy);

        if (string.IsNullOrWhiteSpace(validOrder))
            return sql;

        return sql.Contains("ORDER BY") ? sql : $"{sql} ORDER BY {validOrder}";
    }

    /// <summary>
    /// Validar e limpar string de ORDER BY para evitar SQL injection
    /// </summary>
    private static string ValidateOrderBy(string orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
            return "";

        // Remover caracteres perigosos
        var cleaned = System.Text.RegularExpressions.Regex.Replace(
            orderBy,
            @"[^a-zA-Z0-9_\s,.]",
            ""
        );

        // Validar palavras-chave
        var allowed = new[] { "ASC", "DESC", "AND", "OR" };
        var parts = cleaned.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            if (!char.IsLetterOrDigit(part[0]) && part[0] != '_')
                return ""; // Injeção detectada
        }

        return cleaned.Length > 100 ? "" : cleaned;
    }

    /// <summary>
    /// Gerar WHERE clause com filtro de texto
    /// </summary>
    public static string AddTextSearchFilter(
        this string sql,
        string? searchTerm,
        params string[] searchColumns)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || searchColumns.Length == 0)
            return sql;

        // Escapar single quotes para SQL
        var safeTerm = searchTerm.Replace("'", "''");

        // Montar filtros LIKE para cada coluna
        var filters = string.Join(
            " OR ",
            searchColumns.Select(col => $"UPPER({col}) LIKE UPPER('%{safeTerm}%')")
        );

        var whereClause = $"({filters})";

        if (sql.Contains("WHERE"))
            return $"{sql} AND {whereClause}";
        else
            return $"{sql} WHERE {whereClause}";
    }

    /// <summary>
    /// Contar total de registros (remove ROWS ... TO para COUNT)
    /// </summary>
    public static string GetCountSql(this string sql)
    {
        // Remover ROWS ... TO clause se existir
        var countSql = System.Text.RegularExpressions.Regex.Replace(
            sql,
            @"ROWS\s+\d+\s+TO\s+\d+",
            "",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );

        // Remover ORDER BY se existir (não necessário para COUNT)
        countSql = System.Text.RegularExpressions.Regex.Replace(
            countSql,
            @"ORDER\s+BY\s+.*$",
            "",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );

        // Substituir SELECT ... FROM por SELECT COUNT(*)
        countSql = System.Text.RegularExpressions.Regex.Replace(
            countSql,
            @"SELECT\s+.*?\s+FROM",
            "SELECT COUNT(*) FROM",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );

        return countSql;
    }
}

/// <summary>
/// Builder fluente para construir queries paginadas
/// </summary>
public class SqlPaginationBuilder
{
    private string _sql;
    private string? _orderBy;
    private string? _searchTerm;
    private string[] _searchColumns;
    private int _offset;
    private int _limit;

    public SqlPaginationBuilder(string baseSql)
    {
        _sql = baseSql;
        _searchColumns = Array.Empty<string>();
        _offset = 0;
        _limit = 20;
    }

    public SqlPaginationBuilder OrderBy(string? orderBy)
    {
        _orderBy = orderBy;
        return this;
    }

    public SqlPaginationBuilder Pagination(int offset, int limit)
    {
        _offset = offset;
        _limit = limit;
        return this;
    }

    public SqlPaginationBuilder Search(string? searchTerm, params string[] columns)
    {
        _searchTerm = searchTerm;
        _searchColumns = columns;
        return this;
    }

    public string Build()
    {
        var sql = _sql;

        // Aplicar pesquisa
        if (!string.IsNullOrWhiteSpace(_searchTerm))
            sql = sql.AddTextSearchFilter(_searchTerm, _searchColumns);

        // Aplicar ordenação
        if (!string.IsNullOrWhiteSpace(_orderBy))
            sql = sql.AddOrderBy(_orderBy);

        // Aplicar paginação
        sql = sql.AddPaginationToSql(_offset, _limit);

        return sql;
    }

    public string BuildCount()
    {
        var sql = _sql;

        // Aplicar pesquisa (sem paginação para COUNT)
        if (!string.IsNullOrWhiteSpace(_searchTerm))
            sql = sql.AddTextSearchFilter(_searchTerm, _searchColumns);

        return sql.GetCountSql();
    }
}
