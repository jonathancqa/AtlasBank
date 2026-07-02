using AtlasBank.SharedKernel.Abstractions;
using AtlasBank.SharedKernel.Primitives;

namespace AtlasBank.Accounts.Domain.ValueObjects;

/// <summary>
/// Value Object que representa um endereço de e-mail válido.
/// </summary>
public sealed class Email : ValueObject
{
    /// <summary>Endereço de e-mail normalizado em letras minúsculas.</summary>
    public string Address { get; }

    private Email(string address) => Address = address;

    /// <summary>
    /// Cria um Email validando formato e tamanho.
    /// </summary>
    public static Result<Email> Create(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return Result.Failure<Email>("Email is required.");

        if (address.Length > 254)
            return Result.Failure<Email>("Email must not exceed 254 characters.");

        if (!address.Contains('@') || !address.Contains('.'))
            return Result.Failure<Email>("Email format is invalid.");

        return Result.Success(new Email(address.Trim().ToLowerInvariant()));
    }

    public override string ToString() => Address;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Address;
    }
}