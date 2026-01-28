namespace CAP.BSP.DSP.Application.Ports;

/// <summary>
/// Repository port for querying the claim type reference data from MongoDB.
/// Provides validation and lookup services for claim types.
/// </summary>
public interface ITypeSinistreReferenceRepository
{
    /// <summary>
    /// Checks if a claim type code exists in the reference database.
    /// </summary>
    /// <param name="code">The claim type code (e.g., "ACCIDENT_CORPOREL").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the code exists and is active, false otherwise.</returns>
    Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a claim type by its code.
    /// </summary>
    /// <param name="code">The claim type code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The claim type reference data, or null if not found.</returns>
    Task<object?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all active claim types.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of all active claim types.</returns>
    Task<IEnumerable<object>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}
