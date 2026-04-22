using Dapper;
using IComanda.API.Data;
using IComanda.API.Models.Entities;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;
using IComanda.API.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Repositories.Implementations;

public class ProdutoRepository : IProdutoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<ProdutoRepository> _logger;

    public ProdutoRepository(IDbConnectionFactory connectionFactory, ILogger<ProdutoRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<Produto>> BuscarProdutosAsync(string? termo, bool? ativo = null, int? grupo = null, int pagina = 1, int itensPorPagina = 50)
    {
        using var connection = _connectionFactory.CreateConnection();

        var offset = (pagina - 1) * itensPorPagina;

        // Query simplificada - começando da tabela principal PRODUTOESERVICO
        // Usar maiúsculas para compatibilidade com Firebird e aliases para mapeamento correto
        var sql = @"
            SELECT FIRST @ItensPorPagina SKIP @Offset
                p3.ID as Id, 
                p1.CODIGOBARRA as CodigoBarra, 
                p3.CODIGOINTERNO as CodigoInterno, 
                p3.DESCRICAO as Descricao, 
                p3.CARACTERISTICA as Caracteristica,
                p2.QUANTIDADE as Quantidade, 
                p2.QUANTIDADEMINIMA as QuantidadeMinima, 
                p2.LOCALIZACAO as Localizacao,
                p4.PRECOCUSTO as PrecoCusto, 
                p4.PRECOVENDA as PrecoVenda, 
                p4.ATACADO as Atacado, 
                p4.PRECO3 as Preco3,
                p3.UN_MEDIDA as UnMedida, 
                p3.ATIVO as Ativo, 
                p3.GRUPO as Grupo, 
                p1.PESAVEL as Pesavel, 
                p3.MARCA as Marca,
                p3.CATEGORIA as Categoria, 
                p3.COR as Cor, 
                p3.TAMANHO as Tamanho, 
                p3.DATAINCLUSAO as DataInclusao,
                p2.DATAULTIMAVENDA as DataUltimaVenda, 
                p2.DATAULTIMACOMPRA as DataUltimaCompra
            FROM PRODUTOESERVICO p3
            LEFT JOIN PRODUTO p1 ON p1.ID = p3.ID
            LEFT JOIN PRODUTOEMPRESA p2 ON p2.ID = p3.ID
            LEFT JOIN PRODUTOESERVICOEMPRESA p4 ON p4.ID = p3.ID
            WHERE 1=1";

        var parameters = new DynamicParameters();
        parameters.Add("@ItensPorPagina", itensPorPagina);
        parameters.Add("@Offset", offset);

        // Filtro de termo de busca
        if (!string.IsNullOrWhiteSpace(termo))
        {
            var termoTrimmed = termo.Trim();
            // Converter para maiúsculo em C# para garantir case-insensitive no Firebird
            var termoUpper = termoTrimmed.ToUpperInvariant();
            // Tratar busca com NULLs corretamente
            // Para código de barras, tentar busca exata primeiro, depois parcial
            // Para outros campos, usar CONTAINING para busca parcial (CONTAINING já é case-insensitive no Firebird)
            sql += @" AND (
                (p1.CODIGOBARRA IS NOT NULL AND (UPPER(p1.CODIGOBARRA) = @Termo OR UPPER(p1.CODIGOBARRA) CONTAINING @Termo)) OR
                (p3.DESCRICAO IS NOT NULL AND UPPER(p3.DESCRICAO) CONTAINING @Termo) OR
                (p3.CARACTERISTICA IS NOT NULL AND UPPER(p3.CARACTERISTICA) CONTAINING @Termo) OR
                (p3.CODIGOINTERNO IS NOT NULL AND (UPPER(p3.CODIGOINTERNO) = @Termo OR UPPER(p3.CODIGOINTERNO) CONTAINING @Termo)) OR
                p3.ID = @TermoNum
            )";
            parameters.Add("@Termo", termoUpper);
            if (int.TryParse(termoTrimmed, out var termoNum))
                parameters.Add("@TermoNum", termoNum);
            else
                parameters.Add("@TermoNum", 0);
            
            _logger.LogInformation("🔍 [ProdutoRepository] Buscando com termo: '{Termo}'", termoTrimmed);
        }

        // Filtro de ativo
        if (ativo.HasValue)
        {
            // Firebird: ativo é SMALLINT (1/0) ou NULL
            if (ativo.Value)
            {
                // Produto ativo: ATIVO = 1 ou NULL (tratar NULL como ativo)
                sql += " AND (p3.ATIVO = 1 OR p3.ATIVO IS NULL)";
                _logger.LogInformation("🔍 [ProdutoRepository] Filtrando apenas produtos ativos (ATIVO = 1 ou NULL)");
            }
            else
            {
                // Produto inativo: ATIVO = 0
                sql += " AND p3.ATIVO = 0";
                _logger.LogInformation("🔍 [ProdutoRepository] Filtrando apenas produtos inativos (ATIVO = 0)");
            }
        }
        else
        {
            // Sem filtro de ativo - retornar todos (ativos e inativos)
            // Isso é útil quando há termo de busca e queremos encontrar tudo
            _logger.LogInformation("🔍 [ProdutoRepository] Sem filtro de ativo - retornando todos os produtos (ativos e inativos)");
        }

        // Filtro de grupo
        if (grupo.HasValue)
        {
            sql += " AND p3.GRUPO = @Grupo";
            parameters.Add("@Grupo", grupo.Value);
            _logger.LogInformation("🔍 [ProdutoRepository] Buscando produtos do grupo: {GrupoId}", grupo.Value);
        }
        else
        {
            // Quando não especifica grupo, buscar em todos os grupos (incluindo NULL e 0)
            // Não adicionar filtro de grupo - buscar em todos
            _logger.LogInformation("🔍 [ProdutoRepository] Buscando produtos de TODOS os grupos (sem filtro de grupo)");
        }

        sql += " ORDER BY p3.DESCRICAO";

        _logger.LogInformation("📝 [ProdutoRepository] SQL FINAL: {Sql}", sql);
        _logger.LogInformation("📝 [ProdutoRepository] Parâmetros: Termo='{Termo}', Grupo={Grupo}, Ativo={Ativo}, Pagina={Pagina}, ItensPorPagina={ItensPorPagina}", 
            termo ?? "(vazio)", grupo?.ToString() ?? "null", ativo?.ToString() ?? "null", pagina, itensPorPagina);
        
        // Log detalhado dos parâmetros
        _logger.LogInformation("📝 [ProdutoRepository] Parâmetros detalhados:");
        foreach (var param in parameters.ParameterNames)
        {
            _logger.LogInformation("   - {Param}: {Value}", param, parameters.Get<object>(param));
        }

        try
        {
            // Usar mapeamento direto do Dapper - ele já trata NULLs automaticamente
            var produtos = await connection.QueryAsync<Produto>(sql, parameters);
            var produtosList = produtos.ToList();
            
            
            _logger.LogInformation("✅ [ProdutoRepository] Encontrados {Count} produtos", produtosList.Count);
            if (produtosList.Count > 0 && grupo.HasValue)
            {
                _logger.LogInformation("📦 [ProdutoRepository] Primeiros produtos do grupo {GrupoId}: {Produtos}", 
                    grupo.Value, string.Join(", ", produtosList.Take(3).Select(p => $"{p.Id}:{p.Descricao}")));
            }
            else if (produtosList.Count == 0 && grupo.HasValue)
            {
                _logger.LogWarning("⚠️ [ProdutoRepository] Nenhum produto encontrado para grupo {GrupoId}.", grupo.Value);
            }
            
            return produtosList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [ProdutoRepository] Erro ao executar query - SQL: {Sql}, Mensagem: {Message}, StackTrace: {StackTrace}", 
                sql, ex.Message, ex.StackTrace);
            if (ex.InnerException != null)
            {
                _logger.LogError("❌ [ProdutoRepository] InnerException: {Message}", ex.InnerException.Message);
            }
            throw;
        }
    }

    public async Task<Produto?> GetProdutoAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT p1.id, p1.codigobarra, p3.codigointerno, p3.descricao, p3.caracteristica,
                   p2.quantidade, p2.quantidademinima, p2.localizacao,
                   p4.precocusto, p4.precovenda, p4.atacado, p4.preco3,
                   p3.un_medida, p3.ativo, p3.grupo, p1.pesavel, p3.marca,
                   p3.categoria, p3.cor, p3.tamanho, p3.datainclusao,
                   p2.dataultimavenda, p2.dataultimacompra
            FROM PRODUTO p1
            INNER JOIN PRODUTOEMPRESA p2 ON p1.id = p2.id
            INNER JOIN PRODUTOESERVICO p3 ON p1.id = p3.id
            INNER JOIN PRODUTOESERVICOEMPRESA p4 ON p1.id = p4.id
            WHERE p1.id = @Id";

        return await connection.QueryFirstOrDefaultAsync<Produto>(sql, new { Id = id });
    }

    public async Task<Dictionary<int, string>> GetDescricoesPorCodigosAsync(IEnumerable<int> codigos)
    {
        var lista = codigos.Distinct().ToList();
        if (lista.Count == 0)
            return new Dictionary<int, string>();

        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT p1.id, p3.descricao
            FROM PRODUTO p1
            INNER JOIN PRODUTOESERVICO p3 ON p1.id = p3.id
            WHERE p1.id IN @Codigos";

        var resultados = await connection.QueryAsync<(int Id, string Descricao)>(sql, new { Codigos = lista });
        return resultados.ToDictionary(r => r.Id, r => r.Descricao ?? string.Empty);
    }

    public async Task<Produto?> GetProdutoPorCodigoBarraAsync(string codigoBarra)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT p1.id, p1.codigobarra, p3.codigointerno, p3.descricao, p3.caracteristica,
                   p2.quantidade, p2.quantidademinima, p2.localizacao,
                   p4.precocusto, p4.precovenda, p4.atacado, p4.preco3,
                   p3.un_medida, p3.ativo, p3.grupo, p1.pesavel, p3.marca,
                   p3.categoria, p3.cor, p3.tamanho, p3.datainclusao,
                   p2.dataultimavenda, p2.dataultimacompra
            FROM PRODUTO p1
            INNER JOIN PRODUTOEMPRESA p2 ON p1.id = p2.id
            INNER JOIN PRODUTOESERVICO p3 ON p1.id = p3.id
            INNER JOIN PRODUTOESERVICOEMPRESA p4 ON p1.id = p4.id
            WHERE p1.codigobarra = @CodigoBarra AND p3.ativo = 1";

        return await connection.QueryFirstOrDefaultAsync<Produto>(sql, new { CodigoBarra = codigoBarra });
    }

    public async Task<Produto?> GetProdutoPorCodigoInternoAsync(string codigoInterno)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT p1.id, p1.codigobarra, p3.codigointerno, p3.descricao, p3.caracteristica,
                   p2.quantidade, p2.quantidademinima, p2.localizacao,
                   p4.precocusto, p4.precovenda, p4.atacado, p4.preco3,
                   p3.un_medida, p3.ativo, p3.grupo, p1.pesavel, p3.marca,
                   p3.categoria, p3.cor, p3.tamanho, p3.datainclusao,
                   p2.dataultimavenda, p2.dataultimacompra
            FROM PRODUTO p1
            INNER JOIN PRODUTOEMPRESA p2 ON p1.id = p2.id
            INNER JOIN PRODUTOESERVICO p3 ON p1.id = p3.id
            INNER JOIN PRODUTOESERVICOEMPRESA p4 ON p1.id = p4.id
            WHERE p3.codigointerno = @CodigoInterno AND p3.ativo = 1";

        return await connection.QueryFirstOrDefaultAsync<Produto>(sql, new { CodigoInterno = codigoInterno });
    }

    public async Task<int> ContarProdutosAsync(string? termo, bool? ativo = true, int? grupo = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT COUNT(*)
            FROM PRODUTOESERVICO p3
            LEFT JOIN PRODUTO p1 ON p1.id = p3.id
            LEFT JOIN PRODUTOEMPRESA p2 ON p2.id = p3.id
            LEFT JOIN PRODUTOESERVICOEMPRESA p4 ON p4.id = p3.id
            WHERE 1=1";

        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(termo))
        {
            var termoTrimmed = termo.Trim();
            // Tratar busca com NULLs corretamente - usar CONTAINING para busca parcial
            sql += @" AND (
                (p1.codigobarra IS NOT NULL AND UPPER(p1.codigobarra) CONTAINING UPPER(@Termo)) OR
                (p3.descricao IS NOT NULL AND UPPER(p3.descricao) CONTAINING UPPER(@Termo)) OR
                (p3.caracteristica IS NOT NULL AND UPPER(p3.caracteristica) CONTAINING UPPER(@Termo)) OR
                (p3.codigointerno IS NOT NULL AND UPPER(p3.codigointerno) CONTAINING UPPER(@Termo)) OR
                p3.id = @TermoNum
            )";
            parameters.Add("@Termo", termoTrimmed);
            if (int.TryParse(termoTrimmed, out var termoNum))
                parameters.Add("@TermoNum", termoNum);
            else
                parameters.Add("@TermoNum", 0);
        }

        if (ativo.HasValue)
        {
            // Firebird: ativo é SMALLINT (1/0) ou NULL
            if (ativo.Value)
            {
                // Produto ativo: ATIVO = 1 ou NULL (tratar NULL como ativo)
                sql += " AND (p3.ativo = 1 OR p3.ativo IS NULL)";
            }
            else
            {
                // Produto inativo: ATIVO = 0
                sql += " AND p3.ativo = 0";
            }
        }

        if (grupo.HasValue)
        {
            sql += " AND p3.grupo = @Grupo";
            parameters.Add("@Grupo", grupo.Value);
        }

        return await connection.QuerySingleAsync<int>(sql, parameters);
    }

    public async Task<ProdutoCompletoDto?> GetProdutoCompletoAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Query simplificada usando apenas colunas que sabemos que existem (igual à BuscarProdutosCompletosAsync)
        var sql = @"
            SELECT 
                p3.ID as Id,
                p1.CODIGOBARRA as CodigoBarra,
                CAST(NULL AS VARCHAR(50)) as PadraoBarra,
                CAST(NULL AS BLOB) as Imagem,
                CAST(NULL AS VARCHAR(50)) as Classificacao,
                CAST(NULL AS INTEGER) as DiasValidade,
                CAST(NULL AS DECIMAL(18,2)) as PesoLiquido,
                CAST(NULL AS DECIMAL(18,2)) as PesoBruto,
                CAST(NULL AS VARCHAR(50)) as TipoValidade,
                p1.PESAVEL as Pesavel,
                CAST(NULL AS INTEGER) as ComposicaoId,
                CAST(NULL AS INTEGER) as MarcaId,
                CAST(NULL AS INTEGER) as MedicamentoId,
                CAST(NULL AS INTEGER) as CombustivelId,
                CAST(NULL AS VARCHAR(50)) as Impressao,
                CAST(NULL AS VARCHAR(50)) as CodigoBarras1,
                p2.QUANTIDADE as Quantidade,
                p2.QUANTIDADEMINIMA as QuantidadeMinima,
                CAST(NULL AS DECIMAL(18,2)) as QuantidadeMaxima,
                p2.LOCALIZACAO as Localizacao,
                p2.DATAULTIMAVENDA as DataUltimaVenda,
                p2.DATAULTIMACOMPRA as DataUltimaCompra,
                CAST(NULL AS VARCHAR(50)) as ChavePaf,
                CAST(NULL AS INTEGER) as ProdutoTributacaoEstadualId,
                CAST(NULL AS VARCHAR(100)) as Fabricante,
                CAST(NULL AS TIMESTAMP) as UltimoReajuste,
                CAST(NULL AS TIMESTAMP) as DataAlteracao,
                CAST(NULL AS CHAR(1)) as StatusSazional,
                p4.PRECOVENDA as PrecoVenda,
                CAST(NULL AS SMALLINT) as Vendavel,
                p4.PRECOCUSTO as PrecoCusto,
                CAST(NULL AS DECIMAL(18,2)) as PrecoCustoMedio,
                CAST(NULL AS DECIMAL(18,2)) as LimiteDesconto,
                CAST(NULL AS VARCHAR(50)) as NumeroFci,
                CAST(NULL AS VARCHAR(50)) as ChavePafEmpresa,
                CAST(NULL AS INTEGER) as ProdutoEmpresaId,
                CAST(NULL AS INTEGER) as ServicoEmpresaId,
                CAST(NULL AS INTEGER) as PessoaId,
                CAST(NULL AS INTEGER) as IndiceCalcularPrecoVendaId,
                CAST(NULL AS INTEGER) as IndiceCalcularPrecoCustoId,
                CAST(NULL AS INTEGER) as ProdutoServicoId,
                CAST(NULL AS DECIMAL(18,2)) as PrecoDolar,
                CAST(NULL AS DECIMAL(18,2)) as Percentual,
                p4.ATACADO as Atacado,
                p4.PRECO3 as Preco3,
                CAST(NULL AS TIMESTAMP) as DataAlteracaoPreco,
                CAST(NULL AS SMALLINT) as SituacaoRpl,
                CAST(NULL AS DECIMAL(18,2)) as MargemSeguranca,
                CAST(NULL AS DECIMAL(18,2)) as CustoSeguranca,
                CAST(NULL AS DECIMAL(18,2)) as VarejoPlus,
                CAST(NULL AS DECIMAL(18,2)) as AtacadoPlus,
                p3.DESCRICAO as Descricao,
                p3.CARACTERISTICA as Caracteristica,
                p3.CODIGOINTERNO as CodigoInterno,
                p3.DATAINCLUSAO as DataInclusao,
                p3.ATIVO as Ativo,
                CAST(NULL AS VARCHAR(500)) as Observacao,
                CAST(NULL AS VARCHAR(50)) as Iat,
                CAST(NULL AS VARCHAR(50)) as Ippt,
                CAST(NULL AS VARCHAR(50)) as CodigoContabil,
                CAST(NULL AS DECIMAL(18,2)) as PercentualComissao,
                CAST(NULL AS VARCHAR(50)) as ChavePafP2,
                CAST(NULL AS VARCHAR(50)) as ChavePafE2,
                CAST(NULL AS INTEGER) as SubgrupoId,
                CAST(NULL AS INTEGER) as GeneroItemId,
                CAST(NULL AS INTEGER) as TipoItemId,
                CAST(NULL AS INTEGER) as UnidadeMedidaId,
                CAST(NULL AS INTEGER) as ProdutoId,
                CAST(NULL AS INTEGER) as ServicoId,
                CAST(NULL AS INTEGER) as TributacaoFederalId,
                CAST(NULL AS INTEGER) as NcmId,
                CAST(NULL AS INTEGER) as NaturezaReceitaId,
                p3.UN_MEDIDA as UnMedida,
                p3.GRUPO as Grupo,
                CAST(NULL AS CHAR(1)) as Grade,
                CAST(NULL AS DECIMAL(18,2)) as CustoMedio,
                CAST(NULL AS DECIMAL(18,2)) as UltimaCompra,
                CAST(NULL AS TIMESTAMP) as DtCadastro,
                CAST(NULL AS VARCHAR(10)) as Cfop,
                CAST(NULL AS VARCHAR(10)) as Csosn,
                CAST(NULL AS VARCHAR(10)) as CEST,
                CAST(NULL AS DECIMAL(18,2)) as Despesas,
                CAST(NULL AS DECIMAL(18,2)) as TotCusto,
                CAST(NULL AS DECIMAL(18,2)) as Cf,
                CAST(NULL AS DECIMAL(18,2)) as Frete,
                CAST(NULL AS DECIMAL(18,2)) as IcmsSt,
                CAST(NULL AS DECIMAL(18,2)) as Ipi,
                CAST(NULL AS VARCHAR(50)) as RefCor,
                p3.COR as Cor,
                p3.TAMANHO as Tamanho,
                CAST(NULL AS VARCHAR(200)) as Composicao,
                CAST(NULL AS VARCHAR(100)) as Titulo,
                CAST(NULL AS VARCHAR(50)) as Partida,
                CAST(NULL AS VARCHAR(50)) as Tipo,
                p3.CATEGORIA as Categoria,
                CAST(NULL AS VARCHAR(50)) as NTam,
                p3.MARCA as Marca,
                CAST(NULL AS DECIMAL(18,2)) as SacoKg,
                CAST(NULL AS INTEGER) as CodRastreavel,
                CAST(NULL AS VARCHAR(50)) as TipoProduto,
                CAST(NULL AS VARCHAR(10)) as CstOrigem,
                CAST(NULL AS VARCHAR(10)) as Cst,
                CAST(NULL AS VARCHAR(10)) as Ncm,
                CAST(NULL AS DECIMAL(18,2)) as Icms,
                CAST(NULL AS DECIMAL(18,2)) as Margem,
                CAST(NULL AS INTEGER) as CodEstacao,
                CAST(NULL AS VARCHAR(10)) as CstPis,
                CAST(NULL AS DECIMAL(18,2)) as AliqPis,
                CAST(NULL AS VARCHAR(10)) as CstCofins,
                CAST(NULL AS DECIMAL(18,2)) as AliqCofins,
                CAST(NULL AS VARCHAR(10)) as CstIpi,
                CAST(NULL AS DECIMAL(18,2)) as AliqIpi,
                CAST(NULL AS VARCHAR(50)) as EnquadraIpi,
                CAST(NULL AS DECIMAL(18,2)) as Fcp,
                CAST(NULL AS DECIMAL(18,2)) as Mva,
                CAST(NULL AS TIMESTAMP) as DataAlteracaoProduto,
                CAST(NULL AS VARCHAR(50)) as NaturezaReceita,
                CAST(NULL AS VARCHAR(10)) as CfopInter,
                CAST(NULL AS DECIMAL(18,2)) as RedBaseCalculo,
                CAST(NULL AS DECIMAL(18,2)) as FracaoMl,
                CAST(NULL AS INTEGER) as Monofasico,
                CAST(NULL AS SMALLINT) as SituacaoRplProduto,
                CAST(NULL AS DECIMAL(18,2)) as CaixaM2,
                CAST(NULL AS VARCHAR(40)) as GrupoFiscal,
                CAST(NULL AS DECIMAL(18,2)) as PesoBobina,
                CAST(NULL AS DECIMAL(18,2)) as Corte,
                CAST(NULL AS INTEGER) as Defensivo
            FROM PRODUTOESERVICO p3
            LEFT JOIN PRODUTO p1 ON p1.ID = p3.ID
            LEFT JOIN PRODUTOEMPRESA p2 ON p2.ID = p3.ID
            LEFT JOIN PRODUTOESERVICOEMPRESA p4 ON p4.ID = p3.ID
            WHERE p3.ID = @Id";

        try
        {
            var produto = await connection.QueryFirstOrDefaultAsync<ProdutoCompletoDto>(sql, new { Id = id });
            _logger.LogInformation("✅ [ProdutoRepository] Produto completo carregado - ID: {Id}, Descrição: {Descricao}", 
                id, produto?.Descricao ?? "não encontrado");
            return produto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [ProdutoRepository] Erro ao buscar produto completo ID: {Id} - Mensagem: {Message}", 
                id, ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<ProdutoCompletoDto>> BuscarProdutosCompletosAsync(string? termo, bool? ativo = null, int pagina = 1, int itensPorPagina = 50)
    {
        using var connection = _connectionFactory.CreateConnection();

        var offset = (pagina - 1) * itensPorPagina;

        // Query simplificada usando apenas colunas que sabemos que existem (baseada na query que funciona)
        var sql = @"
            SELECT FIRST @ItensPorPagina SKIP @Offset
                p3.ID as Id,
                p1.CODIGOBARRA as CodigoBarra,
                CAST(NULL AS VARCHAR(50)) as PadraoBarra,
                CAST(NULL AS BLOB) as Imagem,
                CAST(NULL AS VARCHAR(50)) as Classificacao,
                CAST(NULL AS INTEGER) as DiasValidade,
                CAST(NULL AS DECIMAL(18,2)) as PesoLiquido,
                CAST(NULL AS DECIMAL(18,2)) as PesoBruto,
                CAST(NULL AS VARCHAR(50)) as TipoValidade,
                p1.PESAVEL as Pesavel,
                CAST(NULL AS INTEGER) as ComposicaoId,
                CAST(NULL AS INTEGER) as MarcaId,
                CAST(NULL AS INTEGER) as MedicamentoId,
                CAST(NULL AS INTEGER) as CombustivelId,
                CAST(NULL AS VARCHAR(50)) as Impressao,
                CAST(NULL AS VARCHAR(50)) as CodigoBarras1,
                p2.QUANTIDADE as Quantidade,
                p2.QUANTIDADEMINIMA as QuantidadeMinima,
                CAST(NULL AS DECIMAL(18,2)) as QuantidadeMaxima,
                p2.LOCALIZACAO as Localizacao,
                p2.DATAULTIMAVENDA as DataUltimaVenda,
                p2.DATAULTIMACOMPRA as DataUltimaCompra,
                CAST(NULL AS VARCHAR(50)) as ChavePaf,
                CAST(NULL AS INTEGER) as ProdutoTributacaoEstadualId,
                CAST(NULL AS VARCHAR(100)) as Fabricante,
                CAST(NULL AS TIMESTAMP) as UltimoReajuste,
                CAST(NULL AS TIMESTAMP) as DataAlteracao,
                CAST(NULL AS CHAR(1)) as StatusSazional,
                p4.PRECOVENDA as PrecoVenda,
                CAST(NULL AS SMALLINT) as Vendavel,
                p4.PRECOCUSTO as PrecoCusto,
                CAST(NULL AS DECIMAL(18,2)) as PrecoCustoMedio,
                CAST(NULL AS DECIMAL(18,2)) as LimiteDesconto,
                CAST(NULL AS VARCHAR(50)) as NumeroFci,
                CAST(NULL AS VARCHAR(50)) as ChavePafEmpresa,
                CAST(NULL AS INTEGER) as ProdutoEmpresaId,
                CAST(NULL AS INTEGER) as ServicoEmpresaId,
                CAST(NULL AS INTEGER) as PessoaId,
                CAST(NULL AS INTEGER) as IndiceCalcularPrecoVendaId,
                CAST(NULL AS INTEGER) as IndiceCalcularPrecoCustoId,
                CAST(NULL AS INTEGER) as ProdutoServicoId,
                CAST(NULL AS DECIMAL(18,2)) as PrecoDolar,
                CAST(NULL AS DECIMAL(18,2)) as Percentual,
                p4.ATACADO as Atacado,
                p4.PRECO3 as Preco3,
                CAST(NULL AS TIMESTAMP) as DataAlteracaoPreco,
                CAST(NULL AS SMALLINT) as SituacaoRpl,
                CAST(NULL AS DECIMAL(18,2)) as MargemSeguranca,
                CAST(NULL AS DECIMAL(18,2)) as CustoSeguranca,
                CAST(NULL AS DECIMAL(18,2)) as VarejoPlus,
                CAST(NULL AS DECIMAL(18,2)) as AtacadoPlus,
                p3.DESCRICAO as Descricao,
                p3.CARACTERISTICA as Caracteristica,
                p3.CODIGOINTERNO as CodigoInterno,
                p3.DATAINCLUSAO as DataInclusao,
                p3.ATIVO as Ativo,
                CAST(NULL AS VARCHAR(500)) as Observacao,
                CAST(NULL AS VARCHAR(50)) as Iat,
                CAST(NULL AS VARCHAR(50)) as Ippt,
                CAST(NULL AS VARCHAR(50)) as CodigoContabil,
                CAST(NULL AS DECIMAL(18,2)) as PercentualComissao,
                CAST(NULL AS VARCHAR(50)) as ChavePafP2,
                CAST(NULL AS VARCHAR(50)) as ChavePafE2,
                CAST(NULL AS INTEGER) as SubgrupoId,
                CAST(NULL AS INTEGER) as GeneroItemId,
                CAST(NULL AS INTEGER) as TipoItemId,
                CAST(NULL AS INTEGER) as UnidadeMedidaId,
                CAST(NULL AS INTEGER) as ProdutoId,
                CAST(NULL AS INTEGER) as ServicoId,
                CAST(NULL AS INTEGER) as TributacaoFederalId,
                CAST(NULL AS INTEGER) as NcmId,
                CAST(NULL AS INTEGER) as NaturezaReceitaId,
                p3.UN_MEDIDA as UnMedida,
                p3.GRUPO as Grupo,
                CAST(NULL AS CHAR(1)) as Grade,
                CAST(NULL AS DECIMAL(18,2)) as CustoMedio,
                CAST(NULL AS DECIMAL(18,2)) as UltimaCompra,
                CAST(NULL AS TIMESTAMP) as DtCadastro,
                CAST(NULL AS VARCHAR(10)) as Cfop,
                CAST(NULL AS VARCHAR(10)) as Csosn,
                CAST(NULL AS VARCHAR(10)) as CEST,
                CAST(NULL AS DECIMAL(18,2)) as Despesas,
                CAST(NULL AS DECIMAL(18,2)) as TotCusto,
                CAST(NULL AS DECIMAL(18,2)) as Cf,
                CAST(NULL AS DECIMAL(18,2)) as Frete,
                CAST(NULL AS DECIMAL(18,2)) as IcmsSt,
                CAST(NULL AS DECIMAL(18,2)) as Ipi,
                CAST(NULL AS VARCHAR(50)) as RefCor,
                p3.COR as Cor,
                p3.TAMANHO as Tamanho,
                CAST(NULL AS VARCHAR(200)) as Composicao,
                CAST(NULL AS VARCHAR(100)) as Titulo,
                CAST(NULL AS VARCHAR(50)) as Partida,
                CAST(NULL AS VARCHAR(50)) as Tipo,
                p3.CATEGORIA as Categoria,
                CAST(NULL AS VARCHAR(50)) as NTam,
                p3.MARCA as Marca,
                CAST(NULL AS DECIMAL(18,2)) as SacoKg,
                CAST(NULL AS INTEGER) as CodRastreavel,
                CAST(NULL AS VARCHAR(50)) as TipoProduto,
                CAST(NULL AS VARCHAR(10)) as CstOrigem,
                CAST(NULL AS VARCHAR(10)) as Cst,
                CAST(NULL AS VARCHAR(10)) as Ncm,
                CAST(NULL AS DECIMAL(18,2)) as Icms,
                CAST(NULL AS DECIMAL(18,2)) as Margem,
                CAST(NULL AS INTEGER) as CodEstacao,
                CAST(NULL AS VARCHAR(10)) as CstPis,
                CAST(NULL AS DECIMAL(18,2)) as AliqPis,
                CAST(NULL AS VARCHAR(10)) as CstCofins,
                CAST(NULL AS DECIMAL(18,2)) as AliqCofins,
                CAST(NULL AS VARCHAR(10)) as CstIpi,
                CAST(NULL AS DECIMAL(18,2)) as AliqIpi,
                CAST(NULL AS VARCHAR(50)) as EnquadraIpi,
                CAST(NULL AS DECIMAL(18,2)) as Fcp,
                CAST(NULL AS DECIMAL(18,2)) as Mva,
                CAST(NULL AS TIMESTAMP) as DataAlteracaoProduto,
                CAST(NULL AS VARCHAR(50)) as NaturezaReceita,
                CAST(NULL AS VARCHAR(10)) as CfopInter,
                CAST(NULL AS DECIMAL(18,2)) as RedBaseCalculo,
                CAST(NULL AS DECIMAL(18,2)) as FracaoMl,
                CAST(NULL AS INTEGER) as Monofasico,
                CAST(NULL AS SMALLINT) as SituacaoRplProduto,
                CAST(NULL AS DECIMAL(18,2)) as CaixaM2,
                CAST(NULL AS VARCHAR(40)) as GrupoFiscal,
                CAST(NULL AS DECIMAL(18,2)) as PesoBobina,
                CAST(NULL AS DECIMAL(18,2)) as Corte,
                CAST(NULL AS INTEGER) as Defensivo
            FROM PRODUTOESERVICO p3
            LEFT JOIN PRODUTO p1 ON p1.ID = p3.ID
            LEFT JOIN PRODUTOEMPRESA p2 ON p2.ID = p3.ID
            LEFT JOIN PRODUTOESERVICOEMPRESA p4 ON p4.ID = p3.ID
            WHERE 1=1";

        var parameters = new DynamicParameters();
        parameters.Add("@ItensPorPagina", itensPorPagina);
        parameters.Add("@Offset", offset);

        if (!string.IsNullOrWhiteSpace(termo))
        {
            var termoTrimmed = termo.Trim();
            // Converter para maiúsculo em C# para garantir case-insensitive no Firebird
            var termoUpper = termoTrimmed.ToUpperInvariant();
            sql += @" AND (
                (p1.CODIGOBARRA IS NOT NULL AND (UPPER(p1.CODIGOBARRA) = @Termo OR UPPER(p1.CODIGOBARRA) CONTAINING @Termo)) OR
                (p3.DESCRICAO IS NOT NULL AND UPPER(p3.DESCRICAO) CONTAINING @Termo) OR
                (p3.CARACTERISTICA IS NOT NULL AND UPPER(p3.CARACTERISTICA) CONTAINING @Termo) OR
                (p3.CODIGOINTERNO IS NOT NULL AND (UPPER(p3.CODIGOINTERNO) = @Termo OR UPPER(p3.CODIGOINTERNO) CONTAINING @Termo)) OR
                p3.ID = @TermoNum
            )";
            parameters.Add("@Termo", termoUpper);
            if (int.TryParse(termoTrimmed, out var termoNum))
                parameters.Add("@TermoNum", termoNum);
            else
                parameters.Add("@TermoNum", 0);
        }

        if (ativo.HasValue)
        {
            // Firebird: ativo é SMALLINT (1/0) ou NULL
            if (ativo.Value)
            {
                // Produto ativo: ATIVO = 1 ou NULL (tratar NULL como ativo)
                sql += " AND (p3.ATIVO = 1 OR p3.ATIVO IS NULL)";
            }
            else
            {
                // Produto inativo: ATIVO = 0
                sql += " AND p3.ATIVO = 0";
            }
        }

        sql += " ORDER BY p3.DESCRICAO";

        _logger.LogInformation("📝 [ProdutoRepository] SQL BuscarProdutosCompletos: {Sql}", sql);
        _logger.LogInformation("📝 [ProdutoRepository] Parâmetros: Termo={Termo}, Ativo={Ativo}, Pagina={Pagina}, ItensPorPagina={ItensPorPagina}", 
            termo ?? "(vazio)", ativo, pagina, itensPorPagina);

        try
        {
            var produtos = await connection.QueryAsync<ProdutoCompletoDto>(sql, parameters);
            var produtosList = produtos.ToList();
            
            _logger.LogInformation("✅ [ProdutoRepository] Encontrados {Count} produtos completos", produtosList.Count);
            
            return produtosList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [ProdutoRepository] Erro ao executar query BuscarProdutosCompletos - Mensagem: {Message}, InnerException: {InnerException}, StackTrace: {StackTrace}", 
                ex.Message, ex.InnerException?.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<int> CriarProdutoCompletoAsync(CriarProdutoRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Garantir que a conexão está aberta
        if (connection.State != System.Data.ConnectionState.Open)
        {
            connection.Open();
        }
        
        using var transaction = connection.BeginTransaction();

        try
        {
            _logger.LogInformation("🔵 [ProdutoRepository] Iniciando criação de produto - Descrição: {Descricao}", request.Descricao);

            // Obter próximo ID (tentar usar generator, senão buscar o máximo)
            int novoId;
            try
            {
                var idSql = "SELECT GEN_ID(GEN_PRODUTO_ID, 1) AS ID FROM RDB$DATABASE";
                var result = await connection.QuerySingleAsync<dynamic>(idSql, transaction: transaction);
                novoId = Convert.ToInt32(result.ID);
                _logger.LogInformation("✅ [ProdutoRepository] ID gerado via generator: {Id}", novoId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("⚠️ [ProdutoRepository] Generator não encontrado, buscando máximo ID. Erro: {Error}", ex.Message);
                // Se não existir generator, buscar o máximo ID + 1
                var sqlMax = "SELECT COALESCE(MAX(ID), 0) + 1 AS ID FROM PRODUTO";
                novoId = await connection.QuerySingleAsync<int>(sqlMax, transaction: transaction);
                _logger.LogInformation("✅ [ProdutoRepository] ID gerado via MAX: {Id}", novoId);
            }

            // Inserir na tabela PRODUTO
            _logger.LogInformation("🔵 [ProdutoRepository] Inserindo na tabela PRODUTO");
            
            var sqlProduto = @"
                INSERT INTO PRODUTO (ID, CODIGOBARRA, PADRAOBARRA, IMAGEM, CLASSIFICACAO, DIASVALIDADE,
                    PESOLIQUIDO, PESOBRUTO, TIPOVALIDADE, PESAVEL, COMPOSICAOID, MARCAID, MEDICAMENTOID,
                    COMBUSTIVELID, IMPRESSAO, CODIGOBARRAS1)
                VALUES (@Id, @CodigoBarra, @PadraoBarra, @Imagem, @Classificacao, @DiasValidade,
                    @PesoLiquido, @PesoBruto, @TipoValidade, @Pesavel, @ComposicaoId, @MarcaId, @MedicamentoId,
                    @CombustivelId, @Impressao, @CodigoBarras1)";

            await connection.ExecuteAsync(sqlProduto, new
            {
                Id = novoId,
                CodigoBarra = request.CodigoBarra ?? (string?)null,
                PadraoBarra = request.PadraoBarra ?? (string?)null,
                Imagem = request.Imagem ?? (byte[]?)null,
                Classificacao = request.Classificacao ?? (string?)null,
                DiasValidade = request.DiasValidade ?? (int?)null,
                PesoLiquido = request.PesoLiquido ?? (decimal?)null,
                PesoBruto = request.PesoBruto ?? (decimal?)null,
                TipoValidade = request.TipoValidade ?? (string?)null,
                Pesavel = request.Pesavel ?? (short?)null,
                ComposicaoId = request.ComposicaoId ?? (int?)null,
                MarcaId = request.MarcaId ?? (int?)null,
                MedicamentoId = request.MedicamentoId ?? (int?)null,
                CombustivelId = request.CombustivelId ?? (int?)null,
                Impressao = request.Impressao ?? (string?)null,
                CodigoBarras1 = request.CodigoBarras1 ?? (string?)null
            }, transaction: transaction);
            
            _logger.LogInformation("✅ [ProdutoRepository] Inserido na tabela PRODUTO");

            // Inserir na tabela PRODUTOEMPRESA
            _logger.LogInformation("🔵 [ProdutoRepository] Inserindo na tabela PRODUTOEMPRESA");
            
            var sqlProdutoEmpresa = @"
                INSERT INTO PRODUTOEMPRESA (ID, QUANTIDADE, QUANTIDADEMINIMA, QUANTIDADEMAXIMA, LOCALIZACAO,
                    FABRICANTE, STATUS_SAZIONAL)
                VALUES (@Id, @Quantidade, @QuantidadeMinima, @QuantidadeMaxima, @Localizacao,
                    @Fabricante, @StatusSazional)";

            await connection.ExecuteAsync(sqlProdutoEmpresa, new
            {
                Id = novoId,
                Quantidade = request.Quantidade ?? 0,
                QuantidadeMinima = request.QuantidadeMinima ?? (decimal?)null,
                QuantidadeMaxima = request.QuantidadeMaxima ?? (decimal?)null,
                Localizacao = request.Localizacao ?? (string?)null,
                Fabricante = request.Fabricante ?? (string?)null,
                StatusSazional = request.StatusSazional ?? (char?)null
            }, transaction: transaction);
            
            _logger.LogInformation("✅ [ProdutoRepository] Inserido na tabela PRODUTOEMPRESA");

            // Inserir na tabela PRODUTOESERVICO
            _logger.LogInformation("🔵 [ProdutoRepository] Inserindo na tabela PRODUTOESERVICO - TipoItemId: {TipoItemId}, UnidadeMedidaId: {UnidadeMedidaId}", 
                request.TipoItemId, request.UnidadeMedidaId);

            var sqlProdutoServico = @"
                INSERT INTO PRODUTOESERVICO (ID, DESCRICAO, CARACTERISTICA, CODIGOINTERNO, DATAINCLUSAO, ATIVO,
                    OBSERVACAO, TIPOITEMID, UNIDADEMEDIDAID, SUBGRUPOID, GENEROITEMID, GRUPO, UN_MEDIDA,
                    CFOP, CSOSN, CEST, COR, TAMANHO, CATEGORIA, MARCA, TIPO, NCM, CST, CST_ORIGEM, ICMS, MARGEM)
                VALUES (@Id, @Descricao, @Caracteristica, @CodigoInterno, @DataInclusao, @Ativo,
                    @Observacao, @TipoItemId, @UnidadeMedidaId, @SubgrupoId, @GeneroItemId, @Grupo, @UnMedida,
                    @Cfop, @Csosn, @CEST, @Cor, @Tamanho, @Categoria, @Marca, @Tipo, @Ncm, @Cst, @CstOrigem, @Icms, @Margem)";

            var paramsProdutoServico = new
            {
                Id = novoId,
                Descricao = request.Descricao ?? string.Empty,
                Caracteristica = request.Caracteristica ?? (string?)null,
                CodigoInterno = request.CodigoInterno ?? (string?)null,
                DataInclusao = DateTime.Now,
                Ativo = request.Ativo ?? 1,
                Observacao = request.Observacao ?? (string?)null,
                TipoItemId = request.TipoItemId,
                UnidadeMedidaId = request.UnidadeMedidaId,
                SubgrupoId = request.SubgrupoId ?? (int?)null,
                GeneroItemId = request.GeneroItemId ?? (int?)null,
                Grupo = request.Grupo ?? (int?)null,
                UnMedida = request.UnMedida ?? (string?)null,
                Cfop = request.Cfop ?? (string?)null,
                Csosn = request.Csosn ?? (string?)null,
                CEST = request.CEST ?? (string?)null,
                Cor = request.Cor ?? (string?)null,
                Tamanho = request.Tamanho ?? (string?)null,
                Categoria = request.Categoria ?? (string?)null,
                Marca = request.Marca ?? (string?)null,
                Tipo = request.Tipo ?? (string?)null,
                Ncm = request.Ncm ?? (string?)null,
                Cst = request.Cst ?? (string?)null,
                CstOrigem = request.CstOrigem ?? (string?)null,
                Icms = request.Icms ?? (decimal?)null,
                Margem = request.Margem ?? (decimal?)null
            };

            await connection.ExecuteAsync(sqlProdutoServico, paramsProdutoServico, transaction: transaction);
            _logger.LogInformation("✅ [ProdutoRepository] Inserido na tabela PRODUTOESERVICO");

            // Inserir na tabela PRODUTOESERVICOEMPRESA
            _logger.LogInformation("🔵 [ProdutoRepository] Inserindo na tabela PRODUTOESERVICOEMPRESA - PessoaId: {PessoaId}", request.PessoaId);
            
            var sqlProdutoServicoEmpresa = @"
                INSERT INTO PRODUTOESERVICOEMPRESA (ID, PRECOVENDA, VENDAVEL, PRECOCUSTO, PRECOCUSTOMEDIO,
                    LIMITEDESCONTO, ATACADO, PRECO3, PRECODOLAR, PERCENTUAL, PESSOAID)
                VALUES (@Id, @PrecoVenda, @Vendavel, @PrecoCusto, @PrecoCustoMedio,
                    @LimiteDesconto, @Atacado, @Preco3, @PrecoDolar, @Percentual, @PessoaId)";

            await connection.ExecuteAsync(sqlProdutoServicoEmpresa, new
            {
                Id = novoId,
                PrecoVenda = request.PrecoVenda ?? (decimal?)null,
                Vendavel = request.Vendavel ?? (short?)null,
                PrecoCusto = request.PrecoCusto ?? (decimal?)null,
                PrecoCustoMedio = request.PrecoCustoMedio ?? (decimal?)null,
                LimiteDesconto = request.LimiteDesconto ?? (decimal?)null,
                Atacado = request.Atacado ?? (decimal?)null,
                Preco3 = request.Preco3 ?? (decimal?)null,
                PrecoDolar = request.PrecoDolar ?? (decimal?)null,
                Percentual = request.Percentual ?? (decimal?)null,
                PessoaId = request.PessoaId
            }, transaction: transaction);
            
            _logger.LogInformation("✅ [ProdutoRepository] Inserido na tabela PRODUTOESERVICOEMPRESA");

            transaction.Commit();
            _logger.LogInformation("✅ [ProdutoRepository] Produto criado com sucesso - ID: {Id}", novoId);
            return novoId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [ProdutoRepository] Erro ao criar produto - Mensagem: {Message}, StackTrace: {StackTrace}", 
                ex.Message, ex.StackTrace);
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> AtualizarProdutoCompletoAsync(int id, AtualizarProdutoRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Garantir que a conexão está aberta
        if (connection.State != System.Data.ConnectionState.Open)
        {
            connection.Open();
        }
        
        using var transaction = connection.BeginTransaction();

        try
        {
            _logger.LogInformation("🔄 [ProdutoRepository] Iniciando atualização de produto - ID: {Id}", id);
            // Atualizar PRODUTO (apenas campos fornecidos)
            var camposProduto = new List<string>();
            var paramProduto = new DynamicParameters();
            paramProduto.Add("@Id", id);

            if (request.CodigoBarra != null) { camposProduto.Add("CODIGOBARRA = @CodigoBarra"); paramProduto.Add("@CodigoBarra", request.CodigoBarra); }
            if (request.PadraoBarra != null) { camposProduto.Add("PADRAOBARRA = @PadraoBarra"); paramProduto.Add("@PadraoBarra", request.PadraoBarra); }
            if (request.Imagem != null) { camposProduto.Add("IMAGEM = @Imagem"); paramProduto.Add("@Imagem", request.Imagem); }
            if (request.Classificacao != null) { camposProduto.Add("CLASSIFICACAO = @Classificacao"); paramProduto.Add("@Classificacao", request.Classificacao); }
            if (request.DiasValidade.HasValue) { camposProduto.Add("DIASVALIDADE = @DiasValidade"); paramProduto.Add("@DiasValidade", request.DiasValidade); }
            if (request.PesoLiquido.HasValue) { camposProduto.Add("PESOLIQUIDO = @PesoLiquido"); paramProduto.Add("@PesoLiquido", request.PesoLiquido); }
            if (request.PesoBruto.HasValue) { camposProduto.Add("PESOBRUTO = @PesoBruto"); paramProduto.Add("@PesoBruto", request.PesoBruto); }
            if (request.TipoValidade != null) { camposProduto.Add("TIPOVALIDADE = @TipoValidade"); paramProduto.Add("@TipoValidade", request.TipoValidade); }
            if (request.Pesavel.HasValue) { camposProduto.Add("PESAVEL = @Pesavel"); paramProduto.Add("@Pesavel", request.Pesavel); }
            if (request.ComposicaoId.HasValue) { camposProduto.Add("COMPOSICAOID = @ComposicaoId"); paramProduto.Add("@ComposicaoId", request.ComposicaoId); }
            if (request.MarcaId.HasValue) { camposProduto.Add("MARCAID = @MarcaId"); paramProduto.Add("@MarcaId", request.MarcaId); }
            if (request.MedicamentoId != null) { camposProduto.Add("MEDICAMENTOID = @MedicamentoId"); paramProduto.Add("@MedicamentoId", request.MedicamentoId); }
            if (request.CombustivelId != null) { camposProduto.Add("COMBUSTIVELID = @CombustivelId"); paramProduto.Add("@CombustivelId", request.CombustivelId); }
            if (request.Impressao != null) { camposProduto.Add("IMPRESSAO = @Impressao"); paramProduto.Add("@Impressao", request.Impressao); }
            if (request.CodigoBarras1 != null) { camposProduto.Add("CODIGOBARRAS1 = @CodigoBarras1"); paramProduto.Add("@CodigoBarras1", request.CodigoBarras1); }

            if (camposProduto.Any())
            {
                var sqlProduto = $"UPDATE PRODUTO SET {string.Join(", ", camposProduto)} WHERE ID = @Id";
                _logger.LogInformation("🔄 [ProdutoRepository] Atualizando PRODUTO - SQL: {Sql}", sqlProduto);
                await connection.ExecuteAsync(sqlProduto, paramProduto, transaction: transaction);
            }
            else
            {
                _logger.LogInformation("ℹ️ [ProdutoRepository] Nenhum campo para atualizar na tabela PRODUTO");
            }

            // Atualizar PRODUTOEMPRESA
            var camposEmpresa = new List<string>();
            var paramEmpresa = new DynamicParameters();
            paramEmpresa.Add("@Id", id);

            if (request.Quantidade.HasValue) { camposEmpresa.Add("QUANTIDADE = @Quantidade"); paramEmpresa.Add("@Quantidade", request.Quantidade); }
            if (request.QuantidadeMinima.HasValue) { camposEmpresa.Add("QUANTIDADEMINIMA = @QuantidadeMinima"); paramEmpresa.Add("@QuantidadeMinima", request.QuantidadeMinima); }
            if (request.QuantidadeMaxima.HasValue) { camposEmpresa.Add("QUANTIDADEMAXIMA = @QuantidadeMaxima"); paramEmpresa.Add("@QuantidadeMaxima", request.QuantidadeMaxima); }
            if (request.Localizacao != null) { camposEmpresa.Add("LOCALIZACAO = @Localizacao"); paramEmpresa.Add("@Localizacao", request.Localizacao); }
            if (request.Fabricante != null) { camposEmpresa.Add("FABRICANTE = @Fabricante"); paramEmpresa.Add("@Fabricante", request.Fabricante); }
            if (request.StatusSazional.HasValue) { camposEmpresa.Add("STATUS_SAZIONAL = @StatusSazional"); paramEmpresa.Add("@StatusSazional", request.StatusSazional); }

            if (camposEmpresa.Any())
            {
                var sqlEmpresa = $"UPDATE PRODUTOEMPRESA SET {string.Join(", ", camposEmpresa)} WHERE ID = @Id";
                _logger.LogInformation("🔄 [ProdutoRepository] Atualizando PRODUTOEMPRESA - SQL: {Sql}", sqlEmpresa);
                await connection.ExecuteAsync(sqlEmpresa, paramEmpresa, transaction: transaction);
            }
            else
            {
                _logger.LogInformation("ℹ️ [ProdutoRepository] Nenhum campo para atualizar na tabela PRODUTOEMPRESA");
            }

            // Atualizar PRODUTOESERVICO
            var camposServico = new List<string>();
            var paramServico = new DynamicParameters();
            paramServico.Add("@Id", id);

            if (request.Descricao != null) { camposServico.Add("DESCRICAO = @Descricao"); paramServico.Add("@Descricao", request.Descricao); }
            if (request.Caracteristica != null) { camposServico.Add("CARACTERISTICA = @Caracteristica"); paramServico.Add("@Caracteristica", request.Caracteristica); }
            if (request.CodigoInterno != null) { camposServico.Add("CODIGOINTERNO = @CodigoInterno"); paramServico.Add("@CodigoInterno", request.CodigoInterno); }
            if (request.Ativo.HasValue) { camposServico.Add("ATIVO = @Ativo"); paramServico.Add("@Ativo", request.Ativo); }
            if (request.Observacao != null) { camposServico.Add("OBSERVACAO = @Observacao"); paramServico.Add("@Observacao", request.Observacao); }
            if (request.TipoItemId.HasValue) { camposServico.Add("TIPOITEMID = @TipoItemId"); paramServico.Add("@TipoItemId", request.TipoItemId); }
            if (request.UnidadeMedidaId.HasValue) { camposServico.Add("UNIDADEMEDIDAID = @UnidadeMedidaId"); paramServico.Add("@UnidadeMedidaId", request.UnidadeMedidaId); }
            if (request.SubgrupoId.HasValue) { camposServico.Add("SUBGRUPOID = @SubgrupoId"); paramServico.Add("@SubgrupoId", request.SubgrupoId); }
            if (request.GeneroItemId.HasValue) { camposServico.Add("GENEROITEMID = @GeneroItemId"); paramServico.Add("@GeneroItemId", request.GeneroItemId); }
            if (request.Grupo.HasValue) { camposServico.Add("GRUPO = @Grupo"); paramServico.Add("@Grupo", request.Grupo); }
            if (request.UnMedida != null) { camposServico.Add("UN_MEDIDA = @UnMedida"); paramServico.Add("@UnMedida", request.UnMedida); }
            if (request.Cfop != null) { camposServico.Add("CFOP = @Cfop"); paramServico.Add("@Cfop", request.Cfop); }
            if (request.Csosn != null) { camposServico.Add("CSOSN = @Csosn"); paramServico.Add("@Csosn", request.Csosn); }
            if (request.CEST != null) { camposServico.Add("CEST = @CEST"); paramServico.Add("@CEST", request.CEST); }
            if (request.Cor != null) { camposServico.Add("COR = @Cor"); paramServico.Add("@Cor", request.Cor); }
            if (request.Tamanho != null) { camposServico.Add("TAMANHO = @Tamanho"); paramServico.Add("@Tamanho", request.Tamanho); }
            if (request.Categoria != null) { camposServico.Add("CATEGORIA = @Categoria"); paramServico.Add("@Categoria", request.Categoria); }
            if (request.Marca != null) { camposServico.Add("MARCA = @Marca"); paramServico.Add("@Marca", request.Marca); }
            if (request.Tipo != null) { camposServico.Add("TIPO = @Tipo"); paramServico.Add("@Tipo", request.Tipo); }
            if (request.Ncm != null) { camposServico.Add("NCM = @Ncm"); paramServico.Add("@Ncm", request.Ncm); }
            if (request.Cst != null) { camposServico.Add("CST = @Cst"); paramServico.Add("@Cst", request.Cst); }
            if (request.CstOrigem != null) { camposServico.Add("CST_ORIGEM = @CstOrigem"); paramServico.Add("@CstOrigem", request.CstOrigem); }
            if (request.Icms.HasValue) { camposServico.Add("ICMS = @Icms"); paramServico.Add("@Icms", request.Icms); }
            if (request.Margem.HasValue) { camposServico.Add("MARGEM = @Margem"); paramServico.Add("@Margem", request.Margem); }

            // Remover DATA_ALTERACAO se a coluna não existir - usar apenas se houver outros campos para atualizar
            // camposServico.Add("DATA_ALTERACAO = @DataAlteracao");
            // paramServico.Add("@DataAlteracao", DateTime.Now);

            if (camposServico.Any())
            {
                var sqlServico = $"UPDATE PRODUTOESERVICO SET {string.Join(", ", camposServico)} WHERE ID = @Id";
                _logger.LogInformation("🔄 [ProdutoRepository] Atualizando PRODUTOESERVICO - SQL: {Sql}", sqlServico);
                await connection.ExecuteAsync(sqlServico, paramServico, transaction: transaction);
            }

            // Atualizar PRODUTOESERVICOEMPRESA
            var camposPreco = new List<string>();
            var paramPreco = new DynamicParameters();
            paramPreco.Add("@Id", id);

            if (request.PrecoVenda.HasValue) { camposPreco.Add("PRECOVENDA = @PrecoVenda"); paramPreco.Add("@PrecoVenda", request.PrecoVenda); }
            if (request.Vendavel.HasValue) { camposPreco.Add("VENDAVEL = @Vendavel"); paramPreco.Add("@Vendavel", request.Vendavel); }
            if (request.PrecoCusto.HasValue) { camposPreco.Add("PRECOCUSTO = @PrecoCusto"); paramPreco.Add("@PrecoCusto", request.PrecoCusto); }
            if (request.PrecoCustoMedio.HasValue) { camposPreco.Add("PRECOCUSTOMEDIO = @PrecoCustoMedio"); paramPreco.Add("@PrecoCustoMedio", request.PrecoCustoMedio); }
            if (request.LimiteDesconto.HasValue) { camposPreco.Add("LIMITEDESCONTO = @LimiteDesconto"); paramPreco.Add("@LimiteDesconto", request.LimiteDesconto); }
            if (request.Atacado.HasValue) { camposPreco.Add("ATACADO = @Atacado"); paramPreco.Add("@Atacado", request.Atacado); }
            if (request.Preco3.HasValue) { camposPreco.Add("PRECO3 = @Preco3"); paramPreco.Add("@Preco3", request.Preco3); }
            if (request.PrecoDolar.HasValue) { camposPreco.Add("PRECODOLAR = @PrecoDolar"); paramPreco.Add("@PrecoDolar", request.PrecoDolar); }
            if (request.Percentual.HasValue) { camposPreco.Add("PERCENTUAL = @Percentual"); paramPreco.Add("@Percentual", request.Percentual); }

            // Remover DATA_ALTERACAO se a coluna não existir - usar apenas se houver outros campos para atualizar
            // camposPreco.Add("DATA_ALTERACAO = @DataAlteracao");
            // paramPreco.Add("@DataAlteracao", DateTime.Now);

            if (camposPreco.Any())
            {
                var sqlPreco = $"UPDATE PRODUTOESERVICOEMPRESA SET {string.Join(", ", camposPreco)} WHERE ID = @Id";
                _logger.LogInformation("🔄 [ProdutoRepository] Atualizando PRODUTOESERVICOEMPRESA - SQL: {Sql}", sqlPreco);
                await connection.ExecuteAsync(sqlPreco, paramPreco, transaction: transaction);
            }
            else
            {
                _logger.LogInformation("ℹ️ [ProdutoRepository] Nenhum campo para atualizar na tabela PRODUTOESERVICOEMPRESA");
            }

            _logger.LogInformation("💾 [ProdutoRepository] Commitando transação...");
            transaction.Commit();
            _logger.LogInformation("✅ [ProdutoRepository] Produto atualizado com sucesso - ID: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [ProdutoRepository] Erro ao atualizar produto - ID: {Id}, Mensagem: {Message}, InnerException: {InnerException}", 
                id, ex.Message, ex.InnerException?.Message);
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> ExcluirProdutoAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Verificar se o produto está vinculado a vendas (ITEVENDAS) - evita erro de FK
        var countVendas = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM ITEVENDAS WHERE CODIGO = @Id", new { Id = id });
        if (countVendas > 0)
        {
            _logger.LogWarning("Não é possível excluir produto {Id}: possui {Count} item(ns) em vendas (ITEVENDAS)", id, countVendas);
            throw new InvalidOperationException($"Não é possível excluir o produto pois ele está vinculado a {countVendas} item(ns) em vendas. Inative o produto em vez de excluí-lo.");
        }

        // Verificar itens temporários (frente_tmpitvendas) - opcional: bloquear ou apenas avisar
        var countTmp = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM frente_tmpitvendas WHERE codigo = @Id", new { Id = id });
        if (countTmp > 0)
        {
            _logger.LogWarning("Não é possível excluir produto {Id}: possui {Count} item(ns) em comandas/vendas em aberto (frente_tmpitvendas)", id, countTmp);
            throw new InvalidOperationException($"Não é possível excluir o produto pois ele está no carrinho de {countTmp} item(ns) em comandas em aberto. Remova o item das comandas antes de excluir.");
        }

        using var transaction = connection.BeginTransaction();
        try
        {
            // Excluir das 4 tabelas (ordem inversa das foreign keys)
            await connection.ExecuteAsync("DELETE FROM PRODUTOESERVICOEMPRESA WHERE ID = @Id", new { Id = id }, transaction: transaction);
            await connection.ExecuteAsync("DELETE FROM PRODUTOESERVICO WHERE ID = @Id", new { Id = id }, transaction: transaction);
            await connection.ExecuteAsync("DELETE FROM PRODUTOEMPRESA WHERE ID = @Id", new { Id = id }, transaction: transaction);
            await connection.ExecuteAsync("DELETE FROM PRODUTO WHERE ID = @Id", new { Id = id }, transaction: transaction);

            transaction.Commit();
            _logger.LogInformation("Produto excluído com sucesso - ID: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Erro ao excluir produto - ID: {Id}", id);
            throw;
        }
    }

}
