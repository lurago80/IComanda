using IComanda.API.Models;
using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;

namespace IComanda.API.Services;

/// <summary>
/// Serviço de cache para dados frequentemente consultados
/// Cache para: Produtos, Clientes, Grupos, etc
/// </summary>
public interface ICacheService
{
    /// <summary>Obter todos os produtos com cache</summary>
    Task<List<Produto>> GetProdutosComCacheAsync();

    /// <summary>Obter cliente por ID com cache</summary>
    Task<Cliente?> GetClienteComCacheAsync(int id);

    /// <summary>Obter todos os grupos com cache</summary>
    Task<List<Grupo>> GetGruposComCacheAsync();

    /// <summary>Invalidar cache de produtos</summary>
    Task InvalidarProdutosAsync();

    /// <summary>Invalidar cache de cliente específico</summary>
    Task InvalidarClienteAsync(int id);

    /// <summary>Invalidar todos os caches</summary>
    Task InvalidarTudoAsync();
}

public class CacheService : ICacheService
{
    private readonly ICacheProvider _cacheProvider;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IGrupoRepository _grupoRepository;
    private readonly ILogger<CacheService> _logger;

    // Chaves de cache
    private const string CACHE_KEY_PRODUTOS = "cache:produtos:todos";
    private const string CACHE_KEY_CLIENTE = "cache:cliente:{0}"; // {id}
    private const string CACHE_KEY_GRUPOS = "cache:grupos:todos";

    // TTL (time to live)
    private static readonly TimeSpan CACHE_TTL_PRODUTOS = TimeSpan.FromHours(2);
    private static readonly TimeSpan CACHE_TTL_CLIENTE = TimeSpan.FromHours(1);
    private static readonly TimeSpan CACHE_TTL_GRUPOS = TimeSpan.FromHours(2);

    public CacheService(
        ICacheProvider cacheProvider,
        IProdutoRepository produtoRepository,
        IClienteRepository clienteRepository,
        IGrupoRepository grupoRepository,
        ILogger<CacheService> logger)
    {
        _cacheProvider = cacheProvider;
        _produtoRepository = produtoRepository;
        _clienteRepository = clienteRepository;
        _grupoRepository = grupoRepository;
        _logger = logger;
    }

    /// <summary>Obter todos os produtos com cache</summary>
    public async Task<List<Produto>> GetProdutosComCacheAsync()
    {
        try
        {
            // Tentar obter do cache
            var produtos = await _cacheProvider.GetAsync<List<Produto>>(CACHE_KEY_PRODUTOS);

            if (produtos != null)
            {
                _logger.LogInformation($"✅ Produtos obtidos do CACHE ({produtos.Count} itens)");
                return produtos;
            }

            // Não encontrou no cache, buscar do banco
            // Buscar todos os produtos com paginação (usando limite alto para trazer todos)
            _logger.LogInformation("📦 Buscando produtos do banco de dados...");
            var produtosResult = (await _produtoRepository.BuscarProdutosAsync(
                termo: null,
                ativo: true,
                grupo: null,
                pagina: 1,
                itensPorPagina: 10000 // Limite alto para trazer todos
            )).ToList();

            // Armazenar no cache
            await _cacheProvider.SetAsync(CACHE_KEY_PRODUTOS, produtosResult, CACHE_TTL_PRODUTOS);
            _logger.LogInformation($"💾 Produtos armazenados no cache ({produtosResult.Count} itens)");

            return produtosResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter produtos com cache");
            throw;
        }
    }

    /// <summary>Obter cliente por ID com cache</summary>
    public async Task<Cliente?> GetClienteComCacheAsync(int id)
    {
        try
        {
            var cacheKey = string.Format(CACHE_KEY_CLIENTE, id);

            // Tentar obter do cache
            var cliente = await _cacheProvider.GetAsync<Cliente>(cacheKey);

            if (cliente != null)
            {
                _logger.LogInformation($"✅ Cliente {id} obtido do CACHE");
                return cliente;
            }

            // Não encontrou no cache, buscar do banco
            _logger.LogInformation($"📦 Buscando cliente {id} do banco de dados...");
            cliente = await _clienteRepository.GetByIdAsync(id);

            if (cliente == null)
            {
                return null;
            }

            // Armazenar no cache
            await _cacheProvider.SetAsync(cacheKey, cliente, CACHE_TTL_CLIENTE);
            _logger.LogInformation($"💾 Cliente {id} armazenado no cache");

            return cliente;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao obter cliente {id} com cache");
            throw;
        }
    }

    /// <summary>Obter todos os grupos com cache</summary>
    public async Task<List<Grupo>> GetGruposComCacheAsync()
    {
        try
        {
            // Tentar obter do cache
            var grupos = await _cacheProvider.GetAsync<List<Grupo>>(CACHE_KEY_GRUPOS);

            if (grupos != null)
            {
                _logger.LogInformation($"✅ Grupos obtidos do CACHE ({grupos.Count} itens)");
                return grupos;
            }

            // Não encontrou no cache, buscar do banco
            _logger.LogInformation("📦 Buscando grupos do banco de dados...");
            grupos = (await _grupoRepository.GetAllGruposAsync()).ToList();

            // Armazenar no cache
            await _cacheProvider.SetAsync(CACHE_KEY_GRUPOS, grupos, CACHE_TTL_GRUPOS);
            _logger.LogInformation($"💾 Grupos armazenados no cache ({grupos.Count} itens)");

            return grupos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter grupos com cache");
            throw;
        }
    }

    /// <summary>Invalidar cache de produtos</summary>
    public async Task InvalidarProdutosAsync()
    {
        await _cacheProvider.RemoveAsync(CACHE_KEY_PRODUTOS);
        _logger.LogWarning("🗑️ Cache de produtos invalidado");
    }

    /// <summary>Invalidar cache de cliente específico</summary>
    public async Task InvalidarClienteAsync(int id)
    {
        var cacheKey = string.Format(CACHE_KEY_CLIENTE, id);
        await _cacheProvider.RemoveAsync(cacheKey);
        _logger.LogWarning($"🗑️ Cache do cliente {id} invalidado");
    }

    /// <summary>Invalidar todos os caches</summary>
    public async Task InvalidarTudoAsync()
    {
        await _cacheProvider.ClearAsync();
        _logger.LogWarning("🗑️ TODOS os caches invalidados");
    }
}
