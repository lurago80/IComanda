using IComanda.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace IComanda.API.Extensions;

/// <summary>
/// Atributo customizado para autorizacao por múltiplos roles
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeRolesAttribute : AuthorizeAttribute
{
    public AuthorizeRolesAttribute(params UserRole[] roles)
    {
        if (roles.Length == 0)
        {
            Roles = null;
            return;
        }

        Roles = string.Join(",", roles.Select(r => r.ToString()));
    }
}

/// <summary>
/// Atributo customizado para autorização por permissão específica
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizePermissionAttribute : AuthorizeAttribute
{
    public UserPermission RequiredPermission { get; set; }

    public AuthorizePermissionAttribute(UserPermission permission)
    {
        RequiredPermission = permission;
        // Sempre requer autenticação
        AuthenticationSchemes = "Bearer";
    }
}

/// <summary>
/// Atributo para indicar que o endpoint requer múltiplas permissões (AND)
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizePermissionsAttribute : AuthorizeAttribute
{
    public UserPermission[] RequiredPermissions { get; set; }

    public AuthorizePermissionsAttribute(params UserPermission[] permissions)
    {
        RequiredPermissions = permissions;
        // Sempre requer autenticação
        AuthenticationSchemes = "Bearer";
    }
}

/// <summary>
/// Policy-based authorization handler para validar permissões
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var permissionClaim = context.User.FindFirst("Permission");
        
        if (permissionClaim != null && Enum.TryParse<UserPermission>(permissionClaim.Value, out var userPermission))
        {
            if (userPermission == requirement.Permission)
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Requirement para validação de permissão via policies
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public UserPermission Permission { get; set; }

    public PermissionRequirement(UserPermission permission)
    {
        Permission = permission;
    }
}
