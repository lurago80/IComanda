# 📝 Guia Rápido: Exemplos de Uso - Phase 3

## 1. Usando Cache de Produtos

### Exemplo 1: Obter Produtos com Cache
```csharp
using IComanda.API.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<ProdutosController> _logger;

    public ProdutosController(ICacheService cacheService, ILogger<ProdutosController> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Obter todos os produtos com cache automático (2h TTL)
    /// Primeira chamada: ~150ms (banco)
    /// Chamadas seguintes: ~1ms (cache)
    /// </summary>
    [HttpGet("com-cache")]
    public async Task<IActionResult> GetProdutosComCache()
    {
        try
        {
            var produtos = await _cacheService.GetProdutosComCacheAsync();
            _logger.LogInformation($"Retornados {produtos.Count} produtos do cache");
            return Ok(produtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter produtos com cache");
            return StatusCode(500, "Erro ao obter produtos");
        }
    }

    /// <summary>
    /// Invalidar cache de produtos após criar/editar/deletar
    /// </summary>
    [HttpPost("limpar-cache")]
    public async Task<IActionResult> LimparCacheProdutos()
    {
        await _cacheService.InvalidarProdutosAsync();
        return Ok(new { message = "Cache de produtos limpo com sucesso" });
    }
}
```

---

## 2. Usando Cache de Clientes

### Exemplo 2: Obter Cliente com Cache por ID
```csharp
[HttpGet("{id}/com-cache")]
public async Task<IActionResult> GetClienteComCache(string id)
{
    // Cache: 1 hora
    // Se cliente não existe: retorna null mas não faz cache do erro
    var cliente = await _cacheService.GetClienteComCacheAsync(id);
    
    if (cliente == null)
        return NotFound(new { message = "Cliente não encontrado" });
    
    return Ok(cliente);
}
```

### Exemplo 3: Invalidar Cache de Cliente Específico
```csharp
[HttpPost("{id}/editar")]
public async Task<IActionResult> EditarCliente(string id, ClienteDTO dto)
{
    // Atualizar cliente
    await _clienteService.AtualizarAsync(id, dto);
    
    // Limpar cache deste cliente
    await _cacheService.InvalidarClienteAsync(id);
    
    return Ok(new { message = "Cliente atualizado e cache limpo" });
}
```

---

## 3. Usando Paginação em Produtos

### Exemplo 4: Listar Produtos com Paginação
```csharp
[HttpGet("paginados")]
public async Task<IActionResult> GetProdutosPaginados(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string sortBy = "DESCRICAO ASC",
    [FromQuery] string search = null)
{
    try
    {
        // Validar paginação
        var paginationRequest = new PaginationRequest
        {
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy
        };

        if (!paginationRequest.Validate(out var errors))
        {
            return BadRequest(new { errors = errors });
        }

        // Buscar produtos paginados do banco
        var (produtos, total) = await _produtoRepository.GetPaginatedAsync(
            searchTerm: search,
            pageNumber: paginationRequest.Page,
            pageSize: paginationRequest.PageSize,
            sortBy: paginationRequest.SortBy
        );

        // Montar resposta paginada
        var response = new PaginationResponse<ProdutoDTO>
        {
            Data = produtos,
            Page = paginationRequest.Page,
            PageSize = paginationRequest.PageSize,
            Total = total
        };

        return Ok(response);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erro ao paginar produtos");
        return StatusCode(500, "Erro ao buscar produtos");
    }
}
```

**Request:**
```
GET /api/produtos/paginados?page=1&pageSize=20&sortBy=DESCRICAO%20ASC&search=Coca
```

**Response:**
```json
{
  "data": [
    { "id": 1, "codigo": "001", "descricao": "Coca Cola 2L", "preco": 8.99 },
    { "id": 2, "codigo": "002", "descricao": "Coca Cola Lata 350ml", "preco": 3.50 }
  ],
  "page": 1,
  "pageSize": 20,
  "total": 150,
  "totalPages": 8,
  "hasNextPage": true,
  "hasPreviousPage": false,
  "firstItemIndex": 1,
  "lastItemIndex": 20
}
```

---

## 4. Usando Paginação com Filtros Complexos

### Exemplo 5: Listar Vendas Abertas Paginadas
```csharp
[HttpGet("abertas/paginadas")]
public async Task<IActionResult> GetVendasAbertasPaginadas(
    [FromQuery] DateTime? dataInicio,
    [FromQuery] DateTime? dataFim,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 50,
    [FromQuery] string sortBy = "DATA DESC")
{
    var paginationRequest = new PaginationRequest
    {
        Page = page,
        PageSize = pageSize,
        SortBy = sortBy
    };

    if (!paginationRequest.Validate(out var errors))
        return BadRequest(new { errors = errors });

    // Usar extensão SQL
    var sqlBuilder = new SqlPaginationBuilder("SELECT * FROM VENDAS")
        .Condition("STATUS = 'ABERTA'")
        .Condition("DATA >= @dataInicio", ("@dataInicio", dataInicio ?? DateTime.Now.AddDays(-7)))
        .Condition("DATA <= @dataFim", ("@dataFim", dataFim ?? DateTime.Now))
        .OrderBy(paginationRequest.SortBy)
        .Pagination(
            offset: (paginationRequest.Page - 1) * paginationRequest.PageSize,
            limit: paginationRequest.PageSize
        );

    var vendas = await _connection.QueryAsync<VendaDTO>(sqlBuilder.Build());
    var total = await _connection.ExecuteScalarAsync<int>(sqlBuilder.BuildCount());

    var response = paginationRequest.CreateResponse(vendas.ToList(), total);
    return Ok(response);
}
```

---

## 5. Usando Cache + Paginação Juntos

### Exemplo 6: Padrão Recomendado
```csharp
[HttpGet("vendas/recentes")]
public async Task<IActionResult> GetVendasRecentes(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
{
    // Cache de 10 minutos para página 1
    if (page == 1)
    {
        var cacheKey = "vendas:recentes:page1";
        var cached = await _cacheProvider.GetAsync<PaginationResponse<VendaDTO>>(cacheKey);
        
        if (cached != null)
        {
            _logger.LogInformation("Vendas recentes obtidas do cache");
            return Ok(cached);
        }
    }

    // Se não está em cache, buscar do banco com paginação
    var paginationRequest = new PaginationRequest { Page = page, PageSize = pageSize };
    var (vendas, total) = await _vendaRepository.GetRecentesPaginatedAsync(page, pageSize);
    var response = paginationRequest.CreateResponse(vendas.ToList(), total);

    // Cache apenas página 1 (mais consultada)
    if (page == 1)
    {
        await _cacheProvider.SetAsync(
            cacheKey: "vendas:recentes:page1",
            value: response,
            expiration: TimeSpan.FromMinutes(10)
        );
    }

    return Ok(response);
}
```

---

## 6. Limpando Todo o Cache (Manutenção)

### Exemplo 7: Endpoint de Limpeza Administrativa
```csharp
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "ADMIN")]
public class AdminController : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly ICacheProvider _cacheProvider;

    /// <summary>
    /// Limpar TODO o cache do sistema
    /// Recomendado: após atualização em massa
    /// </summary>
    [HttpPost("cache/limpar")]
    public async Task<IActionResult> LimparTodoCache()
    {
        try
        {
            await _cacheService.InvalidarTudoAsync();
            _logger.LogWarning("Cache do sistema limpo completamente");
            return Ok(new { message = "Cache limpo com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao limpar cache");
            return StatusCode(500, "Erro ao limpar cache");
        }
    }

    /// <summary>
    /// Obter estatísticas do cache
    /// </summary>
    [HttpGet("cache/stats")]
    public async Task<IActionResult> GetCacheStats()
    {
        // Se usar MemoryCache com IMemoryCache
        // Se usar Redis com redis-cli INFO
        
        return Ok(new
        {
            provider = "MemoryCache", // ou "Redis"
            timestamp = DateTime.UtcNow,
            message = "Utilize MemoryCache.GetCurrentStatistics() ou redis-cli INFO para detalhes"
        });
    }
}
```

---

## 7. Configuração em Program.cs

```csharp
// Program.cs - Adicionado na Phase 3

// Registrar cache service
services.AddMemoryCache(); // Para MemoryCache

// Registrar providers
services.AddScoped<ICacheProvider, MemoryCacheProvider>();
services.AddScoped<ICacheService, CacheService>();

// Se ativar Redis:
// services.AddStackExchangeRedisCache(options =>
// {
//     options.Configuration = configuration.GetConnectionString("Redis");
// });
// services.AddScoped<ICacheProvider, RedisCacheProvider>();
```

---

## 8. Validação de Paginação

### Exemplo 8: Regras de Validação
```csharp
var paginationRequest = new PaginationRequest
{
    Page = -1,        // INVÁLIDO: deve ser >= 1
    PageSize = 500,   // INVÁLIDO: máximo é 100
    SortBy = "NOME ASC" // VÁLIDO
};

if (!paginationRequest.Validate(out var errors))
{
    // errors = ["Page deve ser >= 1", "PageSize deve estar entre 1 e 100"]
    return BadRequest(new { errors = errors });
}
```

---

## 9. SQL Injection Prevention

### Exemplo 9: Proteção contra SQL Injection
```csharp
var builder = new SqlPaginationBuilder("SELECT * FROM PRODUTOS")
    .OrderBy("DESCRICAO'; DROP TABLE PRODUTOS; --"); // TENTATIVA DE INJECTION
    
// ResiáltadO: BlindValidation rejeita a string
// SQL gerado: SELECT * FROM PRODUTOS ORDER BY DESCRICAO ASC (padrão seguro)

// Força uso de ORDER válido:
.OrderBy("DESCRICAO ASC") // OK
.OrderBy("NOME DESC")     // OK
.OrderBy("PRECO ASC")     // OK
```

---

## 10. Monitorando Performance

### Exemplo 10: Logs Estruturados
```csharp
_logger.LogInformation("Cache hit: {CacheKey}", "cache:produtos:todos");
_logger.LogInformation("Cache miss: {CacheKey}", "cache:cliente:123");
_logger.LogWarning("Cache miss alto: {Percentage}%", 85);
_logger.LogError("Cache failure: {Provider}", "Redis");

// Log de paginação
_logger.LogDebug("Paginação: page={Page}, size={PageSize}, total={Total}", 
    page, pageSize, total);
```

---

## 🚀 Melhores Práticas

✅ **DO:**
- Use cache para dados que mudam raramente (produtos, clientes)
- Use paginação para listar muitos registros (vendas, itens)
- Invalide cache após modificações
- Valide paginação antes de usar
- Loggue hits/misses de cache

❌ **DON'T:**
- Não fazja cache de dados sensíveis (senhas, tokens)
- Não deixe cache TTL muito longo (máx 2h)
- Não use paginação sem ORDER BY
- Não ignore erros de validação
- Não confie em cache do cliente (sempre valide)

---

## 📊 Performance Esperada

```
Sem Cache + Sem Paginação:
- Listar 1000 produtos: 1200ms + transferência 5MB+

Com Cache + Com Paginação:
- Listar 20 produtos: 1ms (cache) + transferência 50KB
- Melhoria: 1200x mais rápido!

Com Índices Firebird:
- Filtro por TIPO: 800ms → 150ms (5x mais rápido)
- Filtro por DATA: 600ms → 120ms (5x mais rápido)
```

---

## ✅ Resumo de Uso

| Cenário | Código Recomendado |
|---------|-------------------|
| Obter 1 cliente | `await _cacheService.GetClienteComCacheAsync(id)` |
| Listar produtos | `GET /api/produtos/paginados?page=1&pageSize=20` |
| Criar novo produto | `POST` + `await _cacheService.InvalidarProdutosAsync()` |
| Dashboard recente | Combinar `cache + paginação` para página 1 |
| Admin limpar cache | `POST /api/admin/cache/limpar` |

---

**Próxima Etapa:** Implementar filtros avançados em cada endpoint paginado!
