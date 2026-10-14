namespace AtlasBank.Wallets.Application.ReadModels;

/// <summary>
/// Read model para extrato — leitura otimizada via Dapper, sem reconstituir o agregado.
/// </summary>
public sealed class TransactionReadModel
{
    public Guid Id { get; init; }
    public Guid WalletId { get; init; }
    public string Type { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public Guid? RelatedWalletId { get; init; }
    public DateTime CreatedAt { get; init; }
}