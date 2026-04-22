using System.Net;
using System.Text.Json;

namespace IComanda.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // CORREÇÃO: log com stack trace completo para facilitar diagnóstico de quedas
            _logger.LogError(ex,
                "Erro não tratado. Método={Method} Path={Path} IP={IP} StatusCode={StatusCode}",
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress,
                context.Response.StatusCode);

            await HandleExceptionAsync(context, ex, _env.IsDevelopment());
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception, bool isDevelopment)
    {
        // Não sobrescrever se a resposta já foi iniciada
        if (context.Response.HasStarted)
            return;

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        object response;

        if (isDevelopment)
        {
            // Em desenvolvimento, expor detalhes completos para facilitar debug
            response = new
            {
                error = new
                {
                    message = "Erro interno do servidor",
                    details = exception.Message,
                    stackTrace = exception.StackTrace,
                    innerException = exception.InnerException?.Message,
                    timestamp = DateTime.UtcNow
                }
            };
        }
        else
        {
            // Em produção, não expor detalhes internos
            response = new
            {
                error = new
                {
                    message = "Erro interno do servidor. Contate o administrador.",
                    timestamp = DateTime.UtcNow
                }
            };
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
