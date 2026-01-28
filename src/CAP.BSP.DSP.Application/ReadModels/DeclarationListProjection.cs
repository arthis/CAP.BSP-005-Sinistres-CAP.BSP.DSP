using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CAP.BSP.DSP.Application.ReadModels;

/// <summary>
/// MongoDB document for declaration list view (lightweight projection for queries).
/// Used by RechercherDeclarations query for listing claims.
/// </summary>
public class DeclarationListProjection
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string IdentifiantSinistre { get; set; } = string.Empty;

    [BsonElement("declarationId")]
    public string? DeclarationId { get; set; }

    [BsonElement("identifiantContrat")]
    public string IdentifiantContrat { get; set; } = string.Empty;

    [BsonElement("dateSurvenance")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime DateSurvenance { get; set; }

    [BsonElement("dateDeclaration")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime DateDeclaration { get; set; }

    [BsonElement("statut")]
    public string Statut { get; set; } = string.Empty;

    [BsonElement("typeSinistre")]
    public string? TypeSinistre { get; set; }

    [BsonElement("typeSinistreLibelle")]
    public string? TypeSinistreLibelle { get; set; }

    [BsonElement("userId")]
    public string? UserId { get; set; }

    [BsonElement("correlationId")]
    public string? CorrelationId { get; set; }

    [BsonElement("eventHistory")]
    [BsonIgnoreIfNull]
    public List<object>? EventHistory { get; set; }

    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAt { get; set; }
}
