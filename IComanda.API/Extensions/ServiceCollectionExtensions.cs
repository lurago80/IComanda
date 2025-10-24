using IComanda.API.Data;
using IComanda.API.Repositories.Implementations;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Services.Implementations;
using IComanda.API.Services.Interfaces;
using IComanda.API.Mappings;

namespace IComanda.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<IGrupoRepository, GrupoRepository>();
        services.AddScoped<IVendaRepository, VendaRepository>();
        services.AddScoped<IComandaRepository, ComandaRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IProdutoService, ProdutoService>();
        services.AddScoped<IGrupoService, GrupoService>();
        services.AddScoped<IVendaService, VendaService>();
        services.AddScoped<IComandaService, ComandaService>();
        services.AddScoped<IClienteService, ClienteService>();

        return services;
    }

    public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(ProdutoMappingProfile));
        return services;
    }
}
