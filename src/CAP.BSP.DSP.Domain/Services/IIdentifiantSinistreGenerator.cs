namespace CAP.BSP.DSP.Domain.Services;

/// <summary>
/// Service interface for generating unique claim identifiers.
/// Implements sequential ID generation using MongoDB findAndModify for atomic increments.
/// </summary>
public interface IIdentifiantSinistreGenerator
{
    /// <summary>
    /// Generates the next sequential claim identifier.
    /// Format: SIN-YYYY-NNNNNN (e.g., SIN-2026-000001)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The next unique claim identifier.</returns>
    Task<string> GenerateNextAsync(CancellationToken cancellationToken = default);
}
