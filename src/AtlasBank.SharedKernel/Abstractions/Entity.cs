namespace AtlasBank.SharedKernel.Abstractions;

/// <summary>
/// Classe base para entidades de domínio.
/// Uma entidade é definida pela sua identidade (Id), não pelos seus valores.
/// </summary>
public abstract class Entity
{
    /// <summary>
    /// Identificador único da entidade.
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Construtor padrão — gera um novo Id automaticamente.
    /// </summary>
    protected Entity() => Id = Guid.NewGuid();

    /// <summary>
    /// Construtor para reconstituição — usado ao carregar do banco de dados.
    /// </summary>
    protected Entity(Guid id) => Id = id;

    /// <summary>
    /// Duas entidades são iguais se têm o mesmo tipo e o mesmo Id.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity? left, Entity? right)
        => Equals(left, right);

    public static bool operator !=(Entity? left, Entity? right)
        => !Equals(left, right);
}
