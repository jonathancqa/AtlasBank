namespace AtlasBank.Wallets.Domain.Events;

/// <summary>
/// Evento disparado quando uma transferência é concluída com sucesso.
/// </summary>
public sealed record TransferCompletedEvent(
    Guid SourceWalletId,
    Guid DestinationWalletId,
    Guid TransactionId,
    decimal Amount,
    string Currency,
    DateTime CompletedAt);