namespace IComanda.API.Services.Interfaces;

/// <summary>
/// Serviço para hashing seguro de senhas usando BCrypt
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Gera um hash BCrypt da senha fornecida
    /// </summary>
    /// <param name="password">Senha em texto plano</param>
    /// <returns>Hash BCrypt da senha</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifica se a senha fornecida corresponde ao hash
    /// </summary>
    /// <param name="password">Senha em texto plano</param>
    /// <param name="hashedPassword">Hash BCrypt armazenado</param>
    /// <returns>True se a senha corresponder ao hash, False caso contrário</returns>
    bool VerifyPassword(string password, string hashedPassword);

    /// <summary>
    /// Verifica se a string parece ser um hash BCrypt válido
    /// (usado para migração de senhas em plaintext para hashed)
    /// </summary>
    /// <param name="password">String para verificar</param>
    /// <returns>True se parece ser um hash BCrypt válido</returns>
    bool IsPasswordHashed(string password);
}
