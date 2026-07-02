namespace AtlasBank.Accounts.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando uma nova conta é criada com sucesso.
/// Outros módulos podem reagir a este evento sem acoplamento direto com Accounts.
/// </summary>
public sealed record AccountCreatedEvent(
    Guid AccountId,
    string FullName,
    string Email,
    DateTime CreatedAt);