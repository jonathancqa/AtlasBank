using AtlasBank.SharedKernel.Abstractions;
using AtlasBank.SharedKernel.Primitives;

namespace AtlasBank.SharedKernel.ValueObjects;

/// <summary>
/// Value Object que representa um valor monetário com sua moeda.
/// </summary>
public sealed class Money : ValueObject
{
    /// <summary>Valor monetário. Sempre maior ou igual a zero.</summary>
    public decimal Amount { get; }

    /// <summary>Código ISO 4217 da moeda. Ex: BRL, USD, EUR.</summary>
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Cria uma instância de Money validando amount e currency.
    /// </summary>
    public static Result<Money> Create(decimal amount, string currency)
    {
        if (amount < 0)
            return Result.Failure<Money>("Amount cannot be negative.");

        if (string.IsNullOrWhiteSpace(currency))
            return Result.Failure<Money>("Currency is required.");

        if (currency.Length != 3)
            return Result.Failure<Money>("Currency must be a 3-letter ISO code (e.g. BRL, USD).");

        return Result.Success(new Money(amount, currency.ToUpperInvariant()));
    }

    /// <summary>
    /// Cria um Money com valor zero. Útil para inicializar saldo de carteira.
    /// </summary>
    public static Money Zero(string currency = "BRL") => new(0, currency);

    /// <summary>
    /// Soma dois valores monetários. Falha se as moedas forem diferentes.
    /// </summary>
    public Result<Money> Add(Money other)
    {
        if (Currency != other.Currency)
            return Result.Failure<Money>($"Cannot add {Currency} and {other.Currency}.");

        return Result.Success(new Money(Amount + other.Amount, Currency));
    }

    /// <summary>
    /// Subtrai um valor monetário. Falha se moedas diferentes ou saldo insuficiente.
    /// </summary>
    public Result<Money> Subtract(Money other)
    {
        if (Currency != other.Currency)
            return Result.Failure<Money>($"Cannot subtract {Currency} and {other.Currency}.");

        if (Amount < other.Amount)
            return Result.Failure<Money>("Insufficient funds.");

        return Result.Success(new Money(Amount - other.Amount, Currency));
    }

    /// <summary>Retorna true se este valor é maior que o outro.</summary>
    public bool IsGreaterThan(Money other) => Amount > other.Amount;

    /// <summary>Retorna true se o valor é zero.</summary>
    public bool IsZero() => Amount == 0;

    public override string ToString() => $"{Currency} {Amount:F2}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}