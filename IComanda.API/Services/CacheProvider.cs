using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace IComanda.API.Services;

/// <summary>
/// Interface para abstração de cache distribuído
/// Pode usar Redis em produção ou MemoryCache em desenvolvimento
/// </summary>
public interface ICacheProvider
{
    /// <summary>Obter valor do cache</summary>
    Task<T?> GetAsync<T>(string key);

    /// <summary>Armazenar valor no cache</summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>Remover valor do cache</summary>
    Task RemoveAsync(string key);

    /// <summary>Verificar se chave existe</summary>
    Task<bool> ExistsAsync(string key);

    /// <summary>Limpar todo o cache</summary>
    Task ClearAsync();

    /// <summary>Obter padrão (ex: "produtos:*")</summary>
    Task<List<string>> GetKeysByPatternAsync(string pattern);
}

/// <summary>
/// Implementação de cache em memória (para desenvolvimento)
/// </summary>
public class MemoryCacheProvider : ICacheProvider
{
    private readonly IMemoryCache _memoryCache;
    private readonly ConcurrentBag<string> _keys;
    private readonly ILogger<MemoryCacheProvider> _logger;

    public MemoryCacheProvider(IMemoryCache memoryCache, ILogger<MemoryCacheProvider> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
        _keys = new ConcurrentBag<string>();
    }

    public Task<T?> GetAsync<T>(string key)
    {
        _memoryCache.TryGetValue(key, out T? value);
        if (value != null)
            _logger.LogDebug($"Cache HIT: {key}");
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var cacheOptions = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
            cacheOptions.AbsoluteExpirationRelativeToNow = expiration;
        else
            cacheOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

        _memoryCache.Set(key, value, cacheOptions);
        _keys.Add(key);
        _logger.LogDebug($"Cache SET: {key} (expires in {expiration?.TotalMinutes}m)");
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _memoryCache.Remove(key);
        _logger.LogDebug($"Cache REMOVE: {key}");
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key)
    {
        var exists = _memoryCache.TryGetValue(key, out _);
        return Task.FromResult(exists);
    }

    public Task ClearAsync()
    {
        foreach (var key in _keys)
        {
            _memoryCache.Remove(key);
        }
        _logger.LogDebug("Cache CLEAR: all entries");
        return Task.CompletedTask;
    }

    public Task<List<string>> GetKeysByPatternAsync(string pattern)
    {
        var regex = new System.Text.RegularExpressions.Regex(
            "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
                .Replace("\\*", ".*") + "$");

        var matchingKeys = _keys.Where(k => regex.IsMatch(k)).ToList();
        return Task.FromResult(matchingKeys);
    }
}

/*
/// <summary>
/// Implementação de cache com Redis (para produção)
/// NOTA: Requer package "StackExchange.Redis"
/// Para ativar:
///   1. Instale: dotnet add package StackExchange.Redis
///   2. Descomente este código
///   3. Descomentar using StackExchange.Redis em Program.cs
///   4. Configurar Redis connection string em appsettings.json
/// </summary>
/*
public class RedisCacheProvider : ICacheProvider
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly ILogger<RedisCacheProvider> _logger;

    public RedisCacheProvider(IConnectionMultiplexer redis, ILogger<RedisCacheProvider> logger)
    {
        _redis = redis;
        _db = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var value = await _db.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                _logger.LogDebug($"Cache MISS: {key}");
                return default;
            }

            var json = value.ToString();
            var obj = System.Text.Json.JsonSerializer.Deserialize<T>(json);
            _logger.LogDebug($"Cache HIT: {key}");
            return obj;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao obter cache: {key}");
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, json, expiration);
            _logger.LogDebug($"Cache SET: {key} (expires in {expiration?.TotalMinutes}m)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao armazenar cache: {key}");
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _db.KeyDeleteAsync(key);
            _logger.LogDebug($"Cache REMOVE: {key}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao remover cache: {key}");
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            return await _db.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao verificar cache: {key}");
            return false;
        }
    }

    public async Task ClearAsync()
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            await server.FlushDatabaseAsync();
            _logger.LogDebug("Cache CLEAR: all entries");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao limpar cache");
        }
    }

    public async Task<List<string>> GetKeysByPatternAsync(string pattern)
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = new List<string>();

            await foreach (var key in server.KeysAsync(pattern: pattern))
            {
                keys.Add(key.ToString());
            }

            return keys;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao obter chaves: {pattern}");
            return new List<string>();
        }
    }
}
*/
