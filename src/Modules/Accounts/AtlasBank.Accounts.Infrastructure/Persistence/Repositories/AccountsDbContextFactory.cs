using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AtlasBank.Accounts.Infrastructure.Persistence;

/// <summary>
/// Factory usada pelo EF Core Tools em tempo de design (migrations).
/// Não é usada em produção.
/// </summary>
public sealed class AccountsDbContextFactory : IDesignTimeDbContextFactory<AccountsDbContext>
{
    public AccountsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AccountsDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=atlasbank;Username=postgres;Password=postgres");

        return new AccountsDbContext(optionsBuilder.Options);
    }
}