using FluentValidation;

namespace CAP.BSP.DSP.Application.Commands.DeclarerSinistre;

/// <summary>
/// Validator for DeclarerSinistreCommand.
/// Enforces business rules before command execution.
/// </summary>
public class DeclarerSinistreCommandValidator : AbstractValidator<DeclarerSinistreCommand>
{
    public DeclarerSinistreCommandValidator()
    {
        // US4: Contract identifier is mandatory
        RuleFor(x => x.IdentifiantContrat)
            .NotEmpty()
            .WithMessage("IdentifiantContrat obligatoire: toute déclaration de sinistre doit référencer un contrat")
            .Matches(@"^POL-\d{8}-\d{5}$")
            .WithMessage("IdentifiantContrat invalide: le format attendu est POL-YYYYMMDD-XXXXX");

        // US1/US3: Occurrence date is mandatory and must not be in the future
        RuleFor(x => x.DateSurvenance)
            .NotEmpty()
            .WithMessage("DateSurvenance obligatoire")
            .LessThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("DateSurvenance invalide: la date de survenance ne peut pas être dans le futur");

        // User ID is mandatory
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId obligatoire");

        // Correlation ID is mandatory
        RuleFor(x => x.CorrelationId)
            .NotEmpty()
            .WithMessage("CorrelationId obligatoire");
    }
}
