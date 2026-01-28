using CAP.BSP.DSP.Application.ReadModels;
using CAP.BSP.DSP.Infrastructure.Persistence.MongoDB;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;
using Xunit;

namespace CAP.BSP.DSP.Infrastructure.Tests.Persistence;

/// <summary>
/// Tests for MongoDeclarationReadModelRepository using real MongoDB instance.
/// Validates CRUD operations on read models with environment labeling.
/// </summary>
[Collection("InfrastructureTests")]
public class MongoDeclarationReadModelRepositoryTests : IAsyncLifetime
{
    private readonly MongoDbContext _context;
    private readonly MongoDeclarationReadModelRepository _repository;
    private readonly IMongoCollection<DeclarationDetailProjection> _collection;
    private const string CollectionName = "declarationReadModel";

    public MongoDeclarationReadModelRepositoryTests(InfrastructureTestsFixture fixture)
    {
        _context = new MongoDbContext(
            fixture.MongoConnectionString,
            fixture.MongoDatabaseName,
            fixture.MongoEnvironmentLabel);

        _repository = new MongoDeclarationReadModelRepository(
            _context,
            NullLogger<MongoDeclarationReadModelRepository>.Instance);

        _collection = _context.GetCollection<DeclarationDetailProjection>(CollectionName);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _collection.DeleteManyAsync(FilterDefinition<DeclarationDetailProjection>.Empty);
    }

    [Fact]
    public async Task AddAsync_ShouldInsertProjection()
    {
        var projection = CreateTestProjection("SIN-2026-000001");

        await _repository.AddAsync(projection);

        var found = await _collection
            .Find(d => d.IdentifiantSinistre == "SIN-2026-000001")
            .FirstOrDefaultAsync();

        Assert.NotNull(found);
        Assert.Equal("SIN-2026-000001", found.IdentifiantSinistre);
    }

    [Fact]
    public async Task FindByIdAsync_WhenExists_ShouldReturnProjection()
    {
        var projection = CreateTestProjection("SIN-2026-000002");
        await _collection.InsertOneAsync(projection);

        var result = await _repository.FindByIdAsync("SIN-2026-000002");

        Assert.NotNull(result);
        var found = Assert.IsType<DeclarationDetailProjection>(result);
        Assert.Equal("SIN-2026-000002", found.IdentifiantSinistre);
    }

    [Fact]
    public async Task FindByIdAsync_WhenNotExists_ShouldReturnNull()
    {
        var result = await _repository.FindByIdAsync("SIN-9999-999999");
        Assert.Null(result);
    }

    [Fact]
    public async Task FindByContractAsync_ShouldReturnMatchingDeclarations()
    {
        var projection1 = CreateTestProjection("SIN-2026-000003");
        var projection2 = CreateTestProjection("SIN-2026-000004");
        await _collection.InsertManyAsync(new[] { projection1, projection2 });

        var results = await _repository.FindByContractAsync("POL-20260101-12345");

        var list = results.ToList();
        Assert.True(list.Count >= 2);
    }

    [Fact]
    public async Task EnvironmentLabel_ShouldIsolateDatabases()
    {
        var testProjection = CreateTestProjection("SIN-2026-ENV001");
        await _repository.AddAsync(testProjection);

        var devContext = new MongoDbContext(
            "mongodb://admin:admin123@localhost:27017",
            "cap_bsp_dsp",
            "dev");
        var devRepository = new MongoDeclarationReadModelRepository(
            devContext,
            NullLogger<MongoDeclarationReadModelRepository>.Instance);

        var resultInDev = await devRepository.FindByIdAsync("SIN-2026-ENV001");

        Assert.Null(resultInDev);

        var devCollection = devContext.GetCollection<DeclarationDetailProjection>(CollectionName);
        await devCollection.DeleteManyAsync(FilterDefinition<DeclarationDetailProjection>.Empty);
    }

    private DeclarationDetailProjection CreateTestProjection(string identifiantSinistre)
    {
        return new DeclarationDetailProjection
        {
            IdentifiantSinistre = identifiantSinistre,
            DeclarationId = Guid.NewGuid().ToString(),
            IdentifiantContrat = "POL-20260101-12345",
            DateSurvenance = DateTime.UtcNow.Date,
            DateDeclaration = DateTime.UtcNow,
            Statut = "EnAttente",
            UserId = "user123",
            CorrelationId = "corr123",
            EventHistory = new List<EventHistoryItem>
            {
                new() { EventType = "SinistreDeclare", OccurredAt = DateTime.UtcNow, UserId = "user123" }
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
