using AtlasBank.Accounts.Application.Abstractions;
using AtlasBank.Accounts.Domain.Entities;
using AtlasBank.SharedKernel.Primitives;
using MediatR;

namespace AtlasBank.Accounts.Application.Commands.CreateAccount;

/// <summary>
/// Handler responsável por processar o comando de criação de conta.
/// Orquestra as regras de negócio sem conhecer detalhes de infraestrutura.
/// </summary>
public sealed class CreateAccountHandler : IRequestHandler<CreateAccountCommand, Result<Guid>>
{
    private readonly IAccountRepository _repository;

    public CreateAccountHandler(IAccountRepository repository)
        => _repository = repository;

    public async Task<Result<Guid>> Handle(
        CreateAccountCommand command,
        CancellationToken cancellationToken)
    {
        // Verifica duplicidade de e-mail
        var emailExists = await _repository.ExistsByEmailAsync(
            command.Email, cancellationToken);

        if (emailExists)
            return Result.Failure<Guid>("An account with this email already exists.");

        // Verifica duplicidade de CPF
        var documentExists = await _repository.ExistsByDocumentAsync(
            command.Document, cancellationToken);

        if (documentExists)
            return Result.Failure<Guid>("An account with this document already exists.");

        // Gera hash da senha — nunca persistir senha em texto puro
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(command.Password);

        // Cria o agregado — validações de domínio acontecem aqui
        var accountResult = Account.Create(
            command.FullName,
            command.Email,
            command.Document,
            passwordHash);

        if (accountResult.IsFailure)
            return Result.Failure<Guid>(accountResult.Error);

        await _repository.AddAsync(accountResult.Value, cancellationToken);

        return Result.Success(accountResult.Value.Id);
    }
}