using AtlasBank.Wallets.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtlasBank.Wallets.Infrastructure.Persistence;

/// <summary>
/// DbContext do módulo Wallets.
/// Schema isolado — não enxerga tabelas de outros módulos.
/// </summary>
public sealed class WalletsDbContext : DbContext
{
    public WalletsDbContext(DbContextOptions<WalletsDbContext> options)
        : base(options) { }

    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("wallets");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WalletsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}