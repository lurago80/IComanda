using IComanda.API.Models;
using IComanda.API.Models.Requests;
using IComanda.API.Models.DTOs;
using IComanda.API.Services;
using IComanda.API.Services.Interfaces;
using IComanda.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace IComanda.API.Controllers;

/// <summary>
/// Controlador de autenticação - Gera tokens JWT
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IJwtTokenProvider _jwtTokenProvider;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    private readonly IWebHostEnvironment _env;

    public AuthController(
        IUsuarioRepository usuarioRepository,
        IJwtTokenProvider jwtTokenProvider,
        IRefreshTokenService refreshTokenService,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        ILogger<AuthController> logger,
        IWebHostEnvironment env)
    {
        _usuarioRepository = usuarioRepository;
        _jwtTokenProvider = jwtTokenProvider;
        _refreshTokenService = refreshTokenService;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _logger = logger;
        _env = env;
    }

    /// <summary>
    /// Realiza login e retorna um token JWT
    /// </summary>
    /// <param name="request">Credenciais de login (username e password)</param>
    /// <returns>Token JWT e informações do usuário</returns>
    /// <response code="200">Login bem-sucedido, token retornado</response>
    /// <response code="401">Credenciais inválidas</response>
    /// <response code="400">Requisição inválida</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] IComanda.API.Models.Requests.LoginRequest request)
    {
        try
        {
            // Validação básica
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Tentativa de login com credenciais vazias");
                return BadRequest(new { error = "Username e password são obrigatórios" });
            }

            // Buscar usuário no banco de dados por nome
            var usuario = await _usuarioRepository.BuscarPorNomeAsync(request.Username);

            if (usuario == null)
            {
                _logger.LogWarning("Usuário não encontrado: {Username}", request.Username);
                return Unauthorized(new { error = "Usuário ou senha inválidos" });
            }

            // Validar se o usuário está ativo
            if (usuario.Ativo?.Equals("1", StringComparison.OrdinalIgnoreCase) != true)
            {
                _logger.LogWarning("Usuário inativo tentando fazer login: {UsuarioId}", usuario.Id);
                return Unauthorized(new { error = "Usuário inativo" });
            }

            // Validar se o usuário está bloqueado
            if (usuario.Bloqueio?.Equals("1", StringComparison.OrdinalIgnoreCase) == true)
            {
                _logger.LogWarning("Usuário bloqueado tentando fazer login: {UsuarioId}", usuario.Id);
                return Unauthorized(new { error = "Usuário bloqueado" });
            }

            // Validar senha (BCrypt ou plaintext)
            bool senhaValida = false;

            if (string.IsNullOrWhiteSpace(usuario.Senha))
            {
                _logger.LogWarning("Usuário sem senha definida: {UsuarioId}", usuario.Id);
                return Unauthorized(new { error = "Usuário ou senha inválidos" });
            }

            // Verificar se a senha está em hash BCrypt ou plaintext
            if (_passwordHasher.IsPasswordHashed(usuario.Senha))
            {
                senhaValida = _passwordHasher.VerifyPassword(request.Password, usuario.Senha);
            }
            else
            {
                // Comparação em tempo constante para evitar timing attack
                var senhaBytes = Encoding.UTF8.GetBytes(usuario.Senha);
                var inputBytes = Encoding.UTF8.GetBytes(request.Password);
                senhaValida = senhaBytes.Length == inputBytes.Length &&
                              CryptographicOperations.FixedTimeEquals(senhaBytes, inputBytes);
            }

            if (!senhaValida)
            {
                _logger.LogWarning("Senha inválida para usuário: {UsuarioId}", usuario.Id);
                return Unauthorized(new { error = "Usuário ou senha inválidos" });
            }

            // Determinar o role do usuário baseado nos campos CANCELAR e VISUALIZAR
            var role = DetermineUserRole(usuario);

            // Gerar token JWT
            var token = _jwtTokenProvider.GenerateToken(
                userId: usuario.Id,
                username: usuario.Nome ?? "",
                role: role
            );

            // Gerar refresh token
            var refreshToken = _refreshTokenService.GenerateRefreshToken(
                userId: usuario.Id,
                username: usuario.Nome ?? "",
                role: role
            );

            var expirationHours = int.Parse(_configuration["Jwt:ExpirationHours"] ?? "2");

            // Gravar token em cookie httpOnly para segurança (não acessível via JS)
            Response.Cookies.Append("jwt_access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = _env.IsProduction(), // true em produção (HTTPS), false em desenvolvimento
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(expirationHours)
            });

            _logger.LogInformation("Usuário autenticado com sucesso: {UsuarioId}", usuario.Id);

            return Ok(new LoginResponse
            {
                Id = usuario.Id,
                Nome = usuario.Nome ?? "",
                Token = token,
                RefreshToken = refreshToken.Token,
                Tipo = usuario.Tipo ?? "",
                PodeVisualizar = usuario.Visualizar?.Equals("1") ?? false,
                PodeVerTotal = usuario.Total?.Equals("1") ?? false,
                PodeCancelar = usuario.Cancelar?.Equals("1") ?? false,
                ExpiresIn = expirationHours,
                UserId = usuario.Id,
                Username = usuario.Nome ?? "",
                Role = role.ToString(),
                TokenType = "Bearer"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar login");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "Erro ao processar login" });
        }
    }

    /// <summary>
    /// Renova um token JWT usando um refresh token
    /// </summary>
    /// <param name="request">Token expirado e refresh token</param>
    /// <returns>Novo token JWT e refresh token</returns>
    /// <response code="200">Tokens renovados com sucesso</response>
    /// <response code="401">Refresh token inválido ou expirado</response>
    /// <response code="400">Requisição inválida</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new { error = "Refresh token é obrigatório" });
            }

            // Validar refresh token
            var storedRefreshToken = _refreshTokenService.ValidateRefreshToken(request.RefreshToken);
            if (storedRefreshToken == null)
            {
                _logger.LogWarning("Tentativa de refresh com token inválido");
                return Unauthorized(new { error = "Refresh token inválido ou expirado" });
            }

            // Revogar o refresh token antigo (one-time use)
            _refreshTokenService.RevokeRefreshToken(request.RefreshToken, "Usado para refresh");

            // Gerar novo token JWT
            var newToken = _jwtTokenProvider.GenerateToken(
                userId: storedRefreshToken.UserId,
                username: storedRefreshToken.Username,
                role: storedRefreshToken.Role
            );

            // Gerar novo refresh token
            var newRefreshToken = _refreshTokenService.GenerateRefreshToken(
                userId: storedRefreshToken.UserId,
                username: storedRefreshToken.Username,
                role: storedRefreshToken.Role
            );

            var expirationHours = int.Parse(_configuration["Jwt:ExpirationHours"] ?? "2");
            var refreshExpirationDays = int.Parse(_configuration["Jwt:RefreshExpirationDays"] ?? "7");

            // Renovar cookie httpOnly com o novo token
            Response.Cookies.Append("jwt_access_token", newToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = _env.IsProduction(), // true em produção (HTTPS), false em desenvolvimento
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(expirationHours)
            });

            _logger.LogInformation("Token renovado para usuário: {UserId}", storedRefreshToken.UserId);

            return Ok(new RefreshTokenResponse
            {
                Token = newToken,
                RefreshToken = newRefreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddHours(expirationHours),
                RefreshExpiresAt = DateTime.UtcNow.AddDays(refreshExpirationDays)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao renovar token");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Erro ao renovar token" });
        }
    }

    /// <summary>
    /// Revoga um refresh token (logout)
    /// </summary>
    /// <param name="refreshToken">Refresh token para revogar</param>
    /// <response code="200">Token revogado com sucesso</response>
    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult RevokeToken([FromBody] string refreshToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest(new { error = "Refresh token é obrigatório" });
            }

            _refreshTokenService.RevokeRefreshToken(refreshToken, "Logout do usuário");

            return Ok(new { message = "Token revogado com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao revogar token");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Erro ao revogar token" });
        }
    }

    /// <summary>
    /// Valida se um token JWT é válido
    /// </summary>
    /// <param name="token">Token JWT para validar</param>
    /// <returns>Informações sobre o token se válido</returns>
    /// <response code="200">Token válido</response>
    /// <response code="401">Token inválido ou expirado</response>
    [HttpPost("validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult ValidateToken([FromBody] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new { error = "Token é obrigatório" });
        }

        var principal = _jwtTokenProvider.ValidateToken(token);
        if (principal == null)
        {
            return Unauthorized(new { error = "Token inválido ou expirado" });
        }

        var userId = _jwtTokenProvider.GetUserIdFromToken(token);
        var username = _jwtTokenProvider.GetUsernameFromToken(token);
        var role = _jwtTokenProvider.GetRoleFromToken(token);

        return Ok(new
        {
            valid = true,
            userId,
            username,
            role = role?.ToString()
        });
    }

    /// <summary>
    /// Realiza logout removendo o cookie de autenticação
    /// </summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("jwt_access_token");
        _logger.LogInformation("Logout realizado");
        return Ok(new { message = "Logout realizado com sucesso" });
    }

    /// <summary>
    /// Retorna o perfil do usuário autenticado (role + permissões)
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        var userId = User.FindFirst("UserId")?.Value;
        var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        var role = User.FindFirst("Role")?.Value;
        var permissions = User.FindAll("Permission").Select(c => c.Value).ToList();

        return Ok(new
        {
            userId = userId != null ? int.Parse(userId) : 0,
            username,
            role,
            permissions
        });
    }

    /// <summary>
    /// Determina o role de um usuário baseado nos campos CANCELAR e VISUALIZAR
    /// </summary>
    private UserRole DetermineUserRole(Models.Entities.Usuario usuario)
    {
        // Lógica de determinação de role baseada nos campos do usuário
        // Campos: Cancelar (bool) e Visualizar (bool)
        
        var podeCancel = usuario.Cancelar?.Equals("1", StringComparison.OrdinalIgnoreCase) ?? false;
        var podeVisualizar = usuario.Visualizar?.Equals("1", StringComparison.OrdinalIgnoreCase) ?? false;
        
        // Se pode cancelar = Admin ou Gerente
        if (podeCancel)
        {
            return UserRole.Gerente; // Pode cancelar vendas
        }
        
        // Se pode visualizar = Caixa ou Garcom
        if (podeVisualizar)
        {
            return UserRole.Caixa; // Acesso a recebimentos
        }
        
        // Padrão é Garçom (sem permissões especiais)
        return UserRole.Garcom;
    }
}
