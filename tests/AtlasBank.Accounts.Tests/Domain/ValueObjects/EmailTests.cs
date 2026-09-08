using AtlasBank.Accounts.Domain.ValueObjects;
using FluentAssertions;

namespace AtlasBank.Accounts.Tests.Domain.ValueObjects;

public sealed class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_ShouldSucceed()
    {
        // Arrange
        var email = "jonathan@atlasbank.com";

        // Act
        var result = Email.Create(email);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Address.Should().Be(email.ToLowerInvariant());
    }

    [Fact]
    public void Create_WithUpperCaseEmail_ShouldNormalizToLowerCase()
    {
        // Arrange
        var email = "JONATHAN@ATLASBANK.COM";

        // Act
        var result = Email.Create(email);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Address.Should().Be("jonathan@atlasbank.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyEmail_ShouldFail(string? email)
    {
        // Act
        var result = Email.Create(email!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Email is required.");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@dot")]
    [InlineData("missingatsign.com")]
    public void Create_WithInvalidFormat_ShouldFail(string email)
    {
        // Act
        var result = Email.Create(email);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Email format is invalid.");
    }

    [Fact]
    public void Create_WithEmailExceeding254Chars_ShouldFail()
    {
        // Arrange
        var email = new string('a', 250) + "@b.com";

        // Act
        var result = Email.Create(email);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Email must not exceed 254 characters.");
    }

    [Fact]
    public void TwoEmailsWithSameAddress_ShouldBeEqual()
    {
        // Arrange
        var email1 = Email.Create("jonathan@atlasbank.com").Value;
        var email2 = Email.Create("jonathan@atlasbank.com").Value;

        // Assert
        email1.Should().Be(email2);
    }

    [Fact]
    public void TwoEmailsWithDifferentAddress_ShouldNotBeEqual()
    {
        // Arrange
        var email1 = Email.Create("jonathan@atlasbank.com").Value;
        var email2 = Email.Create("other@atlasbank.com").Value;

        // Assert
        email1.Should().NotBe(email2);
    }
}