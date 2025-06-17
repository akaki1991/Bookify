namespace Bookify.Domain.Abstractions;

public abstract class Entity
{
    public Entity(Guid id) => Id = id;

    protected Entity() { }

    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; init; }

    public IReadOnlyList<IDomainEvent> GetDomainEvents() => [.. _domainEvents];

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
