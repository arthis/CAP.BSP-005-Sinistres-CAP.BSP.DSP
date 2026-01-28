using System.Text;
using System.Text.Json;
using CAP.BSP.DSP.Application.Ports;
using CAP.BSP.DSP.Domain.Events;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace CAP.BSP.DSP.Infrastructure.Messaging;

public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly RabbitMqConnection _connection;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly string _exchangeName;
    private const string ExchangeType = "topic";

    public RabbitMqEventPublisher(
        RabbitMqConnection connection,
        ILogger<RabbitMqEventPublisher> logger,
        string environment = "")
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _exchangeName = string.IsNullOrWhiteSpace(environment) 
            ? "bsp.events" 
            : $"bsp.events.{environment}";
        
        EnsureExchangeExists();
    }

    public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        IModel? channel = null;
        try
        {
            channel = _connection.GetChannel();
            var routingKey = GetRoutingKey(domainEvent);
            var eventType = domainEvent.GetType().Name;

            _logger.LogInformation(
                "Publishing event {EventType} with ID {EventId} to exchange {ExchangeName} with routing key {RoutingKey}",
                eventType,
                domainEvent.EventId,
                _exchangeName,
                routingKey);

            var message = SerializeEvent(domainEvent);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.MessageId = domainEvent.EventId.ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Type = eventType;
            
            // Add correlation ID to headers
            properties.Headers = new Dictionary<string, object>
            {
                { "correlationId", domainEvent.CorrelationId ?? string.Empty },
                { "causationId", domainEvent.CausationId ?? string.Empty },
                { "eventType", eventType },
                { "occurredAt", domainEvent.OccurredAt.ToString("O") }
            };

            channel.BasicPublish(
                exchange: _exchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation(
                "Event {EventType} with ID {EventId} published successfully",
                eventType,
                domainEvent.EventId);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to publish event {EventType} with ID {EventId}",
                domainEvent.GetType().Name,
                domainEvent.EventId);
            throw;
        }
        finally
        {
            if (channel != null)
            {
                _connection.ReturnChannel(channel);
            }
        }
    }

    public async Task PublishBatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            await PublishAsync(domainEvent, cancellationToken);
        }
    }

    private string GetRoutingKey(IDomainEvent domainEvent)
    {
        // Convert event type to routing key: SinistreDeclare -> sinistre.declare
        var eventType = domainEvent.GetType().Name;
        
        // Handle specific event types
        return eventType switch
        {
            "SinistreDeclare" => "sinistre.declare",
            "SinistreValide" => "sinistre.valide",
            "SinistreAnnule" => "sinistre.annule",
            _ => $"sinistre.{eventType.ToLowerInvariant()}"
        };
    }

    private string SerializeEvent(IDomainEvent domainEvent)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        return JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), options);
    }

    private void EnsureExchangeExists()
    {
        IModel? channel = null;
        try
        {
            channel = _connection.GetChannel();
            channel.ExchangeDeclare(
                exchange: _exchangeName,
                type: ExchangeType,
                durable: true,
                autoDelete: false);

            _logger.LogInformation("Exchange {ExchangeName} declared successfully", _exchangeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to declare exchange {ExchangeName}", _exchangeName);
            throw;
        }
        finally
        {
            if (channel != null)
            {
                _connection.ReturnChannel(channel);
            }
        }
    }
}
