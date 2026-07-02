namespace AtlasBank.SharedKernel.Primitives;

/// <summary>
/// Representa o resultado de uma operação, com sucesso ou falha.
/// Elimina o uso de exceptions para controle de fluxo de negócio.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, string error)
    {
        if (isSuccess && error != string.Empty)
            throw new InvalidOperationException("Successful result cannot have an error.");
        if (!isSuccess && error == string.Empty)
            throw new InvalidOperationException("Failed result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>Indica se a operação foi bem-sucedida.</summary>
    public bool IsSuccess { get; }

    /// <summary>Indica se a operação falhou.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Mensagem de erro em caso de falha. Vazio em caso de sucesso.</summary>
    public string Error { get; }

    /// <summary>Cria um resultado de sucesso sem valor de retorno.</summary>
    public static Result Success() => new(true, string.Empty);

    /// <summary>Cria um resultado de falha com mensagem de erro.</summary>
    public static Result Failure(string error) => new(false, error);

    /// <summary>Cria um resultado de sucesso com valor de retorno.</summary>
    public static Result<T> Success<T>(T value) => new(value, true, string.Empty);

    /// <summary>Cria um resultado de falha tipado com mensagem de erro.</summary>
    public static Result<T> Failure<T>(string error) => new(default, false, error);
}

/// <summary>
/// Resultado de uma operação que retorna um valor em caso de sucesso.
/// </summary>
public class Result<T> : Result
{
    private readonly T? _value;

    protected internal Result(T? value, bool isSuccess, string error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Valor retornado pela operação.
    /// Lança exceção se acessado em caso de falha.
    /// </summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of a failed result.");
}