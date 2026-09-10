using AtlasBank.SharedKernel.Primitives;
using AtlasBank.Wallets.Application.Abstractions;
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
        var wallet = await _repository.GetByIdAsync(query.WalletId, cancellationToken);

        if (wallet is null)
            return Result.Failure<GetStatementResponse>("Wallet not found.");

        // Filtra por período
        var transactions = wallet.Transactions.AsEnumerable();

        if (query.From.HasValue)
            transactions = transactions.Where(t => t.CreatedAt >= query.From.Value);

        if (query.To.HasValue)
            transactions = transactions.Where(t => t.CreatedAt <= query.To.Value);

        var total = transactions.Count();

        // Paginação
        var paged = transactions
            .OrderByDescending(t => t.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(t => new TransactionResponse(
                t.Id,
                t.Type,
                t.Amount.Amount,
                t.Amount.Currency,
                t.RelatedWalletId,
                t.CreatedAt))
            .ToList();

        return Result.Success(new GetStatementResponse(
            wallet.Id,
            wallet.Balance.Amount,
            wallet.Balance.Currency,
            total,
            query.Page,
            query.PageSize,
            paged));
    }
}