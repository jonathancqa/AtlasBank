using AtlasBank.SharedKernel.Primitives;
using MediatR;

namespace AtlasBank.Wallets.Application.Commands.Withdraw;

/// <summary>
/// Comando para realizar um saque de uma carteira.
/// IdempotencyKey garante que requisições repetidas não gerem saques duplicados.
/// </summary>
public sealed record WithdrawCommand(
    Guid WalletId,
    decimal Amount,
    string Currency,
    string IdempotencyKey) : IRequest<Result>;