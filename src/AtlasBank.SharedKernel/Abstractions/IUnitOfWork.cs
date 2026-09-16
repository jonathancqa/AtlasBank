namespace AtlasBank.SharedKernel.Abstractions;

/// <summary>
/// Contrato para Unit of Work — gerencia o commit das operações de forma atômica.
/// Garante que múltiplas mudanças em agregados diferentes sejam persistidas juntas ou não.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persiste todas as mudanças pendentes no banco de dados.
    /// </summary>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}