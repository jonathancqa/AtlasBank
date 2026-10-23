using AtlasBank.Accounts.Application.Abstractions;
using AtlasBank.Accounts.Application.Abstractions.Services;
using AtlasBank.Accounts.Application.Commands.CreateAccount;
using AtlasBank.Accounts.Infrastructure.Persistence;
using AtlasBank.Accounts.Infrastructure.Persistence.Repositories;
using AtlasBank.Accounts.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AtlasBank.SharedKernel.Behaviors;
using FluentValidation;
using MediatR;

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
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAccountsUnitOfWork, AccountsUnitOfWork>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateAccountCommand).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(CreateAccountCommand).Assembly);

        return services;
    }
}