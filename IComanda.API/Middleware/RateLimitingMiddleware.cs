using System.Collections.Concurrent;

namespace IComanda.API.Middleware;

/// <summary>
/// Middleware para implementar rate limiting por IP ou por usuário autenticado.
/// Limita MaxRequestsPerMinute requisições por minuto por IP/usuário.
/// 
/// CORREÇÃO: uso de SlidingWindow thread-safe com long (Interlocked) para evitar
/// race conditions e vazamento de memória que causavam quedas do serviço.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    // Chave = identificador (IP ou UserId), valor = fila thread-safe de timestamps
    private static readonly ConcurrentDictionary<string, ConcurrentQueue<long>> RequestLog =
        new ConcurrentDictionary<string, ConcurrentQueue<long>>(StringComparer.OrdinalIgnoreCase);

    // Limites de requisição
    private const int MaxRequestsPerMinute = 300;
    private const int CleanupIntervalSeconds = 120;

    private static long _lastCleanupTicks = DateTime.UtcNow.Ticks;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Limpeza periódica assíncrona sem bloquear a requisição
        var lastCleanup = new DateTime(Interlocked.Read(ref _lastCleanupTicks), DateTimeKind.Utc);
        if ((DateTime.UtcNow - lastCleanup).TotalSeconds > CleanupIntervalSeconds)
        {
            // Usar CompareExchange para garantir que apenas uma thread faça a limpeza
            var expected = lastCleanup.Ticks;
            var newValue = DateTime.UtcNow.Ticks;
            if (Interlocked.CompareExchange(ref _lastCleanupTicks, newValue, expected) == expected)
            {
                _ = Task.Run(CleanupOldRequests);
            }
        }

        var identifier = GetIdentifier(context);

        if (!IsRequestAllowed(identifier))
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Too Many Requests",
                message = $"Limite de requisições excedido. Máximo de {MaxRequestsPerMinute} requisições por minuto.",
                retryAfter = 60
            };

            _logger.LogWarning("Rate limit excedido para {Identifier}", identifier);
            await context.Response.WriteAsJsonAsync(response);
            return;
        }

        await _next(context);
    }

    /// <summary>Obtém o identificador único para rate limiting</summary>
    private static string GetIdentifier(HttpContext context)
    {
        var userIdClaim = context.User?.FindFirst("UserId");
        if (userIdClaim != null && !string.IsNullOrEmpty(userIdClaim.Value))
            return $"USER_{userIdClaim.Value}";

        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "UNKNOWN";
        return $"IP_{ipAddress}";
    }

    /// <summary>
    /// Verifica e registra a requisição usando ConcurrentQueue (thread-safe sem locks).
    /// Retorna false se o limite foi atingido.
    /// </summary>
    private bool IsRequestAllowed(string identifier)
    {
        var now = DateTime.UtcNow;
        var oneMinuteAgo = now.AddMinutes(-1).Ticks;
        var nowTicks = now.Ticks;

        var queue = RequestLog.GetOrAdd(identifier, _ => new ConcurrentQueue<long>());

        // Enfileirar a nova requisição
        queue.Enqueue(nowTicks);

        // Contar quantas requisições estão dentro da janela de 1 minuto
        // (descartar as expiradas via TryDequeue para manter a fila enxuta)
        while (queue.TryPeek(out var oldest) && oldest < oneMinuteAgo)
        {
            queue.TryDequeue(out _);
        }

        var count = queue.Count;

        if (count > MaxRequestsPerMinute)
        {
            _logger.LogWarning("Rate limit excedido para {Identifier}: {Count} req/min", identifier, count);
            return false;
        }

        return true;
    }

    /// <summary>Remove entradas completamente vazias ou antigas para evitar vazamento de memória</summary>
    private static void CleanupOldRequests()
    {
        var oneHourAgo = DateTime.UtcNow.AddHours(-1).Ticks;

        foreach (var kvp in RequestLog)
        {
            var queue = kvp.Value;

            // Remover timestamps mais antigos que 1 hora
            while (queue.TryPeek(out var oldest) && oldest < oneHourAgo)
                queue.TryDequeue(out _);

            // Remover a entrada do dicionário se a fila estiver vazia
            if (queue.IsEmpty)
                RequestLog.TryRemove(kvp.Key, out _);
        }
    }
}

/// <summary>Extensão para adicionar rate limiting ao pipeline</summary>
public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}
