using AtlasBank.Accounts.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtlasBank.Accounts.Infrastructure.Persistence;

/// <summary>
/// DbContext do módulo Accounts.
/// Cada módulo tem seu próprio DbContext — isolamento de dados entre módulos.
/// </summary>
public sealed class AccountsDbContext : DbContext
{
    public AccountsDbContext(DbContextOptions<AccountsDbContext> options)
        : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("accounts");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}