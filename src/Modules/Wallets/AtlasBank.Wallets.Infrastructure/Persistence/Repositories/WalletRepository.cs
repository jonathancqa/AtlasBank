using AtlasBank.Wallets.Application.Abstractions;
using AtlasBank.Wallets.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtlasBank.Wallets.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação do repositório de carteiras usando Entity Framework Core.
/// </summary>
public sealed class WalletRepository : IWalletRepository
{
    private readonly WalletsDbContext _context;

    public WalletRepository(WalletsDbContext context)
        => _context = context;

    public async Task<Wallet?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => await _context.Wallets
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public async Task<Wallet?> GetByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
        => await _context.Wallets
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.AccountId == accountId, cancellationToken);

    public async Task<bool> ExistsByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
        => await _context.Wallets
            .AnyAsync(w => w.AccountId == accountId, cancellationToken);

    public async Task<bool> ExistsByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default)
        => await _context.Transactions
            .AnyAsync(t => t.IdempotencyKey.Value == idempotencyKey, cancellationToken);

    public async Task AddAsync(
        Wallet wallet,
        CancellationToken cancellationToken = default)
    {
        await _context.Wallets.AddAsync(wallet, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Wallet wallet,
        CancellationToken cancellationToken = default)
    {
        _context.Entry(wallet).State = EntityState.Modified;

        foreach (var transaction in wallet.Transactions)
        {
            var entry = _context.Entry(transaction);
            if (entry.State == EntityState.Detached)
            {
                _context.Transactions.Add(transaction);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}