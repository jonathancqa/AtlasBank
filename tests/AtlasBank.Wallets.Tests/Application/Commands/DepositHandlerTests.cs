using AtlasBank.SharedKernel.ValueObjects;
using AtlasBank.Wallets.Application.Abstractions;
using AtlasBank.Wallets.Application.Commands.Deposit;
using AtlasBank.Wallets.Domain.Entities;
using AtlasBank.Wallets.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace AtlasBank.Wallets.Tests.Application.Commands;

public sealed class DepositHandlerTests
{
    private readonly IWalletRepository _repository;
    private readonly DepositHandler _handler;

    public DepositHandlerTests()
    {
        _repository = Substitute.For<IWalletRepository>();
        _handler = new DepositHandler(_repository);
    }

    private static Wallet CreateWalletWithBalance(decimal amount = 0)
    {
        var wallet = Wallet.Create(Guid.NewGuid()).Value;
        if (amount > 0)
            wallet.Deposit(Money.Create(amount, "BRL").Value, IdempotencyKey.Generate());
        return wallet;
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldSucceed()
    {
        // Arrange
        var wallet = CreateWalletWithBalance();
        _repository.GetByIdAsync(wallet.Id, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var command = new DepositCommand(wallet.Id, 100, "BRL", Guid.NewGuid().ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCallUpdateAsync()
    {
        // Arrange
        var wallet = CreateWalletWithBalance();
        _repository.GetByIdAsync(wallet.Id, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var command = new DepositCommand(wallet.Id, 100, "BRL", Guid.NewGuid().ToString());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).UpdateAsync(wallet, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithWalletNotFound_ShouldFail()
    {
        // Arrange
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Wallet?)null);

        var command = new DepositCommand(Guid.NewGuid(), 100, "BRL", Guid.NewGuid().ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Wallet not found.");
    }

    [Fact]
    public async Task Handle_WithNegativeAmount_ShouldFail()
    {
        // Arrange
        var wallet = CreateWalletWithBalance();
        _repository.GetByIdAsync(wallet.Id, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var command = new DepositCommand(wallet.Id, -100, "BRL", Guid.NewGuid().ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Amount cannot be negative.");
    }

    [Fact]
    public async Task Handle_WithZeroAmount_ShouldFail()
    {
        // Arrange
        var wallet = CreateWalletWithBalance();
        _repository.GetByIdAsync(wallet.Id, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var command = new DepositCommand(wallet.Id, 0, "BRL", Guid.NewGuid().ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Deposit amount must be greater than zero.");
    }

    [Fact]
    public async Task Handle_WithEmptyIdempotencyKey_ShouldFail()
    {
        // Arrange
        var command = new DepositCommand(Guid.NewGuid(), 100, "BRL", "");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Idempotency key is required.");
    }
}