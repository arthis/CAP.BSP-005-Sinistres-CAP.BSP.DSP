using CAP.BSP.DSP.Application.Ports;
using CAP.BSP.DSP.Application.Queries.ObtenirStatutDeclaration;
using CAP.BSP.DSP.Application.ReadModels;
using Microsoft.Extensions.Logging;
using Moq;

namespace CAP.BSP.DSP.Application.Tests.Queries;

public class ObtenirStatutDeclarationQueryHandlerTests
{
    private readonly Mock<IDeclarationReadModelRepository> _readModelRepositoryMock;
    private readonly Mock<ILogger<ObtenirStatutDeclarationQueryHandler>> _loggerMock;
    private readonly ObtenirStatutDeclarationQueryHandler _handler;

    public ObtenirStatutDeclarationQueryHandlerTests()
    {
        _readModelRepositoryMock = new Mock<IDeclarationReadModelRepository>();
        _loggerMock = new Mock<ILogger<ObtenirStatutDeclarationQueryHandler>>();

        _handler = new ObtenirStatutDeclarationQueryHandler(
            _readModelRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingDeclaration_ShouldReturnSuccess()
    {
        // Arrange
        var identifiantSinistre = "SIN-2026-000001";
        var query = new ObtenirStatutDeclarationQuery
        {
            IdentifiantSinistre = identifiantSinistre
        };

        var projection = new DeclarationDetailProjection
        {
            IdentifiantSinistre = identifiantSinistre,
            DeclarationId = Guid.NewGuid().ToString(),
            IdentifiantContrat = "POL-20260128-00001",
            DateSurvenance = DateTime.UtcNow.AddDays(-1),
            DateDeclaration = DateTime.UtcNow,
            Statut = "Declaree",
            TypeSinistre = null,
            TypeSinistreLibelle = null,
            UserId = "user-123",
            CorrelationId = Guid.NewGuid().ToString(),
            EventHistory = new List<EventHistoryItem>
            {
                new EventHistoryItem
                {
                    EventType = "SinistreDeclare",
                    OccurredAt = DateTime.UtcNow,
                    UserId = "user-123"
                }
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _readModelRepositoryMock
            .Setup(x => x.FindByIdAsync(identifiantSinistre, It.IsAny<CancellationToken>()))
            .ReturnsAsync(projection);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(identifiantSinistre, result.Data.IdentifiantSinistre);
        Assert.Equal("POL-20260128-00001", result.Data.IdentifiantContrat);
        Assert.Equal("Declaree", result.Data.Statut);
        
        _readModelRepositoryMock.Verify(
            x => x.FindByIdAsync(identifiantSinistre, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingDeclaration_ShouldReturnFailure()
    {
        // Arrange
        var identifiantSinistre = "SIN-2026-999999";
        var query = new ObtenirStatutDeclarationQuery
        {
            IdentifiantSinistre = identifiantSinistre
        };

        _readModelRepositoryMock
            .Setup(x => x.FindByIdAsync(identifiantSinistre, It.IsAny<CancellationToken>()))
            .ReturnsAsync((object?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Declaration not found", result.ErrorMessage);
        Assert.Contains(identifiantSinistre, result.ErrorMessage);
    }

    [Fact]
    public async Task Handle_WithEmptyIdentifiant_ShouldReturnFailure()
    {
        // Arrange
        var query = new ObtenirStatutDeclarationQuery
        {
            IdentifiantSinistre = ""
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("IdentifiantSinistre is required", result.ErrorMessage);
        
        _readModelRepositoryMock.Verify(
            x => x.FindByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithNullIdentifiant_ShouldReturnFailure()
    {
        // Arrange
        var query = new ObtenirStatutDeclarationQuery
        {
            IdentifiantSinistre = null!
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("IdentifiantSinistre is required", result.ErrorMessage);
    }

    [Fact]
    public async Task Handle_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var identifiantSinistre = "SIN-2026-000001";
        var declarationId = Guid.NewGuid().ToString();
        var correlationId = Guid.NewGuid().ToString();
        var createdAt = DateTime.UtcNow.AddHours(-2);
        var updatedAt = DateTime.UtcNow.AddHours(-1);

        var query = new ObtenirStatutDeclarationQuery
        {
            IdentifiantSinistre = identifiantSinistre
        };

        var projection = new DeclarationDetailProjection
        {
            IdentifiantSinistre = identifiantSinistre,
            DeclarationId = declarationId,
            IdentifiantContrat = "POL-20260128-00001",
            DateSurvenance = DateTime.UtcNow.AddDays(-5),
            DateDeclaration = DateTime.UtcNow.AddDays(-4),
            Statut = "Declaree",
            TypeSinistre = "AUTO",
            TypeSinistreLibelle = "Automobile",
            UserId = "user-123",
            CorrelationId = correlationId,
            EventHistory = new List<EventHistoryItem>
            {
                new EventHistoryItem
                {
                    EventType = "SinistreDeclare",
                    OccurredAt = DateTime.UtcNow.AddDays(-4),
                    UserId = "user-123"
                }
            },
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        _readModelRepositoryMock
            .Setup(x => x.FindByIdAsync(identifiantSinistre, It.IsAny<CancellationToken>()))
            .ReturnsAsync(projection);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var dto = result.Data!;
        
        Assert.Equal(identifiantSinistre, dto.IdentifiantSinistre);
        Assert.Equal(declarationId, dto.DeclarationId);
        Assert.Equal("POL-20260128-00001", dto.IdentifiantContrat);
        Assert.Equal(projection.DateSurvenance, dto.DateSurvenance);
        Assert.Equal(projection.DateDeclaration, dto.DateDeclaration);
        Assert.Equal("Declaree", dto.Statut);
        Assert.Equal("AUTO", dto.TypeSinistre);
        Assert.Equal("Automobile", dto.TypeSinistreLibelle);
        Assert.Equal("user-123", dto.UserId);
        Assert.Equal(correlationId, dto.CorrelationId);
        Assert.Equal(createdAt, dto.CreatedAt);
        Assert.Equal(updatedAt, dto.UpdatedAt);
        
        Assert.Single(dto.EventHistory);
        Assert.Equal("SinistreDeclare", dto.EventHistory.First().EventType);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var query = new ObtenirStatutDeclarationQuery
        {
            IdentifiantSinistre = "SIN-2026-000001"
        };

        _readModelRepositoryMock
            .Setup(x => x.FindByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("MongoDB connection failed"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Failed to retrieve declaration status", result.ErrorMessage);
    }

    [Fact]
    public void Constructor_WithNullDependencies_ShouldThrowArgumentNullException()
    {
        // Assert
        Assert.Throws<ArgumentNullException>(() => new ObtenirStatutDeclarationQueryHandler(
            null!,
            _loggerMock.Object));

        Assert.Throws<ArgumentNullException>(() => new ObtenirStatutDeclarationQueryHandler(
            _readModelRepositoryMock.Object,
            null!));
    }
}
