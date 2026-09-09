namespace AtlasBank.Wallets.Domain.Events;

/// <summary>
/// Evento disparado quando um depósito é concluído com sucesso.
/// </summary>
public sealed record DepositCompletedEvent(
    Guid WalletId,
    Guid TransactionId,
    decimal Amount,
    string Currency,
    DateTime CompletedAt);