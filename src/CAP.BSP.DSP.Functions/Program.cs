using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CAP.BSP.DSP.Infrastructure.Observability;
using CAP.BSP.DSP.Infrastructure.Persistence.EventStore;
using CAP.BSP.DSP.Infrastructure.Persistence.MongoDB;
using CAP.BSP.DSP.Infrastructure.Messaging;
using CAP.BSP.DSP.Infrastructure.DomainServices;
using CAP.BSP.DSP.Application.Ports;
using CAP.BSP.DSP.Domain.Services;
using CAP.BSP.DSP.Application.Commands.DeclarerSinistre;
using FluentValidation;
using Serilog;

// Configure Serilog
Log.Logger = SerilogConfiguration.ConfigureFromEnvironment();

try
{
    var host = new HostBuilder()
        .ConfigureFunctionsWorkerDefaults()
        .ConfigureServices(services =>
        {
            // MediatR registration - scans Application assembly for handlers
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(DeclarerSinistreCommandHandler).Assembly);
            });

            // FluentValidation registration
            services.AddValidatorsFromAssembly(typeof(DeclarerSinistreCommandValidator).Assembly);

            // Infrastructure connections
            services.AddSingleton(sp => new MongoDbContext(
                Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") ?? "mongodb://localhost:27017",
                Environment.GetEnvironmentVariable("MONGODB_DATABASE") ?? "capBspDeclarationSinistre"));
            
            services.AddSingleton(sp => new EventStoreConnection(
                Environment.GetEnvironmentVariable("EVENTSTORE_CONNECTION_STRING") ?? "esdb://localhost:2113?tls=false"));
            
            services.AddSingleton(sp => new RabbitMqConnection(
                Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME") ?? "localhost",
                Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "guest",
                Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest"));

            // Repository registrations
            services.AddSingleton<IDeclarationRepository, EventStoreDeclarationRepository>();
            services.AddSingleton<IDeclarationReadModelRepository, MongoDeclarationReadModelRepository>();
            services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

            // Domain services
            services.AddSingleton<IIdentifiantSinistreGenerator, SequentialIdentifiantSinistreGenerator>();
        })
        .UseSerilog()
        .Build();

    Log.Information("CAP.BSP.DSP Azure Functions starting...");
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "CAP.BSP.DSP Azure Functions terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
