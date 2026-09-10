using AtlasBank.SharedKernel.Primitives;
using MediatR;

namespace AtlasBank.Wallets.Application.Commands.Transfer;

/// <summary>
/// Comando para realizar uma transferência entre carteiras.
/// Débito e crédito são atômicos — nunca um sem o outro.
/// IdempotencyKey garante que requisições repetidas não gerem transferências duplicadas.
/// </summary>
public sealed record TransferCommand(
    Guid SourceWalletId,
    Guid DestinationWalletId,
    decimal Amount,
    string Currency,
    string IdempotencyKey) : IRequest<Result>;