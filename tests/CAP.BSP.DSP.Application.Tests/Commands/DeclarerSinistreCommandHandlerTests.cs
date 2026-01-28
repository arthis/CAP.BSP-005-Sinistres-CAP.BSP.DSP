using CAP.BSP.DSP.Application.Commands.DeclarerSinistre;
using CAP.BSP.DSP.Application.Ports;
using CAP.BSP.DSP.Domain.Aggregates;
using CAP.BSP.DSP.Domain.Aggregates.DeclarationSinistre;
using CAP.BSP.DSP.Domain.Events;
using CAP.BSP.DSP.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace CAP.BSP.DSP.Application.Tests.Commands;

public class DeclarerSinistreCommandHandlerTests
{
    private readonly Mock<IIdentifiantSinistreGenerator> _identifiantGeneratorMock;
    private readonly Mock<IDeclarationRepository> _repositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly Mock<IPublisher> _mediatorPublisherMock;
    private readonly Mock<ILogger<DeclarerSinistreCommandHandler>> _loggerMock;
    private readonly DeclarerSinistreCommandHandler _handler;

    public DeclarerSinistreCommandHandlerTests()
    {
        _identifiantGeneratorMock = new Mock<IIdentifiantSinistreGenerator>();
        _repositoryMock = new Mock<IDeclarationRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _mediatorPublisherMock = new Mock<IPublisher>();
        _loggerMock = new Mock<ILogger<DeclarerSinistreCommandHandler>>();

        _handler = new DeclarerSinistreCommandHandler(
            _identifiantGeneratorMock.Object,
            _repositoryMock.Object,
            _eventPublisherMock.Object,
            _mediatorPublisherMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var command = new DeclarerSinistreCommand
        {
            IdentifiantContrat = "POL-20260128-00001",
            DateSurvenance = DateTime.UtcNow.AddDays(-1),
            CorrelationId = Guid.NewGuid().ToString(),
            UserId = "user-123"
        };

        _identifiantGeneratorMock
            .Setup(x => x.GenerateNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("SIN-2026-000001");

        _repositoryMock
            .Setup(x => x.SaveAsync(It.IsAny<AggregateRoot>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _eventPublisherMock
            .Setup(x => x.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mediatorPublisherMock
            .Setup(x => x.Publish<IDomainEvent>(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        
        _identifiantGeneratorMock.Verify(x => x.GenerateNextAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveAsync(It.IsAny<AggregateRoot>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventPublisherMock.Verify(x => x.PublishAsync(It.IsAny<SinistreDeclare>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorPublisherMock.Verify(x => x.Publish<IDomainEvent>(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldGenerateClaimIdentifier()
    {
        // Arrange
        var expectedIdentifiant = "SIN-2026-000042";
        var command = new DeclarerSinistreCommand
        {
            IdentifiantContrat = "POL-20260128-00001",
            DateSurvenance = DateTime.UtcNow.AddDays(-1),
            CorrelationId = Guid.NewGuid().ToString(),
            UserId = "user-123"
        };

        _identifiantGeneratorMock
            .Setup(x => x.GenerateNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedIdentifiant);

        _repositoryMock
            .Setup(x => x.SaveAsync(It.IsAny<AggregateRoot>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        
        // Use reflection to access anonymous type property
        var identifiantSinistre = result.Data.GetType().GetProperty("IdentifiantSinistre")?.GetValue(result.Data);
        Assert.Equal(expectedIdentifiant, identifiantSinistre?.ToString());
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldSaveAggregateToRepository()
    {
        // Arrange
        var command = new DeclarerSinistreCommand
        {
            IdentifiantContrat = "POL-20260128-00001",
            DateSurvenance = DateTime.UtcNow.AddDays(-1),
            CorrelationId = Guid.NewGuid().ToString(),
            UserId = "user-123"
        };

        _identifiantGeneratorMock
            .Setup(x => x.GenerateNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("SIN-2026-000001");

        DeclarationSinistre? savedDeclaration = null;
        _repositoryMock
            .Setup(x => x.SaveAsync(It.IsAny<AggregateRoot>(), It.IsAny<CancellationToken>()))
            .Callback<AggregateRoot, CancellationToken>((aggregate, _) => savedDeclaration = aggregate as DeclarationSinistre)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(savedDeclaration);
        Assert.Equal("SIN-2026-000001", savedDeclaration.IdentifiantSinistre!.Value);
        Assert.Equal(command.IdentifiantContrat, savedDeclaration.IdentifiantContrat!.Value);
        _repositoryMock.Verify(x => x.SaveAsync(It.IsAny<AggregateRoot>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldPublishEventsToRabbitMQ()
    {
        // Arrange
        var command = new DeclarerSinistreCommand
        {
            IdentifiantContrat = "POL-20260128-00001",
            DateSurvenance = DateTime.UtcNow.AddDays(-1),
            CorrelationId = Guid.NewGuid().ToString(),
            UserId = "user-123"
        };

        _identifiantGeneratorMock
            .Setup(x => x.GenerateNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("SIN-2026-000001");

        _repositoryMock
            .Setup(x => x.SaveAsync(It.IsAny<AggregateRoot>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        SinistreDeclare? publishedEvent = null;
        _eventPublisherMock
            .Setup(x => x.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Callback<IDomainEvent, CancellationToken>((evt, _) => publishedEvent = evt as SinistreDeclare)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(publishedEvent);
        Assert.Equal("SIN-2026-000001", publishedEvent.IdentifiantSinistre);
        Assert.Equal(command.IdentifiantContrat, publishedEvent.IdentifiantContrat);
        Assert.Equal(command.CorrelationId, publishedEvent.CorrelationId);
        Assert.Equal(command.UserId, publishedEvent.UserId);
        _eventPublisherMock.Verify(x => x.PublishAsync(It.IsAny<SinistreDeclare>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldPublishEventsToMediatR()
    {
        // Arrange
        var command = new DeclarerSinistreCommand
        {
            IdentifiantContrat = "POL-20260128-00001",
            DateSurvenance = DateTime.UtcNow.AddDays(-1),
            CorrelationId = Guid.NewGuid().ToString(),
            UserId = "user-123"
        };

        _identifiantGeneratorMock
            .Setup(x => x.GenerateNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("SIN-2026-000001");

        _repositoryMock
            .Setup(x => x.SaveAsync(It.IsAny<AggregateRoot>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mediatorPublisherMock.Verify(
            x => x.Publish<IDomainEvent>(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new DeclarerSinistreCommand
        {
            IdentifiantContrat = "POL-20260128-00001",
            DateSurvenance = DateTime.UtcNow.AddDays(-1),
            CorrelationId = Guid.NewGuid().ToString(),
            UserId = "user-123"
        };

        _identifiantGeneratorMock
            .Setup(x => x.GenerateNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("SIN-2026-000001");

        _repositoryMock
            .Setup(x => x.SaveAsync(It.IsAny<AggregateRoot>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Failed to declare claim", result.Error);
        Assert.Equal("COMMAND_EXECUTION_ERROR", result.ErrorCode);
        
        _eventPublisherMock.Verify(x => x.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mediatorPublisherMock.Verify(x => x.Publish<IDomainEvent>(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenEventPublisherFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new DeclarerSinistreCommand
        {
            IdentifiantContrat = "POL-20260128-00001",
            DateSurvenance = DateTime.UtcNow.AddDays(-1),
            CorrelationId = Guid.NewGuid().ToString(),
            UserId = "user-123"
        };

        _identifiantGeneratorMock
            .Setup(x => x.GenerateNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("SIN-2026-000001");

        _repositoryMock
            .Setup(x => x.SaveAsync(It.IsAny<AggregateRoot>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _eventPublisherMock
            .Setup(x => x.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("RabbitMQ connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Failed to declare claim", result.Error);
    }

    [Fact]
    public async Task Handle_WithInvalidContractFormat_ShouldReturnFailure()
    {
        // Arrange
        var command = new DeclarerSinistreCommand
        {
            IdentifiantContrat = "INVALID-FORMAT",
            DateSurvenance = DateTime.UtcNow.AddDays(-1),
            CorrelationId = Guid.NewGuid().ToString(),
            UserId = "user-123"
        };

        _identifiantGeneratorMock
            .Setup(x => x.GenerateNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("SIN-2026-000001");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Failed to declare claim", result.Error);
        
        _repositoryMock.Verify(x => x.SaveAsync(It.IsAny<AggregateRoot>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithFutureDate_ShouldReturnFailure()
    {
        // Arrange
        var command = new DeclarerSinistreCommand
        {
            IdentifiantContrat = "POL-20260128-00001",
            DateSurvenance = DateTime.UtcNow.AddDays(1), // Future date
            CorrelationId = Guid.NewGuid().ToString(),
            UserId = "user-123"
        };

        _identifiantGeneratorMock
            .Setup(x => x.GenerateNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("SIN-2026-000001");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Failed to declare claim", result.Error);
        
        _repositoryMock.Verify(x => x.SaveAsync(It.IsAny<AggregateRoot>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Constructor_WithNullDependencies_ShouldThrowArgumentNullException()
    {
        // Assert
        Assert.Throws<ArgumentNullException>(() => new DeclarerSinistreCommandHandler(
            null!,
            _repositoryMock.Object,
            _eventPublisherMock.Object,
            _mediatorPublisherMock.Object,
            _loggerMock.Object));

        Assert.Throws<ArgumentNullException>(() => new DeclarerSinistreCommandHandler(
            _identifiantGeneratorMock.Object,
            null!,
            _eventPublisherMock.Object,
            _mediatorPublisherMock.Object,
            _loggerMock.Object));

        Assert.Throws<ArgumentNullException>(() => new DeclarerSinistreCommandHandler(
            _identifiantGeneratorMock.Object,
            _repositoryMock.Object,
            null!,
            _mediatorPublisherMock.Object,
            _loggerMock.Object));

        Assert.Throws<ArgumentNullException>(() => new DeclarerSinistreCommandHandler(
            _identifiantGeneratorMock.Object,
            _repositoryMock.Object,
            _eventPublisherMock.Object,
            null!,
            _loggerMock.Object));

        Assert.Throws<ArgumentNullException>(() => new DeclarerSinistreCommandHandler(
            _identifiantGeneratorMock.Object,
            _repositoryMock.Object,
            _eventPublisherMock.Object,
            _mediatorPublisherMock.Object,
            null!));
    }
}
