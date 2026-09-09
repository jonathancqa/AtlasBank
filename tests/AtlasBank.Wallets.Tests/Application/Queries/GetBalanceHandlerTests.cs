using AtlasBank.SharedKernel.ValueObjects;
using AtlasBank.Wallets.Application.Abstractions;
using AtlasBank.Wallets.Application.Queries.GetBalance;
using AtlasBank.Wallets.Domain.Entities;
using AtlasBank.Wallets.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace AtlasBank.Wallets.Tests.Application.Queries;

public sealed class GetBalanceHandlerTests
{
    private readonly IWalletRepository _repository;
    private readonly GetBalanceHandler _handler;

    public GetBalanceHandlerTests()
    {
        _repository = Substitute.For<IWalletRepository>();
        _handler = new GetBalanceHandler(_repository);
    }

    private static Wallet CreateWalletWithBalance(decimal amount = 100)
    {
        var wallet = Wallet.Create(Guid.NewGuid()).Value;
        if (amount > 0)
            wallet.Deposit(Money.Create(amount, "BRL").Value, IdempotencyKey.Generate());
        return wallet;
    }

    [Fact]
    public async Task Handle_WithValidWalletId_ShouldReturnBalance()
    {
        // Arrange
        var wallet = CreateWalletWithBalance(100);
        _repository.GetByIdAsync(wallet.Id, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var query = new GetBalanceQuery(wallet.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(100);
        result.Value.Currency.Should().Be("BRL");
        result.Value.WalletId.Should().Be(wallet.Id);
    }

    [Fact]
    public async Task Handle_WithWalletNotFound_ShouldFail()
    {
        // Arrange
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Wallet?)null);

        var query = new GetBalanceQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Wallet not found.");
    }

    [Fact]
    public async Task Handle_WithZeroBalance_ShouldReturnZero()
    {
        // Arrange
        var wallet = CreateWalletWithBalance(0);
        _repository.GetByIdAsync(wallet.Id, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var query = new GetBalanceQuery(wallet.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(0);
    }
}