using AtlasBank.SharedKernel.Primitives;
using AtlasBank.SharedKernel.ValueObjects;
using AtlasBank.Wallets.Application.Abstractions;
using AtlasBank.Wallets.Domain.ValueObjects;
using MediatR;

namespace AtlasBank.Wallets.Application.Commands.Transfer;

/// <summary>
/// Handler responsável por processar o comando de transferência.
/// Carrega ambas as carteiras e executa a transferência atomicamente no domínio.
/// </summary>
public sealed class TransferHandler : IRequestHandler<TransferCommand, Result>
{
    private readonly IWalletRepository _repository;

    public TransferHandler(IWalletRepository repository)
        => _repository = repository;

    public async Task<Result> Handle(
        TransferCommand command,
        CancellationToken cancellationToken)
    {
        if (command.SourceWalletId == command.DestinationWalletId)
            return Result.Failure("Cannot transfer to the same wallet.");

        // Valida a chave de idempotência
        var keyResult = IdempotencyKey.Create(command.IdempotencyKey);
        if (keyResult.IsFailure)
            return Result.Failure(keyResult.Error);

        // Valida o valor monetário
        var moneyResult = Money.Create(command.Amount, command.Currency);
        if (moneyResult.IsFailure)
            return Result.Failure(moneyResult.Error);

        // Busca carteira origem
        var source = await _repository.GetByIdAsync(
            command.SourceWalletId, cancellationToken);
        if (source is null)
            return Result.Failure("Source wallet not found.");

        // Busca carteira destino
        var destination = await _repository.GetByIdAsync(
            command.DestinationWalletId, cancellationToken);
        if (destination is null)
            return Result.Failure("Destination wallet not found.");

        // Executa a transferência no domínio
        var transferResult = source.Transfer(
            destination,
            moneyResult.Value,
            keyResult.Value);

        if (transferResult.IsFailure)
            return Result.Failure(transferResult.Error);

        // Persiste ambas as carteiras
        await _repository.UpdateAsync(source, cancellationToken);
        await _repository.UpdateAsync(destination, cancellationToken);

        return Result.Success();
    }
}