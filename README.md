# CAP.BSP.DSP.000 — Déclaration du sinistre / de la prestation

**Business Capability**: Claims Declaration  
**L1 Parent**: CAP.BSP.005 (Sinistres & Prestations)  
**Zoning**: BUSINESS SERVICE PRODUCTION  
**Owner**: Gestion Sinistres / Prestations

## Business Context

### Purpose

This service implements the **Claims Declaration** capability (CAP.BSP.DSP.000), responsible for capturing and qualifying guaranteed events (multi-channel) and linking them to insurance contracts. This is the entry point of the claims management lifecycle for the mutual insurance organization.

### Position in Value Chain

```
┌─────────────────────────────────────────────────────────────────────┐
│ CAP.BSP.005 — Sinistres & Prestations (Claims & Benefits Management)│
└─────────────────────────────────────────────────────────────────────┘
          │
          ├─► CAP.BSP.DSP.000 ✓ YOU ARE HERE (Claims Declaration)
          ├─► CAP.BSP.OED.000 (Case Opening & Registration)
          ├─► CAP.BSP.IND.000 (Case Investigation)
          ├─► CAP.BSP.EDP.000 (Damage Assessment)
          ├─► CAP.BSP.INR.000 (Indemnification & Settlement)
          ├─► CAP.BSP.PTP.000 (Third-Party & Provider Management)
          ├─► CAP.BSP.RES.000 (Recovery & Subrogation)
          ├─► CAP.BSP.DLC.000 (Fraud Detection & Control)
          ├─► CAP.BSP.CAD.000 (Case Closure & Archiving)
          └─► CAP.BSP.PRS.000 (Claims Monitoring & Reporting)
```

### Key Responsibilities

1. **Multi-Channel Declaration Capture**
   - Web portal, mobile app, phone, email, agent interface
   - Initial event capture with minimal validation
   - Real-time acknowledgment to declarant

2. **Event Qualification**
   - Link declaration to active insurance contract (CAP.BSP.004)
   - Validate event type against contract guarantees
   - Classify claim type (DOMMAGE, MÉDICAL, RESPONSABILITÉ)
   - Apply initial business rules (eligibility, exclusions)

3. **Event Publication**
   - Emit `SinistreDéclaré` domain event (canonical event for downstream processing)
   - Trigger downstream workflows (case opening, fraud detection)
   - Ensure reliable event delivery (transactional outbox pattern)

4. **Compliance**
   - Legal acknowledgment within 5 business days (Code des Assurances)
   - RGPD-compliant data handling (encryption, data residency)
   - Complete audit trail for regulatory review

## Architecture Principles

This service strictly adheres to the project [Constitution](.specify/memory/constitution.md) which mandates:

1. **Domain-Driven Design (DDD)** — Ubiquitous language, bounded contexts, aggregates
2. **Event-Driven Architecture (EDA)** — Event sourcing, event contracts, idempotency
3. **CQRS Pattern** — Command/query separation, read models, eventual consistency
4. **Immutability** — Immutable domain objects, functional core
5. **Test-First Development** — TDD mandatory, 80% domain coverage
6. **Clean Architecture** — Hexagonal architecture, dependency inversion
7. **Observability & Reliability** — 99.9% uptime, structured logging, distributed tracing

### Architecture Diagram

```
┌──────────────────────────────────────────────────────────────────────┐
│                        PRESENTATION LAYER                            │
│  ┌────────────┐  ┌────────────┐  ┌──────────────────────────────┐  │
│  │ REST API   │  │ Event      │  │ CLI (admin/debugging)        │  │
│  │ /api/v1/.. │  │ Handlers   │  │                              │  │
│  └────────────┘  └────────────┘  └──────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────────┘
                               ↓
┌──────────────────────────────────────────────────────────────────────┐
│                       APPLICATION LAYER                              │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │ Command Handlers                                            │   │
│  │  • DéclarerSinistre    • RattacherContrat                   │   │
│  │  • QualifierDéclaration • AnnulerDéclaration                │   │
│  ├─────────────────────────────────────────────────────────────┤   │
│  │ Query Handlers                                              │   │
│  │  • RechercherDéclarations • ObtenirStatutDéclaration        │   │
│  ├─────────────────────────────────────────────────────────────┤   │
│  │ Event Handlers (subscriptions)                              │   │
│  │  • ContratÉmis  • DemandeClientQualifiée                    │   │
│  │  • ProduitAssurancePublié                                   │   │
│  └─────────────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────────────┘
                               ↓
┌──────────────────────────────────────────────────────────────────────┐
│                         DOMAIN LAYER                                 │
│  ┌────────────────────────────────────────────────────────────┐    │
│  │ Aggregates                                                 │    │
│  │  • DéclarationSinistre (root)                              │    │
│  │  • RattachementContrat                                     │    │
│  ├────────────────────────────────────────────────────────────┤    │
│  │ Value Objects                                              │    │
│  │  • IdentifiantSinistre  • TypeSinistre                     │    │
│  │  • DateSurvenance       • DateDéclaration                  │    │
│  │  • StatutDéclaration    • CanalDéclaration                 │    │
│  ├────────────────────────────────────────────────────────────┤    │
│  │ Domain Events                                              │    │
│  │  • SinistreDéclaré      • DéclarationValidée               │    │
│  │  • ContratRattaché      • DéclarationAnnulée               │    │
│  ├────────────────────────────────────────────────────────────┤    │
│  │ Domain Services                                            │    │
│  │  • ValidationGaranties  • ClassificationSinistre           │    │
│  ├────────────────────────────────────────────────────────────┤    │
│  │ Ports (interfaces)                                         │    │
│  │  • IDéclarationRepository  • IEventPublisher               │    │
│  │  • IContratService (anti-corruption layer)                 │    │
│  └────────────────────────────────────────────────────────────┘    │
└──────────────────────────────────────────────────────────────────────┘
                               ↓
┌──────────────────────────────────────────────────────────────────────┐
│                     INFRASTRUCTURE LAYER                             │
│  ┌────────────────────────────────────────────────────────────┐    │
│  │ Adapters (port implementations)                            │    │
│  │  • PostgresEventStore      • PostgresReadModelRepository   │    │
│  │  • KafkaEventPublisher     • RedisCache                    │    │
│  │  • ContratServiceAdapter (gRPC client to CAP.BSP.004)      │    │
│  ├────────────────────────────────────────────────────────────┤    │
│  │ Cross-Cutting Concerns                                     │    │
│  │  • StructuredLogger        • DistributedTracer             │    │
│  │  • PrometheusMetrics       • HealthChecks                  │    │
│  │  • SecretManager           • ConfigurationProvider         │    │
│  └────────────────────────────────────────────────────────────┘    │
└──────────────────────────────────────────────────────────────────────┘
```

## Event Contracts

### Published Events (Producer)

This service emits the following domain event to the event backbone:

- **`SinistreDéclaré`** (Claims Declared)
  - Emitted when: A claim declaration has been captured and validated
  - Schema: As defined in [events-BSP-005-Sinistres & Prestations.yaml](../../../map/bcm/events-BSP-005-Sinistres%20&%20Prestations.yaml)
  - Consumers: CAP.BSP.OED.000 (Case Opening), CAP.BSP.DLC.000 (Fraud Detection)

### Subscribed Events (Consumer)

This service subscribes to upstream events:

- **`ContratÉmis`** (from CAP.BSP.004)
  - Purpose: Maintain read model of active contracts for declaration linkage
  - Idempotency: By `identifiantContrat` + event ID

- **`DemandeClientQualifiée`** (from CAP.BSP.006)
  - Purpose: Pre-populate declaration from customer service request
  - Idempotency: By `identifiantDemande` + event ID

- **`ProduitAssurancePublié`** (from CAP.BSP.001)
  - Purpose: Update guarantee rules for claim eligibility validation
  - Idempotency: By `identifiantProduit` + `version`

## Technology Stack

### Core Technologies
- **Language**: Python 3.12+ (type hints, mypy strict mode)
- **Framework**: FastAPI (REST API), Pydantic (validation)
- **Async**: asyncio, aiohttp

### Data & Persistence
- **Event Store**: PostgreSQL 16+ with JSONB (event sourcing table)
- **Read Models**: PostgreSQL (relational views) + Redis (caching)
- **Schema Migrations**: Alembic

### Messaging & Events
- **Event Backbone**: Apache Kafka (or Azure Event Hubs)
- **Schema Registry**: Confluent Schema Registry
- **Event Serialization**: JSON with JSON Schema validation

### Observability
- **Logging**: structlog (structured JSON logs)
- **Tracing**: OpenTelemetry (W3C Trace Context)
- **Metrics**: Prometheus/OpenTelemetry
- **APM**: Jaeger or Application Insights

### Testing
- **Unit/Integration**: pytest, pytest-asyncio
- **Contract Tests**: JSON Schema validation
- **Property-Based**: Hypothesis
- **Coverage**: pytest-cov (80% domain layer minimum)

### CI/CD
- **CI**: GitHub Actions / Azure DevOps
- **Deployment**: Kubernetes (Helm charts)
- **Secret Management**: Azure Key Vault
- **Feature Flags**: LaunchDarkly or custom

## Project Structure

```
CAP.BSP.DSP/
├── README.md                           # This file
├── .specify/
│   ├── memory/
│   │   └── constitution.md             # Project governance (CRITICAL)
│   └── templates/                       # Specification templates
├── src/
│   ├── domain/                          # Domain layer (NO external dependencies)
│   │   ├── aggregates/
│   │   │   ├── declaration_sinistre.py
│   │   │   └── rattachement_contrat.py
│   │   ├── value_objects/
│   │   │   ├── identifiant_sinistre.py
│   │   │   ├── type_sinistre.py
│   │   │   ├── date_survenance.py
│   │   │   └── statut_declaration.py
│   │   ├── events/
│   │   │   ├── sinistre_declare.py
│   │   │   ├── declaration_validee.py
│   │   │   └── contrat_rattache.py
│   │   ├── services/
│   │   │   ├── validation_garanties.py
│   │   │   └── classification_sinistre.py
│   │   └── ports/
│   │       ├── i_declaration_repository.py
│   │       ├── i_event_publisher.py
│   │       └── i_contrat_service.py
│   ├── application/                     # Application layer (use cases)
│   │   ├── commands/
│   │   │   ├── declarer_sinistre.py
│   │   │   ├── rattacher_contrat.py
│   │   │   └── qualifier_declaration.py
│   │   ├── queries/
│   │   │   ├── rechercher_declarations.py
│   │   │   └── obtenir_statut_declaration.py
│   │   └── event_handlers/
│   │       ├── contrat_emis_handler.py
│   │       ├── demande_client_qualifiee_handler.py
│   │       └── produit_assurance_publie_handler.py
│   ├── infrastructure/                  # Infrastructure layer (adapters)
│   │   ├── persistence/
│   │   │   ├── postgres_event_store.py
│   │   │   ├── postgres_read_model_repository.py
│   │   │   └── migrations/              # Alembic migrations
│   │   ├── messaging/
│   │   │   ├── kafka_event_publisher.py
│   │   │   ├── kafka_event_subscriber.py
│   │   │   └── outbox_processor.py
│   │   ├── caching/
│   │   │   └── redis_cache.py
│   │   ├── external/
│   │   │   └── contrat_service_adapter.py
│   │   └── observability/
│   │       ├── structured_logger.py
│   │       ├── distributed_tracer.py
│   │       └── prometheus_metrics.py
│   ├── presentation/                    # Presentation layer (API, CLI)
│   │   ├── api/
│   │   │   ├── v1/
│   │   │   │   ├── declarations.py
│   │   │   │   └── health.py
│   │   │   └── middleware/
│   │   │       ├── tracing.py
│   │   │       ├── logging.py
│   │   │       └── error_handling.py
│   │   ├── cli/
│   │   │   └── admin_commands.py
│   │   └── event_listeners/
│   │       └── kafka_listener.py
│   └── main.py                          # Application entry point
├── tests/
│   ├── unit/                            # Unit tests (domain + application)
│   │   ├── domain/
│   │   │   ├── test_declaration_sinistre.py
│   │   │   └── test_validation_garanties.py
│   │   └── application/
│   │       └── test_declarer_sinistre_command.py
│   ├── integration/                     # Integration tests
│   │   ├── test_event_publishing.py
│   │   ├── test_event_handling_idempotency.py
│   │   └── test_postgres_event_store.py
│   ├── contract/                        # Contract tests (event schemas)
│   │   ├── test_sinistre_declare_schema.py
│   │   └── test_contrat_emis_subscription.py
│   └── e2e/                             # End-to-end tests
│       └── test_declaration_workflow.py
├── specs/                               # Feature specifications (generated)
│   └── [###-feature]/
│       ├── spec.md
│       ├── plan.md
│       ├── tasks.md
│       └── contracts/
├── docker/
│   ├── Dockerfile
│   └── docker-compose.yml               # Local development stack
├── k8s/                                 # Kubernetes manifests
│   ├── deployment.yaml
│   ├── service.yaml
│   └── configmap.yaml
├── .github/
│   └── workflows/
│       ├── ci.yml                       # Continuous Integration
│       └── cd.yml                       # Continuous Deployment
├── pyproject.toml                       # Python dependencies (Poetry)
├── mypy.ini                             # Static type checking config
├── pytest.ini                           # Test configuration
└── .env.example                         # Example environment variables
```

## Getting Started

### Prerequisites

- Python 3.12+
- Docker & Docker Compose (for local dependencies)
- Poetry (dependency management)
- Access to event backbone (Kafka/Azure Event Hubs)

### Local Development Setup

#### Prerequisites

- .NET 8.0 SDK or later
- Azure Functions Core Tools v4
- Docker & Docker Compose
- (Optional) MongoDB Compass for database inspection
- (Optional) Postman/Insomnia for API testing

#### Quick Start (Automated)

```bash
# Clone repository
git clone <repo-url>
cd CAP.BSP.DSP

# Run quickstart script (starts infrastructure + Functions)
./quickstart.sh
```

The script will:
1. Start all infrastructure services (EventStoreDB, MongoDB, RabbitMQ, Jaeger, Prometheus)
2. Wait for services to be healthy
3. Start Azure Functions on http://localhost:7071

#### Manual Setup

```bash
# 1. Start infrastructure services
cd docker
docker-compose up -d

# Wait for services to be healthy (30-60 seconds)
docker-compose ps

# 2. Build the solution
cd ..
dotnet build

# 3. Start Azure Functions
cd src/CAP.BSP.DSP.Functions
func start
```

**Infrastructure Services:**
- **EventStoreDB UI**: http://localhost:2113 (event store, no auth)
- **MongoDB**: localhost:27017 (read models, admin/admin123)
- **RabbitMQ Management**: http://localhost:15672 (message broker, guest/guest)
- **Jaeger UI**: http://localhost:16686 (distributed tracing)
- **Prometheus**: http://localhost:9090 (metrics)

**Azure Functions API**: http://localhost:7071

### Running Tests

```bash
# Build solution
dotnet build

# Run all tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverageReportsFormat=opencover

# Run specific test project
dotnet test tests/CAP.BSP.DSP.Domain.Tests
dotnet test tests/CAP.BSP.DSP.Application.Tests
dotnet test tests/CAP.BSP.DSP.Integration.Tests

# Run tests in watch mode (auto-rerun on file changes)
dotnet watch test --project tests/CAP.BSP.DSP.Domain.Tests
```

### API Endpoints

**Commands (Write Side):**

```bash
# POST /api/v1/declarations - Submit new claim declaration
curl -X POST http://localhost:7071/api/v1/declarations \
  -H "Content-Type: application/json" \
  -H "X-User-ID: user@example.com" \
  -H "X-Correlation-ID: $(uuidgen)" \
  -d '{
    "identifiantContrat": "CTR-2026-001234",
    "dateSurvenance": "2026-01-20T10:30:00Z"
  }'

# Response: 201 Created
# {
#   "identifiantSinistre": "SIN-2026-000001",
#   "dateDeclaration": "2026-01-28T14:30:00Z"
# }
```

**Queries (Read Side):**

```bash
# GET /api/v1/declarations - Search declarations with filters
curl "http://localhost:7071/api/v1/declarations?identifiantContrat=CTR-2026-001234&limit=10&offset=0"

# GET /api/v1/declarations/{id} - Get claim details by ID
curl "http://localhost:7071/api/v1/declarations/SIN-2026-000001"
```

### Debugging & Monitoring

- **View Event Streams**: Open http://localhost:2113 → Stream Browser → Search for `declaration-sinistre-`
- **View Messages**: Open http://localhost:15672 → Exchanges → `bsp.events` → Bindings
- **View Traces**: Open http://localhost:16686 → Search for service `cap-bsp-dsp-functions`
- **View Read Models**: Use MongoDB Compass to connect to `mongodb://admin:admin123@localhost:27017` and inspect `cap_bsp_dsp.declarations` collection

### Stopping Services

```bash
# Stop Azure Functions: Ctrl+C in terminal

# Stop infrastructure
cd docker
docker-compose down

# Stop and remove all data (clean slate)
docker-compose down -v
```

## Compliance & Regulations

### RGPD (General Data Protection Regulation)

- **Data Encryption**: Personal data (nom assuré, email, téléphone) encrypted at rest (AES-256)
- **Data Residency**: All personal data stored in EU datacenters
- **Log Redaction**: Personal data redacted in application logs
- **Retention**: Declaration data retained for 10 years (legal requirement)

### Code des Assurances

- **Acknowledgment Deadline**: Declaration acknowledgment sent within 5 business days
- **Audit Trail**: Complete audit trail for every declaration (who, when, what)
- **Data Archiving**: Probative archiving (archivage probatoire) for regulatory compliance

### Solvabilité II

- **Risk Metrics**: Operational risk metrics tracked and reported
- **Data Quality**: High data quality standards for actuarial calculations
- **Audit Readiness**: Quarterly compliance audits, spot-checks on 10% of code

## Contributing

### Development Workflow

1. **Read the Constitution** (`.specify/memory/constitution.md`) — MANDATORY for all contributors
2. **Create Feature Branch**: `git checkout -b <###-feature-name>`
3. **Write Tests First**: Follow TDD principles (RED → GREEN → REFACTOR)
4. **Implement Code**: Adhere to architecture layers (domain → application → infrastructure)
5. **Constitution Check**: Verify compliance in PR description
6. **Code Review**: Minimum 2 approvals required for domain changes
7. **Merge**: Squash and merge to main after CI passes

### Pull Request Template

Every PR MUST include:

```markdown
## Constitution Check

- [ ] Domain layer uses ubiquitous language (French terminology)
- [ ] All domain objects are immutable
- [ ] Event contracts match BCM schema definitions
- [ ] Tests written BEFORE implementation (TDD)
- [ ] 80% domain layer coverage achieved
- [ ] Clean architecture layers respected (no infrastructure in domain)
- [ ] Structured logging implemented
- [ ] Event handlers are idempotent

## Complexity Justification

[If deviating from constitution principles, document ADR and justification here]
```

## Support & Contact

- **Product Owner**: Gestion Sinistres / Prestations
- **Tech Lead**: [TBD]
- **Enterprise Architect**: EA / Urbanisation
- **Urbanist**: Business Architecture

## References

- [Project Constitution](.specify/memory/constitution.md) ← **READ THIS FIRST**
- [BCM Capabilities Mapping](../../../map/bcm/mappings/capabilities-sinistres.yaml)
- [Event Contracts](../../../map/bcm/events-BSP-005-Sinistres%20&%20Prestations.yaml)
- [ADR-BSP-005-001: Capability Decomposition](../../../map/adr/functional/BSP/BSP-005/ADR-BSP-005-001-decoupage-10-capacites.md)

---

**Version**: 1.0.0  
**Last Updated**: 2026-01-27  
**Status**: INITIAL — Constitution ratified, implementation pending
