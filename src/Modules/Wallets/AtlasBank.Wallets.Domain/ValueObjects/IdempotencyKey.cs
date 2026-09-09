using AtlasBank.SharedKernel.Abstractions;
using AtlasBank.SharedKernel.Primitives;

namespace AtlasBank.Wallets.Domain.ValueObjects;

/// <summary>
/// Value Object que representa uma chave de idempotência.
/// Garante que requisições repetidas não gerem transações duplicadas.
/// </summary>
public sealed class IdempotencyKey: ValueObject
{
    /// <summary>Vaor único da chave de idempotência.</summary>
    public string Value { get; }

    private IdempotencyKey(string value) => Value = value;

    /// <summary>
    /// Cria uma chave de idempotência a partir de um valor fornecido pelo cliente.
    /// </summary>
    public static Result<IdempotencyKey> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<IdempotencyKey>("Idempotency key is required.");

        if (value.Length > 100)
            return Result.Failure<IdempotencyKey>("Idempotency key must not exceed 100 characters.");

        return Result.Success(new IdempotencyKey(value));
    }

    /// <summary>
    /// Gera uma nova chave de idempotência única automaticamente.
    /// </summary>
    public static IdempotencyKey Generate()
        => new(Guid.NewGuid().ToString());

    public override string ToString() => Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}