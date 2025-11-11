using Dapper;
using IComanda.API.Data;
using IComanda.API.Models.Entities;
using IComanda.API.Models.Requests;
using IComanda.API.Repositories.Interfaces;

namespace IComanda.API.Repositories.Implementations;

public class ClienteRepository : IClienteRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ClienteRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Cliente>> BuscarClientesAsync(BuscarClienteRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereConditions = new List<string>();
        var parameters = new DynamicParameters();

        // Filtro por termo de busca
        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            whereConditions.Add("(UPPER(NOME) LIKE UPPER(@Q) OR UPPER(FANTASIA) LIKE UPPER(@Q) OR CPFCNPJ LIKE @Q OR TELEFONE LIKE @Q OR CELULAR LIKE @Q)");
            parameters.Add("@Q", $"%{request.Q}%");
        }

        // Filtro por status ativo
        if (request.Ativo.HasValue)
        {
            whereConditions.Add("ATIVO = @Ativo");
            parameters.Add("@Ativo", request.Ativo.Value ? 1 : 0);
        }

        // Filtro por não bloqueado
        if (request.NaoBloqueado.HasValue && request.NaoBloqueado.Value)
        {
            whereConditions.Add("BLOQUEADO = 0");
        }

        // Filtro por vendedor
        if (request.IdVendedor.HasValue)
        {
            whereConditions.Add("IDVENDEDOR = @IdVendedor");
            parameters.Add("@IdVendedor", request.IdVendedor.Value);
        }

        // Filtro por classificação
        if (!string.IsNullOrWhiteSpace(request.Classificacao))
        {
            whereConditions.Add("CLASSIF = @Classificacao");
            parameters.Add("@Classificacao", request.Classificacao);
        }

        var whereClause = whereConditions.Any() ? $"WHERE {string.Join(" AND ", whereConditions)}" : "";

        // Paginação
        var offset = 0;
        var limit = 50; // Default

        if (request.Pagina.HasValue && request.ItensPorPagina.HasValue)
        {
            offset = (request.Pagina.Value - 1) * request.ItensPorPagina.Value;
            limit = request.ItensPorPagina.Value;
        }

        var sql = $@"
            SELECT FIRST {limit} SKIP {offset}
                ID, NOME, CPFCNPJ, RGIE, TELEFONE, CELULAR, OBS, DATANAS,
                END1, NUM1, COMPL1, BAIRRO1, CIDADE1, UF1, EMAIL,
                END2, NUM2, COMPL2, BAIRRO2, CIDADE2, UF2,
                TABPRE, ATIVO, EMAIL1, CEP1, CEP2, CLASSIF, ACESSO, LIMITE,
                RG_ORGAO, RG_ESTADO, RG_EMISSAO, TELFAX, DT_CADASTRO,
                TIPO_PRODUTOR, IP, INCRA, NIRF, DECLA_ITR, CARRO,
                PLACA_CARRO1, PLACA_CARRO2, PLACA_CARRO3, MOTO,
                PLACA_MOTO1, PLACA_MOTO2, PLACA_MOTO3, FOTO, CEI,
                NUMPROPRIO, FANTASIA, CNAE, IDVENDEDOR, IBGE_UF,
                IBGE_MUNICIPIO, CONTRIBUINTE, MENSAL, SITE, BLOQUEADO, IDROTA
            FROM CLIENTES
            {whereClause}
            ORDER BY NOME";

        return await connection.QueryAsync<Cliente>(sql, parameters);
    }

    public async Task<Cliente?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT ID, NOME, CPFCNPJ, RGIE, TELEFONE, CELULAR, OBS, DATANAS,
                   END1, NUM1, COMPL1, BAIRRO1, CIDADE1, UF1, EMAIL,
                   END2, NUM2, COMPL2, BAIRRO2, CIDADE2, UF2,
                   TABPRE, ATIVO, EMAIL1, CEP1, CEP2, CLASSIF, ACESSO, LIMITE,
                   RG_ORGAO, RG_ESTADO, RG_EMISSAO, TELFAX, DT_CADASTRO,
                   TIPO_PRODUTOR, IP, INCRA, NIRF, DECLA_ITR, CARRO,
                   PLACA_CARRO1, PLACA_CARRO2, PLACA_CARRO3, MOTO,
                   PLACA_MOTO1, PLACA_MOTO2, PLACA_MOTO3, FOTO, CEI,
                   NUMPROPRIO, FANTASIA, CNAE, IDVENDEDOR, IBGE_UF,
                   IBGE_MUNICIPIO, CONTRIBUINTE, MENSAL, SITE, BLOQUEADO, IDROTA
            FROM CLIENTES
            WHERE ID = @Id";

        return await connection.QueryFirstOrDefaultAsync<Cliente>(sql, new { Id = id });
    }

    public async Task<Cliente?> GetByCpfCnpjAsync(string cpfCnpj)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT ID, NOME, CPFCNPJ, RGIE, TELEFONE, CELULAR, OBS, DATANAS,
                   END1, NUM1, COMPL1, BAIRRO1, CIDADE1, UF1, EMAIL,
                   END2, NUM2, COMPL2, BAIRRO2, CIDADE2, UF2,
                   TABPRE, ATIVO, EMAIL1, CEP1, CEP2, CLASSIF, ACESSO, LIMITE,
                   RG_ORGAO, RG_ESTADO, RG_EMISSAO, TELFAX, DT_CADASTRO,
                   TIPO_PRODUTOR, IP, INCRA, NIRF, DECLA_ITR, CARRO,
                   PLACA_CARRO1, PLACA_CARRO2, PLACA_CARRO3, MOTO,
                   PLACA_MOTO1, PLACA_MOTO2, PLACA_MOTO3, FOTO, CEI,
                   NUMPROPRIO, FANTASIA, CNAE, IDVENDEDOR, IBGE_UF,
                   IBGE_MUNICIPIO, CONTRIBUINTE, MENSAL, SITE, BLOQUEADO, IDROTA
            FROM CLIENTES
            WHERE CPFCNPJ = @CpfCnpj";

        return await connection.QueryFirstOrDefaultAsync<Cliente>(sql, new { CpfCnpj = cpfCnpj });
    }

    public async Task<IEnumerable<Cliente>> GetByVendedorAsync(int idVendedor)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT ID, NOME, CPFCNPJ, RGIE, TELEFONE, CELULAR, OBS, DATANAS,
                   END1, NUM1, COMPL1, BAIRRO1, CIDADE1, UF1, EMAIL,
                   END2, NUM2, COMPL2, BAIRRO2, CIDADE2, UF2,
                   TABPRE, ATIVO, EMAIL1, CEP1, CEP2, CLASSIF, ACESSO, LIMITE,
                   RG_ORGAO, RG_ESTADO, RG_EMISSAO, TELFAX, DT_CADASTRO,
                   TIPO_PRODUTOR, IP, INCRA, NIRF, DECLA_ITR, CARRO,
                   PLACA_CARRO1, PLACA_CARRO2, PLACA_CARRO3, MOTO,
                   PLACA_MOTO1, PLACA_MOTO2, PLACA_MOTO3, FOTO, CEI,
                   NUMPROPRIO, FANTASIA, CNAE, IDVENDEDOR, IBGE_UF,
                   IBGE_MUNICIPIO, CONTRIBUINTE, MENSAL, SITE, BLOQUEADO, IDROTA
            FROM CLIENTES
            WHERE IDVENDEDOR = @IdVendedor
            ORDER BY NOME";

        return await connection.QueryAsync<Cliente>(sql, new { IdVendedor = idVendedor });
    }

    public async Task<int> ContarClientesAsync(BuscarClienteRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereConditions = new List<string>();
        var parameters = new DynamicParameters();

        // Aplicar os mesmos filtros da busca
        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            whereConditions.Add("(UPPER(NOME) LIKE UPPER(@Q) OR UPPER(FANTASIA) LIKE UPPER(@Q) OR CPFCNPJ LIKE @Q OR TELEFONE LIKE @Q OR CELULAR LIKE @Q)");
            parameters.Add("@Q", $"%{request.Q}%");
        }

        if (request.Ativo.HasValue)
        {
            whereConditions.Add("ATIVO = @Ativo");
            parameters.Add("@Ativo", request.Ativo.Value ? 1 : 0);
        }

        if (request.NaoBloqueado.HasValue && request.NaoBloqueado.Value)
        {
            whereConditions.Add("BLOQUEADO = 0");
        }

        if (request.IdVendedor.HasValue)
        {
            whereConditions.Add("IDVENDEDOR = @IdVendedor");
            parameters.Add("@IdVendedor", request.IdVendedor.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Classificacao))
        {
            whereConditions.Add("CLASSIF = @Classificacao");
            parameters.Add("@Classificacao", request.Classificacao);
        }

        var whereClause = whereConditions.Any() ? $"WHERE {string.Join(" AND ", whereConditions)}" : "";

        var sql = $"SELECT COUNT(*) FROM CLIENTES {whereClause}";

        return await connection.QuerySingleAsync<int>(sql, parameters);
    }

    public async Task<bool> ExistePorCpfCnpjAsync(string cpfCnpj)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = "SELECT COUNT(*) FROM CLIENTES WHERE CPFCNPJ = @CpfCnpj";
        var count = await connection.QuerySingleAsync<int>(sql, new { CpfCnpj = cpfCnpj });

        return count > 0;
    }

    public async Task<bool> ExistePorTelefoneAsync(string telefone)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = "SELECT COUNT(*) FROM CLIENTES WHERE TELEFONE = @Telefone";
        var count = await connection.QuerySingleAsync<int>(sql, new { Telefone = telefone });

        return count > 0;
    }

    public async Task<Cliente?> GetByTelefoneAsync(string telefone)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT ID, NOME, CPFCNPJ, RGIE, TELEFONE, CELULAR, OBS, DATANAS,
                   END1, NUM1, COMPL1, BAIRRO1, CIDADE1, UF1, EMAIL,
                   END2, NUM2, COMPL2, BAIRRO2, CIDADE2, UF2,
                   TABPRE, ATIVO, EMAIL1, CEP1, CEP2, CLASSIF, ACESSO, LIMITE,
                   RG_ORGAO, RG_ESTADO, RG_EMISSAO, TELFAX, DT_CADASTRO,
                   TIPO_PRODUTOR, IP, INCRA, NIRF, DECLA_ITR, CARRO,
                   PLACA_CARRO1, PLACA_CARRO2, PLACA_CARRO3, MOTO,
                   PLACA_MOTO1, PLACA_MOTO2, PLACA_MOTO3, FOTO, CEI,
                   NUMPROPRIO, FANTASIA, CNAE, IDVENDEDOR, IBGE_UF,
                   IBGE_MUNICIPIO, CONTRIBUINTE, MENSAL, SITE, BLOQUEADO, IDROTA
            FROM CLIENTES
            WHERE TELEFONE = @Telefone";

        return await connection.QueryFirstOrDefaultAsync<Cliente>(sql, new { Telefone = telefone });
    }

    public async Task<bool> CriarClienteAsync(Cliente cliente)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            INSERT INTO CLIENTES (
                NOME, CPFCNPJ, TELEFONE, CELULAR, FANTASIA, 
                DT_CADASTRO, ATIVO, BLOQUEADO
            ) VALUES (
                @Nome, @CpfCnpj, @Telefone, @Celular, @Fantasia, 
                @DataCadastro, @Ativo, 0
            )";

        var parameters = new
        {
            Nome = cliente.Nome,
            CpfCnpj = cliente.CpfCnpj,
            Telefone = cliente.Telefone,
            Celular = cliente.Celular,
            Fantasia = cliente.Fantasia,
            DataCadastro = DateTime.Now,
            Ativo = 1
        };

        var rowsAffected = await connection.ExecuteAsync(sql, parameters);

        return rowsAffected > 0;
    }
}
