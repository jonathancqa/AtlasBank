namespace AtlasBank.Wallets.Domain.Events;

/// <summary>
/// Evento disparado quando uma nova carteira é criada.
/// </summary>
public sealed record WalletCreatedEvent(
    Guid WalletId,
    Guid AccountId,
    DateTime CreatedAt);