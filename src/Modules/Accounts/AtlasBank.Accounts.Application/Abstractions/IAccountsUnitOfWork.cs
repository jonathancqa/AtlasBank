namespace AtlasBank.Accounts.Application.Abstractions;

/// <summary>
/// Unit of Work específico do módulo Accounts.
/// Garante que operações de conta sejam commitadas no AccountsDbContext correto.
/// </summary>
public interface IAccountsUnitOfWork
{
    /// <summary>Persiste todas as mudanças pendentes do módulo Accounts.</summary>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}