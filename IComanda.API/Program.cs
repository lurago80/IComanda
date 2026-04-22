using IComanda.API.Data;
using IComanda.API.Repositories;
using IComanda.API.Services;
using IComanda.API.Extensions;
using IComanda.API.HealthChecks;
using IComanda.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
// NOTA: Descomentar se instalar StackExchange.Redis via NuGet
// using StackExchange.Redis;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        var basePath = AppContext.BaseDirectory;
        config.Sources.Clear();
        config.SetBasePath(basePath)
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
              .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true) // configurações do cliente — nunca sobrescrever em atualizações
              .AddEnvironmentVariables();
    })
    .ConfigureLogging((context, logging) =>
    {
        logging.AddConsole();
        logging.AddDebug();

        // Configurar logs para gravar em arquivo
        var logsPath = Path.Combine(AppContext.BaseDirectory, "logs");
        if (!Directory.Exists(logsPath))
        {
            Directory.CreateDirectory(logsPath);
        }

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(logsPath, "icomanda-.txt"),
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        logging.AddSerilog(dispose: true);
    });

// Aplicar UseWindowsService apenas se estiver rodando como serviço Windows
// Verifica se não está em modo interativo (console) ou se foi explicitamente solicitado
var isWindowsService = !Environment.UserInteractive || args.Contains("--windows-service");
if (isWindowsService && OperatingSystem.IsWindows())
{
    builder.UseWindowsService(options =>
    {
        options.ServiceName = "IComandaAPI";
    });
}

builder.ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.ConfigureServices((context, services) =>
        {
            var configuration = context.Configuration;

            // Add services to the container
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.WriteIndented = false;
                });
            services.AddEndpointsApiExplorer();

            // Swagger
            services.AddSwaggerGen(c =>
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

    // Configuração do JWT no Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
            });
            });

            // Database
            services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

            // Inicializador de banco — módulo Força de Vendas
            services.AddScoped<IComanda.API.Services.Implementations.ForcaVendasDbInitializer>();

            // Migrações automáticas de colunas/registros novos
            services.AddScoped<IComanda.API.Services.Implementations.IComandaDbMigrationService>();

            // Repositories e Services
            services.AddRepositories();
            services.AddServices();

            // AutoMapper
            services.AddAutoMapperProfiles();

            // ================================================================
            // CACHE - Phase 3 Performance
            // ================================================================
            // Adicionar Memory Cache (sempre necessário)
            services.AddMemoryCache();

            // Registrar Cache Provider
            // NOTA: Para usar Redis em produção, instale o pacote:
            //   dotnet add package StackExchange.Redis
            // Depois descomente o using StackExchange.Redis no topo deste arquivo
            // e descomentar a seção Redis abaixo

            // Por enquanto, usar apenas MemoryCache (suficiente para maioria dos casos)
            services.AddSingleton<ICacheProvider, MemoryCacheProvider>();
            services.AddScoped<ICacheService, CacheService>();

            // JWT Token Provider
            services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();

            // Configurar autenticação JWT
            var jwtKey = configuration["Jwt:Key"] 
                ?? throw new InvalidOperationException("Jwt:Key não está configurada em appsettings.json. Por favor, adicione a seção 'Jwt' no appsettings.json");
            
            var jwtExpirationHours = int.Parse(configuration["Jwt:ExpirationHours"] ?? "2");
            var jwtRefreshDays = int.Parse(configuration["Jwt:RefreshExpirationDays"] ?? "7");
            
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"] ?? "IComanda.API",
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"] ?? "IComanda.Client",
                    ValidateLifetime = true,
                    // CORREÇÃO: ClockSkew de 5 minutos para tolerar diferenças de relógio entre
                    // servidor e cliente, evitando desconexões inesperadas por expiração prematura.
                    ClockSkew = TimeSpan.FromMinutes(5),
                    RoleClaimType = "Role"
                };
                
                options.Events = new JwtBearerEvents
                {
                    // Ler JWT do cookie httpOnly se não vier no header Authorization
                    OnMessageReceived = context =>
                    {
                        var cookieToken = context.Request.Cookies["jwt_access_token"];
                        if (!string.IsNullOrEmpty(cookieToken))
                            context.Token = cookieToken;
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            context.Response.Headers.Append("X-Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        // Personalizaara resposta quando o token está inválido/expirado
                        context.HandleResponse();
                        context.Response.ContentType = "application/json";
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        
                        var response = new
                        {
                            error = "Unauthorized",
                            message = "Token invalido ou expirado. Por favor, faça login novamente."
                        };
                        
                        return context.Response.WriteAsJsonAsync(response);
                    }
                };
            });

            // Configurar autorização com suporte a roles e policies
            services.AddAuthorization(options =>
            {
                // Policies por role
                options.AddPolicy("AdminOnly", policy => 
                    policy.RequireRole("Admin"));
                
                options.AddPolicy("GerenteOrAdmin", policy => 
                    policy.RequireRole("Admin", "Gerente"));
                
                options.AddPolicy("CaixaOrAbove", policy => 
                    policy.RequireRole("Admin", "Gerente", "Caixa"));
                
                options.AddPolicy("DeliveryAccess", policy => 
                    policy.RequireRole("Admin", "Gerente", "Entregador"));

                options.AddPolicy("ForcaVendasAccess", policy =>
                    policy.RequireRole("Admin", "Gerente", "Vendedor"));
            });

            // CORS para React - Permitir localhost e rede local
            services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", policy =>
                {
                    policy.WithOrigins(
                            "http://localhost:3000", 
                            "http://localhost:3001", 
                            "http://localhost:65375", 
                            "https://localhost:65374",
                            "http://192.168.0.22:3000",
                            "http://192.168.0.22:3001",
                            "http://192.168.0.22:65375"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                    
                    // Permitir qualquer IP na rede local 192.168.0.x (desenvolvimento)
                    policy.SetIsOriginAllowed(origin =>
                    {
                        if (string.IsNullOrEmpty(origin)) return false;
                        
                        var uri = new System.Uri(origin);
                        var host = uri.Host;
                        var port = uri.Port;
                        
                        // Localhost
                        if (host == "localhost" || host == "127.0.0.1")
                            return true;
                        
                        // Portas válidas para qualquer rede local
                        var isValidPort = port == 3000 || port == 3001 || port == 65375 || port == -1 || port == 80;

                        // Redes locais comuns (192.168.0.x, 192.168.1.x, 10.0.0.x, 172.16-31.x.x)
                        if (isValidPort)
                        {
                            if (host.StartsWith("192.168.")) return true;
                            if (host.StartsWith("10."))       return true;
                            if (host.StartsWith("172."))
                            {
                                // 172.16.0.0 – 172.31.255.255
                                var parts = host.Split('.');
                                if (parts.Length >= 2 && int.TryParse(parts[1], out var second)
                                    && second >= 16 && second <= 31)
                                    return true;
                            }
                        }
                        
                        return false;
                    });
                });
            });

            // Health Checks
            services.AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>("database");
        });

        webBuilder.Configure((context, app) =>
        {
            var env = context.HostingEnvironment;

            // Configure the HTTP request pipeline
            // Swagger sempre disponível para facilitar o desenvolvimento
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "IComanda API v1");
                c.RoutePrefix = "swagger"; // Swagger em /swagger
                c.DocumentTitle = "IComanda API Documentation";
            });

            // Desabilitado para evitar problemas no desenvolvimento local
            // app.UseHttpsRedirection();

            // CORREÇÃO: ExceptionMiddleware deve ser o PRIMEIRO middleware para capturar
            // exceções de qualquer etapa do pipeline (inclusive autenticação).
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseCors("AllowReactApp");

            // Middleware de rate limiting (aplicado antes da autenticação)
            app.UseRateLimiting();

            // Servir arquivos estáticos do frontend React (wwwroot/)
            app.UseDefaultFiles();  // reescreve / → /index.html
            app.UseStaticFiles();   // serve arquivos de wwwroot/

            // Routing
            app.UseRouting();

            // Autenticação (deve vir antes de Authorization)
            app.UseAuthentication();

            // Autorização (deve vir após Routing e antes de Endpoints)
            app.UseAuthorization();

            // Endpoints
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
                // Fallback para React Router (SPA) — todas as rotas não-API vão para index.html
                endpoints.MapFallbackToFile("index.html");
            });
        });

        // Configurar Kestrel para escutar na porta configurada
        webBuilder.ConfigureKestrel((context, options) =>
        {
            var port = context.Configuration.GetValue<int>("Kestrel:Port", 65375);

            // Escutar em todas as interfaces para permitir acesso de celulares e outros
            // dispositivos na rede local (não apenas localhost)
            options.ListenAnyIP(port);
        });
    });

var host = builder.Build();

try
{
    Log.Information("Iniciando IComanda API");
    Log.Information("Ambiente: {Mode}", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production");

    var config = host.Services.GetRequiredService<IConfiguration>();
    var port = config.GetValue<int>("Kestrel:Port", 65375);
    Log.Information("Porta: {Port}", port);

    // Verificar/criar tabelas necessárias na inicialização
    try
    {
        using (var scope = host.Services.CreateScope())
        {
            var historicoRepo = scope.ServiceProvider.GetService<IComanda.API.Repositories.Interfaces.IHistoricoRepository>();
            if (historicoRepo != null)
                await historicoRepo.EnsureTableExistsAsync();
        }
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Não foi possível verificar/criar tabela HISTORICO_ALTERACOES na inicialização.");
    }

    try
    {
        using (var scope = host.Services.CreateScope())
        {
            var refreshTokenRepo = scope.ServiceProvider.GetService<IComanda.API.Repositories.Interfaces.IRefreshTokenRepository>();
            if (refreshTokenRepo != null)
                await refreshTokenRepo.EnsureTableExistsAsync();
            else
                Log.Warning("IRefreshTokenRepository não registrado — verificação de REFRESH_TOKEN ignorada.");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Falha ao verificar/criar tabela REFRESH_TOKEN. Execute o script criar-tabela-refresh-token.sql manualmente.");
    }

    try
    {
        using (var scope = host.Services.CreateScope())
        {
            var fvInit = scope.ServiceProvider.GetRequiredService<IComanda.API.Services.Implementations.ForcaVendasDbInitializer>();
            await fvInit.EnsureTablesAsync();
        }
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "[FV] Não foi possível verificar/criar tabelas Força de Vendas na inicialização.");
    }

    try
    {
        using (var scope = host.Services.CreateScope())
        {
            var dbMigration = scope.ServiceProvider.GetRequiredService<IComanda.API.Services.Implementations.IComandaDbMigrationService>();
            await dbMigration.EnsureMigrationsAsync();
        }
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "[Migration] Não foi possível aplicar migrações automáticas na inicialização.");
    }

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação encerrada inesperadamente");
    throw;
}
finally
{
    Log.Information("Encerrando IComanda API");
    Log.CloseAndFlush();
}
