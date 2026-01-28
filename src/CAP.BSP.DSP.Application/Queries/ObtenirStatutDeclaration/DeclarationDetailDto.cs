namespace CAP.BSP.DSP.Application.Queries.ObtenirStatutDeclaration;

/// <summary>
/// Data Transfer Object for detailed declaration information.
/// Includes complete claim data and event history.
/// </summary>
public record DeclarationDetailDto
{
    public string IdentifiantSinistre { get; init; } = string.Empty;
    public string DeclarationId { get; init; } = string.Empty;
    public string IdentifiantContrat { get; init; } = string.Empty;
    public DateTime DateSurvenance { get; init; }
    public DateTime DateDeclaration { get; init; }
    public string Statut { get; init; } = string.Empty;
    public string? TypeSinistre { get; init; }
    public string? TypeSinistreLibelle { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string CorrelationId { get; init; } = string.Empty;
    public List<EventHistoryDto> EventHistory { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// Data Transfer Object for event history items.
/// </summary>
public record EventHistoryDto
{
    public string EventType { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; }
    public string UserId { get; init; } = string.Empty;
}
