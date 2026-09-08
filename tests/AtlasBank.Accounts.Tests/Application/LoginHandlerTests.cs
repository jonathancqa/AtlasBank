using AtlasBank.Accounts.Application.Abstractions;
using AtlasBank.Accounts.Application.Abstractions.Services;
using AtlasBank.Accounts.Application.Commands.Login;
using AtlasBank.Accounts.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace AtlasBank.Accounts.Tests.Application;

public sealed class LoginHandlerTests
{
    private readonly IAccountRepository _repository;
    private readonly IJwtService _jwtService;
    private readonly LoginHandler _handler;

    private const string ValidEmail = "jonathan@atlasbank.com";
    private const string ValidPassword = "Atlas@2026";
    private const string ValidToken = "valid.jwt.token";

    public LoginHandlerTests()
    {
        _repository = Substitute.For<IAccountRepository>();
        _jwtService = Substitute.For<IJwtService>();
        _handler = new LoginHandler(_repository, _jwtService);
    }

    private static Account CreateActiveAccount()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(ValidPassword);
        return Account.Create(
            "Jonathan Alves",
            ValidEmail,
            "40734024800",
            passwordHash).Value;
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var account = CreateActiveAccount();

        _repository.GetByEmailAsync(ValidEmail, Arg.Any<CancellationToken>())
            .Returns(account);
        _jwtService.GenerateToken(account).Returns(ValidToken);

        var command = new LoginCommand(ValidEmail, ValidPassword);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(ValidToken);
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldFail()
    {
        // Arrange
        _repository.GetByEmailAsync(ValidEmail, Arg.Any<CancellationToken>())
            .Returns((Account?)null);

        var command = new LoginCommand(ValidEmail, ValidPassword);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid credentials.");
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldFail()
    {
        // Arrange
        var account = CreateActiveAccount();

        _repository.GetByEmailAsync(ValidEmail, Arg.Any<CancellationToken>())
            .Returns(account);

        var command = new LoginCommand(ValidEmail, "WrongPassword@123");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid credentials.");
    }

    [Fact]
    public async Task Handle_WithInactiveAccount_ShouldFail()
    {
        // Arrange
        var account = CreateActiveAccount();
        account.Deactivate();

        _repository.GetByEmailAsync(ValidEmail, Arg.Any<CancellationToken>())
            .Returns(account);

        var command = new LoginCommand(ValidEmail, ValidPassword);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Account is inactive.");
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldNotExposeWhichFieldIsWrong()
    {
        // Arrange — tanto email inválido quanto senha inválida retornam a mesma mensagem
        _repository.GetByEmailAsync("wrong@email.com", Arg.Any<CancellationToken>())
            .Returns((Account?)null);

        var commandWithWrongEmail = new LoginCommand("wrong@email.com", ValidPassword);
        var commandWithWrongPassword = new LoginCommand(ValidEmail, "WrongPassword");

        // Act
        var resultWrongEmail = await _handler.Handle(commandWithWrongEmail, CancellationToken.None);

        // Assert
        resultWrongEmail.Error.Should().Be("Invalid credentials.");
    }
}