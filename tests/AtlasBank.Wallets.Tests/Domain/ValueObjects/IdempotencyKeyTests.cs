using AtlasBank.Wallets.Domain.ValueObjects;
using FluentAssertions;

namespace AtlasBank.Wallets.Tests.Domain.ValueObjects;

public sealed class IdempotencyKeyTests
{
    [Fact]
    public void Create_WithValidValue_ShouldSucceed()
    {
        // Arrange
        var value = Guid.NewGuid().ToString();

        // Act
        var result = IdempotencyKey.Create(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyValue_ShouldFail(string? value)
    {
        // Act
        var result = IdempotencyKey.Create(value!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Idempotency key is required.");
    }

    [Fact]
    public void Create_WithValueExceeding100Chars_ShouldFail()
    {
        // Arrange
        var value = new string('a', 101);

        // Act
        var result = IdempotencyKey.Create(value);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Idempotency key must not exceed 100 characters.");
    }

    [Fact]
    public void Generate_ShouldReturnUniqueKeys()
    {
        // Act
        var key1 = IdempotencyKey.Generate();
        var key2 = IdempotencyKey.Generate();

        // Assert
        key1.Value.Should().NotBe(key2.Value);
    }

    [Fact]
    public void TwoKeysWithSameValue_ShouldBeEqual()
    {
        // Arrange
        var value = Guid.NewGuid().ToString();
        var key1 = IdempotencyKey.Create(value).Value;
        var key2 = IdempotencyKey.Create(value).Value;

        // Assert
        key1.Should().Be(key2);
    }

    [Fact]
    public void TwoKeysWithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var key1 = IdempotencyKey.Generate();
        var key2 = IdempotencyKey.Generate();

        // Assert
        key1.Should().NotBe(key2);
    }
}