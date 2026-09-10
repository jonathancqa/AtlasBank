using AtlasBank.SharedKernel.Primitives;
using MediatR;

namespace AtlasBank.Wallets.Application.Commands.CreateWallet;

/// <summary>
/// Comando para criação de uma nova carteira digital.
/// </summary>
public sealed record CreateWalletCommand(
    Guid AccountId,
    string Currency = "BRL") : IRequest<Result<Guid>>;