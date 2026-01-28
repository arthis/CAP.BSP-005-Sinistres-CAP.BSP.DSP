using CAP.BSP.DSP.Application.EventHandlers;
using CAP.BSP.DSP.Application.Ports;
using CAP.BSP.DSP.Application.ReadModels;
using CAP.BSP.DSP.Domain.Events;
using Microsoft.Extensions.Logging;
using Moq;

namespace CAP.BSP.DSP.Application.Tests.EventHandlers;

public class SinistreDeclareEventHandlerTests
{
    private readonly Mock<IDeclarationReadModelRepository> _readModelRepositoryMock;
    private readonly Mock<ILogger<SinistreDeclareEventHandler>> _loggerMock;
    private readonly SinistreDeclareEventHandler _handler;

    public SinistreDeclareEventHandlerTests()
    {
        _readModelRepositoryMock = new Mock<IDeclarationReadModelRepository>();
        _loggerMock = new Mock<ILogger<SinistreDeclareEventHandler>>();

        _handler = new SinistreDeclareEventHandler(
            _readModelRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidEvent_ShouldCreateReadModelProjection()
    {
        // Arrange
        var declarationId = Guid.NewGuid();
        var @event = new SinistreDeclare
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            DeclarationId = declarationId,
            IdentifiantSinistre = "SIN-2026-000001",
            IdentifiantContrat = "POL-20260128-00001",
            DateSurvenance = DateTime.UtcNow.AddDays(-1),
            DateDeclaration = DateTime.UtcNow,
            Statut = "Declaree",
            CorrelationId = Guid.NewGuid().ToString(),
            UserId = "user-123"
        };

        DeclarationDetailProjection? savedProjection = null;
        _readModelRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((proj, _) => savedProjection = proj as DeclarationDetailProjection)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        Assert.NotNull(savedProjection);
        Assert.Equal(@event.IdentifiantSinistre, savedProjection.IdentifiantSinistre);
        Assert.Equal(@event.IdentifiantContrat, savedProjection.IdentifiantContrat);
        Assert.Equal(@event.DateSurvenance, savedProjection.DateSurvenance);
        Assert.Equal(@event.Statut, savedProjection.Statut);
        Assert.Equal(@event.UserId, savedProjection.UserId);
        Assert.Equal(@event.CorrelationId, savedProjection.CorrelationId);
        
        _readModelRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<DeclarationDetailProjection>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidEvent_ShouldIncludeEventHistory()
    {
        // Arrange
        var @event = new SinistreDeclare
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            DeclarationId = Guid.NewGuid(),
            IdentifiantSinistre = "SIN-2026-000001",
            IdentifiantContrat = "POL-20260128-00001",
            DateSurvenance = DateTime.UtcNow.AddDays(-1),
            DateDeclaration = DateTime.UtcNow,
            Statut = "Declaree",
            CorrelationId = Guid.NewGuid().ToString(),
            UserId = "user-123"
        };

        DeclarationDetailProjection? savedProjection = null;
        _readModelRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((proj, _) => savedProjection = proj as DeclarationDetailProjection)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        Assert.NotNull(savedProjection);
        Assert.NotNull(savedProjection.EventHistory);
        Assert.Single(savedProjection.EventHistory);
        
        var eventHistoryItem = savedProjection.EventHistory.First();
        Assert.Equal("SinistreDeclare", eventHistoryItem.EventType);
        Assert.Equal(@event.OccurredAt, eventHistoryItem.OccurredAt);
        Assert.Equal(@event.UserId, eventHistoryItem.UserId);
    }

    [Fact]
    public async Task Handle_WithNullUserId_ShouldUseSystemAsDefault()
    {
        // Arrange
        var @event = new SinistreDeclare
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            DeclarationId = Guid.NewGuid(),
            IdentifiantSinistre = "SIN-2026-000001",
            IdentifiantContrat = "POL-20260128-00001",
            DateSurvenance = DateTime.UtcNow.AddDays(-1),
            DateDeclaration = DateTime.UtcNow,
            Statut = "Declaree",
            CorrelationId = Guid.NewGuid().ToString(),
            UserId = null! // Null user
        };

        DeclarationDetailProjection? savedProjection = null;
        _readModelRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((proj, _) => savedProjection = proj as DeclarationDetailProjection)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        Assert.NotNull(savedProjection);
        Assert.Equal("system", savedProjection.UserId);
        
        var eventHistoryItem = savedProjection.EventHistory.First();
        Assert.Equal("system", eventHistoryItem.UserId);
    }

    [Fact]
    public async Task Handle_WithNullCorrelationId_ShouldUseEmptyString()
    {
        // Arrange
        var @event = new SinistreDeclare
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            DeclarationId = Guid.NewGuid(),
            IdentifiantSinistre = "SIN-2026-000001",
            IdentifiantContrat = "POL-20260128-00001",
            DateSurvenance = DateTime.UtcNow.AddDays(-1),
            DateDeclaration = DateTime.UtcNow,
            Statut = "Declaree",
            CorrelationId = null!, // Null correlation
            UserId = "user-123"
        };

        DeclarationDetailProjection? savedProjection = null;
        _readModelRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((proj, _) => savedProjection = proj as DeclarationDetailProjection)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        Assert.NotNull(savedProjection);
        Assert.Equal(string.Empty, savedProjection.CorrelationId);
    }

    [Fact]
    public async Task Handle_WhenRepositoryFails_ShouldThrowException()
    {
        // Arrange
        var @event = new SinistreDeclare
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            DeclarationId = Guid.NewGuid(),
            IdentifiantSinistre = "SIN-2026-000001",
            IdentifiantContrat = "POL-20260128-00001",
            DateSurvenance = DateTime.UtcNow.AddDays(-1),
            DateDeclaration = DateTime.UtcNow,
            Statut = "Declaree",
            CorrelationId = Guid.NewGuid().ToString(),
            UserId = "user-123"
        };

        _readModelRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("MongoDB connection failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(@event, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldSetCreatedAndUpdatedTimestamps()
    {
        // Arrange
        var beforeExecution = DateTime.UtcNow;
        
        var @event = new SinistreDeclare
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            DeclarationId = Guid.NewGuid(),
            IdentifiantSinistre = "SIN-2026-000001",
            IdentifiantContrat = "POL-20260128-00001",
            DateSurvenance = DateTime.UtcNow.AddDays(-1),
            DateDeclaration = DateTime.UtcNow,
            Statut = "Declaree",
            CorrelationId = Guid.NewGuid().ToString(),
            UserId = "user-123"
        };

        DeclarationDetailProjection? savedProjection = null;
        _readModelRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((proj, _) => savedProjection = proj as DeclarationDetailProjection)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(@event, CancellationToken.None);
        var afterExecution = DateTime.UtcNow;

        // Assert
        Assert.NotNull(savedProjection);
        Assert.True(savedProjection.CreatedAt >= beforeExecution);
        Assert.True(savedProjection.CreatedAt <= afterExecution);
        Assert.True(savedProjection.UpdatedAt >= beforeExecution);
        Assert.True(savedProjection.UpdatedAt <= afterExecution);
        Assert.Equal(savedProjection.CreatedAt, savedProjection.UpdatedAt);
    }

    [Fact]
    public void Constructor_WithNullDependencies_ShouldThrowArgumentNullException()
    {
        // Assert
        Assert.Throws<ArgumentNullException>(() => new SinistreDeclareEventHandler(
            null!,
            _loggerMock.Object));

        Assert.Throws<ArgumentNullException>(() => new SinistreDeclareEventHandler(
            _readModelRepositoryMock.Object,
            null!));
    }
}
