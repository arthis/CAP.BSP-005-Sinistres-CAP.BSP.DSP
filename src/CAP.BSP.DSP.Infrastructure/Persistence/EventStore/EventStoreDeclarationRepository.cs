using CAP.BSP.DSP.Application.Ports;
using CAP.BSP.DSP.Domain.Aggregates;
using EventStore.Client;

namespace CAP.BSP.DSP.Infrastructure.Persistence.EventStore;

/// <summary>
/// EventStoreDB implementation of IDeclarationRepository.
/// Persists aggregates as event streams using event sourcing pattern.
/// </summary>
public class EventStoreDeclarationRepository : IDeclarationRepository
{
    private readonly EventStoreConnection _connection;

    public EventStoreDeclarationRepository(EventStoreConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <summary>
    /// Saves an aggregate to EventStoreDB by appending its uncommitted events.
    /// Stream name format: declaration-{aggregateId}
    /// </summary>
    public async Task SaveAsync(AggregateRoot aggregate, CancellationToken cancellationToken = default)
    {
        if (aggregate == null)
        {
            throw new ArgumentNullException(nameof(aggregate));
        }

        var domainEvents = aggregate.DomainEvents.ToList();
        if (!domainEvents.Any())
        {
            return; // No events to persist
        }

        // Get aggregate ID from first event (DeclarationId)
        var firstEvent = domainEvents.First() as Domain.Events.SinistreDeclare;
        if (firstEvent == null)
        {
            throw new InvalidOperationException("First event must be SinistreDeclare");
        }

        var streamName = $"declaration-{firstEvent.DeclarationId}";
        var eventData = domainEvents.Select(EventSerializer.SerializeEvent).ToArray();

        // Append events to stream (optimistic concurrency with expected version)
        var expectedVersion = aggregate.Version == 0
            ? StreamState.NoStream
            : StreamState.StreamExists;

        await _connection.Client.AppendToStreamAsync(
            streamName,
            expectedVersion,
            eventData,
            cancellationToken: cancellationToken
        );

        aggregate.ClearDomainEvents();
    }

    /// <summary>
    /// Retrieves an aggregate by rehydrating it from its event stream.
    /// </summary>
    public async Task<T?> GetByIdAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : AggregateRoot, new()
    {
        var streamName = $"declaration-{id}";

        try
        {
            var events = _connection.Client.ReadStreamAsync(
                Direction.Forwards,
                streamName,
                StreamPosition.Start,
                cancellationToken: cancellationToken
            );

            var domainEvents = new List<Domain.Events.IDomainEvent>();
            await foreach (var resolvedEvent in events)
            {
                var domainEvent = EventSerializer.DeserializeEvent<Domain.Events.SinistreDeclare>(resolvedEvent);
                if (domainEvent != null)
                {
                    domainEvents.Add(domainEvent);
                }
            }

            if (!domainEvents.Any())
            {
                return null;
            }

            var aggregate = new T();
            aggregate.LoadFromHistory(domainEvents);
            return aggregate;
        }
        catch (StreamNotFoundException)
        {
            return null;
        }
    }
}
