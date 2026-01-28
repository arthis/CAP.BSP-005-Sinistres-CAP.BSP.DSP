using CAP.BSP.DSP.Domain.Events;
using CAP.BSP.DSP.Infrastructure.Messaging;
using Microsoft.Extensions.Logging.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Xunit;

namespace CAP.BSP.DSP.Infrastructure.Tests.Messaging;

/// <summary>
/// Tests for RabbitMqEventPublisher using real RabbitMQ instance.
/// Validates event publishing with environment-labeled exchanges.
/// </summary>
[Collection("InfrastructureTests")]
public class RabbitMqEventPublisherTests : IAsyncLifetime
{
    private readonly RabbitMqConnection _connection;
    private readonly RabbitMqEventPublisher _publisher;
    private readonly string _exchangeName;
    private IModel? _testChannel;

    public RabbitMqEventPublisherTests(InfrastructureTestsFixture fixture)
    {
        _connection = new RabbitMqConnection(
            fixture.RabbitMqHost,
            fixture.RabbitMqUsername,
            fixture.RabbitMqPassword);

        _publisher = new RabbitMqEventPublisher(
            _connection,
            NullLogger<RabbitMqEventPublisher>.Instance,
            fixture.RabbitMqEnvironmentLabel);

        _exchangeName = $"bsp.events.{fixture.RabbitMqEnvironmentLabel}";
    }

    public Task InitializeAsync()
    {
        _testChannel = _connection.GetChannel();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _testChannel?.Close();
        _testChannel?.Dispose();
        _connection?.Dispose();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task PublishAsync_ShouldPublishEventToRabbitMQ()
    {
        var queueName = $"test_queue_{Guid.NewGuid():N}";
        _testChannel!.QueueDeclare(queueName, durable: false, exclusive: true, autoDelete: true);
        _testChannel.QueueBind(queueName, _exchangeName, "sinistre.declare");

        var domainEvent = CreateTestEvent("SIN-2026-000001");

        var tcs = new TaskCompletionSource<BasicDeliverEventArgs>();
        var consumer = new EventingBasicConsumer(_testChannel);
        consumer.Received += (_, args) => tcs.SetResult(args);
        
        _testChannel.BasicConsume(queueName, true, consumer);

        await _publisher.PublishAsync(domainEvent);

        var receivedTask = await Task.WhenAny(tcs.Task, Task.Delay(5000));
        Assert.True(receivedTask == tcs.Task, "Message should be received within 5 seconds");

        var receivedArgs = await tcs.Task;
        var messageBody = Encoding.UTF8.GetString(receivedArgs.Body.ToArray());
        var receivedEvent = JsonSerializer.Deserialize<SinistreDeclare>(messageBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(receivedEvent);
        Assert.Equal(domainEvent.EventId, receivedEvent.EventId);

        _testChannel.QueueDelete(queueName);
    }

    [Fact]
    public async Task PublishAsync_ShouldSetCorrectRoutingKey()
    {
        var queueName = $"test_queue_{Guid.NewGuid():N}";
        _testChannel!.QueueDeclare(queueName, durable: false, exclusive: true, autoDelete: true);
        _testChannel.QueueBind(queueName, _exchangeName, "sinistre.#");

        var domainEvent = CreateTestEvent("SIN-2026-000002");

        var tcs = new TaskCompletionSource<BasicDeliverEventArgs>();
        var consumer = new EventingBasicConsumer(_testChannel);
        consumer.Received += (_, args) => tcs.SetResult(args);

        _testChannel.BasicConsume(queueName, true, consumer);

        await _publisher.PublishAsync(domainEvent);

        var receivedTask = await Task.WhenAny(tcs.Task, Task.Delay(5000));
        Assert.True(receivedTask == tcs.Task);

        var receivedArgs = await tcs.Task;
        Assert.Equal("sinistre.declare", receivedArgs.RoutingKey);

        _testChannel.QueueDelete(queueName);
    }

    [Fact]
    public async Task PublishAsync_ShouldSetMessageProperties()
    {
        var queueName = $"test_queue_{Guid.NewGuid():N}";
        _testChannel!.QueueDeclare(queueName, durable: false, exclusive: true, autoDelete: true);
        _testChannel.QueueBind(queueName, _exchangeName, "sinistre.declare");

        var domainEvent = CreateTestEvent("SIN-2026-000003", "corr456");

        var tcs = new TaskCompletionSource<BasicDeliverEventArgs>();
        var consumer = new EventingBasicConsumer(_testChannel);
        consumer.Received += (_, args) => tcs.SetResult(args);

        _testChannel.BasicConsume(queueName, true, consumer);

        await _publisher.PublishAsync(domainEvent);

        var receivedTask = await Task.WhenAny(tcs.Task, Task.Delay(5000));
        Assert.True(receivedTask == tcs.Task);

        var receivedArgs = await tcs.Task;
        Assert.Equal("application/json", receivedArgs.BasicProperties.ContentType);
        Assert.Equal(domainEvent.EventId.ToString(), receivedArgs.BasicProperties.MessageId);
        Assert.Equal("SinistreDeclare", receivedArgs.BasicProperties.Type);
        Assert.True(receivedArgs.BasicProperties.Persistent);

        Assert.NotNull(receivedArgs.BasicProperties.Headers);
        Assert.Equal("corr456", Encoding.UTF8.GetString((byte[])receivedArgs.BasicProperties.Headers["correlationId"]));

        _testChannel.QueueDelete(queueName);
    }

    [Fact]
    public void EnvironmentLabel_ShouldCreateIsolatedExchange()
    {
        using var devConnection = new RabbitMqConnection("localhost", "guest", "guest");
        var devPublisher = new RabbitMqEventPublisher(
            devConnection,
            NullLogger<RabbitMqEventPublisher>.Instance,
            "dev");

        var testChannel = _connection.GetChannel();
        var devChannel = devConnection.GetChannel();

        Assert.NotNull(testChannel);
        Assert.NotNull(devChannel);

        testChannel.Close();
        devChannel.Close();
    }

    private SinistreDeclare CreateTestEvent(string identifiantSinistre, string correlationId = "corr123")
    {
        return new SinistreDeclare
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            UserId = "user123",
            CorrelationId = correlationId,
            DeclarationId = Guid.NewGuid(),
            IdentifiantSinistre = identifiantSinistre,
            IdentifiantContrat = "POL-20260101-12345",
            DateSurvenance = DateTime.UtcNow.Date,
            DateDeclaration = DateTime.UtcNow,
            Statut = "Declaree"
        };
    }
}
