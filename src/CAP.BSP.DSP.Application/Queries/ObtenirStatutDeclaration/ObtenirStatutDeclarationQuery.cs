using CAP.BSP.DSP.Application.Queries.RechercherDeclarations;
using MediatR;

namespace CAP.BSP.DSP.Application.Queries.ObtenirStatutDeclaration;

/// <summary>
/// Query to get the detailed status of a specific claim declaration.
/// </summary>
public record ObtenirStatutDeclarationQuery : IRequest<QueryResult<DeclarationDetailDto>>
{
    /// <summary>
    /// Unique identifier of the claim (format: SIN-YYYY-NNNNNN).
    /// </summary>
    public string IdentifiantSinistre { get; init; } = string.Empty;
}
