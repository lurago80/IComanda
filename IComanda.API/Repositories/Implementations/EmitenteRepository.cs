using Dapper;
using IComanda.API.Data;
using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace IComanda.API.Repositories.Implementations;

public class EmitenteRepository : IEmitenteRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<EmitenteRepository> _logger;

    public EmitenteRepository(IDbConnectionFactory connectionFactory, ILogger<EmitenteRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Emitente?> GetEmitenteAsync()
    {
        _logger.LogInformation("🔍 [EmitenteRepository] GetEmitenteAsync() chamado");
        try
        {
            _logger.LogInformation("🔍 [EmitenteRepository] Criando conexão com o banco...");
            using var connection = _connectionFactory.CreateConnection();
            _logger.LogInformation("✅ [EmitenteRepository] Conexão criada com sucesso");
            
            // Primeiro, verificar se existe algum registro na tabela
            var countSql = "SELECT COUNT(*) FROM EMITENTE";
            var count = await connection.QuerySingleAsync<int>(countSql);
            _logger.LogInformation("📊 Total de registros na tabela EMITENTE: {Count}", count);
            
            if (count == 0)
            {
                _logger.LogWarning("⚠️ Tabela EMITENTE está vazia");
                return null;
            }

            // Listar todos os IDs disponíveis para diagnóstico
            var idsSql = "SELECT ID FROM EMITENTE ORDER BY ID";
            var ids = await connection.QueryAsync<int>(idsSql);
            var idsList = ids.ToList();
            _logger.LogInformation("📋 IDs disponíveis na tabela EMITENTE: {Ids}", string.Join(", ", idsList));

            // Buscar o registro com ID = 1 (conforme funciona no IBManager)
            // Usando nomes de colunas corretos conforme mapeamento fornecido
            var sql = @"
                SELECT 
                    ID,
                    NOME,
                    FANTASIA AS NOMEFANTASIA,
                    CPFCNPJ AS CNPJ,
                    ENDER AS ENDERECO,
                    NUMEND AS NUMERO,
                    COMPLEMENTO,
                    BAIRRO,
                    CIDADE,
                    UF,
                    CEP,
                    TELEFONE,
                    EMAIL
                FROM EMITENTE
                WHERE ID = 1";

            _logger.LogInformation("🔍 Buscando dados do emitente na tabela EMITENTE (ID = 1)");
            _logger.LogInformation("📝 SQL: {Sql}", sql);

            // Usar QueryFirstOrDefaultAsync com mapeamento dinâmico e depois converter
            // Dapper abre a conexão automaticamente se necessário
            _logger.LogInformation("🔍 [EmitenteRepository] Executando query SQL...");
            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql);
            
            _logger.LogInformation("📦 [EmitenteRepository] Resultado da query: {Result}", result != null ? "Encontrado" : "Nulo");
            
            if (result == null)
            {
                _logger.LogWarning("⚠️ [EmitenteRepository] Nenhum registro encontrado na tabela EMITENTE");
                _logger.LogWarning("   Tabela tem {Count} registros mas query não retornou resultado", count);
                _logger.LogWarning("   IDs disponíveis: {Ids}", string.Join(", ", idsList));
                _logger.LogWarning("   SQL executado: {Sql}", sql);
                
                // Tentar uma query mais simples para diagnóstico
                try
                {
                    var testSql = "SELECT * FROM EMITENTE WHERE ID = 1";
                    _logger.LogInformation("🔍 [EmitenteRepository] Testando query simples: {TestSql}", testSql);
                    var testResult = await connection.QueryFirstOrDefaultAsync<dynamic>(testSql);
                    _logger.LogInformation("📦 [EmitenteRepository] Resultado da query de teste: {TestResult}", testResult != null ? "Encontrado" : "Nulo");
                }
                catch (Exception testEx)
                {
                    _logger.LogError(testEx, "❌ [EmitenteRepository] Erro na query de teste: {Message}", testEx.Message);
                }
                
                return null;
            }

            // Log do ID encontrado (convertendo dynamic para string)
            string idEncontrado = "null";
            if (result.ID != null)
            {
                idEncontrado = Convert.ToString(result.ID) ?? "null";
            }
            _logger.LogInformation("✅ Registro encontrado com ID: {Id}", idEncontrado);

            // Mapear manualmente para garantir compatibilidade
            var dict = result as IDictionary<string, object> ?? new Dictionary<string, object>();
            string? GetString(string key)
            {
                return dict.TryGetValue(key, out var value) ? value?.ToString()?.Trim() : null;
            }

            var emitente = new Emitente
            {
                Id = result.ID != null ? Convert.ToInt32(result.ID) : 0,
                Nome = GetString("NOME"),
                NomeFantasia = GetString("NOMEFANTASIA"),
                Cnpj = GetString("CNPJ"),
                InscricaoEstadual = GetString("INSCRICAOESTADUAL"), // ficará null se coluna não existir
                Endereco = GetString("ENDERECO"),
                Numero = GetString("NUMERO"),
                Complemento = GetString("COMPLEMENTO"),
                Bairro = GetString("BAIRRO"),
                Cidade = GetString("CIDADE"),
                Uf = GetString("UF"),
                Cep = GetString("CEP"),
                Telefone = GetString("TELEFONE"),
                Email = GetString("EMAIL"),
                Site = GetString("SITE") // pode não existir na tabela, ficará null
            };

            _logger.LogInformation("✅ Emitente mapeado com sucesso:");
            _logger.LogInformation("   Nome: {Nome}", emitente.Nome);
            _logger.LogInformation("   NomeFantasia: {NomeFantasia}", emitente.NomeFantasia);
            _logger.LogInformation("   CNPJ: {Cnpj}", emitente.Cnpj);
            _logger.LogInformation("   Endereco: {Endereco}, Numero: {Numero}, Bairro: {Bairro}", 
                emitente.Endereco, emitente.Numero, emitente.Bairro);
            _logger.LogInformation("   Cidade: {Cidade}, UF: {Uf}, CEP: {Cep}", 
                emitente.Cidade, emitente.Uf, emitente.Cep);
            _logger.LogInformation("   Telefone: {Telefone}, Email: {Email}, Site: {Site}", 
                emitente.Telefone, emitente.Email, emitente.Site);

            return emitente;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao buscar emitente: {Message}", ex.Message);
            if (ex.InnerException != null)
            {
                _logger.LogError("   Inner Exception: {InnerMessage}", ex.InnerException.Message);
            }
            return null;
        }
    }

    public async Task SaveEmitenteAsync(Emitente emitente)
    {
        using var connection = _connectionFactory.CreateConnection();

        var countSql = "SELECT COUNT(*) FROM EMITENTE";
        var count = await connection.QuerySingleAsync<int>(countSql);

        if (count == 0)
        {
            var insertSql = @"
                INSERT INTO EMITENTE (ID, NOME, FANTASIA, CPFCNPJ, ENDER, NUMEND, COMPLEMENTO, BAIRRO, CIDADE, UF, CEP, TELEFONE, EMAIL)
                VALUES (1, @Nome, @NomeFantasia, @Cnpj, @Endereco, @Numero, @Complemento, @Bairro, @Cidade, @Uf, @Cep, @Telefone, @Email)";
            await connection.ExecuteAsync(insertSql, new
            {
                emitente.Nome,
                emitente.NomeFantasia,
                emitente.Cnpj,
                emitente.Endereco,
                emitente.Numero,
                emitente.Complemento,
                emitente.Bairro,
                emitente.Cidade,
                emitente.Uf,
                emitente.Cep,
                emitente.Telefone,
                emitente.Email
            });
            _logger.LogInformation("✅ [EmitenteRepository] Emitente inserido com sucesso");
        }
        else
        {
            var updateSql = @"
                UPDATE EMITENTE SET
                    NOME        = @Nome,
                    FANTASIA    = @NomeFantasia,
                    CPFCNPJ     = @Cnpj,
                    ENDER       = @Endereco,
                    NUMEND      = @Numero,
                    COMPLEMENTO = @Complemento,
                    BAIRRO      = @Bairro,
                    CIDADE      = @Cidade,
                    UF          = @Uf,
                    CEP         = @Cep,
                    TELEFONE    = @Telefone,
                    EMAIL       = @Email
                WHERE ID = 1";
            await connection.ExecuteAsync(updateSql, new
            {
                emitente.Nome,
                emitente.NomeFantasia,
                emitente.Cnpj,
                emitente.Endereco,
                emitente.Numero,
                emitente.Complemento,
                emitente.Bairro,
                emitente.Cidade,
                emitente.Uf,
                emitente.Cep,
                emitente.Telefone,
                emitente.Email
            });
            _logger.LogInformation("✅ [EmitenteRepository] Emitente atualizado com sucesso");
        }
    }
}

