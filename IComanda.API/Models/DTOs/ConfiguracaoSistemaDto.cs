namespace IComanda.API.Models.Dtos;

/// <summary>
/// Configurações gerais do sistema (armazenadas na tabela PARAMETROS do Firebird).
/// </summary>
public class ConfiguracaoSistemaDto
{
    /// <summary>
    /// Indica se o módulo de Delivery está habilitado.
    /// Quando false, os botões de Delivery são ocultados no menu principal.
    /// </summary>
    public bool UsarDelivery { get; set; } = true;

    /// <summary>
    /// Indica se o módulo de Força de Vendas está habilitado.
    /// Quando false, os botões de Força de Vendas são ocultados no sistema.
    /// </summary>
    public bool UsarForcaVendas { get; set; } = true;

    /// <summary>
    /// Indica se o módulo de Comanda está habilitado.
    /// Quando false, os botões de Comanda são ocultados no menu principal.
    /// </summary>
    public bool UsarComanda { get; set; } = true;

    /// <summary>
    /// Habilita a funcionalidade de impressão automática de 2 vias.
    /// Quando true, grupos com "Imprimir 2 vias" ativado imprimem automaticamente 2 cópias.
    /// </summary>
    public bool HabilitarImprimirDuasVias { get; set; } = false;

    /// <summary>
    /// Indica se o módulo Cozinha (KDS e Gerenciar Pizzas) está habilitado.
    /// Quando false, os botões de Cozinha são ocultados no menu principal.
    /// </summary>
    public bool UsarCozinha { get; set; } = true;

    /// <summary>
    /// Indica se o módulo Cardápio (QR Code das Mesas) está habilitado.
    /// Quando false, o botão de QR Code das Mesas é ocultado no menu principal.
    /// </summary>
    public bool UsarCardapio { get; set; } = true;
}
