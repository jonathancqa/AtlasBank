using AtlasBank.SharedKernel.ValueObjects;
using AtlasBank.Wallets.Application.Abstractions;
using AtlasBank.Wallets.Application.Commands.Withdraw;
using AtlasBank.Wallets.Domain.Entities;
using AtlasBank.Wallets.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace AtlasBank.Wallets.Tests.Application.Commands;

public sealed class WithdrawHandlerTests
{
    private readonly IWalletRepository _repository;
    private readonly WithdrawHandler _handler;

    public WithdrawHandlerTests()
    {
        _repository = Substitute.For<IWalletRepository>();
        _handler = new WithdrawHandler(_repository);
    }

    private static Wallet CreateWalletWithBalance(decimal amount = 100)
    {
        var wallet = Wallet.Create(Guid.NewGuid()).Value;
        if (amount > 0)
            wallet.Deposit(Money.Create(amount, "BRL").Value, IdempotencyKey.Generate());
        return wallet;
    }

    [Fact]
    public async Task Handle_WithSufficientBalance_ShouldSucceed()
    {
        // Arrange
        var wallet = CreateWalletWithBalance(100);
        _repository.GetByIdAsync(wallet.Id, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var command = new WithdrawCommand(wallet.Id, 50, "BRL", Guid.NewGuid().ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithSufficientBalance_ShouldCallUpdateAsync()
    {
        // Arrange
        var wallet = CreateWalletWithBalance(100);
        _repository.GetByIdAsync(wallet.Id, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var command = new WithdrawCommand(wallet.Id, 50, "BRL", Guid.NewGuid().ToString());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).UpdateAsync(wallet, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInsufficientBalance_ShouldFail()
    {
        // Arrange
        var wallet = CreateWalletWithBalance(50);
        _repository.GetByIdAsync(wallet.Id, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var command = new WithdrawCommand(wallet.Id, 100, "BRL", Guid.NewGuid().ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Insufficient funds.");
    }

    [Fact]
    public async Task Handle_WithWalletNotFound_ShouldFail()
    {
        // Arrange
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Wallet?)null);

        var command = new WithdrawCommand(Guid.NewGuid(), 50, "BRL", Guid.NewGuid().ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Wallet not found.");
    }

    [Fact]
    public async Task Handle_WithZeroAmount_ShouldFail()
    {
        // Arrange
        var wallet = CreateWalletWithBalance(100);
        _repository.GetByIdAsync(wallet.Id, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var command = new WithdrawCommand(wallet.Id, 0, "BRL", Guid.NewGuid().ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Withdrawal amount must be greater than zero.");
    }

    [Fact]
    public async Task Handle_WithEmptyIdempotencyKey_ShouldFail()
    {
        // Arrange
        var command = new WithdrawCommand(Guid.NewGuid(), 50, "BRL", "");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Idempotency key is required.");
    }

    [Fact]
    public async Task Handle_WithNegativeAmount_ShouldFail()
    {
        // Arrange
        var command = new WithdrawCommand(Guid.NewGuid(), -50, "BRL", Guid.NewGuid().ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Amount cannot be negative.");
    }
}