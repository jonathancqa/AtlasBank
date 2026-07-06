using AtlasBank.Accounts.Domain.Entities;

namespace AtlasBank.Accounts.Application.Abstractions;

/// <summary>
/// Contrato do repositório de contas.
/// A Application define a interface — a Infrastructure implementa.
/// O domínio nunca conhece detalhes de persistência.
/// </summary>
public interface IAccountRepository
{
    /// <summary>Busca uma conta pelo Id.</summary>
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Busca uma conta pelo e-mail.</summary>
    Task<Account?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Busca uma conta pelo CPF.</summary>
    Task<Account?> GetByDocumentAsync(string document, CancellationToken cancellationToken = default);

    /// <summary>Verifica se já existe conta com o e-mail informado.</summary>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Verifica se já existe conta com o CPF informado.</summary>
    Task<bool> ExistsByDocumentAsync(string document, CancellationToken cancellationToken = default);

    /// <summary>Persiste uma nova conta.</summary>
    Task AddAsync(Account account, CancellationToken cancellationToken = default);
}