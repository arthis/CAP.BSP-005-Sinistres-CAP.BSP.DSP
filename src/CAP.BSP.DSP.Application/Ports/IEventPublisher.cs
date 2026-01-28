using CAP.BSP.DSP.Domain.Events;

namespace CAP.BSP.DSP.Application.Ports;

/// <summary>
/// Port for publishing domain events to the message broker (RabbitMQ).
/// Implements the Outbox pattern for reliable event publishing.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes a domain event to the message broker.
    /// Events are published to the "bsp.events" topic exchange with routing key "sinistre.{eventType}".
    /// </summary>
    /// <param name="domainEvent">The domain event to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes multiple domain events to the message broker in a batch.
    /// </summary>
    /// <param name="domainEvents">The collection of domain events to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    Task PublishBatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
