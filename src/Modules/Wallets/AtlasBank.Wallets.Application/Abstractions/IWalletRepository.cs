using AtlasBank.Wallets.Domain.Entities;

namespace AtlasBank.Wallets.Application.Abstractions;

/// <summary>
/// Contrato do repositório de carteiras.
/// </summary>
public interface IWalletRepository
{
    /// <summary>Busca uma carteira pelo Id.</summary>
    Task<Wallet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Busca uma carteira pelo Id da conta.</summary>
    Task<Wallet?> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);

    /// <summary>Verifica se já existe carteira para a conta informada.</summary>
    Task<bool> ExistsByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);

    /// <summary>Verifica se já existe transação com a chave de idempotência informada.</summary>
    Task<bool> ExistsByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);

    /// <summary>Persiste uma nova carteira.</summary>
    Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default);

    /// <summary>Persiste alterações em uma carteira existente.</summary>
    Task UpdateAsync(Wallet wallet, CancellationToken cancellationToken = default);
}