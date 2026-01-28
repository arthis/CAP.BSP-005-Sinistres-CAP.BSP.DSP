using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CAP.BSP.DSP.Application.ReadModels;

/// <summary>
/// MongoDB document for declaration detail view (complete projection with all claim data).
/// Used by ObtenirStatutDeclaration query for getting full claim details.
/// </summary>
public class DeclarationDetailProjection
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string IdentifiantSinistre { get; set; } = string.Empty;

    [BsonElement("declarationId")]
    [BsonRepresentation(BsonType.String)]
    public string DeclarationId { get; set; } = string.Empty;

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
    public string UserId { get; set; } = string.Empty;

    [BsonElement("correlationId")]
    public string CorrelationId { get; set; } = string.Empty;

    [BsonElement("eventHistory")]
    public List<EventHistoryItem> EventHistory { get; set; } = new();

    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Represents an event in the claim's history.
/// </summary>
public class EventHistoryItem
{
    [BsonElement("eventType")]
    public string EventType { get; set; } = string.Empty;

    [BsonElement("occurredAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime OccurredAt { get; set; }

    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("metadata")]
    public BsonDocument? Metadata { get; set; }
}
