using AtlasBank.SharedKernel.Primitives;
using AtlasBank.Wallets.Application.Abstractions;
using AtlasBank.Wallets.Domain.Entities;
using MediatR;

namespace AtlasBank.Wallets.Application.Commands.CreateWallet;

/// <summary>
/// Handler responsável por processar o comando de criação de carteira.
/// </summary>
public sealed class CreateWalletHandler : IRequestHandler<CreateWalletCommand, Result<Guid>>
{
    private readonly IWalletRepository _repository;

    public CreateWalletHandler(IWalletRepository repository)
        => _repository = repository;

    public async Task<Result<Guid>> Handle(
        CreateWalletCommand command,
        CancellationToken cancellationToken)
    {
        // Verifica se já existe carteira para essa conta
        var exists = await _repository.ExistsByAccountIdAsync(
            command.AccountId, cancellationToken);

        if (exists)
            return Result.Failure<Guid>("Account already has a wallet.");

        // Cria a carteira no domínio
        var walletResult = Wallet.Create(command.AccountId, command.Currency);

        if (walletResult.IsFailure)
            return Result.Failure<Guid>(walletResult.Error);

        await _repository.AddAsync(walletResult.Value, cancellationToken);

        return Result.Success(walletResult.Value.Id);
    }
}