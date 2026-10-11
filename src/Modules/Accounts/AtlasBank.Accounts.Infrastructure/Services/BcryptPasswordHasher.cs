using AtlasBank.Accounts.Application.Abstractions.Services;

namespace AtlasBank.Accounts.Infrastructure.Services;

/// <summary>
/// Implementação do hasher de senhas usando BCrypt.
/// </summary>
public sealed class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);
}