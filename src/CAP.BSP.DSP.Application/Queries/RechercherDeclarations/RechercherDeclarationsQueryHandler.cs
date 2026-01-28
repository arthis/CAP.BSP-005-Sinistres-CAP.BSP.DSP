using CAP.BSP.DSP.Application.Ports;
using CAP.BSP.DSP.Application.ReadModels;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CAP.BSP.DSP.Application.Queries.RechercherDeclarations;

public class RechercherDeclarationsQueryHandler : IRequestHandler<RechercherDeclarationsQuery, QueryResult<RechercherDeclarationsResult>>
{
    private readonly IDeclarationReadModelRepository _readModelRepository;
    private readonly ILogger<RechercherDeclarationsQueryHandler> _logger;

    public RechercherDeclarationsQueryHandler(
        IDeclarationReadModelRepository readModelRepository,
        ILogger<RechercherDeclarationsQueryHandler> logger)
    {
        _readModelRepository = readModelRepository ?? throw new ArgumentNullException(nameof(readModelRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<QueryResult<RechercherDeclarationsResult>> Handle(
        RechercherDeclarationsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Searching declarations with filters: IdentifiantContrat={IdentifiantContrat}, Statut={Statut}, Limit={Limit}, Offset={Offset}",
                request.IdentifiantContrat ?? "null",
                request.Statut ?? "null",
                request.Limit,
                request.Offset);

            // Validate pagination parameters
            var limit = Math.Min(Math.Max(1, request.Limit), 100); // Clamp between 1 and 100
            var offset = Math.Max(0, request.Offset);

            var results = await _readModelRepository.FindWithFiltersAsync(
                identifiantContrat: request.IdentifiantContrat,
                statut: request.Statut,
                limit: limit,
                offset: offset,
                cancellationToken: cancellationToken);

            var declarations = results
                .Cast<DeclarationListProjection>()
                .Select(p => new DeclarationListItemDto
                {
                    IdentifiantSinistre = p.IdentifiantSinistre,
                    IdentifiantContrat = p.IdentifiantContrat,
                    DateSurvenance = p.DateSurvenance,
                    DateDeclaration = p.DateDeclaration,
                    Statut = p.Statut,
                    TypeSinistre = p.TypeSinistre,
                    TypeSinistreLibelle = p.TypeSinistreLibelle
                })
                .ToList();

            var result = new RechercherDeclarationsResult
            {
                Declarations = declarations,
                TotalCount = declarations.Count,
                Limit = limit,
                Offset = offset
            };

            _logger.LogInformation(
                "Found {Count} declarations matching the search criteria",
                declarations.Count);

            return QueryResult<RechercherDeclarationsResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search declarations");
            return QueryResult<RechercherDeclarationsResult>.Failure($"Failed to search declarations: {ex.Message}");
        }
    }
}
