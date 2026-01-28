using System.Text.Json;
using CAP.BSP.DSP.Domain.Events;
using EventStore.Client;

namespace CAP.BSP.DSP.Infrastructure.Persistence.EventStore;

/// <summary>
/// Serializes domain events to EventStoreDB EventData format.
/// Uses JSON serialization for event payloads.
/// </summary>
public class EventSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a domain event to EventStoreDB EventData.
    /// </summary>
    /// <param name="domainEvent">The domain event to serialize.</param>
    /// <returns>EventData for EventStoreDB.</returns>
    public static EventData SerializeEvent(IDomainEvent domainEvent)
    {
        var eventType = domainEvent.GetType().Name;
        var jsonData = JsonSerializer.SerializeToUtf8Bytes(domainEvent, domainEvent.GetType(), JsonOptions);
        var metadata = JsonSerializer.SerializeToUtf8Bytes(new
        {
            eventType,
            occurredAt = domainEvent.OccurredAt,
            correlationId = domainEvent.CorrelationId,
            causationId = domainEvent.CausationId
        }, JsonOptions);

        return new EventData(
            Uuid.FromGuid(domainEvent.EventId),
            eventType,
            jsonData,
            metadata
        );
    }

    /// <summary>
    /// Deserializes EventStoreDB ResolvedEvent to a domain event.
    /// </summary>
    /// <typeparam name="T">The domain event type.</typeparam>
    /// <param name="resolvedEvent">The resolved event from EventStoreDB.</param>
    /// <returns>The deserialized domain event.</returns>
    public static T? DeserializeEvent<T>(ResolvedEvent resolvedEvent) where T : IDomainEvent
    {
        var jsonData = resolvedEvent.Event.Data.ToArray();
        return JsonSerializer.Deserialize<T>(jsonData, JsonOptions);
    }
}
