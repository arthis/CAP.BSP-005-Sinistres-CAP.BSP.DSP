using RabbitMQ.Client;

namespace CAP.BSP.DSP.Infrastructure.Messaging;

/// <summary>
/// RabbitMQ connection manager with connection pooling.
/// Provides IConnection and IModel (channel) instances for message publishing.
/// </summary>
public class RabbitMqConnection : IDisposable
{
    private readonly IConnection _connection;
    private readonly List<IModel> _channelPool = new();
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the RabbitMqConnection.
    /// </summary>
    /// <param name="hostName">RabbitMQ host name (e.g., "localhost").</param>
    /// <param name="userName">RabbitMQ username.</param>
    /// <param name="password">RabbitMQ password.</param>
    public RabbitMqConnection(string hostName, string userName, string password)
    {
        var factory = new ConnectionFactory
        {
            HostName = hostName,
            UserName = userName,
            Password = password,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        _connection = factory.CreateConnection();
    }

    /// <summary>
    /// Gets a channel from the pool or creates a new one.
    /// Channels should be returned to the pool after use for reuse.
    /// </summary>
    /// <returns>An IModel (channel) instance.</returns>
    public IModel GetChannel()
    {
        lock (_lock)
        {
            if (_channelPool.Count > 0)
            {
                var channel = _channelPool[0];
                _channelPool.RemoveAt(0);
                return channel;
            }

            return _connection.CreateModel();
        }
    }

    /// <summary>
    /// Returns a channel to the pool for reuse.
    /// </summary>
    /// <param name="channel">The channel to return.</param>
    public void ReturnChannel(IModel channel)
    {
        lock (_lock)
        {
            if (channel.IsOpen)
            {
                _channelPool.Add(channel);
            }
            else
            {
                channel.Dispose();
            }
        }
    }

    /// <summary>
    /// Disposes the connection and all pooled channels.
    /// </summary>
    public void Dispose()
    {
        lock (_lock)
        {
            foreach (var channel in _channelPool)
            {
                channel?.Dispose();
            }
            _channelPool.Clear();
        }

        _connection?.Dispose();
    }
}
