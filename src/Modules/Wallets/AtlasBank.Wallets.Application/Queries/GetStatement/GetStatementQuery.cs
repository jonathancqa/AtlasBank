using AtlasBank.SharedKernel.Primitives;
using AtlasBank.Wallets.Domain.Enums;
using MediatR;

namespace AtlasBank.Wallets.Application.Queries.GetStatement;

/// <summary>
/// Query para consultar o extrato de uma carteira com paginação e filtros.
/// </summary>
public sealed record GetStatementQuery(
    Guid WalletId,
    DateTime? From = null,
    DateTime? To = null,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<GetStatementResponse>>;

/// <summary>
/// Resposta da query de extrato.
/// </summary>
public sealed record GetStatementResponse(
    Guid WalletId,
    decimal Balance,
    string Currency,
    int TotalTransactions,
    int Page,
    int PageSize,
    IReadOnlyList<TransactionResponse> Transactions);

/// <summary>
/// Representa uma transação no extrato.
/// </summary>
public sealed record TransactionResponse(
    Guid Id,
    TransactionType Type,
    decimal Amount,
    string Currency,
    Guid? RelatedWalletId,
    DateTime CreatedAt);