using CAP.BSP.DSP.Application.Ports;
using CAP.BSP.DSP.Application.Queries.RechercherDeclarations;
using CAP.BSP.DSP.Application.ReadModels;
using Microsoft.Extensions.Logging;
using Moq;

namespace CAP.BSP.DSP.Application.Tests.Queries;

public class RechercherDeclarationsQueryHandlerTests
{
    private readonly Mock<IDeclarationReadModelRepository> _readModelRepositoryMock;
    private readonly Mock<ILogger<RechercherDeclarationsQueryHandler>> _loggerMock;
    private readonly RechercherDeclarationsQueryHandler _handler;

    public RechercherDeclarationsQueryHandlerTests()
    {
        _readModelRepositoryMock = new Mock<IDeclarationReadModelRepository>();
        _loggerMock = new Mock<ILogger<RechercherDeclarationsQueryHandler>>();

        _handler = new RechercherDeclarationsQueryHandler(
            _readModelRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithNoFilters_ShouldReturnAllDeclarations()
    {
        // Arrange
        var query = new RechercherDeclarationsQuery
        {
            Limit = 50,
            Offset = 0
        };

        var projections = new List<object>
        {
            new DeclarationListProjection
            {
                IdentifiantSinistre = "SIN-2026-000001",
                IdentifiantContrat = "POL-20260128-00001",
                DateSurvenance = DateTime.UtcNow.AddDays(-1),
                DateDeclaration = DateTime.UtcNow,
                Statut = "Declaree"
            },
            new DeclarationListProjection
            {
                IdentifiantSinistre = "SIN-2026-000002",
                IdentifiantContrat = "POL-20260128-00002",
                DateSurvenance = DateTime.UtcNow.AddDays(-2),
                DateDeclaration = DateTime.UtcNow.AddHours(-12),
                Statut = "Declaree"
            }
        };

        _readModelRepositoryMock
            .Setup(x => x.FindWithFiltersAsync(
                null, null, 50, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(projections);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Declarations.Count);
        Assert.Equal(2, result.Data.TotalCount);
    }

    [Fact]
    public async Task Handle_WithContractFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        var contractId = "POL-20260128-00001";
        var query = new RechercherDeclarationsQuery
        {
            IdentifiantContrat = contractId,
            Limit = 50,
            Offset = 0
        };

        var projections = new List<object>
        {
            new DeclarationListProjection
            {
                IdentifiantSinistre = "SIN-2026-000001",
                IdentifiantContrat = contractId,
                DateSurvenance = DateTime.UtcNow.AddDays(-1),
                DateDeclaration = DateTime.UtcNow,
                Statut = "Declaree"
            }
        };

        _readModelRepositoryMock
            .Setup(x => x.FindWithFiltersAsync(
                contractId, null, 50, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(projections);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!.Declarations);
        Assert.All(result.Data.Declarations, d => Assert.Equal(contractId, d.IdentifiantContrat));
        
        _readModelRepositoryMock.Verify(
            x => x.FindWithFiltersAsync(contractId, null, 50, 0, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithStatutFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        var statut = "Declaree";
        var query = new RechercherDeclarationsQuery
        {
            Statut = statut,
            Limit = 50,
            Offset = 0
        };

        var projections = new List<object>
        {
            new DeclarationListProjection
            {
                IdentifiantSinistre = "SIN-2026-000001",
                IdentifiantContrat = "POL-20260128-00001",
                DateSurvenance = DateTime.UtcNow.AddDays(-1),
                DateDeclaration = DateTime.UtcNow,
                Statut = statut
            }
        };

        _readModelRepositoryMock
            .Setup(x => x.FindWithFiltersAsync(
                null, statut, 50, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(projections);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!.Declarations);
        Assert.All(result.Data.Declarations, d => Assert.Equal(statut, d.Statut));
        
        _readModelRepositoryMock.Verify(
            x => x.FindWithFiltersAsync(null, statut, 50, 0, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldRespectLimitAndOffset()
    {
        // Arrange
        var query = new RechercherDeclarationsQuery
        {
            Limit = 10,
            Offset = 5
        };

        var projections = new List<object>
        {
            new DeclarationListProjection
            {
                IdentifiantSinistre = "SIN-2026-000006",
                IdentifiantContrat = "POL-20260128-00001",
                DateSurvenance = DateTime.UtcNow.AddDays(-1),
                DateDeclaration = DateTime.UtcNow,
                Statut = "Declaree"
            }
        };

        _readModelRepositoryMock
            .Setup(x => x.FindWithFiltersAsync(
                null, null, 10, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(projections);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Data!.Limit);
        Assert.Equal(5, result.Data.Offset);
        
        _readModelRepositoryMock.Verify(
            x => x.FindWithFiltersAsync(null, null, 10, 5, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithExcessiveLimit_ShouldClampTo100()
    {
        // Arrange
        var query = new RechercherDeclarationsQuery
        {
            Limit = 500, // Exceeds max
            Offset = 0
        };

        _readModelRepositoryMock
            .Setup(x => x.FindWithFiltersAsync(
                null, null, 100, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<object>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Data!.Limit);
        
        _readModelRepositoryMock.Verify(
            x => x.FindWithFiltersAsync(null, null, 100, 0, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNegativeLimit_ShouldClampTo1()
    {
        // Arrange
        var query = new RechercherDeclarationsQuery
        {
            Limit = -5,
            Offset = 0
        };

        _readModelRepositoryMock
            .Setup(x => x.FindWithFiltersAsync(
                null, null, 1, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<object>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Data!.Limit);
    }

    [Fact]
    public async Task Handle_WithNegativeOffset_ShouldClampTo0()
    {
        // Arrange
        var query = new RechercherDeclarationsQuery
        {
            Limit = 50,
            Offset = -10
        };

        _readModelRepositoryMock
            .Setup(x => x.FindWithFiltersAsync(
                null, null, 50, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<object>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Data!.Offset);
    }

    [Fact]
    public async Task Handle_WithEmptyResults_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new RechercherDeclarationsQuery
        {
            Limit = 50,
            Offset = 0
        };

        _readModelRepositoryMock
            .Setup(x => x.FindWithFiltersAsync(
                null, null, 50, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<object>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data.Declarations);
        Assert.Equal(0, result.Data.TotalCount);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var query = new RechercherDeclarationsQuery
        {
            Limit = 50,
            Offset = 0
        };

        _readModelRepositoryMock
            .Setup(x => x.FindWithFiltersAsync(
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("MongoDB connection failed"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Failed to search declarations", result.ErrorMessage);
    }

    [Fact]
    public void Constructor_WithNullDependencies_ShouldThrowArgumentNullException()
    {
        // Assert
        Assert.Throws<ArgumentNullException>(() => new RechercherDeclarationsQueryHandler(
            null!,
            _loggerMock.Object));

        Assert.Throws<ArgumentNullException>(() => new RechercherDeclarationsQueryHandler(
            _readModelRepositoryMock.Object,
            null!));
    }
}
