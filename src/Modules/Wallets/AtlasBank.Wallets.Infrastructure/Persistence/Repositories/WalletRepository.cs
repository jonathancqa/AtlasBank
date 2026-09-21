using AtlasBank.Wallets.Application.Abstractions;
using AtlasBank.Wallets.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtlasBank.Wallets.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação do repositório de carteiras usando Entity Framework Core.
/// Métodos de leitura usam AsNoTracking — sem rastreamento, mais performático.
/// Métodos de escrita usam tracking — EF Core detecta mudanças automaticamente.
/// </summary>
public sealed class WalletRepository : IWalletRepository
{
    private readonly WalletsDbContext _context;

    public WalletRepository(WalletsDbContext context)
        => _context = context;

    /// <summary>Busca para modificação — com tracking.</summary>
    public async Task<Wallet?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => await _context.Wallets
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    /// <summary>Busca para leitura pura — sem tracking, mais performático.</summary>
    public async Task<Wallet?> GetByIdReadOnlyAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => await _context.Wallets
            .AsNoTracking()
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public async Task<Wallet?> GetByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
        => await _context.Wallets
            .AsNoTracking()
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.AccountId == accountId, cancellationToken);

    public async Task<bool> ExistsByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
        => await _context.Wallets
            .AsNoTracking()
            .AnyAsync(w => w.AccountId == accountId, cancellationToken);

    public async Task<bool> ExistsByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default)
        => await _context.Transactions
            .AsNoTracking()
            .AnyAsync(t => t.IdempotencyKey.Value == idempotencyKey, cancellationToken);

    public async Task AddAsync(
        Wallet wallet,
        CancellationToken cancellationToken = default)
        => await _context.Wallets.AddAsync(wallet, cancellationToken);

    public async Task UpdateAsync(
        Wallet wallet,
        CancellationToken cancellationToken = default)
    {
        foreach (var transaction in wallet.Transactions)
        {
            if (_context.Entry(transaction).State == EntityState.Detached)
                _context.Transactions.Add(transaction);
        }
    }
}