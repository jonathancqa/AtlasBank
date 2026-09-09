namespace AtlasBank.Wallets.Domain.Enums;

/// <summary>
/// Tipos de transação suportados pelo AtlasBank.
/// </summary>
public enum TransactionType
{
    Deposit,
    Withdrawal,
    TransferIn,
    TransferOut
}