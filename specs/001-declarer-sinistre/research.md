# Research: SinistreDéclaré Event & DeclarerSinistre Command

**Feature**: 001-declarer-sinistre  
**Phase**: 0 (Outline & Research)  
**Created**: 2026-01-27

## Purpose

This document resolves all "NEEDS CLARIFICATION" items from Technical Context and researches best practices for the chosen technology stack (.NET 8 + EventStoreDB + MongoDB + RabbitMQ + Azure Functions).

## Research Tasks

### 1. EventStoreDB with .NET: Event Sourcing Best Practices

**Decision**: Use EventStoreDB 23.x with official EventStore.Client NuGet package

**Rationale**:
- EventStoreDB is purpose-built for event sourcing (append-only, immutable streams)
- Official .NET gRPC client provides async/await support, connection pooling, and subscriptions
- Supports optimistic concurrency control (expected version check) for aggregate consistency
- Built-in projections for read models (can project EventStoreDB → MongoDB)
- Mature ecosystem with extensive documentation and community support

**Implementation Pattern**:
```csharp
// Aggregate stream naming convention
Stream: "declaration-sinistre-{identifiantSinistre}"

// Event metadata
- EventType: "SinistreDeclare" (matches BCM event name)
- EventId: Guid (idempotency key)
- Timestamp: UTC
- CorrelationId: For distributed tracing
- CausationId: Event that caused this event
```

**Key Decisions**:
- **Optimistic Concurrency**: Use `ExpectedVersion.NoStream` for first event, `ExpectedVersion.StreamExists` for subsequent
- **Event Versioning**: Include `$eventVersion` metadata for schema evolution
- **Snapshots**: NOT needed for MVP (DéclarationSinistre has few events, full replay is fast)
- **Projections**: Use EventStoreDB subscriptions (catchup subscriptions) to project to MongoDB

**Alternatives Considered**:
- Marten (PostgreSQL-based event sourcing): Rejected - introduces PostgreSQL dependency, less specialized than EventStoreDB
- Azure Cosmos DB with Change Feed: Rejected - more expensive, less event sourcing features

---

### 2. IdentifiantSinistre Generation Strategy

**Decision**: Use MongoDB sequence collection for sequential IDs with format `SIN-{YEAR}-{SEQUENCE}`

**Rationale**:
- Sequential IDs are user-friendly and sortable (SIN-2026-000001, SIN-2026-000002)
- MongoDB `findAndModify` provides atomic increment operation (no collisions)
- Year-based reset simplifies annual reporting and partitioning
- Predictable format aids customer service and manual lookup

**Implementation**:
```csharp
public class SequentialIdentifiantSinistreGenerator : IIdentifiantSinistreGenerator
{
    private readonly IMongoCollection<SequenceDocument> _sequences;
    
    public async Task<IdentifiantSinistre> GenerateAsync(CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var sequenceId = $"sinistre-{year}";
        
        var filter = Builders<SequenceDocument>.Filter.Eq(x => x.Id, sequenceId);
        var update = Builders<SequenceDocument>.Update.Inc(x => x.Value, 1);
        var options = new FindOneAndUpdateOptions<SequenceDocument>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };
        
        var result = await _sequences.FindOneAndUpdateAsync(filter, update, options, ct);
        var paddedSequence = result.Value.ToString("D6"); // Zero-pad to 6 digits
        
        return IdentifiantSinistre.Create($"SIN-{year}-{paddedSequence}");
    }
}
```

**Concurrency Handling**:
- MongoDB's `findAndModify` is atomic at document level (no race conditions)
- Under high load (100+ concurrent requests), sequential generation can become bottleneck
- Mitigation: Pre-allocate batches of IDs if needed (not required for 10k/day throughput)

**Alternatives Considered**:
- UUID v4: Rejected - not human-friendly, no sortability, doesn't match user requirement for sequential format
- Database sequence (PostgreSQL): Rejected - introduces PostgreSQL dependency
- Distributed Snowflake IDs: Rejected - overkill for 10k/day volume, complex setup

---

### 3. TypeSinistre Validation: Reference Data Management

**Decision**: Store typeSinistre reference data in MongoDB collection, cache in-memory with periodic refresh

**Rationale**:
- MongoDB provides flexible schema for reference data (code, libelle, categorie, actif)
- In-memory caching reduces latency (validation happens on every command)
- Cache invalidation on reference data updates via TTL or change stream
- Allows business users to manage claim types without code deployment

**Data Model**:
```json
// MongoDB collection: typeSinistreReference
{
  "_id": "accident-corporel",
  "code": "ACCIDENT_CORPOREL",
  "libelle": "Accident corporel",
  "categorie": "SANTE",
  "actif": true,
  "dateCreation": "2026-01-01T00:00:00Z",
  "dateModification": "2026-01-01T00:00:00Z"
}
```

**Caching Strategy**:
```csharp
public class CachedTypeSinistreValidator : ITypeSinistreValidator
{
    private readonly IMemoryCache _cache;
    private readonly ITypeSinistreReferenceRepository _repository;
    private const int CacheDurationMinutes = 60;
    
    public async Task<bool> IsValidAsync(string code, CancellationToken ct)
    {
        var validCodes = await _cache.GetOrCreateAsync("typeSinistre:all", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheDurationMinutes);
            var types = await _repository.GetActiveTypesAsync(ct);
            return types.Select(t => t.Code).ToHashSet();
        });
        
        return validCodes.Contains(code);
    }
}
```

**Circuit Breaker Integration** (Polly):
```csharp
var circuitBreakerPolicy = Policy
    .Handle<MongoException>()
    .CircuitBreakerAsync(
        exceptionsAllowedBeforeBreaking: 3,
        durationOfBreak: TimeSpan.FromSeconds(30),
        onBreak: (ex, duration) => Log.Warning("Circuit breaker OPEN: MongoDB unavailable"),
        onReset: () => Log.Information("Circuit breaker RESET: MongoDB recovered")
    );
```

**Alternatives Considered**:
- Hardcoded enum: Rejected - requires code deployment for new claim types
- Azure Table Storage: Rejected - less flexible querying, no change streams
- Redis cache only: Rejected - need persistent store for reference data source of truth

---

### 4. Azure Functions: Cold Start Mitigation

**Decision**: Use Azure Functions Premium Plan with pre-warmed instances for production

**Rationale**:
- Consumption Plan has 2-3s cold starts (violates <500ms p95 latency requirement)
- Premium Plan provides always-ready instances (no cold start for majority of requests)
- Supports VNet integration for secure EventStoreDB/MongoDB access
- Costs ~$150/month for 1 pre-warmed instance (acceptable for production SLA)

**Configuration**:
```json
// host.json
{
  "version": "2.0",
  "extensions": {
    "http": {
      "routePrefix": "api/v1",
      "maxConcurrentRequests": 100
    }
  },
  "functionTimeout": "00:01:00",
  "healthMonitor": {
    "enabled": true,
    "healthCheckInterval": "00:00:10"
  }
}
```

**Development Setup**:
- Local: Use Azurite (Azure Storage Emulator) + Docker Compose for EventStoreDB/MongoDB/RabbitMQ
- Functions run in isolated worker process (dependency injection via Microsoft.Extensions.DependencyInjection)

**Alternatives Considered**:
- Azure Container Instances: Rejected - more complex deployment, no built-in HTTP scaling
- Azure Kubernetes Service: Rejected - overkill for initial MVP, higher operational complexity
- Consumption Plan only: Rejected - cold starts violate latency SLA

---

### 5. RabbitMQ: Event Bus Configuration

**Decision**: Use RabbitMQ with topic exchange for domain events, fanout exchange for integration events

**Rationale**:
- Topic exchange allows selective routing (e.g., `sinistres.declare.#` pattern)
- Fanout exchange for broadcasting to multiple downstream capabilities (OED, DLC, PRS)
- Persistent messages with publisher confirms for reliability
- Dead letter queue (DLQ) for failed event processing

**Exchange/Queue Configuration**:
```text
Exchange: "cap.bsp.dsp.events" (type: topic, durable: true)
Routing Key: "sinistre.declare.v1"

Queue Bindings:
- cap.bsp.oed.sinistre-declare (CAP.BSP.OED subscribes to case opening)
- cap.bsp.dlc.sinistre-declare (CAP.BSP.DLC subscribes for fraud detection)
- cap.bsp.prs.sinistre-declare (CAP.BSP.PRS subscribes for reporting)

Dead Letter Queue: "cap.bsp.dsp.events.dlq"
```

**Publisher Implementation**:
```csharp
public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly IModel _channel;
    
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct) 
        where TEvent : IDomainEvent
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(@event);
        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true; // Durable messages
        properties.MessageId = @event.EventId.ToString();
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        properties.ContentType = "application/json";
        properties.Type = typeof(TEvent).Name; // SinistreDeclare
        
        var routingKey = $"sinistre.declare.v1";
        
        _channel.BasicPublish(
            exchange: "cap.bsp.dsp.events",
            routingKey: routingKey,
            basicProperties: properties,
            body: body
        );
        
        await _channel.WaitForConfirmsOrDieAsync(ct); // Publisher confirms
    }
}
```

**Alternatives Considered**:
- Azure Service Bus: Rejected - higher cost, RabbitMQ is sufficient for MVP
- Apache Kafka: Constitution mentions Kafka preference, but RabbitMQ simpler for single capability MVP
- Azure Event Grid: Rejected - less control over routing, no DLQ

---

### 6. Docker Compose: Local Development Stack

**Decision**: Provide complete docker-compose.yml with EventStoreDB, MongoDB, RabbitMQ, and optional observability stack

**File Structure**:
```yaml
# docker-compose.yml
services:
  eventstoredb:
    image: eventstore/eventstore:23.10.0-bookworm-slim
    environment:
      EVENTSTORE_CLUSTER_SIZE: 1
      EVENTSTORE_RUN_PROJECTIONS: All
      EVENTSTORE_START_STANDARD_PROJECTIONS: true
      EVENTSTORE_INSECURE: true  # Dev only
    ports:
      - "2113:2113"  # HTTP API
      - "1113:1113"  # TCP (for subscriptions)
    volumes:
      - eventstoredb-data:/var/lib/eventstore
  
  mongodb:
    image: mongo:7.0
    environment:
      MONGO_INITDB_ROOT_USERNAME: admin
      MONGO_INITDB_ROOT_PASSWORD: password
      MONGO_INITDB_DATABASE: cap_bsp_dsp
    ports:
      - "27017:27017"
    volumes:
      - mongodb-data:/data/db
      - ./init-scripts/mongo-init.js:/docker-entrypoint-initdb.d/mongo-init.js:ro
  
  rabbitmq:
    image: rabbitmq:3.13-management
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
    ports:
      - "5672:5672"   # AMQP
      - "15672:15672" # Management UI
    volumes:
      - rabbitmq-data:/var/lib/rabbitmq
  
  # Optional: Observability stack
  jaeger:
    image: jaegertracing/all-in-one:1.54
    ports:
      - "16686:16686"  # UI
      - "4317:4317"    # OTLP gRPC
  
  prometheus:
    image: prom/prometheus:v2.49.0
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml:ro
    ports:
      - "9090:9090"

volumes:
  eventstoredb-data:
  mongodb-data:
  rabbitmq-data:
```

**Seed Script** (`mongo-init.js`):
```javascript
db = db.getSiblingDB('cap_bsp_dsp');

// Seed typeSinistre reference data
db.typeSinistreReference.insertMany([
  { 
    _id: 'accident-corporel', 
    code: 'ACCIDENT_CORPOREL', 
    libelle: 'Accident corporel',
    categorie: 'SANTE',
    actif: true,
    dateCreation: new Date(),
    dateModification: new Date()
  },
  { 
    _id: 'degats-materiels', 
    code: 'DEGATS_MATERIELS', 
    libelle: 'Dégâts matériels',
    categorie: 'IARD',
    actif: true,
    dateCreation: new Date(),
    dateModification: new Date()
  },
  { 
    _id: 'responsabilite-civile', 
    code: 'RESPONSABILITE_CIVILE', 
    libelle: 'Responsabilité civile',
    categorie: 'RC',
    actif: true,
    dateCreation: new Date(),
    dateModification: new Date()
  }
]);

// Create indexes
db.declarationReadModel.createIndex({ identifiantSinistre: 1 }, { unique: true });
db.declarationReadModel.createIndex({ identifiantContrat: 1, dateDeclaration: -1 });
db.typeSinistreReference.createIndex({ code: 1 }, { unique: true });
db.typeSinistreReference.createIndex({ actif: 1 });
```

---

## Summary of Decisions

| Research Item | Decision | Rationale |
|---------------|----------|-----------|
| **Event Sourcing** | EventStoreDB 23.x with .NET gRPC client | Purpose-built, mature, official .NET support |
| **ID Generation** | MongoDB sequence with `SIN-{YEAR}-{SEQUENCE}` | Human-friendly, sequential, atomic increment |
| **Reference Data** | MongoDB with in-memory cache (60min TTL) | Flexible schema, fast validation, business-manageable |
| **Cold Start** | Azure Functions Premium Plan | Pre-warmed instances meet <500ms p95 SLA |
| **Event Bus** | RabbitMQ with topic exchange | Flexible routing, persistent messages, DLQ support |
| **Local Dev** | Docker Compose with all infrastructure | Full-stack testability, no cloud dependencies for dev |

## Resolved NEEDS CLARIFICATION

✅ **Organizational Context** (.NET vs Python/Kotlin):
- Assumption: Organization has .NET expertise and Azure commitment
- **ACTION REQUIRED**: Create ADR-DSP-001 documenting technology stack decision
- If Python is mandatory, pivot to FastAPI + EventStoreDB Python client + Pydantic

✅ **All Technical Context items** are now concrete (no remaining NEEDS CLARIFICATION)

## Next Steps (Phase 1)

With research complete, proceed to Phase 1 design:
1. Generate `data-model.md` (aggregates, value objects, events)
2. Generate `contracts/` (JSON Schema for event, OpenAPI for HTTP API)
3. Generate `quickstart.md` (setup instructions, docker-compose usage)
4. Update agent context with .NET stack

---

**Research Complete**: 2026-01-27  
**Ready for Phase 1**: ✅ YES
