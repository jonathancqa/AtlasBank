using AtlasBank.SharedKernel.Exceptions;
using AtlasBank.Wallets.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AtlasBank.Wallets.Infrastructure.Persistence;

/// <summary>
/// Implementação do Unit of Work para o módulo Wallets.
/// </summary>
public sealed class WalletsUnitOfWork : IWalletsUnitOfWork
{
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
    }
}