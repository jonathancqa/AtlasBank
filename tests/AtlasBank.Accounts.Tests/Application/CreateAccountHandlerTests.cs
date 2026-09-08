using AtlasBank.Accounts.Application.Abstractions;
using AtlasBank.Accounts.Application.Commands.CreateAccount;
using AtlasBank.Accounts.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace AtlasBank.Accounts.Tests.Application;

public sealed class CreateAccountHandlerTests
{
    private readonly IAccountRepository _repository;
    private readonly CreateAccountHandler _handler;

    private const string ValidFullName = "Jonathan Alves";
    private const string ValidEmail = "jonathan@atlasbank.com";
    private const string ValidDocument = "40734024800";
    private const string ValidPassword = "Atlas@2026";

    public CreateAccountHandlerTests()
    {
        _repository = Substitute.For<IAccountRepository>();
        _handler = new CreateAccountHandler(_repository);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldSucceed()
    {
        // Arrange
        _repository.ExistsByEmailAsync(ValidEmail, Arg.Any<CancellationToken>())
            .Returns(false);
        _repository.ExistsByDocumentAsync(ValidDocument, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateAccountCommand(ValidFullName, ValidEmail, ValidDocument, ValidPassword);

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
        _repository.ExistsByEmailAsync(ValidEmail, Arg.Any<CancellationToken>())
            .Returns(false);
        _repository.ExistsByDocumentAsync(ValidDocument, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateAccountCommand(ValidFullName, ValidEmail, ValidDocument, ValidPassword);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Any<Account>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ShouldFail()
    {
        // Arrange
        _repository.ExistsByEmailAsync(ValidEmail, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new CreateAccountCommand(ValidFullName, ValidEmail, ValidDocument, ValidPassword);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("An account with this email already exists.");
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ShouldNotCallAddAsync()
    {
        // Arrange
        _repository.ExistsByEmailAsync(ValidEmail, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new CreateAccountCommand(ValidFullName, ValidEmail, ValidDocument, ValidPassword);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repository.DidNotReceive().AddAsync(Arg.Any<Account>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDuplicateDocument_ShouldFail()
    {
        // Arrange
        _repository.ExistsByEmailAsync(ValidEmail, Arg.Any<CancellationToken>())
            .Returns(false);
        _repository.ExistsByDocumentAsync(ValidDocument, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new CreateAccountCommand(ValidFullName, ValidEmail, ValidDocument, ValidPassword);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("An account with this document already exists.");
    }
}