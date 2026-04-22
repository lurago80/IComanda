using Dapper;
using FirebirdSql.Data.FirebirdClient;
using IComanda.API.Data;
using IComanda.API.Models.Entities;
using IComanda.API.Models.Requests;
using IComanda.API.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Repositories.Implementations;

public class ClienteRepository : IClienteRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<ClienteRepository> _logger;

    public ClienteRepository(IDbConnectionFactory connectionFactory, ILogger<ClienteRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<Cliente>> BuscarClientesAsync(BuscarClienteRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereConditions = new List<string>();
        var parameters = new DynamicParameters();

        // Filtro por termo de busca
        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            // Normalizar: remover espaços extras, trim, colapsar múltiplos espaços
            var termoNormalizado = System.Text.RegularExpressions.Regex.Replace(request.Q.Trim(), @"\s+", " ");

            // Remover formatação para busca em CPF/telefone (pontos, traços, parênteses, espaços)
            var termoLimpo = System.Text.RegularExpressions.Regex.Replace(termoNormalizado, @"[^\d\w]", "");

            var termoUpper = termoNormalizado.ToUpperInvariant();
            var termoLimpoUpper = termoLimpo.ToUpperInvariant();

            // Dividir em palavras para busca robusta por nome:
            // "Juliana da Silva Pinto" → procura cada palavra individualmente no NOME
            // Isso resolve: nomes com espaços duplos no banco, nome parcialmente cadastrado,
            // e texto copiado de qualquer fonte que venha com espaços extras.
            var palavras = termoNormalizado
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.ToUpperInvariant())
                .ToList();

            string nomeCondicao;
            string fantasiaCondicao;

            if (palavras.Count > 1)
            {
                // Cada palavra deve aparecer em algum lugar no campo (busca AND por palavras)
                var nomeWordConds = palavras.Select((_, i) => $"UPPER(TRIM(NOME)) LIKE @NomePalavra{i}");
                var fantasiaWordConds = palavras.Select((_, i) => $"UPPER(TRIM(FANTASIA)) LIKE @NomePalavra{i}");
                nomeCondicao = $"({string.Join(" AND ", nomeWordConds)})";
                fantasiaCondicao = $"({string.Join(" AND ", fantasiaWordConds)})";

                for (int i = 0; i < palavras.Count; i++)
                    parameters.Add($"NomePalavra{i}", $"%{palavras[i]}%");
            }
            else
            {
                // Termo único: busca simples
                nomeCondicao = "UPPER(TRIM(NOME)) LIKE @Q";
                fantasiaCondicao = "UPPER(TRIM(FANTASIA)) LIKE @Q";
            }

            whereConditions.Add($@"
                ({nomeCondicao} OR
                 {fantasiaCondicao} OR
                 UPPER(CPFCNPJ) LIKE @Q OR
                 UPPER(CPFCNPJ) LIKE @QLimpo OR
                 UPPER(TELEFONE) LIKE @Q OR
                 UPPER(TELEFONE) LIKE @QLimpo OR
                 UPPER(CELULAR) LIKE @Q OR
                 UPPER(CELULAR) LIKE @QLimpo)");

            parameters.Add("@Q", $"%{termoUpper}%");
            parameters.Add("@QLimpo", $"%{termoLimpoUpper}%");
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
                ID as Id, 
                NOME as Nome, 
                CPFCNPJ as CpfCnpj, 
                RGIE as RgIe, 
                TELEFONE as Telefone, 
                CELULAR as Celular, 
                OBS as Obs, 
                DATANAS as DataNascimento,
                END1 as Endereco1, 
                NUM1 as Numero1, 
                COMPL1 as Complemento1, 
                BAIRRO1 as Bairro1, 
                CIDADE1 as Cidade1, 
                UF1 as Uf1, 
                EMAIL as Email,
                END2 as Endereco2, 
                NUM2 as Numero2, 
                COMPL2 as Complemento2, 
                BAIRRO2 as Bairro2, 
                CIDADE2 as Cidade2, 
                UF2 as Uf2,
                TABPRE as TabelaPreco, 
                ATIVO as Ativo, 
                EMAIL1 as Email1, 
                CEP1 as Cep1, 
                CEP2 as Cep2, 
                CLASSIF as Classificacao, 
                ACESSO as Acesso, 
                LIMITE as Limite,
                RG_ORGAO as RgOrgao, 
                RG_ESTADO as RgEstado, 
                RG_EMISSAO as RgEmissao, 
                TELFAX as TelFax, 
                DT_CADASTRO as DataCadastro,
                TIPO_PRODUTOR as TipoProdutor, 
                IP as Ip, 
                INCRA as Incra, 
                NIRF as Nirf, 
                DECLA_ITR as DeclaItr, 
                CARRO as Carro,
                PLACA_CARRO1 as PlacaCarro1, 
                PLACA_CARRO2 as PlacaCarro2, 
                PLACA_CARRO3 as PlacaCarro3, 
                MOTO as Moto,
                PLACA_MOTO1 as PlacaMoto1, 
                PLACA_MOTO2 as PlacaMoto2, 
                PLACA_MOTO3 as PlacaMoto3, 
                FOTO as Foto, 
                CEI as Cei,
                NUMPROPRIO as NumProprio, 
                FANTASIA as Fantasia, 
                CNAE as Cnae, 
                IDVENDEDOR as IdVendedor, 
                IBGE_UF as IbgeUf,
                IBGE_MUNICIPIO as IbgeMunicipio, 
                CONTRIBUINTE as Contribuinte, 
                MENSAL as Mensal, 
                SITE as Site, 
                BLOQUEADO as Bloqueado, 
                IDROTA as IdRota
            FROM CLIENTES
            {whereClause}
            ORDER BY NOME";

        return await connection.QueryAsync<Cliente>(sql, parameters);
    }

    public async Task<Cliente?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT ID as Id, 
                   NOME as Nome, 
                   CPFCNPJ as CpfCnpj, 
                   RGIE as RgIe, 
                   TELEFONE as Telefone, 
                   CELULAR as Celular, 
                   OBS as Obs, 
                   DATANAS as DataNascimento,
                   END1 as Endereco1, 
                   NUM1 as Numero1, 
                   COMPL1 as Complemento1, 
                   BAIRRO1 as Bairro1, 
                   CIDADE1 as Cidade1, 
                   UF1 as Uf1, 
                   EMAIL as Email,
                   END2 as Endereco2, 
                   NUM2 as Numero2, 
                   COMPL2 as Complemento2, 
                   BAIRRO2 as Bairro2, 
                   CIDADE2 as Cidade2, 
                   UF2 as Uf2,
                   TABPRE as TabelaPreco, 
                   ATIVO as Ativo, 
                   EMAIL1 as Email1, 
                   CEP1 as Cep1, 
                   CEP2 as Cep2, 
                   CLASSIF as Classificacao, 
                   ACESSO as Acesso, 
                   LIMITE as Limite,
                   RG_ORGAO as RgOrgao, 
                   RG_ESTADO as RgEstado, 
                   RG_EMISSAO as RgEmissao, 
                   TELFAX as TelFax, 
                   DT_CADASTRO as DataCadastro,
                   TIPO_PRODUTOR as TipoProdutor, 
                   IP as Ip, 
                   INCRA as Incra, 
                   NIRF as Nirf, 
                   DECLA_ITR as DeclaItr, 
                   CARRO as Carro,
                   PLACA_CARRO1 as PlacaCarro1, 
                   PLACA_CARRO2 as PlacaCarro2, 
                   PLACA_CARRO3 as PlacaCarro3, 
                   MOTO as Moto,
                   PLACA_MOTO1 as PlacaMoto1, 
                   PLACA_MOTO2 as PlacaMoto2, 
                   PLACA_MOTO3 as PlacaMoto3, 
                   FOTO as Foto, 
                   CEI as Cei,
                   NUMPROPRIO as NumProprio, 
                   FANTASIA as Fantasia, 
                   CNAE as Cnae, 
                   IDVENDEDOR as IdVendedor, 
                   IBGE_UF as IbgeUf,
                   IBGE_MUNICIPIO as IbgeMunicipio, 
                   CONTRIBUINTE as Contribuinte, 
                   MENSAL as Mensal, 
                   SITE as Site, 
                   BLOQUEADO as Bloqueado, 
                   IDROTA as IdRota
            FROM CLIENTES
            WHERE ID = @Id";

        return await connection.QueryFirstOrDefaultAsync<Cliente>(sql, new { Id = id });
    }

    public async Task<Cliente?> GetByCpfCnpjAsync(string cpfCnpj)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT ID as Id, 
                   NOME as Nome, 
                   CPFCNPJ as CpfCnpj, 
                   RGIE as RgIe, 
                   TELEFONE as Telefone, 
                   CELULAR as Celular, 
                   OBS as Obs, 
                   DATANAS as DataNascimento,
                   END1 as Endereco1, 
                   NUM1 as Numero1, 
                   COMPL1 as Complemento1, 
                   BAIRRO1 as Bairro1, 
                   CIDADE1 as Cidade1, 
                   UF1 as Uf1, 
                   EMAIL as Email,
                   END2 as Endereco2, 
                   NUM2 as Numero2, 
                   COMPL2 as Complemento2, 
                   BAIRRO2 as Bairro2, 
                   CIDADE2 as Cidade2, 
                   UF2 as Uf2,
                   TABPRE as TabelaPreco, 
                   ATIVO as Ativo, 
                   EMAIL1 as Email1, 
                   CEP1 as Cep1, 
                   CEP2 as Cep2, 
                   CLASSIF as Classificacao, 
                   ACESSO as Acesso, 
                   LIMITE as Limite,
                   RG_ORGAO as RgOrgao, 
                   RG_ESTADO as RgEstado, 
                   RG_EMISSAO as RgEmissao, 
                   TELFAX as TelFax, 
                   DT_CADASTRO as DataCadastro,
                   TIPO_PRODUTOR as TipoProdutor, 
                   IP as Ip, 
                   INCRA as Incra, 
                   NIRF as Nirf, 
                   DECLA_ITR as DeclaItr, 
                   CARRO as Carro,
                   PLACA_CARRO1 as PlacaCarro1, 
                   PLACA_CARRO2 as PlacaCarro2, 
                   PLACA_CARRO3 as PlacaCarro3, 
                   MOTO as Moto,
                   PLACA_MOTO1 as PlacaMoto1, 
                   PLACA_MOTO2 as PlacaMoto2, 
                   PLACA_MOTO3 as PlacaMoto3, 
                   FOTO as Foto, 
                   CEI as Cei,
                   NUMPROPRIO as NumProprio, 
                   FANTASIA as Fantasia, 
                   CNAE as Cnae, 
                   IDVENDEDOR as IdVendedor, 
                   IBGE_UF as IbgeUf,
                   IBGE_MUNICIPIO as IbgeMunicipio, 
                   CONTRIBUINTE as Contribuinte, 
                   MENSAL as Mensal, 
                   SITE as Site, 
                   BLOQUEADO as Bloqueado, 
                   IDROTA as IdRota
            FROM CLIENTES
            WHERE CPFCNPJ = @CpfCnpj";

        return await connection.QueryFirstOrDefaultAsync<Cliente>(sql, new { CpfCnpj = cpfCnpj });
    }

    public async Task<IEnumerable<Cliente>> GetByVendedorAsync(int idVendedor)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT ID as Id, 
                   NOME as Nome, 
                   CPFCNPJ as CpfCnpj, 
                   RGIE as RgIe, 
                   TELEFONE as Telefone, 
                   CELULAR as Celular, 
                   OBS as Obs, 
                   DATANAS as DataNascimento,
                   END1 as Endereco1, 
                   NUM1 as Numero1, 
                   COMPL1 as Complemento1, 
                   BAIRRO1 as Bairro1, 
                   CIDADE1 as Cidade1, 
                   UF1 as Uf1, 
                   EMAIL as Email,
                   END2 as Endereco2, 
                   NUM2 as Numero2, 
                   COMPL2 as Complemento2, 
                   BAIRRO2 as Bairro2, 
                   CIDADE2 as Cidade2, 
                   UF2 as Uf2,
                   TABPRE as TabelaPreco, 
                   ATIVO as Ativo, 
                   EMAIL1 as Email1, 
                   CEP1 as Cep1, 
                   CEP2 as Cep2, 
                   CLASSIF as Classificacao, 
                   ACESSO as Acesso, 
                   LIMITE as Limite,
                   RG_ORGAO as RgOrgao, 
                   RG_ESTADO as RgEstado, 
                   RG_EMISSAO as RgEmissao, 
                   TELFAX as TelFax, 
                   DT_CADASTRO as DataCadastro,
                   TIPO_PRODUTOR as TipoProdutor, 
                   IP as Ip, 
                   INCRA as Incra, 
                   NIRF as Nirf, 
                   DECLA_ITR as DeclaItr, 
                   CARRO as Carro,
                   PLACA_CARRO1 as PlacaCarro1, 
                   PLACA_CARRO2 as PlacaCarro2, 
                   PLACA_CARRO3 as PlacaCarro3, 
                   MOTO as Moto,
                   PLACA_MOTO1 as PlacaMoto1, 
                   PLACA_MOTO2 as PlacaMoto2, 
                   PLACA_MOTO3 as PlacaMoto3, 
                   FOTO as Foto, 
                   CEI as Cei,
                   NUMPROPRIO as NumProprio, 
                   FANTASIA as Fantasia, 
                   CNAE as Cnae, 
                   IDVENDEDOR as IdVendedor, 
                   IBGE_UF as IbgeUf,
                   IBGE_MUNICIPIO as IbgeMunicipio, 
                   CONTRIBUINTE as Contribuinte, 
                   MENSAL as Mensal, 
                   SITE as Site, 
                   BLOQUEADO as Bloqueado, 
                   IDROTA as IdRota
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

        // Mesma lógica robusta da busca: normalizar espaços + busca por palavras
        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            var termoNormalizado = System.Text.RegularExpressions.Regex.Replace(request.Q.Trim(), @"\s+", " ");
            var termoLimpo = System.Text.RegularExpressions.Regex.Replace(termoNormalizado, @"[^\d\w]", "");
            var termoUpper = termoNormalizado.ToUpperInvariant();
            var termoLimpoUpper = termoLimpo.ToUpperInvariant();

            var palavras = termoNormalizado
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.ToUpperInvariant())
                .ToList();

            string nomeCondicao;
            string fantasiaCondicao;

            if (palavras.Count > 1)
            {
                var nomeWordConds = palavras.Select((_, i) => $"UPPER(TRIM(NOME)) LIKE @NomePalavra{i}");
                var fantasiaWordConds = palavras.Select((_, i) => $"UPPER(TRIM(FANTASIA)) LIKE @NomePalavra{i}");
                nomeCondicao = $"({string.Join(" AND ", nomeWordConds)})";
                fantasiaCondicao = $"({string.Join(" AND ", fantasiaWordConds)})";
                for (int i = 0; i < palavras.Count; i++)
                    parameters.Add($"NomePalavra{i}", $"%{palavras[i]}%");
            }
            else
            {
                nomeCondicao = "UPPER(TRIM(NOME)) LIKE @Q";
                fantasiaCondicao = "UPPER(TRIM(FANTASIA)) LIKE @Q";
            }

            whereConditions.Add($@"
                ({nomeCondicao} OR
                 {fantasiaCondicao} OR
                 UPPER(CPFCNPJ) LIKE @Q OR
                 UPPER(CPFCNPJ) LIKE @QLimpo OR
                 UPPER(TELEFONE) LIKE @Q OR
                 UPPER(TELEFONE) LIKE @QLimpo OR
                 UPPER(CELULAR) LIKE @Q OR
                 UPPER(CELULAR) LIKE @QLimpo)");

            parameters.Add("@Q", $"%{termoUpper}%");
            parameters.Add("@QLimpo", $"%{termoLimpoUpper}%");
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

        // Verifica tanto TELEFONE quanto CELULAR para evitar duplicidade em ambas as colunas
        var sql = @"SELECT COUNT(*) FROM CLIENTES
                    WHERE TELEFONE = @Telefone
                       OR CELULAR  = @Telefone";
        var count = await connection.QuerySingleAsync<int>(sql, new { Telefone = telefone });

        return count > 0;
    }

    public async Task<Cliente?> GetByTelefoneAsync(string telefone)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT ID as Id, 
                   NOME as Nome, 
                   CPFCNPJ as CpfCnpj, 
                   RGIE as RgIe, 
                   TELEFONE as Telefone, 
                   CELULAR as Celular, 
                   OBS as Obs, 
                   DATANAS as DataNascimento,
                   END1 as Endereco1, 
                   NUM1 as Numero1, 
                   COMPL1 as Complemento1, 
                   BAIRRO1 as Bairro1, 
                   CIDADE1 as Cidade1, 
                   UF1 as Uf1, 
                   EMAIL as Email,
                   END2 as Endereco2, 
                   NUM2 as Numero2, 
                   COMPL2 as Complemento2, 
                   BAIRRO2 as Bairro2, 
                   CIDADE2 as Cidade2, 
                   UF2 as Uf2,
                   TABPRE as TabelaPreco, 
                   ATIVO as Ativo, 
                   EMAIL1 as Email1, 
                   CEP1 as Cep1, 
                   CEP2 as Cep2, 
                   CLASSIF as Classificacao, 
                   ACESSO as Acesso, 
                   LIMITE as Limite,
                   RG_ORGAO as RgOrgao, 
                   RG_ESTADO as RgEstado, 
                   RG_EMISSAO as RgEmissao, 
                   TELFAX as TelFax, 
                   DT_CADASTRO as DataCadastro,
                   TIPO_PRODUTOR as TipoProdutor, 
                   IP as Ip, 
                   INCRA as Incra, 
                   NIRF as Nirf, 
                   DECLA_ITR as DeclaItr, 
                   CARRO as Carro,
                   PLACA_CARRO1 as PlacaCarro1, 
                   PLACA_CARRO2 as PlacaCarro2, 
                   PLACA_CARRO3 as PlacaCarro3, 
                   MOTO as Moto,
                   PLACA_MOTO1 as PlacaMoto1, 
                   PLACA_MOTO2 as PlacaMoto2, 
                   PLACA_MOTO3 as PlacaMoto3, 
                   FOTO as Foto, 
                   CEI as Cei,
                   NUMPROPRIO as NumProprio, 
                   FANTASIA as Fantasia, 
                   CNAE as Cnae, 
                   IDVENDEDOR as IdVendedor, 
                   IBGE_UF as IbgeUf,
                   IBGE_MUNICIPIO as IbgeMunicipio, 
                   CONTRIBUINTE as Contribuinte, 
                   MENSAL as Mensal, 
                   SITE as Site, 
                   BLOQUEADO as Bloqueado, 
                   IDROTA as IdRota
            FROM CLIENTES
            WHERE TELEFONE = @Telefone";

        return await connection.QueryFirstOrDefaultAsync<Cliente>(sql, new { Telefone = telefone });
    }

    public async Task<bool> CriarClienteAsync(Cliente cliente)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Gerar próximo ID antes de inserir
        int novoId;
        try
        {
            var sqlId = "SELECT GEN_ID(GEN_CLIENTES_ID, 1) AS ID FROM RDB$DATABASE";
            var result = await connection.QuerySingleAsync<dynamic>(sqlId);
            novoId = Convert.ToInt32(result.ID);
        }
        catch
        {
            // Se não existir generator, buscar o máximo
            var sqlMax = "SELECT COALESCE(MAX(ID), 0) + 1 AS ID FROM CLIENTES";
            novoId = await connection.QuerySingleAsync<int>(sqlMax);
        }

        cliente.Id = novoId;

        var sql = @"
            INSERT INTO CLIENTES (
                ID, NOME, CPFCNPJ, TELEFONE, CELULAR, FANTASIA, 
                END1, NUM1, COMPL1, BAIRRO1, CIDADE1, UF1, CEP1,
                DT_CADASTRO, ATIVO, BLOQUEADO
            ) VALUES (
                @Id, @Nome, @CpfCnpj, @Telefone, @Celular, @Fantasia, 
                @Endereco1, @Numero1, @Complemento1, @Bairro1, @Cidade1, @Uf1, @Cep1,
                @DataCadastro, @Ativo, 0
            )";

        var parameters = new
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            // CORREÇÃO: usar null em vez de string vazia para CPFCNPJ e CELULAR.
            // Colunas com UNIQUE CONSTRAINT no Firebird rejeitam múltiplos '' (string vazia),
            // mas aceitam múltiplos NULL, pois NULL não viola unicidade.
            CpfCnpj  = string.IsNullOrWhiteSpace(cliente.CpfCnpj)  ? (string?)null : cliente.CpfCnpj,
            Telefone  = cliente.Telefone ?? string.Empty,
            Celular   = string.IsNullOrWhiteSpace(cliente.Celular)  ? (string?)null : cliente.Celular,
            Fantasia  = cliente.Fantasia ?? string.Empty,
            Endereco1 = cliente.Endereco1 ?? string.Empty,
            Numero1   = cliente.Numero1 ?? string.Empty,
            Complemento1 = cliente.Complemento1 ?? string.Empty,
            Bairro1   = cliente.Bairro1 ?? string.Empty,
            Cidade1   = cliente.Cidade1 ?? string.Empty,
            Uf1       = cliente.Uf1 ?? string.Empty,
            Cep1      = cliente.Cep1 ?? string.Empty,
            DataCadastro = DateTime.Now,
            Ativo = cliente.Ativo ?? 1
        };

        try
        {
            var rowsAffected = await connection.ExecuteAsync(sql, parameters);
            return rowsAffected > 0;
        }
        catch (FbException fbEx)
        {
            // Traduzir erros do Firebird em mensagens compreensíveis
            _logger.LogError(fbEx, "Erro Firebird ao inserir cliente ID={Id} Nome={Nome}", cliente.Id, cliente.Nome);

            var msg = fbEx.Message;

            // Violação de unicidade (CPFCNPJ, TELEFONE, CELULAR...)
            if (msg.Contains("violation of PRIMARY or UNIQUE KEY", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("unique constraint", StringComparison.OrdinalIgnoreCase))
            {
                if (msg.Contains("CPFCNPJ", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Já existe um cliente cadastrado com este CPF/CNPJ.", fbEx);
                if (msg.Contains("TELEFONE", StringComparison.OrdinalIgnoreCase) ||
                    msg.Contains("CELULAR",  StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Já existe um cliente cadastrado com este telefone.", fbEx);

                throw new InvalidOperationException(
                    $"Dado duplicado no banco de dados. Verifique CPF/CNPJ ou telefone. (Detalhe: {msg})", fbEx);
            }

            // Campo muito longo
            if (msg.Contains("truncation", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("TOO BIG",    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Um dos campos está com texto muito longo para o banco de dados. Reduza o endereço ou o nome.", fbEx);
            }

            // Qualquer outro erro do Firebird — propagar com a mensagem real
            throw new InvalidOperationException($"Erro no banco de dados ao cadastrar cliente: {msg}", fbEx);
        }
    }

    public async Task<int> CriarClienteCompletoAsync(Cliente cliente)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Gerar próximo ID
        var sqlId = "SELECT GEN_ID(GEN_CLIENTES_ID, 1) AS ID FROM RDB$DATABASE";
        int novoId;
        try
        {
            var result = await connection.QuerySingleAsync<dynamic>(sqlId);
            novoId = Convert.ToInt32(result.ID);
        }
        catch
        {
            // Se não existir generator, buscar o máximo
            var sqlMax = "SELECT COALESCE(MAX(ID), 0) + 1 AS ID FROM CLIENTES";
            novoId = await connection.QuerySingleAsync<int>(sqlMax);
        }

        cliente.Id = novoId;

        var sql = @"
            INSERT INTO CLIENTES (
                ID, NOME, CPFCNPJ, RGIE, TELEFONE, CELULAR, OBS, DATANAS,
                END1, NUM1, COMPL1, BAIRRO1, CIDADE1, UF1, EMAIL,
                END2, NUM2, COMPL2, BAIRRO2, CIDADE2, UF2,
                TABPRE, ATIVO, EMAIL1, CEP1, CEP2, CLASSIF, ACESSO, LIMITE,
                RG_ORGAO, RG_ESTADO, RG_EMISSAO, TELFAX, DT_CADASTRO,
                TIPO_PRODUTOR, IP, INCRA, NIRF, DECLA_ITR, CARRO,
                PLACA_CARRO1, PLACA_CARRO2, PLACA_CARRO3, MOTO,
                PLACA_MOTO1, PLACA_MOTO2, PLACA_MOTO3, CEI,
                NUMPROPRIO, FANTASIA, CNAE, IDVENDEDOR, IBGE_UF,
                IBGE_MUNICIPIO, CONTRIBUINTE, MENSAL, SITE, BLOQUEADO, IDROTA,
                VALORALUGUEL, VENCALUGUEL, COMISSAO
            ) VALUES (
                @Id, @Nome, @CpfCnpj, @RgIe, @Telefone, @Celular, @Obs, @DataNascimento,
                @Endereco1, @Numero1, @Complemento1, @Bairro1, @Cidade1, @Uf1, @Email,
                @Endereco2, @Numero2, @Complemento2, @Bairro2, @Cidade2, @Uf2,
                @TabelaPreco, @Ativo, @Email1, @Cep1, @Cep2, @Classificacao, @Acesso, @Limite,
                @RgOrgao, @RgEstado, @RgEmissao, @TelFax, @DataCadastro,
                @TipoProdutor, @Ip, @Incra, @Nirf, @DeclaItr, @Carro,
                @PlacaCarro1, @PlacaCarro2, @PlacaCarro3, @Moto,
                @PlacaMoto1, @PlacaMoto2, @PlacaMoto3, @Cei,
                @NumProprio, @Fantasia, @Cnae, @IdVendedor, @IbgeUf,
                @IbgeMunicipio, @Contribuinte, @Mensal, @Site, @Bloqueado, @IdRota,
                @ValorAluguel, @VencAluguel, @Comissao
            )";

        var parameters = new
        {
            cliente.Id,
            cliente.Nome,
            cliente.CpfCnpj,
            cliente.RgIe,
            cliente.Telefone,
            cliente.Celular,
            cliente.Obs,
            cliente.DataNascimento,
            cliente.Endereco1,
            cliente.Numero1,
            cliente.Complemento1,
            cliente.Bairro1,
            cliente.Cidade1,
            cliente.Uf1,
            cliente.Email,
            cliente.Endereco2,
            cliente.Numero2,
            cliente.Complemento2,
            cliente.Bairro2,
            cliente.Cidade2,
            cliente.Uf2,
            cliente.TabelaPreco,
            Ativo = cliente.Ativo ?? 1,
            cliente.Email1,
            cliente.Cep1,
            cliente.Cep2,
            cliente.Classificacao,
            cliente.Acesso,
            cliente.Limite,
            cliente.RgOrgao,
            cliente.RgEstado,
            cliente.RgEmissao,
            cliente.TelFax,
            DataCadastro = cliente.DataCadastro ?? DateTime.Now,
            cliente.TipoProdutor,
            cliente.Ip,
            cliente.Incra,
            cliente.Nirf,
            cliente.DeclaItr,
            cliente.Carro,
            cliente.PlacaCarro1,
            cliente.PlacaCarro2,
            cliente.PlacaCarro3,
            cliente.Moto,
            cliente.PlacaMoto1,
            cliente.PlacaMoto2,
            cliente.PlacaMoto3,
            cliente.Cei,
            cliente.NumProprio,
            cliente.Fantasia,
            cliente.Cnae,
            IdVendedor = cliente.IdVendedor,
            IbgeUf = cliente.IbgeUf,
            IbgeMunicipio = cliente.IbgeMunicipio,
            cliente.Contribuinte,
            cliente.Mensal,
            cliente.Site,
            Bloqueado = cliente.Bloqueado,
            IdRota = cliente.IdRota,
            cliente.ValorAluguel,
            cliente.VencAluguel,
            cliente.Comissao
        };

        await connection.ExecuteAsync(sql, parameters);

        return novoId;
    }

    public async Task<bool> AtualizarClienteAsync(Cliente cliente)
    {
        using var connection = _connectionFactory.CreateConnection();

        _logger.LogInformation("🔄 [ClienteRepository] Atualizando cliente ID: {Id}, Nome: {Nome}", cliente.Id, cliente.Nome);

        var sql = @"
            UPDATE CLIENTES SET
                NOME = @Nome,
                CPFCNPJ = @CpfCnpj,
                RGIE = @RgIe,
                TELEFONE = @Telefone,
                CELULAR = @Celular,
                OBS = @Obs,
                DATANAS = @DataNascimento,
                END1 = @Endereco1,
                NUM1 = @Numero1,
                COMPL1 = @Complemento1,
                BAIRRO1 = @Bairro1,
                CIDADE1 = @Cidade1,
                UF1 = @Uf1,
                EMAIL = @Email,
                END2 = @Endereco2,
                NUM2 = @Numero2,
                COMPL2 = @Complemento2,
                BAIRRO2 = @Bairro2,
                CIDADE2 = @Cidade2,
                UF2 = @Uf2,
                TABPRE = @TabelaPreco,
                ATIVO = @Ativo,
                EMAIL1 = @Email1,
                CEP1 = @Cep1,
                CEP2 = @Cep2,
                CLASSIF = @Classificacao,
                ACESSO = @Acesso,
                LIMITE = @Limite,
                RG_ORGAO = @RgOrgao,
                RG_ESTADO = @RgEstado,
                RG_EMISSAO = @RgEmissao,
                TELFAX = @TelFax,
                TIPO_PRODUTOR = @TipoProdutor,
                IP = @Ip,
                INCRA = @Incra,
                NIRF = @Nirf,
                DECLA_ITR = @DeclaItr,
                CARRO = @Carro,
                PLACA_CARRO1 = @PlacaCarro1,
                PLACA_CARRO2 = @PlacaCarro2,
                PLACA_CARRO3 = @PlacaCarro3,
                MOTO = @Moto,
                PLACA_MOTO1 = @PlacaMoto1,
                PLACA_MOTO2 = @PlacaMoto2,
                PLACA_MOTO3 = @PlacaMoto3,
                CEI = @Cei,
                NUMPROPRIO = @NumProprio,
                FANTASIA = @Fantasia,
                CNAE = @Cnae,
                IDVENDEDOR = @IdVendedor,
                IBGE_UF = @IbgeUf,
                IBGE_MUNICIPIO = @IbgeMunicipio,
                CONTRIBUINTE = @Contribuinte,
                MENSAL = @Mensal,
                SITE = @Site,
                BLOQUEADO = @Bloqueado,
                IDROTA = @IdRota,
                VALORALUGUEL = @ValorAluguel,
                VENCALUGUEL = @VencAluguel,
                COMISSAO = @Comissao
            WHERE ID = @Id";

        var parameters = new
        {
            cliente.Id,
            cliente.Nome,
            cliente.CpfCnpj,
            cliente.RgIe,
            cliente.Telefone,
            cliente.Celular,
            cliente.Obs,
            cliente.DataNascimento,
            cliente.Endereco1,
            cliente.Numero1,
            cliente.Complemento1,
            cliente.Bairro1,
            cliente.Cidade1,
            cliente.Uf1,
            cliente.Email,
            cliente.Endereco2,
            cliente.Numero2,
            cliente.Complemento2,
            cliente.Bairro2,
            cliente.Cidade2,
            cliente.Uf2,
            cliente.TabelaPreco,
            Ativo = cliente.Ativo ?? 1,
            cliente.Email1,
            cliente.Cep1,
            cliente.Cep2,
            cliente.Classificacao,
            cliente.Acesso,
            cliente.Limite,
            cliente.RgOrgao,
            cliente.RgEstado,
            cliente.RgEmissao,
            cliente.TelFax,
            cliente.TipoProdutor,
            cliente.Ip,
            cliente.Incra,
            cliente.Nirf,
            cliente.DeclaItr,
            cliente.Carro,
            cliente.PlacaCarro1,
            cliente.PlacaCarro2,
            cliente.PlacaCarro3,
            cliente.Moto,
            cliente.PlacaMoto1,
            cliente.PlacaMoto2,
            cliente.PlacaMoto3,
            cliente.Cei,
            cliente.NumProprio,
            cliente.Fantasia,
            cliente.Cnae,
            IdVendedor = cliente.IdVendedor,
            IbgeUf = cliente.IbgeUf,
            IbgeMunicipio = cliente.IbgeMunicipio,
            cliente.Contribuinte,
            cliente.Mensal,
            cliente.Site,
            Bloqueado = cliente.Bloqueado,
            IdRota = cliente.IdRota,
            cliente.ValorAluguel,
            cliente.VencAluguel,
            cliente.Comissao
        };

        try
        {
            var rowsAffected = await connection.ExecuteAsync(sql, parameters);
            _logger.LogInformation("✅ [ClienteRepository] Cliente atualizado - ID: {Id}, Linhas afetadas: {Linhas}", cliente.Id, rowsAffected);
            
            if (rowsAffected == 0)
            {
                _logger.LogWarning("⚠️ [ClienteRepository] Nenhuma linha foi atualizada para o cliente ID: {Id}", cliente.Id);
                return false;
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [ClienteRepository] Erro ao atualizar cliente ID: {Id} - SQL: {Sql}", cliente.Id, sql);
            throw;
        }
    }

    public async Task<bool> ExcluirClienteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        // Verificar se há vendas vinculadas
        var countVendas = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM VENDAS WHERE CLIENTE = @Id", new { Id = id });
        if (countVendas > 0)
            throw new InvalidOperationException(
                $"Não é possível excluir o cliente pois ele possui {countVendas} venda(s) vinculada(s). Inative o cliente em vez de excluí-lo.");

        // Verificar se há contas a receber vinculadas
        var countReceber = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM RECEBER WHERE CODIGO = @Id", new { Id = id });
        if (countReceber > 0)
            throw new InvalidOperationException(
                $"Não é possível excluir o cliente pois ele possui {countReceber} conta(s) a receber vinculada(s). Inative o cliente em vez de excluí-lo.");

        using var transaction = connection.BeginTransaction();
        try
        {
            var rowsAffected = await connection.ExecuteAsync(
                "DELETE FROM CLIENTES WHERE ID = @Id", new { Id = id }, transaction: transaction);
            transaction.Commit();
            _logger.LogInformation("✅ [ClienteRepository] Cliente excluído - ID: {Id}", id);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "❌ [ClienteRepository] Erro ao excluir cliente ID: {Id}", id);
            throw;
        }
    }
}
