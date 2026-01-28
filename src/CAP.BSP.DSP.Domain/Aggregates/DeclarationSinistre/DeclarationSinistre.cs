using CAP.BSP.DSP.Domain.Aggregates;
using CAP.BSP.DSP.Domain.Events;
using CAP.BSP.DSP.Domain.ValueObjects;
using CAP.BSP.DSP.Domain.Exceptions;

namespace CAP.BSP.DSP.Domain.Aggregates.DeclarationSinistre;

/// <summary>
/// Aggregate root for claim declarations.
/// Manages the lifecycle and business rules for claim declarations using event sourcing.
/// </summary>
public class DeclarationSinistre : AggregateRoot
{
    /// <summary>
    /// Unique identifier for this declaration (GUID).
    /// </summary>
    public DeclarationSinistreId? Id { get; private set; }

    /// <summary>
    /// Auto-generated claim identifier (US1).
    /// </summary>
    public IdentifiantSinistre? IdentifiantSinistre { get; private set; }

    /// <summary>
    /// Contract identifier - mandatory for all claim declarations (US4).
    /// </summary>
    public IdentifiantContrat? IdentifiantContrat { get; private set; }

    /// <summary>
    /// Date when the claim occurred (US1).
    /// Must be today or in the past (US3).
    /// </summary>
    public DateSurvenance? DateSurvenance { get; private set; }

    /// <summary>
    /// System-generated declaration timestamp (US1).
    /// </summary>
    public DateDeclaration? DateDeclaration { get; private set; }

    /// <summary>
    /// Status of the declaration (US1).
    /// </summary>
    public StatutDeclaration? Statut { get; private set; }

    public DeclarationSinistre()
    {
        // Parameterless constructor for rehydration from event store
    }

    /// <summary>
    /// Factory method to declare a new claim.
    /// Enforces US4 (contract required), US3 (no future dates), and US1 (auto-generate ID).
    /// </summary>
    /// <param name="identifiantSinistre">Auto-generated claim identifier.</param>
    /// <param name="identifiantContrat">The contract identifier.</param>
    /// <param name="dateSurvenance">The date when the claim occurred.</param>
    /// <param name="correlationId">Correlation ID for tracing.</param>
    /// <param name="userId">User ID submitting the claim.</param>
    /// <returns>A new DeclarationSinistre instance.</returns>
    /// <exception cref="IdentifiantContratManquantException">Thrown when contract identifier is null.</exception>
    /// <exception cref="DateSurvenanceFutureException">Thrown when occurrence date is in the future.</exception>
    public static DeclarationSinistre Declarer(
        IdentifiantSinistre identifiantSinistre,
        IdentifiantContrat identifiantContrat,
        DateSurvenance dateSurvenance,
        string correlationId,
        string userId)
    {
        // US4: Enforce mandatory contract reference
        if (identifiantContrat == null)
        {
            throw new IdentifiantContratManquantException();
        }

        // US3: Enforce no future dates (already validated in DateSurvenance.Create)
        if (dateSurvenance == null)
        {
            throw new ArgumentNullException(nameof(dateSurvenance));
        }

        var declaration = new DeclarationSinistre();
        var declarationId = DeclarationSinistreId.New();
        var dateDeclaration = DateDeclaration.Now();

        var @event = new SinistreDeclare
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            DeclarationId = declarationId,
            IdentifiantSinistre = identifiantSinistre.Value,
            IdentifiantContrat = identifiantContrat.Value,
            DateSurvenance = dateSurvenance.Value,
            DateDeclaration = dateDeclaration.Value,
            Statut = nameof(StatutDeclaration.Declaree),
            CorrelationId = correlationId,
            CausationId = null,
            UserId = userId
        };

        declaration.AddDomainEvent(@event);
        return declaration;
    }

    /// <summary>
    /// Applies domain events to update aggregate state.
    /// </summary>
    protected override void Apply(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case SinistreDeclare declared:
                Apply(declared);
                break;
        }
    }

    private void Apply(SinistreDeclare @event)
    {
        Id = DeclarationSinistreId.Create(@event.DeclarationId);
        IdentifiantSinistre = ValueObjects.IdentifiantSinistre.Create(@event.IdentifiantSinistre);
        IdentifiantContrat = ValueObjects.IdentifiantContrat.Create(@event.IdentifiantContrat);
        DateSurvenance = ValueObjects.DateSurvenance.Create(@event.DateSurvenance);
        DateDeclaration = ValueObjects.DateDeclaration.Create(@event.DateDeclaration);
        Statut = Enum.Parse<StatutDeclaration>(@event.Statut);
    }
}
