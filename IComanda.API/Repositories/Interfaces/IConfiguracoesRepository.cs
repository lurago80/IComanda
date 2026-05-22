namespace IComanda.API.Repositories.Interfaces;

/// <summary>
/// Repositório para leitura e escrita de configurações do sistema na tabela PARAMETROS.
/// </summary>
public interface IConfiguracoesRepository
{
    /// <summary>Retorna true se o módulo Delivery está habilitado.</summary>
    Task<bool> GetUsarDeliveryAsync();

    /// <summary>Grava o flag USAR_DELIVERY na tabela PARAMETROS.</summary>
    Task SetUsarDeliveryAsync(bool usarDelivery);

    /// <summary>Retorna true se o módulo Força de Vendas está habilitado.</summary>
    Task<bool> GetUsarForcaVendasAsync();

    /// <summary>Grava o flag USAR_FORCA_VENDAS na tabela PARAMETROS.</summary>
    Task SetUsarForcaVendasAsync(bool usarForcaVendas);

    /// <summary>Retorna true se o módulo Comanda está habilitado.</summary>
    Task<bool> GetUsarComandaAsync();

    /// <summary>Grava o flag USAR_COMANDA na tabela PARAMETROS.</summary>
    Task SetUsarComandaAsync(bool usarComanda);

    /// <summary>Retorna true se a funcionalidade de impressão de 2 vias está habilitada.</summary>
    Task<bool> GetHabilitarImprimirDuasViasAsync();

    /// <summary>Grava o flag HABILITAR_IMPRIMIR_2VIAS na tabela PARAMETROS.</summary>
    Task SetHabilitarImprimirDuasViasAsync(bool habilitar);

    /// <summary>Retorna true se o módulo Cozinha (KDS) está habilitado.</summary>
    Task<bool> GetUsarCozinhaAsync();

    /// <summary>Grava o flag USAR_COZINHA na tabela PARAMETROS.</summary>
    Task SetUsarCozinhaAsync(bool usarCozinha);

    /// <summary>Retorna true se o módulo Cardápio (QR Code) está habilitado.</summary>
    Task<bool> GetUsarCardapioAsync();

    /// <summary>Grava o flag USAR_CARDAPIO na tabela PARAMETROS.</summary>
    Task SetUsarCardapioAsync(bool usarCardapio);
}
