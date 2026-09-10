using AtlasBank.SharedKernel.Primitives;
using AtlasBank.Wallets.Application.Abstractions;
using MediatR;

namespace AtlasBank.Wallets.Application.Queries.GetBalance;

/// <summary>
/// Handler responsável por processar a query de saldo.
/// </summary>
public sealed class GetBalanceHandler : IRequestHandler<GetBalanceQuery, Result<GetBalanceResponse>>
{
    private readonly IWalletRepository _repository;

    public GetBalanceHandler(IWalletRepository repository)
        => _repository = repository;

    public async Task<Result<GetBalanceResponse>> Handle(
        GetBalanceQuery query,
        CancellationToken cancellationToken)
    {
        var wallet = await _repository.GetByIdAsync(query.WalletId, cancellationToken);

        if (wallet is null)
            return Result.Failure<GetBalanceResponse>("Wallet not found.");

        return Result.Success(new GetBalanceResponse(
            wallet.Id,
            wallet.Balance.Amount,
            wallet.Balance.Currency));
    }
}