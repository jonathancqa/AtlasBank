using AtlasBank.Accounts.Domain.ValueObjects;
using FluentAssertions;

namespace AtlasBank.Accounts.Tests.Domain.ValueObjects;

public sealed class DocumentTests
{
    [Fact]
    public void Create_WithValidCpf_ShouldSucceed()
    {
        // Arrange
        var cpf = "407.340.248-00";

        // Act
        var result = Document.Create(cpf);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Number.Should().Be("40734024800");
    }

    [Fact]
    public void Create_WithValidCpfWithoutFormatting_ShouldSucceed()
    {
        // Arrange
        var cpf = "40734024800";

        // Act
        var result = Document.Create(cpf);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Number.Should().Be("40734024800");
    }

    [Fact]
    public void Create_WithValidCpf_ShouldReturnFormattedCpf()
    {
        // Arrange
        var cpf = "40734024800";

        // Act
        var result = Document.Create(cpf);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Formatted.Should().Be("407.340.248-00");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyCpf_ShouldFail(string? cpf)
    {
        // Act
        var result = Document.Create(cpf!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Document is required.");
    }

    [Theory]
    [InlineData("1234567890")]   // 10 dígitos
    [InlineData("123456789012")] // 12 dígitos
    public void Create_WithWrongLength_ShouldFail(string cpf)
    {
        // Act
        var result = Document.Create(cpf);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("CPF must contain 11 digits.");
    }

    [Theory]
    [InlineData("00000000000")]
    [InlineData("11111111111")]
    [InlineData("99999999999")]
    public void Create_WithAllSameDigits_ShouldFail(string cpf)
    {
        // Act
        var result = Document.Create(cpf);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("CPF is invalid.");
    }

    [Fact]
    public void Create_WithInvalidCheckDigits_ShouldFail()
    {
        // Arrange — CPF com dígitos verificadores errados
        var cpf = "40734024801";

        // Act
        var result = Document.Create(cpf);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("CPF is invalid.");
    }

    [Fact]
    public void TwoDocumentsWithSameNumber_ShouldBeEqual()
    {
        // Arrange
        var doc1 = Document.Create("40734024800").Value;
        var doc2 = Document.Create("40734024800").Value;

        // Assert
        doc1.Should().Be(doc2);
    }
}