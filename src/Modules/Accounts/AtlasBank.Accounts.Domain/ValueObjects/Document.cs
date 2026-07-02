using AtlasBank.SharedKernel.Abstractions;
using AtlasBank.SharedKernel.Primitives;

namespace AtlasBank.Accounts.Domain.ValueObjects;

/// <summary>
/// Value Object que representa um CPF válido.
/// Armazena apenas os dígitos, sem formatação.
/// </summary>
public sealed class Document : ValueObject
{
    /// <summary>CPF contendo apenas os 11 dígitos, sem pontos ou traços.</summary>
    public string Number { get; }

    private Document(string number) => Number = number;

    /// <summary>
    /// Cria um Document validando formato e dígitos verificadores do CPF.
    /// </summary>
    public static Result<Document> Create(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            return Result.Failure<Document>("Document is required.");

        var digits = new string(number.Where(char.IsDigit).ToArray());

        if (digits.Length != 11)
            return Result.Failure<Document>("CPF must contain 11 digits.");

        if (digits.Distinct().Count() == 1)
            return Result.Failure<Document>("CPF is invalid.");

        if (!IsValidCpf(digits))
            return Result.Failure<Document>("CPF is invalid.");

        return Result.Success(new Document(digits));
    }

    /// <summary>Retorna o CPF formatado (ex: 123.456.789-09).</summary>
    public string Formatted =>
        $"{Number[..3]}.{Number[3..6]}.{Number[6..9]}-{Number[9..]}";

    public override string ToString() => Formatted;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Number;
    }

    /// <summary>
    /// Valida os dígitos verificadores do CPF pelo algoritmo oficial da Receita Federal.
    /// </summary>
    private static bool IsValidCpf(string digits)
    {
        var numbers = digits.Select(d => int.Parse(d.ToString())).ToArray();

        // Primeiro dígito verificador
        var sum = 0;
        for (var i = 0; i < 9; i++)
            sum += numbers[i] * (10 - i);

        var remainder = sum % 11;
        var firstDigit = remainder < 2 ? 0 : 11 - remainder;

        if (numbers[9] != firstDigit)
            return false;

        // Segundo dígito verificador
        sum = 0;
        for (var i = 0; i < 10; i++)
            sum += numbers[i] * (11 - i);

        remainder = sum % 11;
        var secondDigit = remainder < 2 ? 0 : 11 - remainder;

        return numbers[10] == secondDigit;
    }
}