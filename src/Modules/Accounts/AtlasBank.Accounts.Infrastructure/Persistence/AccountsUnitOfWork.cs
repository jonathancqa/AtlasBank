using AtlasBank.Accounts.Application.Abstractions;
using AtlasBank.Accounts.Infrastructure.Persistence;

namespace AtlasBank.Accounts.Infrastructure.Persistence;

/// <summary>
/// Implementação do Unit of Work para o módulo Accounts.
/// </summary>
public sealed class AccountsUnitOfWork : IAccountsUnitOfWork
{
    private readonly AccountsDbContext _context;

    public AccountsUnitOfWork(AccountsDbContext context)
        => _context = context;

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}