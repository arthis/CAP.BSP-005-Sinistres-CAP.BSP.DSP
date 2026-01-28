using CAP.BSP.DSP.Application.Ports;
using CAP.BSP.DSP.Application.ReadModels;
using CAP.BSP.DSP.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CAP.BSP.DSP.Application.EventHandlers;

/// <summary>
/// Event handler that projects SinistreDeclare domain events to MongoDB read model.
/// Implements eventual consistency pattern for CQRS query side.
/// </summary>
public class SinistreDeclareEventHandler : INotificationHandler<SinistreDeclare>
{
    private readonly IDeclarationReadModelRepository _readModelRepository;
    private readonly ILogger<SinistreDeclareEventHandler> _logger;

    public SinistreDeclareEventHandler(
        IDeclarationReadModelRepository readModelRepository,
        ILogger<SinistreDeclareEventHandler> logger)
    {
        _readModelRepository = readModelRepository ?? throw new ArgumentNullException(nameof(readModelRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(SinistreDeclare notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Projecting SinistreDeclare event {EventId} to read model for claim {IdentifiantSinistre}",
                notification.EventId,
                notification.IdentifiantSinistre);

            var now = DateTime.UtcNow;

            // Create detail projection (complete view)
            var detailProjection = new DeclarationDetailProjection
            {
                IdentifiantSinistre = notification.IdentifiantSinistre,
                DeclarationId = notification.DeclarationId.ToString(),
                IdentifiantContrat = notification.IdentifiantContrat,
                DateSurvenance = notification.DateSurvenance,
                DateDeclaration = notification.DateDeclaration,
                Statut = notification.Statut,
                UserId = notification.UserId ?? "system",
                CorrelationId = notification.CorrelationId ?? string.Empty,
                EventHistory = new List<EventHistoryItem>
                {
                    new EventHistoryItem
                    {
                        EventType = "SinistreDeclare",
                        OccurredAt = notification.OccurredAt,
                        UserId = notification.UserId ?? "system"
                    }
                },
                CreatedAt = now,
                UpdatedAt = now
            };

            await _readModelRepository.AddAsync(detailProjection, cancellationToken);

            _logger.LogInformation(
                "Successfully projected SinistreDeclare event {EventId} to read model for claim {IdentifiantSinistre}",
                notification.EventId,
                notification.IdentifiantSinistre);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to project SinistreDeclare event {EventId} to read model for claim {IdentifiantSinistre}",
                notification.EventId,
                notification.IdentifiantSinistre);
            
            // In production, consider using transactional outbox pattern or retry policies
            throw;
        }
    }
}
