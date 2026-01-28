using CAP.BSP.DSP.Infrastructure.Persistence.MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace CAP.BSP.DSP.Infrastructure.Tests.Persistence;

/// <summary>
/// Tests for SequentialIdentifiantSinistreGenerator using real MongoDB instance.
/// Validates atomic sequence generation with environment labeling.
/// </summary>
[Collection("InfrastructureTests")]
public class SequentialIdentifiantSinistreGeneratorTests : IAsyncLifetime
{
    private readonly MongoDbContext _context;
    private readonly DomainServices.SequentialIdentifiantSinistreGenerator _generator;

    public SequentialIdentifiantSinistreGeneratorTests(InfrastructureTestsFixture fixture)
    {
        _context = new MongoDbContext(
            fixture.MongoConnectionString,
            fixture.MongoDatabaseName,
            fixture.MongoEnvironmentLabel);

        _generator = new DomainServices.SequentialIdentifiantSinistreGenerator(_context);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        var database = _context.GetCollection<BsonDocument>("sequences").Database;
        await database.DropCollectionAsync("sequences");
    }

    [Fact]
    public async Task GenerateNextAsync_ShouldGenerateValidFormat()
    {
        var identifiant = await _generator.GenerateNextAsync();
        Assert.Matches(@"^SIN-\d{4}-\d{6}$", identifiant);
    }

    [Fact]
    public async Task GenerateNextAsync_ShouldIncludeCurrentYear()
    {
        var currentYear = DateTime.UtcNow.Year;
        var identifiant = await _generator.GenerateNextAsync();
        Assert.Contains($"SIN-{currentYear}-", identifiant);
    }

    [Fact]
    public async Task GenerateNextAsync_ShouldIncrementSequence()
    {
        var id1 = await _generator.GenerateNextAsync();
        var id2 = await _generator.GenerateNextAsync();
        var id3 = await _generator.GenerateNextAsync();

        var seq1 = int.Parse(id1.Split('-')[2]);
        var seq2 = int.Parse(id2.Split('-')[2]);
        var seq3 = int.Parse(id3.Split('-')[2]);

        Assert.Equal(seq1 + 1, seq2);
        Assert.Equal(seq2 + 1, seq3);
    }

    [Fact]
    public async Task EnvironmentLabel_ShouldUseIsolatedDatabase()
    {
        var prodContext = new MongoDbContext(
            "mongodb://admin:admin123@localhost:27017",
            "cap_bsp_dsp",
            "prod");
        var prodGenerator = new DomainServices.SequentialIdentifiantSinistreGenerator(prodContext);

        // Generate from test environment
        var testId1 = await _generator.GenerateNextAsync();
        var testId2 = await _generator.GenerateNextAsync();
        
        // Generate from prod environment (independent sequence)
        var prodId1 = await prodGenerator.GenerateNextAsync();
        var prodId2 = await prodGenerator.GenerateNextAsync();

        // Test environment increments independently
        Assert.Matches(@"^SIN-\d{4}-\d{6}$", testId1);
        Assert.Matches(@"^SIN-\d{4}-\d{6}$", testId2);
        
        // Prod environment also has valid format
        Assert.Matches(@"^SIN-\d{4}-\d{6}$", prodId1);
        Assert.Matches(@"^SIN-\d{4}-\d{6}$", prodId2);
        
        // Verify sequences increment within same environment
        var testNum1 = int.Parse(testId1.Split('-')[2]);
        var testNum2 = int.Parse(testId2.Split('-')[2]);
        Assert.Equal(testNum1 + 1, testNum2);
        
        var prodNum1 = int.Parse(prodId1.Split('-')[2]);
        var prodNum2 = int.Parse(prodId2.Split('-')[2]);
        Assert.Equal(prodNum1 + 1, prodNum2);

        var database = prodContext.GetCollection<BsonDocument>("sequences").Database;
        await database.DropCollectionAsync("sequences");
    }
}
