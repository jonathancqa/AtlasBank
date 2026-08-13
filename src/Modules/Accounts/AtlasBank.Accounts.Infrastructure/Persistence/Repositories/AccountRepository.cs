using AtlasBank.Accounts.Application.Abstractions;
using AtlasBank.Accounts.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtlasBank.Accounts.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação do repositório de contas usando Entity Framework Core.
/// </summary>
public sealed class AccountRepository : IAccountRepository
{
    private readonly AccountsDbContext _context;

    public AccountRepository(AccountsDbContext context)
        => _context = context;

    public async Task<Account?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<Account?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
        => await _context.Accounts
            .FirstOrDefaultAsync(a => a.Email.Address == email, cancellationToken);

    public async Task<Account?> GetByDocumentAsync(
        string document,
        CancellationToken cancellationToken = default)
        => await _context.Accounts
            .FirstOrDefaultAsync(a => a.Document.Number == document, cancellationToken);

    public async Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
        => await _context.Accounts
            .AnyAsync(a => a.Email.Address == email, cancellationToken);

    public async Task<bool> ExistsByDocumentAsync(
        string document,
        CancellationToken cancellationToken = default)
        => await _context.Accounts
            .AnyAsync(a => a.Document.Number == document, cancellationToken);

    public async Task AddAsync(
        Account account,
        CancellationToken cancellationToken = default)
        => await _context.Accounts.AddAsync(account, cancellationToken);
}