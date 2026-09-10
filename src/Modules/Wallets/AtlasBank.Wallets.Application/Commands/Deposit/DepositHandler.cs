using AtlasBank.SharedKernel.Primitives;
using AtlasBank.SharedKernel.ValueObjects;
using AtlasBank.Wallets.Application.Abstractions;
using AtlasBank.Wallets.Domain.ValueObjects;
using MediatR;

namespace AtlasBank.Wallets.Application.Commands.Deposit;

/// <summary>
/// Handler responsável por processar o comando de depósito.
/// </summary>
public sealed class DepositHandler : IRequestHandler<DepositCommand, Result>
{
    private readonly IWalletRepository _repository;

    public DepositHandler(IWalletRepository repository)
        => _repository = repository;

    public async Task<Result> Handle(
        DepositCommand command,
        CancellationToken cancellationToken)
    {
        // Valida a chave de idempotência
        var keyResult = IdempotencyKey.Create(command.IdempotencyKey);
        if (keyResult.IsFailure)
            return Result.Failure(keyResult.Error);

        // Valida o valor monetário
        var moneyResult = Money.Create(command.Amount, command.Currency);
        if (moneyResult.IsFailure)
            return Result.Failure(moneyResult.Error);

        // Busca a carteira
        var wallet = await _repository.GetByIdAsync(command.WalletId, cancellationToken);
        if (wallet is null)
            return Result.Failure("Wallet not found.");

        // Executa o depósito no domínio
        var depositResult = wallet.Deposit(moneyResult.Value, keyResult.Value);
        if (depositResult.IsFailure)
            return Result.Failure(depositResult.Error);

        await _repository.UpdateAsync(wallet, cancellationToken);

        return Result.Success();
    }
}