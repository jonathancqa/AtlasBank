using AtlasBank.SharedKernel.Exceptions;
using AtlasBank.Wallets.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AtlasBank.Wallets.Infrastructure.Persistence;

/// <summary>
/// Implementação do Unit of Work para o módulo Wallets.
/// </summary>
public sealed class WalletsUnitOfWork : IWalletsUnitOfWork
{
    private const string IdempotencyIndexName = "IX_transactions_WalletId_idempotency_key";

    private readonly WalletsDbContext _context;

    public WalletsUnitOfWork(WalletsDbContext context)
        => _context = context;

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException("The wallet was modified by another operation. Please retry.", ex);
        }
        catch (DbUpdateException ex) when (IsIdempotencyKeyViolation(ex))
        {            
            throw new ConcurrencyException("A request with the same idempotency key is already being processed. Please retry.", ex);
        }
    }
    private static bool IsIdempotencyKeyViolation(DbUpdateException ex)
        => ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: IdempotencyIndexName
        };
}
