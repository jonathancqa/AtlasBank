using AtlasBank.Wallets.Application.Abstractions;
using AtlasBank.Wallets.Infrastructure.Persistence;

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
        => await _context.SaveChangesAsync(cancellationToken);
}