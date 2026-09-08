using AtlasBank.Accounts.Domain.Entities;
using AtlasBank.Accounts.Domain.Events;
using FluentAssertions;

namespace AtlasBank.Accounts.Tests.Domain.Entities;

public sealed class AccountTests
{
    private const string ValidFullName = "Jonathan Alves";
    private const string ValidEmail = "jonathan@atlasbank.com";
    private const string ValidDocument = "40734024800";
    private const string ValidPasswordHash = "$2a$11$validhashfortesting123456789012345678901234";

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Act
        var result = Account.Create(ValidFullName, ValidEmail, ValidDocument, ValidPasswordHash);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FullName.Should().Be(ValidFullName);
        result.Value.Email.Address.Should().Be(ValidEmail);
        result.Value.Document.Number.Should().Be(ValidDocument);
        result.Value.Status.Should().Be(AccountStatus.Active);
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithValidData_ShouldRaiseDomainEvent()
    {
        // Act
        var result = Account.Create(ValidFullName, ValidEmail, ValidDocument, ValidPasswordHash);

        // Assert
        result.Value.DomainEvents.Should().HaveCount(1);
        result.Value.DomainEvents.First().Should().BeOfType<AccountCreatedEvent>();
    }

    [Fact]
    public void Create_WithValidData_ShouldGenerateId()
    {
        // Act
        var result = Account.Create(ValidFullName, ValidEmail, ValidDocument, ValidPasswordHash);

        // Assert
        result.Value.Id.Should().NotBe(Guid.Empty);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyFullName_ShouldFail(string? fullName)
    {
        // Act
        var result = Account.Create(fullName!, ValidEmail, ValidDocument, ValidPasswordHash);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Full name is required.");
    }

    [Fact]
    public void Create_WithShortFullName_ShouldFail()
    {
        // Act
        var result = Account.Create("Jo", ValidEmail, ValidDocument, ValidPasswordHash);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Full name must be at least 3 characters.");
    }

    [Fact]
    public void Create_WithInvalidEmail_ShouldFail()
    {
        // Act
        var result = Account.Create(ValidFullName, "invalid-email", ValidDocument, ValidPasswordHash);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Email format is invalid.");
    }

    [Fact]
    public void Create_WithInvalidDocument_ShouldFail()
    {
        // Act
        var result = Account.Create(ValidFullName, ValidEmail, "00000000000", ValidPasswordHash);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("CPF is invalid.");
    }

    [Fact]
    public void Create_WithEmptyPasswordHash_ShouldFail()
    {
        // Act
        var result = Account.Create(ValidFullName, ValidEmail, ValidDocument, "");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Password hash is required.");
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldSucceed()
    {
        // Arrange
        var account = Account.Create(ValidFullName, ValidEmail, ValidDocument, ValidPasswordHash).Value;

        // Act
        var result = account.Deactivate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        account.Status.Should().Be(AccountStatus.Inactive);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldFail()
    {
        // Arrange
        var account = Account.Create(ValidFullName, ValidEmail, ValidDocument, ValidPasswordHash).Value;
        account.Deactivate();

        // Act
        var result = account.Deactivate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Account is already inactive.");
    }

    [Fact]
    public void Activate_WhenInactive_ShouldSucceed()
    {
        // Arrange
        var account = Account.Create(ValidFullName, ValidEmail, ValidDocument, ValidPasswordHash).Value;
        account.Deactivate();

        // Act
        var result = account.Activate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        account.Status.Should().Be(AccountStatus.Active);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldFail()
    {
        // Arrange
        var account = Account.Create(ValidFullName, ValidEmail, ValidDocument, ValidPasswordHash).Value;

        // Act
        var result = account.Activate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Account is already active.");
    }
}