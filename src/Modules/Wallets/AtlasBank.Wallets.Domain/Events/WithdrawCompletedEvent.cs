namespace AtlasBank.Wallets.Domain.Events;

/// <summary>
/// Evento disparado quando um saque é concluído com sucesso.
/// </summary>
public sealed record WithdrawCompletedEvent(
    Guid WalletId,
    Guid TransactionId,
    decimal Amount,
    string Currency,
    DateTime CompletedAt);