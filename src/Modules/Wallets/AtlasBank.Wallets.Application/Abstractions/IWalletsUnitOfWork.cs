namespace AtlasBank.Wallets.Application.Abstractions;

/// <summary>
/// Unit of Work específico do módulo Wallets.
/// Garante que operações de carteira sejam commitadas no WalletsDbContext correto.
/// </summary>
public interface IWalletsUnitOfWork
{
    /// <summary>Persiste todas as mudanças pendentes do módulo Wallets.</summary>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}