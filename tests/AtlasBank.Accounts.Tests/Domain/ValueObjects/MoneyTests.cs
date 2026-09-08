using AtlasBank.SharedKernel.ValueObjects;
using FluentAssertions;

namespace AtlasBank.Accounts.Tests.Domain.ValueObjects;

public sealed class MoneyTests
{
    [Fact]
    public void Create_WithValidAmount_ShouldSucceed()
    {
        // Act
        var result = Money.Create(100.50m, "BRL");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(100.50m);
        result.Value.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldSucceed()
    {
        // Act
        var result = Money.Create(0, "BRL");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(0);
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldFail()
    {
        // Act
        var result = Money.Create(-1, "BRL");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Amount cannot be negative.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyCurrency_ShouldFail(string? currency)
    {
        // Act
        var result = Money.Create(100, currency!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Currency is required.");
    }

    [Theory]
    [InlineData("BR")]
    [InlineData("BRLL")]
    public void Create_WithInvalidCurrencyLength_ShouldFail(string currency)
    {
        // Act
        var result = Money.Create(100, currency);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Currency must be a 3-letter ISO code (e.g. BRL, USD).");
    }

    [Fact]
    public void Create_WithLowerCaseCurrency_ShouldNormalizeToUpperCase()
    {
        // Act
        var result = Money.Create(100, "brl");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Add_WithSameCurrency_ShouldSucceed()
    {
        // Arrange
        var money1 = Money.Create(100, "BRL").Value;
        var money2 = Money.Create(50, "BRL").Value;

        // Act
        var result = money1.Add(money2);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(150);
    }

    [Fact]
    public void Add_WithDifferentCurrency_ShouldFail()
    {
        // Arrange
        var money1 = Money.Create(100, "BRL").Value;
        var money2 = Money.Create(50, "USD").Value;

        // Act
        var result = money1.Add(money2);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Cannot add BRL and USD.");
    }

    [Fact]
    public void Subtract_WithSufficientFunds_ShouldSucceed()
    {
        // Arrange
        var money1 = Money.Create(100, "BRL").Value;
        var money2 = Money.Create(50, "BRL").Value;

        // Act
        var result = money1.Subtract(money2);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(50);
    }

    [Fact]
    public void Subtract_WithInsufficientFunds_ShouldFail()
    {
        // Arrange
        var money1 = Money.Create(50, "BRL").Value;
        var money2 = Money.Create(100, "BRL").Value;

        // Act
        var result = money1.Subtract(money2);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Insufficient funds.");
    }

    [Fact]
    public void Subtract_WithDifferentCurrency_ShouldFail()
    {
        // Arrange
        var money1 = Money.Create(100, "BRL").Value;
        var money2 = Money.Create(50, "USD").Value;

        // Act
        var result = money1.Subtract(money2);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Cannot subtract BRL and USD.");
    }

    [Fact]
    public void Zero_ShouldReturnZeroAmount()
    {
        // Act
        var money = Money.Zero();

        // Assert
        money.Amount.Should().Be(0);
        money.Currency.Should().Be("BRL");
        money.IsZero().Should().BeTrue();
    }

    [Fact]
    public void TwoMoneyWithSameValueAndCurrency_ShouldBeEqual()
    {
        // Arrange
        var money1 = Money.Create(100, "BRL").Value;
        var money2 = Money.Create(100, "BRL").Value;

        // Assert
        money1.Should().Be(money2);
    }

    [Fact]
    public void TwoMoneyWithDifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        var money1 = Money.Create(100, "BRL").Value;
        var money2 = Money.Create(200, "BRL").Value;

        // Assert
        money1.Should().NotBe(money2);
    }
}