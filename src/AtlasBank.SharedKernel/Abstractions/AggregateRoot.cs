namespace AtlasBank.SharedKernel.Abstractions;

/// <summary>
/// Classe base para Aggregate Roots. Herda identidade de <see cref="Entity"/>
/// e adiciona suporte a Domain Events para comunicação desacoplada entre módulos.
/// </summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<object> _domainEvents = [];

    /// <summary>
    /// Eventos levantados por este agregado aguardando despacho.
    /// </summary>
    public IReadOnlyList<object> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot() { }
    protected AggregateRoot(Guid id) : base(id) { }

    /// <summary>
    /// Registra um evento de domínio para ser despachado após persistência.
    /// </summary>
    protected void RaiseDomainEvent(object domainEvent)
        => _domainEvents.Add(domainEvent);

    /// <summary>
    /// Limpa os eventos após serem despachados pelo MediatR.
    /// </summary>
    public void ClearDomainEvents()
        => _domainEvents.Clear();
}
