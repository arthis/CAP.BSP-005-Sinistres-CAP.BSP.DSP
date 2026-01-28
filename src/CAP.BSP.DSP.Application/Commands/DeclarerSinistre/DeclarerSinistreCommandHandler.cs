using CAP.BSP.DSP.Application.Ports;
using CAP.BSP.DSP.Domain.Aggregates.DeclarationSinistre;
using CAP.BSP.DSP.Domain.Services;
using CAP.BSP.DSP.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CAP.BSP.DSP.Application.Commands.DeclarerSinistre;

public class DeclarerSinistreCommandHandler : IRequestHandler<DeclarerSinistreCommand, CommandResult>
{
    private readonly IIdentifiantSinistreGenerator _identifiantGenerator;
    private readonly IDeclarationRepository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IPublisher _mediatorPublisher;
    private readonly ILogger<DeclarerSinistreCommandHandler> _logger;

    public DeclarerSinistreCommandHandler(
        IIdentifiantSinistreGenerator identifiantGenerator,
        IDeclarationRepository repository,
        IEventPublisher eventPublisher,
        IPublisher mediatorPublisher,
        ILogger<DeclarerSinistreCommandHandler> logger)
    {
        _identifiantGenerator = identifiantGenerator ?? throw new ArgumentNullException(nameof(identifiantGenerator));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _mediatorPublisher = mediatorPublisher ?? throw new ArgumentNullException(nameof(mediatorPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CommandResult> Handle(DeclarerSinistreCommand command, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Processing DeclarerSinistreCommand for contract {IdentifiantContrat} with correlation ID {CorrelationId}",
                command.IdentifiantContrat,
                command.CorrelationId);

            // Generate unique claim identifier
            var identifiantSinistre = await _identifiantGenerator.GenerateNextAsync(cancellationToken);
            var identifiantSinistreValue = IdentifiantSinistre.Create(identifiantSinistre);

            // Create value objects from command
            var identifiantContrat = IdentifiantContrat.Create(command.IdentifiantContrat);
            var dateSurvenance = DateSurvenance.Create(command.DateSurvenance);
            var dateDeclaration = DateDeclaration.Now();

            // Create aggregate using factory method
            var declaration = DeclarationSinistre.Declarer(
                identifiantSinistreValue,
                identifiantContrat,
                dateSurvenance,
                command.CorrelationId,
                command.UserId);

            // Capture domain events before they are cleared by SaveAsync
            var domainEvents = declaration.DomainEvents.ToList();

            // Persist events to EventStoreDB
            await _repository.SaveAsync(declaration, cancellationToken);

            _logger.LogInformation(
                "Claim declared successfully with identifier {IdentifiantSinistre} for contract {IdentifiantContrat}",
                identifiantSinistre,
                command.IdentifiantContrat);

            // Publish domain events to RabbitMQ and MediatR (for read model projections)
            foreach (var domainEvent in domainEvents)
            {
                // Publish to RabbitMQ for external consumers
                await _eventPublisher.PublishAsync(domainEvent, cancellationToken);
                
                // Publish via MediatR for internal event handlers (projections)
                await _mediatorPublisher.Publish(domainEvent, cancellationToken);
            }

            return CommandResult.Success(new
            {
                IdentifiantSinistre = identifiantSinistre,
                DateDeclaration = dateDeclaration.Value
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to process DeclarerSinistreCommand for contract {IdentifiantContrat} with correlation ID {CorrelationId}",
                command.IdentifiantContrat,
                command.CorrelationId);

            return CommandResult.Failure($"Failed to declare claim: {ex.Message}", "COMMAND_EXECUTION_ERROR");
        }
    }
}
