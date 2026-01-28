using CAP.BSP.DSP.Domain.Events;

namespace CAP.BSP.DSP.Domain.Aggregates;

/// <summary>
/// Base class for all aggregate roots in the domain using event sourcing pattern.
/// Manages domain events and provides the Apply pattern for event sourcing.
/// </summary>
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Current version of the aggregate (event stream version).
    /// Used for optimistic concurrency control in EventStoreDB.
    /// </summary>
    public long Version { get; protected set; } = -1;

    /// <summary>
    /// Read-only collection of uncommitted domain events.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a new domain event to the aggregate's uncommitted events.
    /// Also applies the event to the aggregate's state.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
        Apply(domainEvent);
        Version++;
    }

    /// <summary>
    /// Applies a domain event to the aggregate's state.
    /// Must be implemented by derived classes using pattern matching or visitor pattern.
    /// </summary>
    /// <param name="domainEvent">The domain event to apply.</param>
    protected abstract void Apply(IDomainEvent domainEvent);

    /// <summary>
    /// Clears all uncommitted domain events.
    /// Typically called after events are persisted to EventStoreDB.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Loads aggregate state from historical events (event sourcing rehydration).
    /// </summary>
    /// <param name="historicalEvents">Past events from the event store.</param>
    public void LoadFromHistory(IEnumerable<IDomainEvent> historicalEvents)
    {
        foreach (var @event in historicalEvents)
        {
            Apply(@event);
            Version++;
        }
    }
}
