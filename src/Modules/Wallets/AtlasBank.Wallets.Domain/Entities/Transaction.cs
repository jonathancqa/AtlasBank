using AtlasBank.SharedKernel.Abstractions;
using AtlasBank.SharedKernel.ValueObjects;
using AtlasBank.Wallets.Domain.Enums;
using AtlasBank.Wallets.Domain.ValueObjects;

namespace AtlasBank.Wallets.Domain.Entities;

/// <summary>
/// Representa uma transação financeira dentro de uma carteira.
/// </summary>
public sealed class Transaction : Entity
{
    /// <summary>Id da carteira origem.</summary>
    public Guid WalletId { get; private set; }

    /// <summary>Tipo da transação.</summary>
    public TransactionType Type { get; private set; }

    /// <summary>Valor da transação.</summary>
    public Money Amount { get; private set; } = null!;

    /// <summary>Chave de idempotência — evita duplicação.</summary>
    public IdempotencyKey IdempotencyKey { get; private set; } = null!;

    /// <summary>Id da carteira destino — preenchido apenas em transferências.</summary>
    public Guid? RelatedWalletId { get; private set; }

    /// <summary>Data e hora da transação em UTC.</summary>
    public DateTime CreatedAt { get; private set; }

    private Transaction() { } // EF Core

    private Transaction(
        Guid walletId,
        TransactionType type,
        Money amount,
        IdempotencyKey idempotencyKey,
        Guid? relatedWalletId = null) : base(Guid.NewGuid())
    {
        WalletId = walletId;
        Type = type;
        Amount = amount;
        IdempotencyKey = idempotencyKey;
        RelatedWalletId = relatedWalletId;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>Cria uma transação de depósito.</summary>
    public static Transaction CreateDeposit(
        Guid walletId,
        Money amount,
        IdempotencyKey idempotencyKey)
        => new(walletId, TransactionType.Deposit, amount, idempotencyKey);

    /// <summary>Cria uma transação de saque.</summary>
    public static Transaction CreateWithdrawal(
        Guid walletId,
        Money amount,
        IdempotencyKey idempotencyKey)
        => new(walletId, TransactionType.Withdrawal, amount, idempotencyKey);

    /// <summary>Cria uma transação de transferência enviada.</summary>
    public static Transaction CreateTransferOut(
        Guid walletId,
        Money amount,
        IdempotencyKey idempotencyKey,
        Guid destinationWalletId)
        => new(walletId, TransactionType.TransferOut, amount, idempotencyKey, destinationWalletId);

    /// <summary>Cria uma transação de transferência recebida.</summary>
    public static Transaction CreateTransferIn(
        Guid walletId,
        Money amount,
        IdempotencyKey idempotencyKey,
        Guid sourceWalletId)
        => new(walletId, TransactionType.TransferIn, amount, idempotencyKey, sourceWalletId);
}