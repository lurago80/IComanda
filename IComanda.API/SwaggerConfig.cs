using Microsoft.OpenApi.Models;

namespace IComanda.API;

public static class SwaggerConfig
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "IComanda API",
                Version = "v1",
                Description = @"
                    API moderna para sistema de comandas e vendas.
                    
                    ## Funcionalidades
                    - ✅ Busca de produtos por múltiplos critérios
                    - ✅ Gerenciamento de carrinho temporário
                    - ✅ Sistema de vendas e comandas
                    - ✅ Integração com sistema legado Delphi
                    
                    ## Tecnologias
                    - .NET 8
                    - Dapper + Firebird 2.5
                    - Swagger/OpenAPI
                    - AutoMapper
                    
                    ## Banco de Dados
                    Utiliza as mesmas tabelas do sistema legado:
                    - PRODUTO, PRODUTOEMPRESA, PRODUTOESERVICO, PRODUTOESERVICOEMPRESA
                    - FRENTE_TMPITVENDAS (carrinho temporário)
                    - VENDAS, ITEVENDAS
                ",
                Contact = new OpenApiContact
                {
                    Name = "Desenvolvedor",
                    Email = "dev@icomanda.com",
                    Url = new Uri("https://github.com/icomanda")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Incluir comentários XML
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            // Configurações adicionais
            c.DescribeAllParametersInCamelCase();
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "IComanda API v1");
            c.RoutePrefix = string.Empty; // Swagger na raiz
            c.DocumentTitle = "IComanda API - Documentação";
            c.DefaultModelsExpandDepth(-1); // Esconder modelos por padrão
            c.DisplayRequestDuration();
            c.EnableDeepLinking();
            c.EnableFilter();
            c.ShowExtensions();
            c.EnableValidator();
        });

        return app;
    }
}
