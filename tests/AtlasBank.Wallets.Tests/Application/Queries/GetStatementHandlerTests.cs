using AtlasBank.SharedKernel.ValueObjects;
using AtlasBank.Wallets.Application.Abstractions;
using AtlasBank.Wallets.Application.Queries.GetStatement;
using AtlasBank.Wallets.Domain.Entities;
using AtlasBank.Wallets.Domain.Enums;
using AtlasBank.Wallets.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace AtlasBank.Wallets.Tests.Application.Queries;

public sealed class GetStatementHandlerTests
{
    private readonly IWalletRepository _repository;
    private readonly GetStatementHandler _handler;

    public GetStatementHandlerTests()
    {
        _repository = Substitute.For<IWalletRepository>();
        _handler = new GetStatementHandler(_repository);
    }

    private static Wallet CreateWalletWithTransactions()
    {
        var wallet = Wallet.Create(Guid.NewGuid()).Value;
        wallet.Deposit(Money.Create(100, "BRL").Value, IdempotencyKey.Generate());
        wallet.Deposit(Money.Create(50, "BRL").Value, IdempotencyKey.Generate());
        wallet.Withdraw(Money.Create(30, "BRL").Value, IdempotencyKey.Generate());
        return wallet;
    }

    [Fact]
    public async Task Handle_WithValidWalletId_ShouldReturnStatement()
    {
        // Arrange
        var wallet = CreateWalletWithTransactions();
        _repository.GetByIdAsync(wallet.Id, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var query = new GetStatementQuery(wallet.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.WalletId.Should().Be(wallet.Id);
        result.Value.Balance.Should().Be(120);
        result.Value.TotalTransactions.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WithWalletNotFound_ShouldFail()
    {
        // Arrange
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Wallet?)null);

        var query = new GetStatementQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Wallet not found.");
    }

    [Fact]
    public async Task Handle_ShouldReturnTransactionsOrderedByDateDescending()
    {
        // Arrange
        var wallet = CreateWalletWithTransactions();
        _repository.GetByIdAsync(wallet.Id, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var query = new GetStatementQuery(wallet.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Transactions.Should().BeInDescendingOrder(t => t.CreatedAt);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var wallet = CreateWalletWithTransactions();
        _repository.GetByIdAsync(wallet.Id, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var query = new GetStatementQuery(wallet.Id, Page: 1, PageSize: 2);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Transactions.Should().HaveCount(2);
        result.Value.TotalTransactions.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectTransactionTypes()
    {
        // Arrange
        var wallet = CreateWalletWithTransactions();
        _repository.GetByIdAsync(wallet.Id, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var query = new GetStatementQuery(wallet.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Transactions
            .Count(t => t.Type == TransactionType.Deposit)
            .Should().Be(2);

        result.Value.Transactions
            .Count(t => t.Type == TransactionType.Withdrawal)
            .Should().Be(1);
    }
}