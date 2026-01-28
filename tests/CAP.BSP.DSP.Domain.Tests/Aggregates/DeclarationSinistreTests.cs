using CAP.BSP.DSP.Domain.Aggregates.DeclarationSinistre;
using CAP.BSP.DSP.Domain.Events;
using CAP.BSP.DSP.Domain.Exceptions;
using CAP.BSP.DSP.Domain.ValueObjects;

namespace CAP.BSP.DSP.Domain.Tests.Aggregates;

public class DeclarationSinistreTests
{
    private const string ValidContractId = "POL-20260128-00001";
    private const string ValidClaimId = "SIN-2026-000001";
    private const string ValidUserId = "user-123";
    private const string ValidCorrelationId = "correlation-123";

    [Fact]
    public void Declarer_WithValidData_ShouldCreateDeclaration()
    {
        // Arrange
        var identifiantSinistre = IdentifiantSinistre.Create(ValidClaimId);
        var identifiantContrat = IdentifiantContrat.Create(ValidContractId);
        var dateSurvenance = DateSurvenance.Create(DateTime.UtcNow.AddDays(-1));

        // Act
        var declaration = DeclarationSinistre.Declarer(
            identifiantSinistre,
            identifiantContrat,
            dateSurvenance,
            ValidCorrelationId,
            ValidUserId);

        // Assert
        Assert.NotNull(declaration);
        Assert.NotNull(declaration.Id);
        Assert.Equal(identifiantSinistre, declaration.IdentifiantSinistre);
        Assert.Equal(identifiantContrat, declaration.IdentifiantContrat);
        Assert.Equal(dateSurvenance, declaration.DateSurvenance);
        Assert.Equal(StatutDeclaration.Declaree, declaration.Statut);
        Assert.NotNull(declaration.DateDeclaration);
    }

    [Fact]
    public void Declarer_ShouldGenerateDomainEvent()
    {
        // Arrange
        var identifiantSinistre = IdentifiantSinistre.Create(ValidClaimId);
        var identifiantContrat = IdentifiantContrat.Create(ValidContractId);
        var dateSurvenance = DateSurvenance.Create(DateTime.UtcNow.AddDays(-1));

        // Act
        var declaration = DeclarationSinistre.Declarer(
            identifiantSinistre,
            identifiantContrat,
            dateSurvenance,
            ValidCorrelationId,
            ValidUserId);

        // Assert
        var domainEvents = declaration.DomainEvents.ToList();
        Assert.Single(domainEvents);
        
        var declarationEvent = domainEvents[0] as SinistreDeclare;
        Assert.NotNull(declarationEvent);
        Assert.Equal(ValidClaimId, declarationEvent.IdentifiantSinistre);
        Assert.Equal(ValidContractId, declarationEvent.IdentifiantContrat);
        Assert.Equal(dateSurvenance.Value, declarationEvent.DateSurvenance);
        Assert.Equal(ValidCorrelationId, declarationEvent.CorrelationId);
        Assert.Equal(ValidUserId, declarationEvent.UserId);
        Assert.Equal(nameof(StatutDeclaration.Declaree), declarationEvent.Statut);
    }

    [Fact]
    public void Declarer_WithNullContractId_ShouldThrowIdentifiantContratManquantException()
    {
        // Arrange
        var identifiantSinistre = IdentifiantSinistre.Create(ValidClaimId);
        var dateSurvenance = DateSurvenance.Create(DateTime.UtcNow.AddDays(-1));

        // Act & Assert
        Assert.Throws<IdentifiantContratManquantException>(() =>
            DeclarationSinistre.Declarer(
                identifiantSinistre,
                null!,
                dateSurvenance,
                ValidCorrelationId,
                ValidUserId));
    }

    [Fact]
    public void Declarer_WithNullDateSurvenance_ShouldThrowArgumentNullException()
    {
        // Arrange
        var identifiantSinistre = IdentifiantSinistre.Create(ValidClaimId);
        var identifiantContrat = IdentifiantContrat.Create(ValidContractId);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            DeclarationSinistre.Declarer(
                identifiantSinistre,
                identifiantContrat,
                null!,
                ValidCorrelationId,
                ValidUserId));
    }

    [Fact]
    public void Declarer_WithTodayDate_ShouldSucceed()
    {
        // Arrange
        var identifiantSinistre = IdentifiantSinistre.Create(ValidClaimId);
        var identifiantContrat = IdentifiantContrat.Create(ValidContractId);
        var dateSurvenance = DateSurvenance.Create(DateTime.UtcNow);

        // Act
        var declaration = DeclarationSinistre.Declarer(
            identifiantSinistre,
            identifiantContrat,
            dateSurvenance,
            ValidCorrelationId,
            ValidUserId);

        // Assert
        Assert.NotNull(declaration);
        Assert.Equal(DateTime.UtcNow.Date, declaration.DateSurvenance!.Value.Date);
    }

    [Fact]
    public void Declarer_WithPastDate_ShouldSucceed()
    {
        // Arrange
        var identifiantSinistre = IdentifiantSinistre.Create(ValidClaimId);
        var identifiantContrat = IdentifiantContrat.Create(ValidContractId);
        var pastDate = DateTime.UtcNow.AddMonths(-6);
        var dateSurvenance = DateSurvenance.Create(pastDate);

        // Act
        var declaration = DeclarationSinistre.Declarer(
            identifiantSinistre,
            identifiantContrat,
            dateSurvenance,
            ValidCorrelationId,
            ValidUserId);

        // Assert
        Assert.NotNull(declaration);
        Assert.Equal(pastDate.Date, declaration.DateSurvenance!.Value.Date);
    }

    [Fact]
    public void Declarer_ShouldSetStatusToDeclaree()
    {
        // Arrange
        var identifiantSinistre = IdentifiantSinistre.Create(ValidClaimId);
        var identifiantContrat = IdentifiantContrat.Create(ValidContractId);
        var dateSurvenance = DateSurvenance.Create(DateTime.UtcNow.AddDays(-1));

        // Act
        var declaration = DeclarationSinistre.Declarer(
            identifiantSinistre,
            identifiantContrat,
            dateSurvenance,
            ValidCorrelationId,
            ValidUserId);

        // Assert
        Assert.Equal(StatutDeclaration.Declaree, declaration.Statut);
    }

    [Fact]
    public void Declarer_ShouldGenerateDeclarationId()
    {
        // Arrange
        var identifiantSinistre = IdentifiantSinistre.Create(ValidClaimId);
        var identifiantContrat = IdentifiantContrat.Create(ValidContractId);
        var dateSurvenance = DateSurvenance.Create(DateTime.UtcNow.AddDays(-1));

        // Act
        var declaration = DeclarationSinistre.Declarer(
            identifiantSinistre,
            identifiantContrat,
            dateSurvenance,
            ValidCorrelationId,
            ValidUserId);

        // Assert
        Assert.NotNull(declaration.Id);
        var domainEvent = declaration.DomainEvents.First() as SinistreDeclare;
        Assert.NotNull(domainEvent);
        Assert.Equal(declaration.Id!.Value, domainEvent.DeclarationId);
    }

    [Fact]
    public void Declarer_ShouldSetDateDeclarationToNow()
    {
        // Arrange
        var identifiantSinistre = IdentifiantSinistre.Create(ValidClaimId);
        var identifiantContrat = IdentifiantContrat.Create(ValidContractId);
        var dateSurvenance = DateSurvenance.Create(DateTime.UtcNow.AddDays(-1));
        var beforeCreation = DateTime.UtcNow;

        // Act
        var declaration = DeclarationSinistre.Declarer(
            identifiantSinistre,
            identifiantContrat,
            dateSurvenance,
            ValidCorrelationId,
            ValidUserId);

        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.NotNull(declaration.DateDeclaration);
        Assert.True(declaration.DateDeclaration.Value >= beforeCreation);
        Assert.True(declaration.DateDeclaration.Value <= afterCreation);
    }

    [Fact]
    public void Declarer_MultipleDeclarations_ShouldHaveUniqueDomainEventIds()
    {
        // Arrange
        var identifiantSinistre1 = IdentifiantSinistre.Create("SIN-2026-000001");
        var identifiantSinistre2 = IdentifiantSinistre.Create("SIN-2026-000002");
        var identifiantContrat = IdentifiantContrat.Create(ValidContractId);
        var dateSurvenance = DateSurvenance.Create(DateTime.UtcNow.AddDays(-1));

        // Act
        var declaration1 = DeclarationSinistre.Declarer(
            identifiantSinistre1, identifiantContrat, dateSurvenance, ValidCorrelationId, ValidUserId);
        var declaration2 = DeclarationSinistre.Declarer(
            identifiantSinistre2, identifiantContrat, dateSurvenance, ValidCorrelationId, ValidUserId);

        // Assert
        var event1 = declaration1.DomainEvents.First() as SinistreDeclare;
        var event2 = declaration2.DomainEvents.First() as SinistreDeclare;
        
        Assert.NotNull(event1);
        Assert.NotNull(event2);
        Assert.NotEqual(event1.EventId, event2.EventId);
        Assert.NotEqual(event1.DeclarationId, event2.DeclarationId);
    }
}
