using AtlasBank.SharedKernel.Primitives;
using AtlasBank.Wallets.Application.Abstractions;
using AtlasBank.Wallets.Domain.Enums;
using MediatR;

namespace AtlasBank.Wallets.Application.Queries.GetStatement;

/// <summary>
/// Handler responsável por processar a query de extrato.
/// </summary>
public sealed class GetStatementHandler : IRequestHandler<GetStatementQuery, Result<GetStatementResponse>>
{
    private readonly IWalletRepository _repository;

    public GetStatementHandler(IWalletRepository repository)
        => _repository = repository;

    public async Task<Result<GetStatementResponse>> Handle(
        GetStatementQuery query,
        CancellationToken cancellationToken)
    {
        var wallet = await _repository.GetByIdReadOnlyAsync(query.WalletId, cancellationToken);

    if (wallet is null)
        return Result.Failure<GetStatementResponse>("Wallet not found.");

    if (wallet.AccountId != query.AccountId)
        return Result.Failure<GetStatementResponse>("Access denied.");

    var (transactions, total) = await _repository.GetStatementAsync(
        query.WalletId,
        query.From,
        query.To,
        query.Page,
        query.PageSize,
        cancellationToken);

    var transactionResponses = transactions.Select(t => new TransactionResponse(
        t.Id,
        Enum.Parse<TransactionType>(t.Type),
        t.Amount,
        t.Currency,
        t.RelatedWalletId,
        t.CreatedAt)).ToList();

    return Result.Success(new GetStatementResponse(
        wallet.Id,
        wallet.Balance.Amount,
        wallet.Balance.Currency,
        total,
        query.Page,
        query.PageSize,
        transactionResponses));
    }
}