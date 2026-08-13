using AtlasBank.Accounts.Application.Abstractions;
using AtlasBank.Accounts.Infrastructure.Persistence;
using AtlasBank.Accounts.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AtlasBank.Accounts.Infrastructure;

/// <summary>
/// Registra os serviços do módulo Accounts no container de DI.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddAccountsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AccountsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<IAccountRepository, AccountRepository>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(
                typeof(DependencyInjection).Assembly));

        return services;
    }
}