using Dapper;
using IComanda.API.Data;
using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Repositories.Implementations;

// Classe auxiliar para mapear o resultado da query com COUNT
internal class GrupoComQuantidadeResult
{
    public int ID { get; set; }
    public string? DESCRICAO { get; set; }
    public long QuantidadeProdutos { get; set; } // COUNT retorna bigint no Firebird
}

public class GrupoRepository : IGrupoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<GrupoRepository> _logger;

    public GrupoRepository(IDbConnectionFactory connectionFactory, ILogger<GrupoRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<Grupo>> GetAllGruposAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        // Primeiro, vamos testar se a tabela existe e tem dados
        try
        {
            var countSql = "SELECT COUNT(*) FROM GRUPO";
            var count = await connection.QuerySingleAsync<int>(countSql);
            _logger.LogInformation("🔢 [GrupoRepository] Total de registros na tabela GRUPO: {Count}", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [GrupoRepository] Erro ao contar registros da tabela GRUPO");
        }

        var sql = @"
            SELECT ID as Id, DESCRICAO as Descricao,
                   COALESCE(IMPRIMIR2VIAS, 0) as ImprimirDuasVias,
                   COALESCE(TIPO, 'NORMAL') as Tipo,
                   COALESCE(PERCENTUAL, 0) as Percentual
            FROM GRUPO
            ORDER BY DESCRICAO";

        _logger.LogInformation("🔍 [GrupoRepository] Executando query GetAllGruposAsync");
        _logger.LogInformation("📝 SQL: {Sql}", sql);

        try
        {
            // Primeiro, vamos tentar buscar como dynamic para ver o que vem do banco
            var resultDynamic = await connection.QueryAsync<dynamic>(sql);
            var dynamicList = resultDynamic.ToList();
            _logger.LogInformation("📊 [GrupoRepository] Query retornou {Count} registros como dynamic", dynamicList.Count);
            
            if (dynamicList.Count > 0)
            {
                var first = dynamicList.First();
                _logger.LogInformation("📋 [GrupoRepository] Primeiro registro dynamic: {Registro}", 
                    $"id={first.id} ({first.id?.GetType()}), descricao={first.descricao} ({first.descricao?.GetType()})");
            }

            // Agora vamos tentar mapear para Grupo
            var result = await connection.QueryAsync<Grupo>(sql);
            var grupos = result.ToList();
            
            _logger.LogInformation("✅ [GrupoRepository] GetAllGruposAsync retornou {Count} grupos mapeados", grupos.Count);
            if (grupos.Count > 0)
            {
                _logger.LogInformation("📋 Grupos encontrados: {Grupos}", 
                    string.Join(", ", grupos.Select(g => $"ID:{g.Id} Desc:{g.Descricao}")));
            }
            else if (dynamicList.Count > 0)
            {
                _logger.LogWarning("⚠️ [GrupoRepository] Query retornou {DynamicCount} registros dynamic mas 0 grupos mapeados - problema no mapeamento!", dynamicList.Count);
            }

            return grupos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [GrupoRepository] Erro ao executar GetAllGruposAsync");
            throw;
        }
    }

    public async Task<Grupo?> GetGrupoAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT ID as Id, DESCRICAO as Descricao,
                   COALESCE(IMPRIMIR2VIAS, 0) as ImprimirDuasVias,
                   COALESCE(TIPO, 'NORMAL') as Tipo,
                   COALESCE(PERCENTUAL, 0) as Percentual
            FROM GRUPO
            WHERE ID = @Id";

        try
        {
            _logger.LogInformation("🔍 [GrupoRepository] Buscando grupo com ID: {Id}", id);
            _logger.LogInformation("📝 SQL: {Sql} com parâmetro Id={Id}", sql, id);

            var grupo = await connection.QueryFirstOrDefaultAsync<Grupo>(sql, new { Id = id });

            if (grupo != null)
            {
                _logger.LogInformation("✅ [GrupoRepository] Grupo encontrado: ID={Id}, Descricao={Descricao}", grupo.Id, grupo.Descricao);
            }
            else
            {
                _logger.LogWarning("⚠️ [GrupoRepository] Grupo com ID {Id} não encontrado", id);
            }

            return grupo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [GrupoRepository] Erro ao buscar grupo com ID {Id}. Mensagem: {Message}. StackTrace: {StackTrace}", 
                id, ex.Message, ex.StackTrace);
            if (ex.InnerException != null)
            {
                _logger.LogError("❌ [GrupoRepository] InnerException: {Message}. StackTrace: {StackTrace}", 
                    ex.InnerException.Message, ex.InnerException.StackTrace);
            }
            throw;
        }
    }

    public async Task<IEnumerable<Grupo>> GetGruposComQuantidadeAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT g.ID as Id,
                   g.DESCRICAO as Descricao,
                   COUNT(p3.ID) as QuantidadeProdutos,
                   COALESCE(g.IMPRIMIR2VIAS, 0) as ImprimirDuasVias,
                   COALESCE(g.TIPO, 'NORMAL') as Tipo,
                   COALESCE(g.PERCENTUAL, 0) as Percentual
            FROM GRUPO g
            LEFT JOIN PRODUTOESERVICO p3 ON g.ID = p3.GRUPO AND CAST(p3.ATIVO AS INTEGER) = 1
            GROUP BY g.ID, g.DESCRICAO, g.IMPRIMIR2VIAS, g.TIPO, g.PERCENTUAL
            HAVING COUNT(p3.ID) > 0
            ORDER BY g.DESCRICAO";

        try
        {
            _logger.LogInformation("🔍 [GrupoRepository] Executando query GetGruposComQuantidadeAsync");
            _logger.LogInformation("📝 SQL: {Sql}", sql);

            var result = await connection.QueryAsync<Grupo>(sql);
            var grupos = result.ToList();

            _logger.LogInformation("✅ [GrupoRepository] GetGruposComQuantidadeAsync retornou {Count} grupos", grupos.Count);
            if (grupos.Count > 0)
            {
                _logger.LogInformation("📋 Grupos encontrados: {Grupos}", 
                    string.Join(", ", grupos.Select(g => $"ID:{g.Id} Desc:{g.Descricao} Qtd:{g.QuantidadeProdutos}")));
            }

            return grupos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [GrupoRepository] Erro ao executar GetGruposComQuantidadeAsync. Mensagem: {Message}. StackTrace: {StackTrace}", 
                ex.Message, ex.StackTrace);
            if (ex.InnerException != null)
            {
                _logger.LogError("❌ [GrupoRepository] InnerException: {Message}. StackTrace: {StackTrace}", 
                    ex.InnerException.Message, ex.InnerException.StackTrace);
            }
            throw;
        }
    }

    public async Task<IEnumerable<Grupo>> GetGruposComQuantidadeTodosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        // Query principal: buscar da tabela GRUPO com contagem de produtos ativos
        // Usando a mesma query que funciona no IBManager
        var sql = @"
            SELECT g.ID as Id,
                   g.DESCRICAO as Descricao,
                   COUNT(p3.ID) as QuantidadeProdutos,
                   COALESCE(g.IMPRIMIR2VIAS, 0) as ImprimirDuasVias,
                   COALESCE(g.TIPO, 'NORMAL') as Tipo,
                   COALESCE(g.PERCENTUAL, 0) as Percentual
            FROM GRUPO g
            LEFT JOIN PRODUTOESERVICO p3 ON g.ID = p3.GRUPO AND CAST(p3.ATIVO AS INTEGER) = 1
            WHERE g.ID IS NOT NULL
            GROUP BY g.ID, g.DESCRICAO, g.IMPRIMIR2VIAS, g.TIPO, g.PERCENTUAL
            ORDER BY g.DESCRICAO";
        
        // Query alternativa: se não houver grupos na tabela GRUPO, buscar grupos distintos da PRODUTOESERVICO
        // Tenta fazer LEFT JOIN com GRUPO para pegar a descrição, se não tiver, usa o ID como descrição
        var sqlAlternativo = @"
            SELECT DISTINCT 
                   COALESCE(g.ID, p3.GRUPO) as Id,
                   COALESCE(g.DESCRICAO, 'Grupo ' || CAST(p3.GRUPO AS VARCHAR(10))) as Descricao,
                   COUNT(p3.ID) as QuantidadeProdutos
            FROM PRODUTOESERVICO p3
            LEFT JOIN GRUPO g ON g.ID = p3.GRUPO
            WHERE p3.GRUPO IS NOT NULL 
              AND CAST(p3.ATIVO AS INTEGER) = 1
            GROUP BY COALESCE(g.ID, p3.GRUPO), COALESCE(g.DESCRICAO, 'Grupo ' || CAST(p3.GRUPO AS VARCHAR(10)))
            ORDER BY COALESCE(g.DESCRICAO, 'Grupo ' || CAST(p3.GRUPO AS VARCHAR(10)))";

        try
        {
            // Primeiro, testar se há dados na tabela GRUPO
            try
            {
                var countSql = "SELECT COUNT(*) FROM GRUPO";
                var totalGrupos = await connection.QuerySingleAsync<int>(countSql);
                _logger.LogInformation("🔢 [GrupoRepository] Total de grupos na tabela GRUPO: {Count}", totalGrupos);
                
                if (totalGrupos > 0)
                {
                    // Buscar um grupo de exemplo para ver a estrutura
                    var exemploSql = "SELECT FIRST 1 ID, DESCRICAO FROM GRUPO";
                    var exemplo = await connection.QueryFirstOrDefaultAsync<dynamic>(exemploSql);
                    if (exemplo != null)
                    {
                        object? idObj = exemplo.ID;
                        object? descObj = exemplo.DESCRICAO;
                        string exemploId = idObj?.ToString() ?? "NULL";
                        string exemploDesc = descObj?.ToString() ?? "NULL";
                        _logger.LogInformation("📋 [GrupoRepository] Exemplo de grupo: ID={Id}, DESCRICAO={Descricao}", 
                            exemploId, exemploDesc);
                    }
                }
                
                // Verificar produtos na tabela PRODUTOESERVICO
                var countProdutosSql = "SELECT COUNT(*) FROM PRODUTOESERVICO";
                var totalProdutos = await connection.QuerySingleAsync<int>(countProdutosSql);
                _logger.LogInformation("🔢 [GrupoRepository] Total de produtos na tabela PRODUTOESERVICO: {Count}", totalProdutos);
                
                // Verificar produtos com grupo = 1
                var countGrupo1Sql = "SELECT COUNT(*) FROM PRODUTOESERVICO WHERE GRUPO = 1";
                var produtosGrupo1 = await connection.QuerySingleAsync<int>(countGrupo1Sql);
                _logger.LogInformation("🔢 [GrupoRepository] Produtos com GRUPO = 1: {Count}", produtosGrupo1);
                
                // Verificar produtos ativos com grupo = 1
                var countAtivoGrupo1Sql = "SELECT COUNT(*) FROM PRODUTOESERVICO WHERE GRUPO = 1 AND CAST(ATIVO AS INTEGER) = 1";
                var produtosAtivosGrupo1 = await connection.QuerySingleAsync<int>(countAtivoGrupo1Sql);
                _logger.LogInformation("🔢 [GrupoRepository] Produtos ATIVOS com GRUPO = 1: {Count}", produtosAtivosGrupo1);
                
                // Verificar estrutura do campo GRUPO (pode ser minúsculo)
                try
                {
                    var testGrupoMinusculo = "SELECT COUNT(*) FROM PRODUTOESERVICO WHERE grupo = 1";
                    var produtosGrupoMinusculo = await connection.QuerySingleAsync<int>(testGrupoMinusculo);
                    _logger.LogInformation("🔢 [GrupoRepository] Produtos com grupo (minúsculo) = 1: {Count}", produtosGrupoMinusculo);
                }
                catch
                {
                    _logger.LogInformation("⚠️ [GrupoRepository] Campo 'grupo' (minúsculo) não encontrado, usando 'GRUPO' (maiúsculo)");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ [GrupoRepository] Erro ao verificar dados da tabela GRUPO");
            }
            
            _logger.LogInformation("🔍 [GrupoRepository] Executando query GetGruposComQuantidadeTodosAsync");
            _logger.LogInformation("📝 SQL Principal (tabela GRUPO):");
            _logger.LogInformation("{Sql}", sql);
            
            // Testar query simples primeiro para ver se encontra produtos
            try
            {
                var testSql = "SELECT COUNT(*) FROM PRODUTOESERVICO WHERE GRUPO = 1";
                var testCount = await connection.QuerySingleAsync<int>(testSql);
                _logger.LogInformation("🔍 [GrupoRepository] TESTE: Produtos com GRUPO = 1 (sem filtro ativo): {Count}", testCount);
                
                var testAtivoSql = "SELECT COUNT(*) FROM PRODUTOESERVICO WHERE GRUPO = 1 AND CAST(ATIVO AS INTEGER) = 1";
                var testAtivoCount = await connection.QuerySingleAsync<int>(testAtivoSql);
                _logger.LogInformation("🔍 [GrupoRepository] TESTE: Produtos ATIVOS com GRUPO = 1: {Count}", testAtivoCount);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ [GrupoRepository] Erro ao testar query de contagem");
            }

            // Mapear diretamente para Grupo usando aliases
            var result = await connection.QueryAsync<Grupo>(sql);
            var resultList = result.ToList();
            
            _logger.LogInformation("📊 [GrupoRepository] Query retornou {Count} registros", resultList.Count);
            
            if (resultList.Count > 0)
            {
                foreach (var registro in resultList)
                {
                    _logger.LogInformation("📋 [GrupoRepository] Registro: ID={Id}, Descricao={Desc}, QuantidadeProdutos={Qtd}", 
                        registro.Id, registro.Descricao, registro.QuantidadeProdutos);
                }
            }
            else
            {
                _logger.LogWarning("⚠️ [GrupoRepository] Query não retornou nenhum registro! Verificando se há problema na query...");
                
                // Testar query sem GROUP BY para ver se retorna algo
                var testSql = "SELECT g.ID as Id, g.DESCRICAO as Descricao FROM GRUPO g WHERE g.ID IS NOT NULL";
                var testResult = await connection.QueryAsync<Grupo>(testSql);
                var testList = testResult.ToList();
                _logger.LogInformation("🔍 [GrupoRepository] TESTE: Query simples retornou {Count} grupos", testList.Count);
            }
            
            // Se não retornou grupos, tentar buscar da PRODUTOESERVICO
            if (resultList.Count == 0)
            {
                _logger.LogInformation("⚠️ [GrupoRepository] Nenhum grupo encontrado na tabela GRUPO. Tentando buscar grupos distintos da PRODUTOESERVICO...");
                _logger.LogInformation("📝 SQL Alternativo (PRODUTOESERVICO):");
                _logger.LogInformation("{Sql}", sqlAlternativo);
                
                var resultAlt = await connection.QueryAsync<Grupo>(sqlAlternativo);
                resultList = resultAlt.ToList();
                
                if (resultList.Count > 0)
                {
                    _logger.LogInformation("✅ [GrupoRepository] Encontrados {Count} grupos distintos na tabela PRODUTOESERVICO", resultList.Count);
                    var primeiroAlt = resultList.First();
                    _logger.LogInformation("📋 [GrupoRepository] Primeiro registro alternativo: ID={Id}, Descricao={Desc}, QuantidadeProdutos={Qtd}", 
                        primeiroAlt.Id, primeiroAlt.Descricao, primeiroAlt.QuantidadeProdutos);
                }
            }

            // Já está mapeado para Grupo, apenas garantir que QuantidadeProdutos seja int
            var grupos = resultList.Select(r => new Grupo
            {
                Id = r.Id,
                Descricao = r.Descricao ?? string.Empty,
                QuantidadeProdutos = (int)r.QuantidadeProdutos,
                ImprimirDuasVias = r.ImprimirDuasVias,
                Tipo = r.Tipo ?? "NORMAL",
                Percentual = r.Percentual
            }).ToList();

            _logger.LogInformation("✅ [GrupoRepository] GetGruposComQuantidadeTodosAsync retornou {Count} grupos mapeados", grupos.Count);
            if (grupos.Count > 0)
            {
                _logger.LogInformation("📋 Grupos mapeados: {Grupos}", 
                    string.Join(", ", grupos.Select(g => $"ID:{g.Id} Desc:{g.Descricao} Qtd:{g.QuantidadeProdutos}")));
            }

            return grupos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [GrupoRepository] Erro ao executar GetGruposComQuantidadeTodosAsync. Mensagem: {Message}. StackTrace: {StackTrace}", 
                ex.Message, ex.StackTrace);
            if (ex.InnerException != null)
            {
                _logger.LogError("❌ [GrupoRepository] InnerException: {Message}. StackTrace: {StackTrace}", 
                    ex.InnerException.Message, ex.InnerException.StackTrace);
            }
            throw;
        }
    }

    public async Task<int> CriarGrupoAsync(string descricao, bool imprimirDuasVias = false, decimal percentual = 0)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Firebird pode não suportar RETURNING em todas as versões
        // Usar abordagem alternativa: inserir e buscar o último ID gerado
        var sql = @"
            INSERT INTO GRUPO (DESCRICAO, IMPRIMIR2VIAS, PERCENTUAL)
            VALUES (@Descricao, @Imprimir2Vias, @Percentual)";

        try
        {
            _logger.LogInformation("🔍 [GrupoRepository] Criando grupo - Descricao: {Descricao}, ImprimirDuasVias: {ImprimirDuasVias}", descricao, imprimirDuasVias);
            _logger.LogInformation("📝 SQL: {Sql}", sql);

            await connection.ExecuteAsync(sql, new { Descricao = descricao, Imprimir2Vias = imprimirDuasVias ? 1 : 0, Percentual = percentual });

            // Buscar o ID do grupo recém-criado
            var sqlBuscarId = @"
                SELECT MAX(ID) 
                FROM GRUPO 
                WHERE DESCRICAO = @Descricao";
            
            var id = await connection.QuerySingleAsync<int>(sqlBuscarId, new { Descricao = descricao });

            _logger.LogInformation("✅ [GrupoRepository] Grupo criado com sucesso - ID: {Id}", id);
            return id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [GrupoRepository] Erro ao criar grupo. Mensagem: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> AtualizarGrupoAsync(int id, string descricao, bool imprimirDuasVias = false, decimal percentual = 0)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            UPDATE GRUPO
            SET DESCRICAO = @Descricao,
                IMPRIMIR2VIAS = @Imprimir2Vias,
                PERCENTUAL = @Percentual
            WHERE ID = @Id";

        try
        {
            _logger.LogInformation("🔍 [GrupoRepository] Atualizando grupo - ID: {Id}, Descricao: {Descricao}, ImprimirDuasVias: {ImprimirDuasVias}", id, descricao, imprimirDuasVias);
            _logger.LogInformation("📝 SQL: {Sql}", sql);

            var linhasAfetadas = await connection.ExecuteAsync(sql, new { Id = id, Descricao = descricao, Imprimir2Vias = imprimirDuasVias ? 1 : 0, Percentual = percentual });

            if (linhasAfetadas > 0)
            {
                _logger.LogInformation("✅ [GrupoRepository] Grupo atualizado com sucesso - ID: {Id}", id);
                return true;
            }
            else
            {
                _logger.LogWarning("⚠️ [GrupoRepository] Nenhum grupo encontrado para atualizar - ID: {Id}", id);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [GrupoRepository] Erro ao atualizar grupo. Mensagem: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> ExcluirGrupoAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Verificar se há produtos usando este grupo
        var sqlVerificar = @"
            SELECT COUNT(*) 
            FROM PRODUTOESERVICO 
            WHERE GRUPO = @Id";

        try
        {
            _logger.LogInformation("🔍 [GrupoRepository] Verificando produtos do grupo antes de excluir - ID: {Id}", id);
            var produtosComGrupo = await connection.QuerySingleAsync<int>(sqlVerificar, new { Id = id });

            if (produtosComGrupo > 0)
            {
                _logger.LogWarning("⚠️ [GrupoRepository] Não é possível excluir grupo {Id} - há {Count} produtos associados", id, produtosComGrupo);
                throw new InvalidOperationException($"Não é possível excluir o grupo pois há {produtosComGrupo} produto(s) associado(s). Remova os produtos do grupo antes de excluí-lo.");
            }

            var sql = @"
                DELETE FROM GRUPO
                WHERE ID = @Id";

            _logger.LogInformation("🔍 [GrupoRepository] Excluindo grupo - ID: {Id}", id);
            _logger.LogInformation("📝 SQL: {Sql}", sql);

            var linhasAfetadas = await connection.ExecuteAsync(sql, new { Id = id });

            if (linhasAfetadas > 0)
            {
                _logger.LogInformation("✅ [GrupoRepository] Grupo excluído com sucesso - ID: {Id}", id);
                return true;
            }
            else
            {
                _logger.LogWarning("⚠️ [GrupoRepository] Nenhum grupo encontrado para excluir - ID: {Id}", id);
                return false;
            }
        }
        catch (InvalidOperationException)
        {
            throw; // Re-lançar exceção de validação
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [GrupoRepository] Erro ao excluir grupo. Mensagem: {Message}", ex.Message);
            throw;
        }
    }
}
