using AtlasBank.SharedKernel.Primitives;
using MediatR;

namespace AtlasBank.Wallets.Application.Queries.GetBalance;

/// <summary>
/// Query para consultar o saldo atual de uma carteira.
/// </summary>
public sealed record GetBalanceQuery(Guid WalletId) : IRequest<Result<GetBalanceResponse>>;

/// <summary>
/// Resposta da query de saldo.
/// </summary>
public sealed record GetBalanceResponse(
    Guid WalletId,
    decimal Amount,
    string Currency);