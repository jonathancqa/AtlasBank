using AtlasBank.SharedKernel.Primitives;
using AtlasBank.SharedKernel.ValueObjects;
using AtlasBank.Wallets.Application.Abstractions;
using AtlasBank.Wallets.Domain.ValueObjects;
using MediatR;

namespace AtlasBank.Wallets.Application.Commands.Withdraw;

/// <summary>
/// Handler responsável por processar o comando de saque.
/// </summary>
public sealed class WithdrawHandler : IRequestHandler<WithdrawCommand, Result>
{
    private readonly IWalletRepository _repository;

    public WithdrawHandler(IWalletRepository repository)
        => _repository = repository;

    public async Task<Result> Handle(
        WithdrawCommand command,
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

        // Executa o saque no domínio
        var withdrawResult = wallet.Withdraw(moneyResult.Value, keyResult.Value);
        if (withdrawResult.IsFailure)
            return Result.Failure(withdrawResult.Error);

        await _repository.UpdateAsync(wallet, cancellationToken);

        return Result.Success();
    }
}