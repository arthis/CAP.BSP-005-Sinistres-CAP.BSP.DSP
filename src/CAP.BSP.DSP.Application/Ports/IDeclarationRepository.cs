using CAP.BSP.DSP.Domain.Aggregates;

namespace CAP.BSP.DSP.Application.Ports;

/// <summary>
/// Repository port for persisting and retrieving declaration aggregates from EventStoreDB.
/// Follows the Repository pattern for aggregate persistence in event sourcing.
/// </summary>
public interface IDeclarationRepository
{
    /// <summary>
    /// Persists a declaration aggregate to the event store.
    /// Appends uncommitted domain events to the aggregate's event stream.
    /// </summary>
    /// <param name="aggregate">The declaration aggregate to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    Task SaveAsync(AggregateRoot aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a declaration aggregate from the event store by its unique identifier.
    /// Rehydrates the aggregate by replaying all historical events from the stream.
    /// </summary>
    /// <typeparam name="T">Type of aggregate root to retrieve.</typeparam>
    /// <param name="id">Unique identifier of the aggregate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The rehydrated aggregate, or null if not found.</returns>
    Task<T?> GetByIdAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : AggregateRoot, new();
}
