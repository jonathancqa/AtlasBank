using AtlasBank.SharedKernel.Abstractions;
using AtlasBank.Accounts.Infrastructure.Persistence;

namespace AtlasBank.Accounts.Infrastructure.Persistence;

/// <summary>
/// Implementação do Unit of Work para o módulo Accounts.
/// Centraliza o SaveChanges garantindo atomicidade entre operações.
/// </summary>
public sealed class AccountsUnitOfWork : IUnitOfWork
{
    private readonly AccountsDbContext _context;

    public AccountsUnitOfWork(AccountsDbContext context)
        => _context = context;

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}