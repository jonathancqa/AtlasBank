using AtlasBank.Wallets.Application.Abstractions;
using AtlasBank.Wallets.Application.Commands.CreateWallet;
using AtlasBank.Wallets.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace AtlasBank.Wallets.Tests.Application.Commands;

public sealed class CreateWalletHandlerTests
{
    private readonly IWalletRepository _repository;
    private readonly CreateWalletHandler _handler;

    private static readonly Guid ValidAccountId = Guid.NewGuid();

    public CreateWalletHandlerTests()
    {
        _repository = Substitute.For<IWalletRepository>();
        _handler = new CreateWalletHandler(_repository);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldSucceed()
    {
        // Arrange
        _repository.ExistsByAccountIdAsync(ValidAccountId, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateWalletCommand(ValidAccountId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCallAddAsync()
    {
        // Arrange
        _repository.ExistsByAccountIdAsync(ValidAccountId, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateWalletCommand(ValidAccountId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Any<Wallet>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWalletAlreadyExists_ShouldFail()
    {
        // Arrange
        _repository.ExistsByAccountIdAsync(ValidAccountId, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new CreateWalletCommand(ValidAccountId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Account already has a wallet.");
    }

    [Fact]
    public async Task Handle_WhenWalletAlreadyExists_ShouldNotCallAddAsync()
    {
        // Arrange
        _repository.ExistsByAccountIdAsync(ValidAccountId, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new CreateWalletCommand(ValidAccountId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repository.DidNotReceive().AddAsync(Arg.Any<Wallet>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyAccountId_ShouldFail()
    {
        // Arrange
        _repository.ExistsByAccountIdAsync(Guid.Empty, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateWalletCommand(Guid.Empty);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Account ID is required.");
    }
}