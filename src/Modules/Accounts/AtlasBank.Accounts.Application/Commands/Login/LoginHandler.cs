using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AtlasBank.Accounts.Application.Abstractions;
using AtlasBank.Accounts.Application.Abstractions.Services;
using AtlasBank.Accounts.Domain.Entities;
using AtlasBank.SharedKernel.Primitives;
using MediatR;

namespace AtlasBank.Accounts.Application.Commands.Login;

/// <summary>
/// Handler responsável por processar o comando de Login.
/// Valida credenciais e retorna token JWT em caso de sucesso.
/// </summary>
public sealed class LoginHandler : IRequestHandler<LoginCommand, Result<string>>
{
    private readonly IAccountRepository _repository;
    private readonly IJwtService _jwtService;

    public LoginHandler(IAccountRepository repository, IJwtService jwtService)
    {
        _repository = repository;
        _jwtService = jwtService;
    }

    public async Task<Result<string>> Handle(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        //Busca conta pelo e-mail
        var account = await _repository.GetByEmailAsync(
            command.Email, cancellationToken);

        if (account is null)
        return Result.Failure<string>("Invalid credentials.");

        // Verifica se a conta está ativa
        if (account.Status == AccountStatus.Inactive)
        return Result.Failure<string>("Account is inactive.");

        // Valida a senha contra o hash armazenado
        var isPasswordValid = BCrypt.Net.BCrypt.Verify(
            command.Password, account.PasswordHash);

        if (!isPasswordValid)
            return Result.Failure<string>("Invalid credentials.");

        // Gera o token JWT
        var token = _jwtService.GenerateToken(account);

        return Result.Success(token);
    }
}