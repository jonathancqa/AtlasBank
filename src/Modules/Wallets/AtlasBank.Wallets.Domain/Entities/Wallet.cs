using AtlasBank.SharedKernel.Abstractions;
using AtlasBank.SharedKernel.Primitives;
using AtlasBank.SharedKernel.ValueObjects;
using AtlasBank.Wallets.Domain.Events;
using AtlasBank.Wallets.Domain.ValueObjects;

namespace AtlasBank.Wallets.Domain.Entities;

/// <summary>
/// Aggregate Root do módulo Wallets.
/// Representa uma carteira digital com saldo e histórico de transações.
/// </summary>
public sealed class Wallet : AggregateRoot
{
    private readonly List<Transaction> _transactions = [];

    /// <summary>Id da conta dona desta carteira.</summary>
    public Guid AccountId { get; private set; }

    /// <summary>Saldo atual da carteira.</summary>
    public Money Balance { get; private set; } = null!;

    /// <summary>Data de criação da carteira.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Histórico de transações da carteira.</summary>
    public IReadOnlyList<Transaction> Transactions => _transactions.AsReadOnly();

    private Wallet() { } // EF Core

    private Wallet(Guid id, Guid accountId, Money balance) : base(id)
    {
        AccountId = accountId;
        Balance = balance;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cria uma nova carteira com saldo zero.
    /// </summary>
    public static Result<Wallet> Create(Guid accountId, string currency = "BRL")
    {
        if (accountId == Guid.Empty)
            return Result.Failure<Wallet>("Account ID is required.");

        var balance = Money.Zero(currency);
        var wallet = new Wallet(Guid.NewGuid(), accountId, balance);

        wallet.RaiseDomainEvent(new WalletCreatedEvent(
            wallet.Id,
            wallet.AccountId,
            wallet.CreatedAt));

        return Result.Success(wallet);
    }

    /// <summary>
    /// Realiza um depósito na carteira.
    /// </summary>
    public Result Deposit(Money amount, IdempotencyKey idempotencyKey)
    {
        if (amount.IsZero())
            return Result.Failure("Deposit amount must be greater than zero.");

        if (HasTransaction(idempotencyKey))
            return Result.Success(); // idempotente — já processado

        var addResult = Balance.Add(amount);
        if (addResult.IsFailure)
            return Result.Failure(addResult.Error);

        Balance = addResult.Value;

        var transaction = Transaction.CreateDeposit(Id, amount, idempotencyKey);
        _transactions.Add(transaction);

        RaiseDomainEvent(new DepositCompletedEvent(
            Id,
            transaction.Id,
            amount.Amount,
            amount.Currency,
            transaction.CreatedAt));

        return Result.Success();
    }

    /// <summary>
    /// Realiza um saque da carteira.
    /// </summary>
    public Result Withdraw(Money amount, IdempotencyKey idempotencyKey)
    {
        if (amount.IsZero())
            return Result.Failure("Withdrawal amount must be greater than zero.");

        if (HasTransaction(idempotencyKey))
            return Result.Success(); // idempotente — já processado

        var subtractResult = Balance.Subtract(amount);
        if (subtractResult.IsFailure)
            return Result.Failure(subtractResult.Error);

        Balance = subtractResult.Value;

        var transaction = Transaction.CreateWithdrawal(Id, amount, idempotencyKey);
        _transactions.Add(transaction);

        RaiseDomainEvent(new WithdrawCompletedEvent(
            Id,
            transaction.Id,
            amount.Amount,
            amount.Currency,
            transaction.CreatedAt));

        return Result.Success();
    }

    /// <summary>
    /// Transfere valor para outra carteira.
    /// Débito e crédito são atômicos — nunca um sem o outro.
    /// </summary>
    public Result Transfer(
        Wallet destination,
        Money amount,
        IdempotencyKey idempotencyKey)
    {
        if (destination.Id == Id)
            return Result.Failure("Cannot transfer to the same wallet.");

        if (amount.IsZero())
            return Result.Failure("Transfer amount must be greater than zero.");

        if (HasTransaction(idempotencyKey))
            return Result.Success(); // idempotente — já processado

        // Débito na origem
        var subtractResult = Balance.Subtract(amount);
        if (subtractResult.IsFailure)
            return Result.Failure(subtractResult.Error);

        Balance = subtractResult.Value;

        // Crédito no destino
        var addResult = destination.Balance.Add(amount);
        if (addResult.IsFailure)
            return Result.Failure(addResult.Error);

        destination.Balance = addResult.Value;
        
        _transactions.Add(Transaction.CreateTransferOut(Id, amount, idempotencyKey, destination.Id));
        destination._transactions.Add(Transaction.CreateTransferIn(destination.Id, amount, idempotencyKey, Id));

        RaiseDomainEvent(new TransferCompletedEvent(
            Id,
            destination.Id,
            Id,
            amount.Amount,
            amount.Currency,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Verifica se já existe uma transação com a chave de idempotência informada.
    /// </summary>
    private bool HasTransaction(IdempotencyKey idempotencyKey)
        => _transactions.Any(t => t.IdempotencyKey.Value == idempotencyKey.Value);
}