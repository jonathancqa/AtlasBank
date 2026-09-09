using AtlasBank.SharedKernel.ValueObjects;
using AtlasBank.Wallets.Application.Abstractions;
using AtlasBank.Wallets.Application.Commands.Transfer;
using AtlasBank.Wallets.Domain.Entities;
using AtlasBank.Wallets.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace AtlasBank.Wallets.Tests.Application.Commands;

public sealed class TransferHandlerTests
{
    private readonly IWalletRepository _repository;
    private readonly TransferHandler _handler;

    public TransferHandlerTests()
    {
        _repository = Substitute.For<IWalletRepository>();
        _handler = new TransferHandler(_repository);
    }

    private static Wallet CreateWalletWithBalance(decimal amount = 100)
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
        var source = CreateWalletWithBalance(100);
        var destination = CreateWalletWithBalance(0);

        _repository.GetByIdAsync(source.Id, Arg.Any<CancellationToken>())
            .Returns(source);
        _repository.GetByIdAsync(destination.Id, Arg.Any<CancellationToken>())
            .Returns(destination);

        var command = new TransferCommand(
            source.Id,
            destination.Id,
            50,
            "BRL",
            Guid.NewGuid().ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCallUpdateAsyncTwice()
    {
        // Arrange
        var source = CreateWalletWithBalance(100);
        var destination = CreateWalletWithBalance(0);

        _repository.GetByIdAsync(source.Id, Arg.Any<CancellationToken>())
            .Returns(source);
        _repository.GetByIdAsync(destination.Id, Arg.Any<CancellationToken>())
            .Returns(destination);

        var command = new TransferCommand(
            source.Id,
            destination.Id,
            50,
            "BRL",
            Guid.NewGuid().ToString());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — persiste ambas as carteiras
        await _repository.Received(2).UpdateAsync(Arg.Any<Wallet>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithSameWalletId_ShouldFail()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var command = new TransferCommand(walletId, walletId, 50, "BRL", Guid.NewGuid().ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Cannot transfer to the same wallet.");
    }

    [Fact]
    public async Task Handle_WithSourceWalletNotFound_ShouldFail()
    {
        // Arrange
        var destination = CreateWalletWithBalance(0);

        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Wallet?)null);

        var command = new TransferCommand(
            Guid.NewGuid(),
            destination.Id,
            50,
            "BRL",
            Guid.NewGuid().ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Source wallet not found.");
    }

    [Fact]
    public async Task Handle_WithDestinationWalletNotFound_ShouldFail()
    {
        // Arrange
        var source = CreateWalletWithBalance(100);

        _repository.GetByIdAsync(source.Id, Arg.Any<CancellationToken>())
            .Returns(source);
        _repository.GetByIdAsync(Arg.Is<Guid>(id => id != source.Id), Arg.Any<CancellationToken>())
            .Returns((Wallet?)null);

        var command = new TransferCommand(
            source.Id,
            Guid.NewGuid(),
            50,
            "BRL",
            Guid.NewGuid().ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Destination wallet not found.");
    }

    [Fact]
    public async Task Handle_WithInsufficientBalance_ShouldFail()
    {
        // Arrange
        var source = CreateWalletWithBalance(50);
        var destination = CreateWalletWithBalance(0);

        _repository.GetByIdAsync(source.Id, Arg.Any<CancellationToken>())
            .Returns(source);
        _repository.GetByIdAsync(destination.Id, Arg.Any<CancellationToken>())
            .Returns(destination);

        var command = new TransferCommand(
            source.Id,
            destination.Id,
            100,
            "BRL",
            Guid.NewGuid().ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Insufficient funds.");
    }

    [Fact]
    public async Task Handle_WithEmptyIdempotencyKey_ShouldFail()
    {
        // Arrange
        var command = new TransferCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            50,
            "BRL",
            "");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Idempotency key is required.");
    }
}