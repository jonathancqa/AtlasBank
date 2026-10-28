namespace AtlasBank.SharedKernel.Exceptions;

/// <summary>
/// Exceção lançada quando ocorre um conflito de concorrência otimista no banco de dados.
/// </summary>
public sealed class ConcurrencyException : Exception
{
    public ConcurrencyException(string message = "A concurrency conflict occurred.")
        : base(message) { }

    public ConcurrencyException(string message, Exception innerException)
        : base(message, innerException) { }
}