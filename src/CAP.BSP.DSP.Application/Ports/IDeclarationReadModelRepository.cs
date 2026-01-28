namespace CAP.BSP.DSP.Application.Ports;

/// <summary>
/// Repository port for querying the declaration read model from MongoDB.
/// Read-only operations for CQRS query side.
/// </summary>
public interface IDeclarationReadModelRepository
{
    /// <summary>
    /// Adds a new declaration to the read model.
    /// Called by event handlers when processing SinistreDéclaré events.
    /// </summary>
    /// <param name="declaration">The declaration read model document.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    Task AddAsync(object declaration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a declaration by its unique identifier.
    /// </summary>
    /// <param name="declarationId">The declaration's unique identifier (ULID).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The declaration read model, or null if not found.</returns>
    Task<object?> FindByIdAsync(string declarationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds all declarations for a specific contract.
    /// </summary>
    /// <param name="numeroContrat">The contract number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of declarations for the contract.</returns>
    Task<IEnumerable<object>> FindByContractAsync(string numeroContrat, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds declarations matching the specified filters with pagination.
    /// </summary>
    /// <param name="identifiantContrat">Optional filter by contract identifier.</param>
    /// <param name="statut">Optional filter by claim status.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip for pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of declarations matching the filters.</returns>
    Task<IEnumerable<object>> FindWithFiltersAsync(
        string? identifiantContrat = null,
        string? statut = null,
        int limit = 50,
        int offset = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing declaration in the read model.
    /// </summary>
    /// <param name="declarationId">The declaration's unique identifier.</param>
    /// <param name="projection">The updated projection object.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    Task UpdateAsync(string declarationId, object projection, CancellationToken cancellationToken = default);
}
