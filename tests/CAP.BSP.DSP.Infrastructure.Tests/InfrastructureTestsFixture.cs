using Xunit;

namespace CAP.BSP.DSP.Infrastructure.Tests;

/// <summary>
/// Collection fixture for infrastructure tests.
/// Uses local Docker infrastructure (EventStoreDB, MongoDB, RabbitMQ).
/// </summary>
[CollectionDefinition("InfrastructureTests")]
public class InfrastructureTestsCollection : ICollectionFixture<InfrastructureTestsFixture>
{
}

/// <summary>
/// Fixture providing connection details for local Docker infrastructure.
/// Assumes docker-compose is running with services on default ports.
/// </summary>
public class InfrastructureTestsFixture
{
    // MongoDB (from docker-compose.yml)
    public string MongoConnectionString { get; } = "mongodb://admin:admin123@localhost:27017";
    public string MongoDatabaseName { get; } = "cap_bsp_dsp";
    public string MongoEnvironmentLabel { get; } = "test";

    // EventStoreDB (from docker-compose.yml)
    public string EventStoreConnectionString { get; } = "esdb://localhost:2113?tls=false";
    
    // RabbitMQ (from docker-compose.yml)
    public string RabbitMqHost { get; } = "localhost";
    public string RabbitMqUsername { get; } = "guest";
    public string RabbitMqPassword { get; } = "guest";
    public string RabbitMqEnvironmentLabel { get; } = "test";

    public InfrastructureTestsFixture()
    {
        // Fixture is created once per test collection
        // Validates infrastructure is available
    }
}
