using IComanda.API.Data;
using IComanda.API.Repositories;
using IComanda.API.Services;
using IComanda.API.Extensions;
using IComanda.API.HealthChecks;
using IComanda.API.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/icomanda-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "IComanda API",
        Version = "v1",
        Description = "API para sistema de comandas e vendas - Modernização do sistema legado Delphi",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Desenvolvedor",
            Email = "dev@icomanda.com"
        }
    });
});

// Database
builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

// Repositories e Services
builder.Services.AddRepositories();
builder.Services.AddServices();

// AutoMapper
builder.Services.AddAutoMapperProfiles();

// CORS para React
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

var app = builder.Build();

// Configure the HTTP request pipeline
// Swagger sempre disponível para facilitar o desenvolvimento
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "IComanda API v1");
    c.RoutePrefix = string.Empty; // Swagger na raiz
    c.DocumentTitle = "IComanda API Documentation";
});

app.UseHttpsRedirection();

app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapControllers();

// Health Check endpoint
app.MapHealthChecks("/health");

// Middleware de tratamento de exceções
app.UseMiddleware<ExceptionMiddleware>();

try
{
    Log.Information("Iniciando IComanda API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação encerrada inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
