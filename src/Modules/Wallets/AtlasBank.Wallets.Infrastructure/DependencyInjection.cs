using AtlasBank.Wallets.Application.Abstractions;
using AtlasBank.Wallets.Application.Commands.CreateWallet;
using AtlasBank.Wallets.Infrastructure.Persistence;
using AtlasBank.Wallets.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AtlasBank.SharedKernel.Behaviors;
using FluentValidation;
using MediatR;

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
        services.AddScoped<IWalletsUnitOfWork, WalletsUnitOfWork>();
        
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateWalletCommand).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(CreateWalletCommand).Assembly);

        return services;
    }
}