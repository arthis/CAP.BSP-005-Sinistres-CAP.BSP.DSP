using CAP.BSP.DSP.Domain.Services;
using CAP.BSP.DSP.Infrastructure.Persistence.MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CAP.BSP.DSP.Infrastructure.DomainServices;

/// <summary>
/// Generates sequential claim identifiers using MongoDB findAndModify for atomic increments.
/// Format: SIN-YYYY-NNNNNN (e.g., SIN-2026-000001)
/// </summary>
public class SequentialIdentifiantSinistreGenerator : IIdentifiantSinistreGenerator
{
    private readonly MongoDbContext _context;
    private const string SequenceName = "identifiantSinistre";

    public SequentialIdentifiantSinistreGenerator(MongoDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Generates the next sequential claim identifier.
    /// Uses MongoDB findAndModify for atomic increment.
    /// </summary>
    public async Task<string> GenerateNextAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var sequenceId = $"{SequenceName}_{year}";

        var filter = Builders<BsonDocument>.Filter.Eq("_id", sequenceId);
        var update = Builders<BsonDocument>.Update.Inc("value", 1);
        var options = new FindOneAndUpdateOptions<BsonDocument>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        var result = await _context.GetCollection<BsonDocument>("sequences")
            .FindOneAndUpdateAsync(filter, update, options, cancellationToken);

        var sequenceNumber = result["value"].AsInt32;
        return $"SIN-{year}-{sequenceNumber:D6}";
    }
}
