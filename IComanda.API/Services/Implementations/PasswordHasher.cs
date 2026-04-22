using IComanda.API.Services.Interfaces;
using BCrypt.Net;

namespace IComanda.API.Services.Implementations;

/// <summary>
/// Implementação do serviço de hashing de senhas usando BCrypt
/// 
/// BCrypt é um algoritmo adaptativo que automaticamente aumenta a complexidade
/// ao longo do tempo para se proteger contra ataques de força bruta.
/// 
/// Work Factor: Usamos 12 (padrão seguro em 2026)
/// - Quanto maior o work factor, mais lento o hash (exponencialmente)
/// - Work Factor 12 = ~0.3s por hash (seguro contra GPU cracking)
/// - Aumentar para 13+ em servidores mais potentes
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private readonly ILogger<PasswordHasher> _logger;
    private const int WorkFactor = 12; // Padrão seguro para 2026

    public PasswordHasher(ILogger<PasswordHasher> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gera um hash BCrypt da senha fornecida
    /// </summary>
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Senha não pode ser vazia", nameof(password));
        }

        try
        {
            // BCrypt.HashPassword gera automaticamente um salt aleatório
            // e o inclui no hash final (formato: $2a$12$[salt][hash])
            var hash = BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
            
            _logger.LogDebug("Password hashed successfully");
            
            return hash;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar hash de senha");
            throw new InvalidOperationException("Erro ao processar senha", ex);
        }
    }

    /// <summary>
    /// Verifica se a senha fornecida corresponde ao hash
    /// </summary>
    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Tentativa de verificação com senha vazia");
            return false;
        }

        if (string.IsNullOrWhiteSpace(hashedPassword))
        {
            _logger.LogWarning("Hash de senha vazio fornecido");
            return false;
        }

        try
        {
            // BCrypt.Verify extrai o salt do hash e compara de forma segura
            // (timing-attack resistant)
            var isValid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            
            if (!isValid)
            {
                _logger.LogDebug("Password verification failed");
            }
            
            return isValid;
        }
        catch (Exception ex)
        {
            // Hash inválido ou corrompido
            _logger.LogWarning(ex, "Erro ao verificar senha - hash pode estar corrompido");
            return false;
        }
    }

    /// <summary>
    /// Verifica se a string parece ser um hash BCrypt válido
    /// 
    /// Formato BCrypt: $2a$12$[22 chars salt][31 chars hash]
    /// Total: 60 caracteres
    /// </summary>
    public bool IsPasswordHashed(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        // BCrypt hash sempre começa com $2a$, $2b$, $2x$ ou $2y$
        // e tem exatamente 60 caracteres
        return password.StartsWith("$2") && 
               password.Length == 60 && 
               password.Split('$').Length == 4;
    }
}
