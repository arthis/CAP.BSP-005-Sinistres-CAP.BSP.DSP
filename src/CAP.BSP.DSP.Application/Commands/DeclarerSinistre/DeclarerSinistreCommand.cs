using CAP.BSP.DSP.Application.Commands;

namespace CAP.BSP.DSP.Application.Commands.DeclarerSinistre;

/// <summary>
/// Command to declare a new claim (US1).
/// Represents the user's intent to submit a claim declaration.
/// </summary>
public record DeclarerSinistreCommand : ICommand
{
    /// <summary>
    /// Contract identifier (required - US4).
    /// Format: POL-YYYYMMDD-XXXXX (e.g., POL-20260127-00001)
    /// </summary>
    public string IdentifiantContrat { get; init; } = string.Empty;

    /// <summary>
    /// Date when the claim occurred (required - US1).
    /// Must be today or in the past (US3).
    /// </summary>
    public DateTime DateSurvenance { get; init; }

    /// <summary>
    /// Correlation ID for distributed tracing.
    /// Links related operations across services.
    /// </summary>
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// User ID of the person submitting the claim.
    /// </summary>
    public string UserId { get; init; } = string.Empty;
}
