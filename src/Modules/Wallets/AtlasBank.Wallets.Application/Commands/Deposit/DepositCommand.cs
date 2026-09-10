using AtlasBank.SharedKernel.Primitives;
using MediatR;

namespace AtlasBank.Wallets.Application.Commands.Deposit;

/// <summary>
/// Comando para realizar um depósito em uma carteira.
/// IdempotencyKey garante que requisições repetidas não gerem depósitos duplicados.
/// </summary>
public sealed record DepositCommand(
    Guid WalletId,
    decimal Amount,
    string Currency,
    string IdempotencyKey) : IRequest<Result>;