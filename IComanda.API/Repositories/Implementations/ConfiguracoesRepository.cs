using Dapper;
using IComanda.API.Data;
using IComanda.API.Repositories.Interfaces;

namespace IComanda.API.Repositories.Implementations;

/// <summary>
/// Lê e grava configurações do sistema na tabela PARAMETROS do Firebird.
/// Usa UPDATE OR INSERT para não depender de INSERT explícito.
/// </summary>
public class ConfiguracoesRepository : IConfiguracoesRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<ConfiguracoesRepository> _logger;

    public ConfiguracoesRepository(
        IDbConnectionFactory connectionFactory,
        ILogger<ConfiguracoesRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<bool> GetUsarDeliveryAsync()
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();
            var valor = await conn.QueryFirstOrDefaultAsync<string>(
                "SELECT FIRST 1 VALOR FROM PARAMETROS WHERE PARAMETRO = 'USAR_DELIVERY'");

            // Se não existe o parâmetro: padrão = habilitado (true)
            if (string.IsNullOrWhiteSpace(valor))
                return true;

            return valor.Trim().ToUpper() != "N";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Configuracoes] Não foi possível ler USAR_DELIVERY — retornando padrão true.");
            return true;
        }
    }

    /// <inheritdoc/>
    public async Task SetUsarDeliveryAsync(bool usarDelivery)
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();
            var valor = usarDelivery ? "S" : "N";

            // Tenta UPDATE primeiro; se 0 linhas afetadas, faz INSERT herdando ID_EMITENTE do primeiro registro existente
            var linhas = await conn.ExecuteAsync(
                "UPDATE PARAMETROS SET VALOR = @Valor WHERE PARAMETRO = 'USAR_DELIVERY'",
                new { Valor = valor });

            if (linhas == 0)
            {
                // Insere herdando ID_EMITENTE de registro existente; TIPO='BOOLEAN' (S/N); ID gerado pelo trigger BI_PARAMETROS_ID
                await conn.ExecuteAsync(
                    @"INSERT INTO PARAMETROS (ID_EMITENTE, PARAMETRO, VALOR, TIPO)
                      SELECT FIRST 1 ID_EMITENTE, 'USAR_DELIVERY', @Valor, 'BOOLEAN' FROM PARAMETROS",
                    new { Valor = valor });
            }

            _logger.LogInformation("[Configuracoes] USAR_DELIVERY gravado como '{Valor}'", valor);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Configuracoes] Não foi possível gravar USAR_DELIVERY — ignorando.");
        }
    }

    /// <inheritdoc/>
    public async Task<bool> GetUsarForcaVendasAsync()
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();
            var valor = await conn.QueryFirstOrDefaultAsync<string>(
                "SELECT FIRST 1 VALOR FROM PARAMETROS WHERE PARAMETRO = 'USAR_FORCA_VENDAS'");

            if (string.IsNullOrWhiteSpace(valor))
                return true; // padrão: habilitado

            return valor.Trim().ToUpper() != "N";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Configuracoes] Não foi possível ler USAR_FORCA_VENDAS — retornando padrão true.");
            return true;
        }
    }

    /// <inheritdoc/>
    public async Task SetUsarForcaVendasAsync(bool usarForcaVendas)
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();
            var valor = usarForcaVendas ? "S" : "N";

            var linhas = await conn.ExecuteAsync(
                "UPDATE PARAMETROS SET VALOR = @Valor WHERE PARAMETRO = 'USAR_FORCA_VENDAS'",
                new { Valor = valor });

            if (linhas == 0)
            {
                await conn.ExecuteAsync(
                    @"INSERT INTO PARAMETROS (ID_EMITENTE, PARAMETRO, VALOR, TIPO)
                      SELECT FIRST 1 ID_EMITENTE, 'USAR_FORCA_VENDAS', @Valor, 'BOOLEAN' FROM PARAMETROS",
                    new { Valor = valor });
            }

            _logger.LogInformation("[Configuracoes] USAR_FORCA_VENDAS gravado como '{Valor}'", valor);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Configuracoes] Não foi possível gravar USAR_FORCA_VENDAS — ignorando.");
        }
    }

    /// <inheritdoc/>
    public async Task<bool> GetUsarComandaAsync()
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();
            var valor = await conn.QueryFirstOrDefaultAsync<string>(
                "SELECT FIRST 1 VALOR FROM PARAMETROS WHERE PARAMETRO = 'USAR_COMANDA'");

            if (string.IsNullOrWhiteSpace(valor))
                return true; // padrão: habilitado

            return valor.Trim().ToUpper() != "N";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Configuracoes] Não foi possível ler USAR_COMANDA — retornando padrão true.");
            return true;
        }
    }

    /// <inheritdoc/>
    public async Task SetUsarComandaAsync(bool usarComanda)
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();
            var valor = usarComanda ? "S" : "N";

            var linhas = await conn.ExecuteAsync(
                "UPDATE PARAMETROS SET VALOR = @Valor WHERE PARAMETRO = 'USAR_COMANDA'",
                new { Valor = valor });

            if (linhas == 0)
            {
                await conn.ExecuteAsync(
                    @"INSERT INTO PARAMETROS (ID_EMITENTE, PARAMETRO, VALOR, TIPO)
                      SELECT FIRST 1 ID_EMITENTE, 'USAR_COMANDA', @Valor, 'BOOLEAN' FROM PARAMETROS",
                    new { Valor = valor });
            }

            _logger.LogInformation("[Configuracoes] USAR_COMANDA gravado como '{Valor}'", valor);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Configuracoes] Não foi possível gravar USAR_COMANDA — ignorando.");
        }
    }

    /// <inheritdoc/>
    public async Task<bool> GetHabilitarImprimirDuasViasAsync()
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();
            var valor = await conn.QueryFirstOrDefaultAsync<string>(
                "SELECT FIRST 1 VALOR FROM PARAMETROS WHERE PARAMETRO = 'HABILITAR_IMPRIMIR_2VIAS'");

            if (string.IsNullOrWhiteSpace(valor))
                return false; // padrão: desabilitado

            return valor.Trim().ToUpper() == "S";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Configuracoes] Não foi possível ler HABILITAR_IMPRIMIR_2VIAS — retornando padrão false.");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task SetHabilitarImprimirDuasViasAsync(bool habilitar)
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();
            var valor = habilitar ? "S" : "N";

            var linhas = await conn.ExecuteAsync(
                "UPDATE PARAMETROS SET VALOR = @Valor WHERE PARAMETRO = 'HABILITAR_IMPRIMIR_2VIAS'",
                new { Valor = valor });

            if (linhas == 0)
            {
                await conn.ExecuteAsync(
                    @"INSERT INTO PARAMETROS (ID_EMITENTE, PARAMETRO, VALOR, TIPO)
                      SELECT FIRST 1 ID_EMITENTE, 'HABILITAR_IMPRIMIR_2VIAS', @Valor, 'BOOLEAN' FROM PARAMETROS",
                    new { Valor = valor });
            }

            _logger.LogInformation("[Configuracoes] HABILITAR_IMPRIMIR_2VIAS gravado como '{Valor}'", valor);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Configuracoes] Não foi possível gravar HABILITAR_IMPRIMIR_2VIAS — ignorando.");
        }
    }

    /// <inheritdoc/>
    public async Task<bool> GetUsarCozinhaAsync()
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();
            var valor = await conn.QueryFirstOrDefaultAsync<string>(
                "SELECT FIRST 1 VALOR FROM PARAMETROS WHERE PARAMETRO = 'USAR_COZINHA'");

            if (string.IsNullOrWhiteSpace(valor))
                return true; // padrão: habilitado

            return valor.Trim().ToUpper() != "N";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Configuracoes] Não foi possível ler USAR_COZINHA — retornando padrão true.");
            return true;
        }
    }

    /// <inheritdoc/>
    public async Task SetUsarCozinhaAsync(bool usarCozinha)
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();
            var valor = usarCozinha ? "S" : "N";

            var linhas = await conn.ExecuteAsync(
                "UPDATE PARAMETROS SET VALOR = @Valor WHERE PARAMETRO = 'USAR_COZINHA'",
                new { Valor = valor });

            if (linhas == 0)
            {
                await conn.ExecuteAsync(
                    @"INSERT INTO PARAMETROS (ID_EMITENTE, PARAMETRO, VALOR, TIPO)
                      SELECT FIRST 1 ID_EMITENTE, 'USAR_COZINHA', @Valor, 'BOOLEAN' FROM PARAMETROS",
                    new { Valor = valor });
            }

            _logger.LogInformation("[Configuracoes] USAR_COZINHA gravado como '{Valor}'", valor);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Configuracoes] Não foi possível gravar USAR_COZINHA — ignorando.");
        }
    }

    /// <inheritdoc/>
    public async Task<bool> GetUsarCardapioAsync()
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();
            var valor = await conn.QueryFirstOrDefaultAsync<string>(
                "SELECT FIRST 1 VALOR FROM PARAMETROS WHERE PARAMETRO = 'USAR_CARDAPIO'");

            if (string.IsNullOrWhiteSpace(valor))
                return true; // padrão: habilitado

            return valor.Trim().ToUpper() != "N";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Configuracoes] Não foi possível ler USAR_CARDAPIO — retornando padrão true.");
            return true;
        }
    }

    /// <inheritdoc/>
    public async Task SetUsarCardapioAsync(bool usarCardapio)
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();
            var valor = usarCardapio ? "S" : "N";

            var linhas = await conn.ExecuteAsync(
                "UPDATE PARAMETROS SET VALOR = @Valor WHERE PARAMETRO = 'USAR_CARDAPIO'",
                new { Valor = valor });

            if (linhas == 0)
            {
                await conn.ExecuteAsync(
                    @"INSERT INTO PARAMETROS (ID_EMITENTE, PARAMETRO, VALOR, TIPO)
                      SELECT FIRST 1 ID_EMITENTE, 'USAR_CARDAPIO', @Valor, 'BOOLEAN' FROM PARAMETROS",
                    new { Valor = valor });
            }

            _logger.LogInformation("[Configuracoes] USAR_CARDAPIO gravado como '{Valor}'", valor);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Configuracoes] Não foi possível gravar USAR_CARDAPIO — ignorando.");
        }
    }
}
