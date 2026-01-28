using CAP.BSP.DSP.Application.Ports;
using CAP.BSP.DSP.Application.ReadModels;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace CAP.BSP.DSP.Infrastructure.Persistence.MongoDB;

public class MongoDeclarationReadModelRepository : IDeclarationReadModelRepository
{
    private readonly MongoDbContext _context;
    private readonly ILogger<MongoDeclarationReadModelRepository> _logger;
    private const string CollectionName = "declarationReadModel";

    public MongoDeclarationReadModelRepository(
        MongoDbContext context,
        ILogger<MongoDeclarationReadModelRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task AddAsync(object projection, CancellationToken cancellationToken = default)
    {
        try
        {
            var collection = _context.GetCollection<DeclarationDetailProjection>(CollectionName);

            if (projection is DeclarationDetailProjection detailProjection)
            {
                await collection.InsertOneAsync(detailProjection, cancellationToken: cancellationToken);
                
                _logger.LogInformation(
                    "Declaration projection created for claim {IdentifiantSinistre}",
                    detailProjection.IdentifiantSinistre);
            }
            else
            {
                throw new ArgumentException($"Unsupported projection type: {projection.GetType().Name}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add declaration projection");
            throw;
        }
    }

    public async Task<object?> FindByIdAsync(string identifiantSinistre, CancellationToken cancellationToken = default)
    {
        try
        {
            var collection = _context.GetCollection<DeclarationDetailProjection>(CollectionName);
            
            var filter = Builders<DeclarationDetailProjection>.Filter.Eq(d => d.IdentifiantSinistre, identifiantSinistre);
            var result = await collection.Find(filter).FirstOrDefaultAsync(cancellationToken);

            if (result != null)
            {
                _logger.LogInformation(
                    "Declaration projection found for claim {IdentifiantSinistre}",
                    identifiantSinistre);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find declaration projection by ID {IdentifiantSinistre}", identifiantSinistre);
            throw;
        }
    }

    public async Task<IEnumerable<object>> FindByContractAsync(
        string identifiantContrat,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var collection = _context.GetCollection<DeclarationListProjection>(CollectionName);
            
            var filter = Builders<DeclarationListProjection>.Filter.Eq(d => d.IdentifiantContrat, identifiantContrat);
            var sort = Builders<DeclarationListProjection>.Sort.Descending(d => d.DateDeclaration);
            
            var results = await collection.Find(filter)
                .Sort(sort)
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "Found {Count} declarations for contract {IdentifiantContrat}",
                results.Count,
                identifiantContrat);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find declarations by contract {IdentifiantContrat}", identifiantContrat);
            throw;
        }
    }

    public async Task<IEnumerable<object>> FindWithFiltersAsync(
        string? identifiantContrat = null,
        string? statut = null,
        int limit = 50,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var collection = _context.GetCollection<DeclarationListProjection>(CollectionName);
            
            var filterBuilder = Builders<DeclarationListProjection>.Filter;
            var filters = new List<FilterDefinition<DeclarationListProjection>>();

            if (!string.IsNullOrEmpty(identifiantContrat))
            {
                filters.Add(filterBuilder.Eq(d => d.IdentifiantContrat, identifiantContrat));
            }

            if (!string.IsNullOrEmpty(statut))
            {
                filters.Add(filterBuilder.Eq(d => d.Statut, statut));
            }

            var combinedFilter = filters.Count > 0
                ? filterBuilder.And(filters)
                : filterBuilder.Empty;

            var sort = Builders<DeclarationListProjection>.Sort.Descending(d => d.DateDeclaration);
            
            var results = await collection.Find(combinedFilter)
                .Sort(sort)
                .Skip(offset)
                .Limit(limit)
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "Found {Count} declarations with filters (contract: {IdentifiantContrat}, statut: {Statut}, limit: {Limit}, offset: {Offset})",
                results.Count,
                identifiantContrat ?? "null",
                statut ?? "null",
                limit,
                offset);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find declarations with filters");
            throw;
        }
    }

    public async Task UpdateAsync(string identifiantSinistre, object projection, CancellationToken cancellationToken = default)
    {
        try
        {
            var collection = _context.GetCollection<DeclarationDetailProjection>(CollectionName);

            if (projection is DeclarationDetailProjection detailProjection)
            {
                var filter = Builders<DeclarationDetailProjection>.Filter.Eq(d => d.IdentifiantSinistre, identifiantSinistre);
                
                detailProjection.UpdatedAt = DateTime.UtcNow;
                
                await collection.ReplaceOneAsync(filter, detailProjection, cancellationToken: cancellationToken);
                
                _logger.LogInformation(
                    "Declaration projection updated for claim {IdentifiantSinistre}",
                    identifiantSinistre);
            }
            else
            {
                throw new ArgumentException($"Unsupported projection type: {projection.GetType().Name}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update declaration projection for {IdentifiantSinistre}", identifiantSinistre);
            throw;
        }
    }
}
