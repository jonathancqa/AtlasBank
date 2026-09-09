using AtlasBank.SharedKernel.ValueObjects;
using AtlasBank.Wallets.Domain.Entities;
using AtlasBank.Wallets.Domain.Events;
using AtlasBank.Wallets.Domain.ValueObjects;
using FluentAssertions;

namespace AtlasBank.Wallets.Tests.Domain.Entities;

public sealed class WalletTests
{
    private static readonly Guid ValidAccountId = Guid.NewGuid();

    // ───────────────────────────────────────────
    // Create
    // ───────────────────────────────────────────

    [Fact]
    public void Create_WithValidAccountId_ShouldSucceed()
    {
        // Act
        var result = Wallet.Create(ValidAccountId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccountId.Should().Be(ValidAccountId);
        result.Value.Balance.Amount.Should().Be(0);
        result.Value.Balance.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Create_ShouldRaiseWalletCreatedEvent()
    {
        // Act
        var result = Wallet.Create(ValidAccountId);

        // Assert
        result.Value.DomainEvents.Should().HaveCount(1);
        result.Value.DomainEvents.First().Should().BeOfType<WalletCreatedEvent>();
    }

    [Fact]
    public void Create_WithEmptyAccountId_ShouldFail()
    {
        // Act
        var result = Wallet.Create(Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Account ID is required.");
    }

    // ───────────────────────────────────────────
    // Deposit
    // ───────────────────────────────────────────

    [Fact]
    public void Deposit_WithValidAmount_ShouldIncreaseBalance()
    {
        // Arrange
        var wallet = Wallet.Create(ValidAccountId).Value;
        var amount = Money.Create(100, "BRL").Value;
        var key = IdempotencyKey.Generate();

        // Act
        var result = wallet.Deposit(amount, key);

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.Balance.Amount.Should().Be(100);
    }

    [Fact]
    public void Deposit_WithValidAmount_ShouldAddTransaction()
    {
        // Arrange
        var wallet = Wallet.Create(ValidAccountId).Value;
        var amount = Money.Create(100, "BRL").Value;
        var key = IdempotencyKey.Generate();

        // Act
        wallet.Deposit(amount, key);

        // Assert
        wallet.Transactions.Should().HaveCount(1);
    }

    [Fact]
    public void Deposit_WithValidAmount_ShouldRaiseDomainEvent()
    {
        // Arrange
        var wallet = Wallet.Create(ValidAccountId).Value;
        wallet.ClearDomainEvents();
        var amount = Money.Create(100, "BRL").Value;
        var key = IdempotencyKey.Generate();

        // Act
        wallet.Deposit(amount, key);

        // Assert
        wallet.DomainEvents.Should().HaveCount(1);
        wallet.DomainEvents.First().Should().BeOfType<DepositCompletedEvent>();
    }

    [Fact]
    public void Deposit_WithZeroAmount_ShouldFail()
    {
        // Arrange
        var wallet = Wallet.Create(ValidAccountId).Value;
        var amount = Money.Zero();
        var key = IdempotencyKey.Generate();

        // Act
        var result = wallet.Deposit(amount, key);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Deposit amount must be greater than zero.");
    }

    [Fact]
    public void Deposit_WithSameIdempotencyKey_ShouldNotDuplicate()
    {
        // Arrange
        var wallet = Wallet.Create(ValidAccountId).Value;
        var amount = Money.Create(100, "BRL").Value;
        var key = IdempotencyKey.Generate();

        // Act
        wallet.Deposit(amount, key);
        var result = wallet.Deposit(amount, key); // replay

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.Balance.Amount.Should().Be(100); // não dobrou
        wallet.Transactions.Should().HaveCount(1); // só uma transação
    }

    // ───────────────────────────────────────────
    // Withdraw
    // ───────────────────────────────────────────

    [Fact]
    public void Withdraw_WithSufficientBalance_ShouldDecreaseBalance()
    {
        // Arrange
        var wallet = Wallet.Create(ValidAccountId).Value;
        wallet.Deposit(Money.Create(100, "BRL").Value, IdempotencyKey.Generate());
        var key = IdempotencyKey.Generate();

        // Act
        var result = wallet.Withdraw(Money.Create(60, "BRL").Value, key);

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.Balance.Amount.Should().Be(40);
    }

    [Fact]
    public void Withdraw_WithInsufficientBalance_ShouldFail()
    {
        // Arrange
        var wallet = Wallet.Create(ValidAccountId).Value;
        wallet.Deposit(Money.Create(50, "BRL").Value, IdempotencyKey.Generate());
        var key = IdempotencyKey.Generate();

        // Act
        var result = wallet.Withdraw(Money.Create(100, "BRL").Value, key);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Insufficient funds.");
    }

    [Fact]
    public void Withdraw_WithZeroAmount_ShouldFail()
    {
        // Arrange
        var wallet = Wallet.Create(ValidAccountId).Value;
        var key = IdempotencyKey.Generate();

        // Act
        var result = wallet.Withdraw(Money.Zero(), key);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Withdrawal amount must be greater than zero.");
    }

    [Fact]
    public void Withdraw_WithSameIdempotencyKey_ShouldNotDuplicate()
    {
        // Arrange
        var wallet = Wallet.Create(ValidAccountId).Value;
        wallet.Deposit(Money.Create(100, "BRL").Value, IdempotencyKey.Generate());
        var key = IdempotencyKey.Generate();

        // Act
        wallet.Withdraw(Money.Create(50, "BRL").Value, key);
        var result = wallet.Withdraw(Money.Create(50, "BRL").Value, key); // replay

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.Balance.Amount.Should().Be(50); // não debitou duas vezes
        wallet.Transactions.Should().HaveCount(2); // depósito + 1 saque
    }

    // ───────────────────────────────────────────
    // Transfer
    // ───────────────────────────────────────────

    [Fact]
    public void Transfer_WithSufficientBalance_ShouldUpdateBothWallets()
    {
        // Arrange
        var source = Wallet.Create(ValidAccountId).Value;
        var destination = Wallet.Create(Guid.NewGuid()).Value;
        source.Deposit(Money.Create(100, "BRL").Value, IdempotencyKey.Generate());
        var key = IdempotencyKey.Generate();

        // Act
        var result = source.Transfer(destination, Money.Create(60, "BRL").Value, key);

        // Assert
        result.IsSuccess.Should().BeTrue();
        source.Balance.Amount.Should().Be(40);
        destination.Balance.Amount.Should().Be(60);
    }

    [Fact]
    public void Transfer_WithInsufficientBalance_ShouldFail()
    {
        // Arrange
        var source = Wallet.Create(ValidAccountId).Value;
        var destination = Wallet.Create(Guid.NewGuid()).Value;
        source.Deposit(Money.Create(50, "BRL").Value, IdempotencyKey.Generate());
        var key = IdempotencyKey.Generate();

        // Act
        var result = source.Transfer(destination, Money.Create(100, "BRL").Value, key);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Insufficient funds.");
        source.Balance.Amount.Should().Be(50); // não alterou
        destination.Balance.Amount.Should().Be(0); // não alterou
    }

    [Fact]
    public void Transfer_ToSameWallet_ShouldFail()
    {
        // Arrange
        var wallet = Wallet.Create(ValidAccountId).Value;
        wallet.Deposit(Money.Create(100, "BRL").Value, IdempotencyKey.Generate());
        var key = IdempotencyKey.Generate();

        // Act
        var result = wallet.Transfer(wallet, Money.Create(50, "BRL").Value, key);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Cannot transfer to the same wallet.");
    }

    [Fact]
    public void Transfer_WithZeroAmount_ShouldFail()
    {
        // Arrange
        var source = Wallet.Create(ValidAccountId).Value;
        var destination = Wallet.Create(Guid.NewGuid()).Value;
        var key = IdempotencyKey.Generate();

        // Act
        var result = source.Transfer(destination, Money.Zero(), key);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Transfer amount must be greater than zero.");
    }

    [Fact]
    public void Transfer_WithSameIdempotencyKey_ShouldNotDuplicate()
    {
        // Arrange
        var source = Wallet.Create(ValidAccountId).Value;
        var destination = Wallet.Create(Guid.NewGuid()).Value;
        source.Deposit(Money.Create(100, "BRL").Value, IdempotencyKey.Generate());
        var key = IdempotencyKey.Generate();

        // Act
        source.Transfer(destination, Money.Create(50, "BRL").Value, key);
        var result = source.Transfer(destination, Money.Create(50, "BRL").Value, key); // replay

        // Assert
        result.IsSuccess.Should().BeTrue();
        source.Balance.Amount.Should().Be(50); // não debitou duas vezes
        destination.Balance.Amount.Should().Be(50); // não creditou duas vezes
    }

    [Fact]
    public void Transfer_ShouldRaiseTransferCompletedEvent()
    {
        // Arrange
        var source = Wallet.Create(ValidAccountId).Value;
        var destination = Wallet.Create(Guid.NewGuid()).Value;
        source.Deposit(Money.Create(100, "BRL").Value, IdempotencyKey.Generate());
        source.ClearDomainEvents();
        var key = IdempotencyKey.Generate();

        // Act
        source.Transfer(destination, Money.Create(50, "BRL").Value, key);

        // Assert
        source.DomainEvents.Should().HaveCount(1);
        source.DomainEvents.First().Should().BeOfType<TransferCompletedEvent>();
    }
}