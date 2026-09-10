using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AtlasBank.Wallets.Infrastructure.Persistence;

/// <summary>
/// Factory usada pelo EF Core Tools em tempo de design (migrations).
/// Não é usada em produção.
/// </summary>
public sealed class WalletsDbContextFactory : IDesignTimeDbContextFactory<WalletsDbContext>
{
    public WalletsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WalletsDbContext>();

        optionsBuilder.UseNpgsql(
            // Apenas para design time (migrations) — em produção vem do appsettings.json
            "Host=localhost;Database=atlasbank;Username=postgres;Password=postgres");

        return new WalletsDbContext(optionsBuilder.Options);
    }
}