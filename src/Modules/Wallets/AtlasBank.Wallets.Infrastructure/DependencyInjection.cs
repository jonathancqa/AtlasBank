using AtlasBank.Wallets.Application.Abstractions;
using AtlasBank.Wallets.Application.Commands.CreateWallet;
using AtlasBank.Wallets.Infrastructure.Persistence;
using AtlasBank.Wallets.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AtlasBank.Wallets.Infrastructure;

/// <summary>
/// Registra os serviços do módulo Wallets no container de DI.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddWalletsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<WalletsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<IWalletRepository, WalletRepository>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(
                typeof(CreateWalletCommand).Assembly));

        return services;
    }
}