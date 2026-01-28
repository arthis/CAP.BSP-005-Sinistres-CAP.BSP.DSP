using CAP.BSP.DSP.Application.Ports;
using CAP.BSP.DSP.Application.Queries.RechercherDeclarations;
using CAP.BSP.DSP.Application.ReadModels;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CAP.BSP.DSP.Application.Queries.ObtenirStatutDeclaration;

public class ObtenirStatutDeclarationQueryHandler : IRequestHandler<ObtenirStatutDeclarationQuery, QueryResult<DeclarationDetailDto>>
{
    private readonly IDeclarationReadModelRepository _readModelRepository;
    private readonly ILogger<ObtenirStatutDeclarationQueryHandler> _logger;

    public ObtenirStatutDeclarationQueryHandler(
        IDeclarationReadModelRepository readModelRepository,
        ILogger<ObtenirStatutDeclarationQueryHandler> logger)
    {
        _readModelRepository = readModelRepository ?? throw new ArgumentNullException(nameof(readModelRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<QueryResult<DeclarationDetailDto>> Handle(
        ObtenirStatutDeclarationQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Retrieving declaration status for claim {IdentifiantSinistre}",
                request.IdentifiantSinistre);

            if (string.IsNullOrWhiteSpace(request.IdentifiantSinistre))
            {
                return QueryResult<DeclarationDetailDto>.Failure("IdentifiantSinistre is required");
            }

            var result = await _readModelRepository.FindByIdAsync(request.IdentifiantSinistre, cancellationToken);

            if (result == null)
            {
                _logger.LogWarning(
                    "Declaration not found for claim {IdentifiantSinistre}",
                    request.IdentifiantSinistre);
                
                return QueryResult<DeclarationDetailDto>.Failure($"Declaration not found: {request.IdentifiantSinistre}");
            }

            var projection = (DeclarationDetailProjection)result;

            var detailDto = new DeclarationDetailDto
            {
                IdentifiantSinistre = projection.IdentifiantSinistre,
                DeclarationId = projection.DeclarationId,
                IdentifiantContrat = projection.IdentifiantContrat,
                DateSurvenance = projection.DateSurvenance,
                DateDeclaration = projection.DateDeclaration,
                Statut = projection.Statut,
                TypeSinistre = projection.TypeSinistre,
                TypeSinistreLibelle = projection.TypeSinistreLibelle,
                UserId = projection.UserId,
                CorrelationId = projection.CorrelationId,
                EventHistory = projection.EventHistory.Select(e => new EventHistoryDto
                {
                    EventType = e.EventType,
                    OccurredAt = e.OccurredAt,
                    UserId = e.UserId
                }).ToList(),
                CreatedAt = projection.CreatedAt,
                UpdatedAt = projection.UpdatedAt
            };

            _logger.LogInformation(
                "Declaration status retrieved successfully for claim {IdentifiantSinistre}",
                request.IdentifiantSinistre);

            return QueryResult<DeclarationDetailDto>.Success(detailDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to retrieve declaration status for claim {IdentifiantSinistre}",
                request.IdentifiantSinistre);
            
            return QueryResult<DeclarationDetailDto>.Failure($"Failed to retrieve declaration status: {ex.Message}");
        }
    }
}
