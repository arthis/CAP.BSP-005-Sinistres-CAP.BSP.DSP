using CAP.BSP.DSP.Domain.Events;

namespace CAP.BSP.DSP.Domain.Events;

/// <summary>
/// Domain event raised when a claim is declared.
/// Represents the fact that a new claim declaration has been submitted.
/// </summary>
public record SinistreDeclare : IDomainEvent
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    public Guid EventId { get; init; }

    /// <summary>
    /// Timestamp when the event occurred (UTC).
    /// </summary>
    public DateTime OccurredAt { get; init; }

    /// <summary>
    /// Unique identifier for the declaration (GUID).
    /// </summary>
    public Guid DeclarationId { get; init; }

    /// <summary>
    /// Auto-generated claim identifier (US1).
    /// Format: SIN-YYYY-NNNNNN (e.g., SIN-2026-000001)
    /// </summary>
    public string IdentifiantSinistre { get; init; } = string.Empty;

    /// <summary>
    /// Contract identifier (US4 - mandatory).
    /// Format: POL-YYYYMMDD-XXXXX
    /// </summary>
    public string IdentifiantContrat { get; init; } = string.Empty;

    /// <summary>
    /// Date when the claim occurred (US1).
    /// Must be today or in the past (US3).
    /// </summary>
    public DateTime DateSurvenance { get; init; }

    /// <summary>
    /// System-generated declaration timestamp (US1).
    /// </summary>
    public DateTime DateDeclaration { get; init; }

    /// <summary>
    /// Status of the declaration (US1).
    /// Initial status is always "Declaree".
    /// </summary>
    public string Statut { get; init; } = string.Empty;

    /// <summary>
    /// Correlation ID for distributed tracing.
    /// </summary>
    public string CorrelationId { get; init; } = string.Empty;

    /// <summary>
    /// Causation ID - the ID of the command or event that caused this event.
    /// </summary>
    public string? CausationId { get; init; }

    /// <summary>
    /// User ID of the person who declared the claim.
    /// </summary>
    public string UserId { get; init; } = string.Empty;
}
