namespace CAP.BSP.DSP.Application.Queries.RechercherDeclarations;

/// <summary>
/// Data Transfer Object for declaration list items.
/// Lightweight representation for list views.
/// </summary>
public record DeclarationListItemDto
{
    public string IdentifiantSinistre { get; init; } = string.Empty;
    public string IdentifiantContrat { get; init; } = string.Empty;
    public DateTime DateSurvenance { get; init; }
    public DateTime DateDeclaration { get; init; }
    public string Statut { get; init; } = string.Empty;
    public string? TypeSinistre { get; init; }
    public string? TypeSinistreLibelle { get; init; }
}
