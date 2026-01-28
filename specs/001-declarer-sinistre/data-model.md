# Data Model: SinistreDéclaré Event & DeclarerSinistre Command

**Feature**: 001-declarer-sinistre  
**Phase**: 1 (Design & Contracts)  
**Created**: 2026-01-27

## Overview

This document defines the domain model for the Claims Declaration capability using Domain-Driven Design (DDD) tactical patterns. The model is implementation-agnostic but includes C# code examples for clarity.

## Domain Model Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                  AGGREGATE ROOT                             │
│  DéclarationSinistre                                        │
│  ─────────────────────────────────────────────────────────  │
│  + Id: DeclarationSinistreId (guid)                         │
│  + IdentifiantSinistre: IdentifiantSinistre (value object)  │
│  + IdentifiantContrat: IdentifiantContrat                   │
│  + TypeSinistre: TypeSinistre                               │
│  + DateSurvenance: DateSurvenance                           │
│  + DateDeclaration: DateDeclaration                         │
│  + Statut: StatutDeclaration                                │
│  ─────────────────────────────────────────────────────────  │
│  + Declarer(identifiantContrat, typeSinistre,               │
│              dateSurvenance): DéclarationSinistre           │
│  + Valider(): void                                          │
│  + Annuler(motif): void                                     │
│  ─────────────────────────────────────────────────────────  │
│  📧 Emits: SinistreDéclaré, DéclarationValidée,             │
│           DéclarationAnnulée                                │
└─────────────────────────────────────────────────────────────┘
                          │
                          │ rebuilds from
                          ↓
        ┌────────────────────────────────────┐
        │      EVENT STREAM (EventStoreDB)   │
        │  Stream: "declaration-sinistre-{id}"│
        │                                     │
        │  [0] SinistreDéclaré                │
        │  [1] DéclarationValidée (future)    │
        │  [2] DéclarationAnnulée (future)    │
        └────────────────────────────────────┘
```

---

## Aggregates

### DéclarationSinistre (Aggregate Root)

**Purpose**: Enforce business invariants for claim declarations and maintain event-sourced state.

**Invariants**:
1. `IdentifiantContrat` MUST NOT be null or empty
2. `TypeSinistre` MUST exist in reference database
3. `DateSurvenance` MUST be ≤ today
4. `IdentifiantSinistre` MUST be unique (enforced by generator)
5. `DateDeclaration` MUST be set to server time (not user-provided)
6. State transitions: DECLARE → VALIDE → ANNULE (one-way, no reversal)

**C# Implementation Sketch**:
```csharp
public class DeclarationSinistre : AggregateRoot
{
    public DeclarationSinistreId Id { get; private set; }
    public IdentifiantSinistre IdentifiantSinistre { get; private set; }
    public IdentifiantContrat IdentifiantContrat { get; private set; }
    public TypeSinistre TypeSinistre { get; private set; }
    public DateSurvenance DateSurvenance { get; private set; }
    public DateDeclaration DateDeclaration { get; private set; }
    public StatutDeclaration Statut { get; private set; }
    
    // Private constructor for event sourcing (replay)
    private DeclarationSinistre(DeclarationSinistreId id)
    {
        Id = id;
    }
    
    // Factory method: Create new declaration (command side)
    public static DeclarationSinistre Declarer(
        IdentifiantSinistre identifiantSinistre,
        IdentifiantContrat identifiantContrat,
        TypeSinistre typeSinistre,
        DateSurvenance dateSurvenance,
        ITypeSinistreValidator validator)
    {
        // Validate business rules
        if (!validator.IsValid(typeSinistre.Code))
            throw new TypeSinistreInvalideException(typeSinistre.Code);
        
        if (dateSurvenance.Value > DateTime.UtcNow.Date)
            throw new DateSurvenanceFutureException(dateSurvenance);
        
        if (identifiantContrat == null)
            throw new IdentifiantContratManquantException();
        
        // Create aggregate and apply event
        var aggregate = new DeclarationSinistre(DeclarationSinistreId.New());
        var @event = new SinistreDeclare(
            identifiantSinistre: identifiantSinistre.Value,
            identifiantContrat: identifiantContrat.Value,
            typeSinistre: typeSinistre.Code,
            dateSurvenance: dateSurvenance.Value,
            dateDeclaration: DateTime.UtcNow,
            statut: "DECLARE"
        );
        
        aggregate.Apply(@event);
        aggregate.AddDomainEvent(@event);
        
        return aggregate;
    }
    
    // Event handler (rebuild state from events)
    private void Apply(SinistreDeclare @event)
    {
        IdentifiantSinistre = IdentifiantSinistre.Create(@event.IdentifiantSinistre);
        IdentifiantContrat = IdentifiantContrat.Create(@event.IdentifiantContrat);
        TypeSinistre = TypeSinistre.Create(@event.TypeSinistre);
        DateSurvenance = DateSurvenance.Create(@event.DateSurvenance);
        DateDeclaration = DateDeclaration.Create(@event.DateDeclaration);
        Statut = StatutDeclaration.Declare;
    }
}
```

**Stream Naming Convention**:
- Stream ID: `"declaration-sinistre-{DeclarationSinistreId}"`
- Example: `"declaration-sinistre-3fa85f64-5717-4562-b3fc-2c963f66afa6"`

---

## Value Objects

Value objects are **immutable**, **equality-comparable by value**, and have **no identity**. They encapsulate domain rules and prevent primitive obsession.

### IdentifiantSinistre

**Purpose**: Unique identifier for a claim in format `SIN-{YEAR}-{SEQUENCE}`

**Rules**:
- Format: `SIN-YYYY-NNNNNN` (e.g., `SIN-2026-000001`)
- Year: 4 digits
- Sequence: 6 digits, zero-padded
- Validation: Regex `^SIN-\d{4}-\d{6}$`

**C# Implementation**:
```csharp
public record IdentifiantSinistre
{
    public string Value { get; }
    
    private IdentifiantSinistre(string value)
    {
        Value = value;
    }
    
    public static IdentifiantSinistre Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("IdentifiantSinistre ne peut pas être vide", nameof(value));
        
        var regex = new Regex(@"^SIN-\d{4}-\d{6}$");
        if (!regex.IsMatch(value))
            throw new ArgumentException(
                $"Format invalide pour IdentifiantSinistre: {value}. Attendu: SIN-YYYY-NNNNNN", 
                nameof(value));
        
        return new IdentifiantSinistre(value);
    }
    
    // Records provide value equality by default
}
```

---

### IdentifiantContrat

**Purpose**: Reference to an insurance contract

**Rules**:
- Non-null, non-empty string
- Format validation: [NEEDS CLARIFICATION from CAP.BSP.004 - assume alphanumeric for now]
- No length limit (flexible for different contract number formats)

**C# Implementation**:
```csharp
public record IdentifiantContrat
{
    public string Value { get; }
    
    private IdentifiantContrat(string value)
    {
        Value = value;
    }
    
    public static IdentifiantContrat Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("IdentifiantContrat ne peut pas être vide", nameof(value));
        
        return new IdentifiantContrat(value);
    }
}
```

---

### TypeSinistre

**Purpose**: Claim type classification (validated against reference database)

**Rules**:
- Code must exist in MongoDB `typeSinistreReference` collection
- Examples: `ACCIDENT_CORPOREL`, `DEGATS_MATERIELS`, `RESPONSABILITE_CIVILE`
- Validation happens in command handler (not in value object constructor)

**C# Implementation**:
```csharp
public record TypeSinistre
{
    public string Code { get; }
    
    private TypeSinistre(string code)
    {
        Code = code;
    }
    
    public static TypeSinistre Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("TypeSinistre ne peut pas être vide", nameof(code));
        
        // Note: Actual validation against reference DB happens in ITypeSinistreValidator
        // This just ensures non-empty
        return new TypeSinistre(code.ToUpperInvariant());
    }
}
```

---

### DateSurvenance

**Purpose**: Date the insured event occurred (must be in the past or today)

**Rules**:
- Date only (no time component)
- Must be ≤ today (UTC)
- Cannot be future date

**C# Implementation**:
```csharp
public record DateSurvenance
{
    public DateTime Value { get; }
    
    private DateSurvenance(DateTime value)
    {
        Value = value.Date; // Strip time component
    }
    
    public static DateSurvenance Create(DateTime value)
    {
        var dateOnly = value.Date;
        var today = DateTime.UtcNow.Date;
        
        if (dateOnly > today)
            throw new DateSurvenanceFutureException(
                $"DateSurvenance ne peut pas être dans le futur. Valeur: {dateOnly:yyyy-MM-dd}, Aujourd'hui: {today:yyyy-MM-dd}");
        
        return new DateSurvenance(dateOnly);
    }
}
```

---

### DateDeclaration

**Purpose**: Timestamp when claim was declared (system-set, NOT user-provided)

**Rules**:
- UTC timestamp (includes date AND time)
- Auto-generated by server
- Immutable after creation

**C# Implementation**:
```csharp
public record DateDeclaration
{
    public DateTime Value { get; }
    
    private DateDeclaration(DateTime value)
    {
        Value = value;
    }
    
    public static DateDeclaration Now()
    {
        return new DateDeclaration(DateTime.UtcNow);
    }
    
    public static DateDeclaration Create(DateTime value)
    {
        return new DateDeclaration(value);
    }
}
```

---

### StatutDeclaration

**Purpose**: Declaration lifecycle state

**States**:
- `DECLARE`: Initial state after declaration
- `VALIDE`: Declaration has been validated (future)
- `ANNULE`: Declaration has been cancelled (future)

**Transitions**:
```
DECLARE → VALIDE → ANNULE
   ↓         ↓
ANNULE   ANNULE
(one-way, no reversal from ANNULE)
```

**C# Implementation**:
```csharp
public enum StatutDeclaration
{
    Declare,  // Initial state
    Valide,   // (future feature)
    Annule    // (future feature)
}
```

---

## Domain Events

Domain events are **immutable**, **past-tense named**, and represent **facts that have occurred**.

### SinistreDéclaré

**Purpose**: Published when a claim has been successfully declared

**BCM Schema Compliance**: MUST match `events-BSP-005-Sinistres & Prestations.yaml`

**Properties**:
```csharp
public record SinistreDeclare : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string IdentifiantSinistre { get; init; }
    public string IdentifiantContrat { get; init; }
    public string TypeSinistre { get; init; }
    public DateTime DateSurvenance { get; init; }
    public DateTime DateDeclaration { get; init; }
    public string Statut { get; init; } // "DECLARE"
    
    // Metadata for event sourcing
    public Guid CorrelationId { get; init; }
    public Guid CausationId { get; init; }
    public string UserId { get; init; } // Who declared (for audit trail)
}
```

**JSON Schema** (see `contracts/SinistreDeclare.event.json`):
```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "properties": {
    "eventId": { "type": "string", "format": "uuid" },
    "occurredAt": { "type": "string", "format": "date-time" },
    "identifiantSinistre": { "type": "string", "pattern": "^SIN-\\d{4}-\\d{6}$" },
    "identifiantContrat": { "type": "string", "minLength": 1 },
    "typeSinistre": { "type": "string", "minLength": 1 },
    "dateSurvenance": { "type": "string", "format": "date" },
    "dateDeclaration": { "type": "string", "format": "date-time" },
    "statut": { "type": "string", "enum": ["DECLARE", "VALIDE", "ANNULE"] }
  },
  "required": ["eventId", "occurredAt", "identifiantSinistre", "identifiantContrat", "typeSinistre", "dateSurvenance", "dateDeclaration", "statut"]
}
```

---

## Commands

Commands are **imperative-named** (verb + object), **mutable** (DTOs), and represent **intent**.

### DeclarerSinistreCommand

**Purpose**: Input DTO for declaring a claim

**Properties**:
```csharp
public record DeclarerSinistreCommand : ICommand<CommandResult>
{
    public string IdentifiantContrat { get; init; }
    public string TypeSinistre { get; init; }
    public DateTime DateSurvenance { get; init; }
    
    // Metadata (set by infrastructure, not user)
    public Guid CorrelationId { get; init; }
    public string UserId { get; init; } // From authentication context
    
    // NOT INCLUDED (auto-generated by handler):
    // - IdentifiantSinistre (generated by IIdentifiantSinistreGenerator)
    // - DateDeclaration (set to DateTime.UtcNow)
}
```

**Validation (FluentValidation)**:
```csharp
public class DeclarerSinistreCommandValidator : AbstractValidator<DeclarerSinistreCommand>
{
    public DeclarerSinistreCommandValidator()
    {
        RuleFor(x => x.IdentifiantContrat)
            .NotEmpty()
            .WithMessage("IdentifiantContrat obligatoire");
        
        RuleFor(x => x.TypeSinistre)
            .NotEmpty()
            .WithMessage("TypeSinistre obligatoire");
        
        RuleFor(x => x.DateSurvenance)
            .LessThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("DateSurvenance ne peut pas être dans le futur");
    }
}
```

**OpenAPI Schema** (see `contracts/openapi.yaml`):
```yaml
DeclarerSinistreCommand:
  type: object
  required:
    - identifiantContrat
    - typeSinistre
    - dateSurvenance
  properties:
    identifiantContrat:
      type: string
      example: "CONTRAT-2026-001"
    typeSinistre:
      type: string
      example: "ACCIDENT_CORPOREL"
    dateSurvenance:
      type: string
      format: date
      example: "2026-01-20"
```

---

## Read Models (MongoDB Projections)

Read models are **denormalized**, **query-optimized** projections built from domain events.

### DeclarationListProjection

**Purpose**: List view for searching/browsing declarations

**Collection**: `declarationReadModel`

**Document Structure**:
```json
{
  "_id": "SIN-2026-000001",
  "identifiantSinistre": "SIN-2026-000001",
  "identifiantContrat": "CONTRAT-2026-001",
  "typeSinistre": "ACCIDENT_CORPOREL",
  "typeSinistreLibelle": "Accident corporel",
  "dateSurvenance": "2026-01-20",
  "dateDeclaration": "2026-01-27T14:30:00Z",
  "statut": "DECLARE",
  "version": 1
}
```

**Indexes**:
- `{ identifiantSinistre: 1 }` (unique, primary key)
- `{ identifiantContrat: 1, dateDeclaration: -1 }` (search by contract, sorted by recency)
- `{ typeSinistre: 1, statut: 1 }` (filter by type + status)

---

### DeclarationDetailProjection

**Purpose**: Detail view for single declaration retrieval

**Collection**: Same as list (`declarationReadModel`) - no separate collection needed

**Query**: `db.declarationReadModel.findOne({ _id: "SIN-2026-000001" })`

---

## Validation Flow

```
HTTP Request → Command → FluentValidation → Domain Validation → Aggregate → Event
                  ↓              ↓                   ↓              ↓          ↓
            Parse JSON    Required fields    Business rules    Apply state  Persist
                          Not null/empty     TypeSinistre      Generate ID  EventStoreDB
                          Format checks      DateSurvenance
                                             IdentifiantContrat
```

**Validation Layers**:
1. **HTTP/JSON**: Parse errors (malformed JSON, type mismatches)
2. **FluentValidation**: Structural validation (required fields, basic format)
3. **Domain Validation**: Business rules (TypeSinistre exists, DateSurvenance ≤ today)
4. **Aggregate Invariants**: State consistency (no duplicate IDs, valid transitions)

---

## Summary

| Concept | Count | Examples |
|---------|-------|----------|
| **Aggregates** | 1 | DéclarationSinistre |
| **Value Objects** | 6 | IdentifiantSinistre, TypeSinistre, DateSurvenance, DateDeclaration, IdentifiantContrat, StatutDeclaration |
| **Domain Events** | 1 (MVP) | SinistreDéclaré (+ 2 future: DéclarationValidée, DéclarationAnnulée) |
| **Commands** | 1 | DeclarerSinistreCommand |
| **Read Models** | 1 | DeclarationListProjection (with detail view) |
| **Domain Services** | 2 | ITypeSinistreValidator, IIdentifiantSinistreGenerator |

**Complexity**: 🟢 LOW - Single aggregate, straightforward validation rules, simple event structure

---

**Data Model Complete**: 2026-01-27  
**Ready for Contracts Generation**: ✅ YES
