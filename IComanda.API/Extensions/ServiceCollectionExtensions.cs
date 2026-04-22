using IComanda.API.Data;
using IComanda.API.Repositories.Implementations;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Services;
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
        services.AddScoped<IItemVendaTemporarioRepository, ItemVendaTemporarioRepository>();
        services.AddScoped<IComandaRepository, ComandaRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IEmitenteRepository, EmitenteRepository>();
        services.AddScoped<IFormaPagamentoRepository, FormaPagamentoRepository>();
        services.AddScoped<IRecebimentoRepository, RecebimentoRepository>();
        services.AddScoped<IReceberRepository, ReceberRepository>();
        services.AddScoped<ICaixaRepository, CaixaRepository>();
        services.AddScoped<ICaixaMovimentoRepository, CaixaMovimentoRepository>();
        services.AddScoped<IRelatorioRepository, RelatorioRepository>();
        services.AddScoped<IMesaRepository, MesaRepository>();
        services.AddScoped<IHistoricoRepository, HistoricoRepository>();
        services.AddScoped<INotificacaoRepository, NotificacaoRepository>();
        services.AddScoped<ITaxaEntregaRepository, TaxaEntregaRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IConfiguracoesRepository, ConfiguracoesRepository>();

        // Força de Vendas
        services.AddScoped<IVendedorRepository, VendedorRepository>();
        services.AddScoped<IPedidoFVRepository, PedidoFVRepository>();
        services.AddScoped<IVisitaFVRepository, VisitaFVRepository>();
        services.AddScoped<IMetaFVRepository, MetaFVRepository>();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IProdutoService, ProdutoService>();
        services.AddScoped<IGrupoService, GrupoService>();
        services.AddScoped<IVendaService, VendaService>();
        services.AddScoped<IComandaService, ComandaService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IImpressaoService, ImpressaoService>();
        services.AddScoped<IRecebimentoService, RecebimentoService>();
        services.AddScoped<IReceberService, ReceberService>();
        services.AddScoped<ICaixaService, CaixaService>();
        services.AddScoped<ICaixaMovimentoService, CaixaMovimentoService>();
        services.AddScoped<IRelatorioService, RelatorioService>();
        services.AddScoped<IMesaService, MesaService>();
        services.AddScoped<IHistoricoService, HistoricoService>();
        services.AddScoped<INotificacaoService, NotificacaoService>();
        services.AddScoped<ITaxaEntregaService, TaxaEntregaService>();
        
        // Força de Vendas
        services.AddScoped<IVendedorService, VendedorService>();
        services.AddScoped<IPedidoFVService, PedidoFVService>();
        services.AddScoped<IRotaFVService, RotaFVService>();
        services.AddScoped<IMetaFVService, MetaFVService>();

        // Security Services
        services.AddSingleton<IPasswordHasher, PasswordHasher>(); // Singleton - thread-safe, sem estado
        
        // CORREÇÃO: Usar RefreshTokenDatabaseService (Firebird) em vez da implementação em memória.
        // O serviço em memória perdia todos os tokens a cada reinício do serviço Windows,
        // forçando todos os usuários a fazer login novamente.
        // Deve ser Scoped pois depende do repositório (também Scoped).
        services.AddScoped<IRefreshTokenService, RefreshTokenDatabaseService>();
        
        // WhatsApp Services - Múltiplos provedores
        services.AddHttpClient(); // Para Evolution API
        services.AddSingleton<WhatsAppService>(); // Serviço Selenium original
        services.AddSingleton<IWhatsAppProvider, Services.Implementations.WhatsAppLinkProvider>();
        services.AddSingleton<IWhatsAppProvider, Services.Implementations.WhatsAppBaileysProvider>();
        services.AddSingleton<IWhatsAppProvider, Services.Implementations.WhatsAppEvolutionApiProvider>();
        services.AddSingleton<IWhatsAppProvider, Services.Implementations.WhatsAppSeleniumProvider>();
        // WhatsApp Service principal (multi-provider)
        services.AddSingleton<IWhatsAppService, Services.Implementations.WhatsAppMultiProviderService>();
        // services.AddScoped<IAuthService, AuthService>(); // TEMPORARIAMENTE DESABILITADO

        return services;
    }

    public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<ProdutoMappingProfile>();
            cfg.AddProfile<ClienteMappingProfile>();
            cfg.AddProfile<ForcaVendasMappingProfile>();
        });
        return services;
    }
}
