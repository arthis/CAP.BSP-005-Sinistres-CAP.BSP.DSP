using MongoDB.Driver;

namespace CAP.BSP.DSP.Infrastructure.Persistence.MongoDB;

/// <summary>
/// MongoDB database context providing access to collections.
/// Manages connection to MongoDB and provides typed collection access.
/// </summary>
public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    /// <summary>
    /// Initializes a new instance of the MongoDbContext.
    /// </summary>
    /// <param name="connectionString">MongoDB connection string.</param>
    /// <param name="databaseName">Name of the database to connect to.</param>
    /// <param name="environment">Environment label (e.g., dev, test, prod). Default is empty string.</param>
    public MongoDbContext(string connectionString, string databaseName, string environment = "")
    {
        var client = new MongoClient(connectionString);
        var dbName = string.IsNullOrWhiteSpace(environment) 
            ? databaseName 
            : $"{databaseName}_{environment}";
        _database = client.GetDatabase(dbName);
    }

    /// <summary>
    /// Gets the declaration read model collection.
    /// Stores denormalized claim declarations for query operations.
    /// </summary>
    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return _database.GetCollection<T>(collectionName);
    }

    /// <summary>
    /// Declaration read model collection.
    /// </summary>
    public IMongoCollection<object> DeclarationReadModel =>
        _database.GetCollection<object>("declarationReadModel");

    /// <summary>
    /// Claim type reference data collection.
    /// </summary>
    public IMongoCollection<object> TypeSinistreReference =>
        _database.GetCollection<object>("typeSinistreReference");

    /// <summary>
    /// Sequences collection for generating sequential identifiers.
    /// Used for auto-incrementing claim numbers.
    /// </summary>
    public IMongoCollection<object> Sequences =>
        _database.GetCollection<object>("sequences");
}
