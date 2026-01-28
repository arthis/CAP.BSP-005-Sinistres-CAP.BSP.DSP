using MediatR;

namespace CAP.BSP.DSP.Domain.Events;

/// <summary>
/// Marker interface for domain events in the event sourcing system.
/// All domain events must implement this interface to be persisted in EventStoreDB.
/// Inherits from INotification to enable MediatR event publishing.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Timestamp when the event occurred (UTC).
    /// </summary>
    DateTime OccurredAt { get; }

    /// <summary>
    /// Correlation ID for tracing related operations across services.
    /// </summary>
    string CorrelationId { get; }

    /// <summary>
    /// Causation ID - the ID of the command or event that caused this event.
    /// </summary>
    string? CausationId { get; }
}
