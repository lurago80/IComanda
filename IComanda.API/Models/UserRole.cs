namespace IComanda.API.Models;

/// <summary>
/// Define os papéis (roles) dos usuários do sistema
/// Mapeado com base na tabela USUARIOS.CANCELAR e USUARIOS.OPERADOR
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Administrador - Acesso total ao sistema
    /// </summary>
    Admin = 1,
    
    /// <summary>
    /// Gerente - Acesso a relatórios e fechamento de caixa
    /// </summary>
    Gerente = 2,
    
    /// <summary>
    /// Garçom - Acesso apenas a criar comandas e vendas
    /// </summary>
    Garcom = 3,
    
    /// <summary>
    /// Caixa - Acesso a recebimentos e fechamento de venda
    /// </summary>
    Caixa = 4,
    
    /// <summary>
    /// Entregador - Acesso apenas a pedidos de delivery
    /// </summary>
    Entregador = 5
}

/// <summary>
/// Define as permissões granulares do sistema
/// </summary>
public enum UserPermission
{
    /// <summary>Criar vendas e comandas</summary>
    CreateVendas = 1,
    
    /// <summary>Editar vendas existentes</summary>
    EditVendas = 2,
    
    /// <summary>Cancelar vendas (requer flag CANCELAR na tabela USUARIOS)</summary>
    CancelVendas = 3,
    
    /// <summary>Acessar módulo de recebimentos</summary>
    AccessRecebimentos = 4,
    
    /// <summary>Fechar vendas e caixa</summary>
    CloseSales = 5,
    
    /// <summary>Acessar relatórios</summary>
    AccessReports = 6,
    
    /// <summary>Gerenciar usuários</summary>
    ManageUsers = 7,
    
    /// <summary>Acessar pedidos de delivery</summary>
    AccessDelivery = 8,
    
    /// <summary>Administrar configurações do sistema</summary>
    AdminSettings = 9,
    
    /// <summary>Consultar histórico de operações</summary>
    ViewAudit = 10
}

/// <summary>
/// Mapeamento entre roles e suas permissões padrão
/// </summary>
public static class RolePermissionMapping
{
    public static readonly Dictionary<UserRole, HashSet<UserPermission>> Permissions = new()
    {
        // Admin tem todas as permissões
        {
            UserRole.Admin, new HashSet<UserPermission>
            {
                UserPermission.CreateVendas,
                UserPermission.EditVendas,
                UserPermission.CancelVendas,
                UserPermission.AccessRecebimentos,
                UserPermission.CloseSales,
                UserPermission.AccessReports,
                UserPermission.ManageUsers,
                UserPermission.AccessDelivery,
                UserPermission.AdminSettings,
                UserPermission.ViewAudit
            }
        },
        
        // Gerente pode: criar, editar, fechar, acessar relatórios e audit
        {
            UserRole.Gerente, new HashSet<UserPermission>
            {
                UserPermission.CreateVendas,
                UserPermission.EditVendas,
                UserPermission.CancelVendas,
                UserPermission.AccessRecebimentos,
                UserPermission.CloseSales,
                UserPermission.AccessReports,
                UserPermission.ViewAudit
            }
        },
        
        // Garçom pode: apenas criar vendas
        {
            UserRole.Garcom, new HashSet<UserPermission>
            {
                UserPermission.CreateVendas
            }
        },
        
        // Caixa pode: criar, recebimentos, fechar
        {
            UserRole.Caixa, new HashSet<UserPermission>
            {
                UserPermission.CreateVendas,
                UserPermission.AccessRecebimentos,
                UserPermission.CloseSales
            }
        },
        
        // Entregador pode: acessar delivery
        {
            UserRole.Entregador, new HashSet<UserPermission>
            {
                UserPermission.AccessDelivery,
                UserPermission.ViewAudit
            }
        }
    };
    
    /// <summary>Verifica se um role tem uma permissão específica</summary>
    public static bool HasPermission(UserRole role, UserPermission permission)
    {
        if (Permissions.TryGetValue(role, out var rolePermissions))
        {
            return rolePermissions.Contains(permission);
        }
        return false;
    }
    
    /// <summary>Obtém todas as permissões de um role</summary>
    public static HashSet<UserPermission> GetPermissions(UserRole role)
    {
        if (Permissions.TryGetValue(role, out var permissions))
        {
            return new HashSet<UserPermission>(permissions);
        }
        return new HashSet<UserPermission>();
    }
}
