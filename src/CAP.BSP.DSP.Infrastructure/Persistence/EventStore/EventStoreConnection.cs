using EventStore.Client;

namespace CAP.BSP.DSP.Infrastructure.Persistence.EventStore;

/// <summary>
/// EventStoreDB connection manager.
/// Provides configured EventStoreClient for event sourcing operations.
/// </summary>
public class EventStoreConnection
{
    private readonly EventStoreClient _client;

    /// <summary>
    /// Initializes a new instance of the EventStoreConnection.
    /// </summary>
    /// <param name="connectionString">EventStoreDB connection string (e.g., "esdb://localhost:2113?tls=false").</param>
    public EventStoreConnection(string connectionString)
    {
        var settings = EventStoreClientSettings.Create(connectionString);
        _client = new EventStoreClient(settings);
    }

    /// <summary>
    /// Gets the EventStoreClient instance for stream operations.
    /// </summary>
    public EventStoreClient Client => _client;

    /// <summary>
    /// Disposes the EventStoreClient connection.
    /// </summary>
    public void Dispose()
    {
        _client?.Dispose();
    }
}
