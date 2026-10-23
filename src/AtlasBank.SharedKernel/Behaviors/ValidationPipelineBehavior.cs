using FluentValidation;
using MediatR;
using AtlasBank.SharedKernel.Primitives;

namespace AtlasBank.SharedKernel.Behaviors;

/// <summary>
/// Pipeline behavior do MediatR que intercepta requests e executa validações
/// antes de chegar no Handler. Elimina validação manual em cada Handler.
/// </summary>
public sealed class ValidationPipelineBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(e => e is not null)
            .ToList();

        if (failures.Count != 0)
        {
            var errors = string.Join("; ", failures.Select(f => f.ErrorMessage));
            return CreateFailureResult<TResponse>(errors);
        }

        return await next();
    }

    private static TResponse CreateFailureResult<T>(string error) where T : Result
    {
        // Usa reflection para criar Result.Failure ou Result<T>.Failure
        var type = typeof(T);

        if (type == typeof(Result))
            return (TResponse)(object)Result.Failure(error);

        var genericType = type.GetGenericArguments()[0];
        var method = typeof(Result)
            .GetMethods()
            .First(m => m.Name == "Failure" && m.IsGenericMethod)
            .MakeGenericMethod(genericType);

        return (TResponse)method.Invoke(null, [error])!;
    }
}