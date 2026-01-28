<!--
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
SYNC IMPACT REPORT — Constitution v1.0.0
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Version Change: Template → 1.0.0 (MAJOR)

Reason: Initial ratification establishing governance framework for CAP.BSP.DSP.000 
        (Claims Declaration capability) as a DDD/CQRS/EDA backend service

Principles Defined:
  1. Domain-Driven Design (DDD) — Ubiquitous Language, Bounded Contexts, Aggregates
  2. Event-Driven Architecture (EDA) — Event Sourcing, Event Contracts, Idempotency
  3. CQRS Pattern — Command/Query Separation, Read Models, Eventual Consistency
  4. Immutability — Immutable Domain Objects, Value Objects, Functional Core
  5. Test-First Development — TDD Mandatory, Contract Tests, Integration Tests
  6. Clean Architecture — Hexagonal Architecture, Dependency Inversion, Ports/Adapters
  7. Observability & Reliability — Structured Logging, Distributed Tracing, SLA Compliance

Sections Added:
  - Core Principles (7 principles)
  - Technical Constraints
  - Quality & Compliance Standards
  - Governance

Templates Status:
  ✅ plan-template.md — Validated (DDD phases added)
  ✅ spec-template.md — Validated (Event contracts, bounded contexts)
  ✅ tasks-template.md — Validated (TDD workflow, event implementation)

Follow-up Actions:
  - README.md to be created documenting business capability CAP.BSP.DSP.000
  - CI/CD pipeline configuration pending
  - Architecture Decision Records (ADRs) to be initialized
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
-->

# CAP.BSP.DSP Constitution
**Business Capability**: Déclaration du sinistre / de la prestation (Claims Declaration)

## Core Principles

### I. Domain-Driven Design (DDD)

**ALL** code MUST adhere to DDD tactical and strategic patterns:

- **Ubiquitous Language**: Every domain concept MUST use exact French terminology from business capability model (Sinistre, DéclarationSinistre, GarantieContrat, etc.). NO translations, NO technical jargon in domain layer.
- **Bounded Contexts**: CAP.BSP.DSP.000 is a discrete bounded context. Integration with other L2 capabilities (OED, IND, EDP, etc.) MUST occur only via published domain events and anti-corruption layers.
- **Aggregates**: Domain aggregates MUST enforce business invariants. Root entities control transactional boundaries. Example: `DéclarationSinistre` aggregate validates `typeSinistre`, `dateDeclaration`, and contract linkage atomically.
- **Value Objects**: All domain primitives MUST be immutable value objects (IdentifiantSinistre, DateSurvenance, TypeSinistre, etc.). NO primitive obsession.
- **Domain Events**: Every state transition MUST produce a domain event (`SinistreDéclaré`, `DéclarationValidée`, etc.) following event contracts defined in BCM.

**Rationale**: DDD ensures alignment between code structure and business domain model (CAP.BSP.005 → CAP.BSP.DSP.000), enabling business experts to validate implementation and reducing translation errors between business requirements and code.

### II. Event-Driven Architecture (EDA)

**ALL** inter-capability communication MUST be event-driven:

- **Event Sourcing**: Domain aggregates MUST persist state changes as immutable event streams. Current state derives from event replay.
- **Event Contracts**: Published events MUST strictly conform to BCM event schemas (`events-BSP-005-Sinistres & Prestations.yaml`). Breaking changes require MAJOR version bump.
- **Event Publishing**: Domain events MUST be published to event backbone (Kafka/EventHub) as part of transactional boundary (outbox pattern or transactional messaging).
- **Event Subscription**: Service MUST subscribe to upstream events (`ContratÉmis`, `DemandeClientQualifiée`, `ProduitAssurancePublié`) and handle them idempotently.
- **Idempotency**: ALL event handlers MUST be idempotent. Use event ID as idempotency key. Duplicate events MUST NOT cause duplicate side effects.
- **Ordering**: Event processing MUST handle out-of-order delivery gracefully. Use causal ordering where business-critical (e.g., `ContratÉmis` before `SinistreDéclaré`).

**Rationale**: EDA decouples CAP.BSP.DSP.000 from upstream/downstream capabilities (CAP.BSP.004 Contrats, CAP.BSP.OED Dossiers), enabling independent deployment, resilience to downstream failures, and temporal decoupling for async workflows.

### III. CQRS Pattern

**Command and Query responsibilities MUST be separated**:

- **Command Side**: Commands (`DéclarerSinistre`, `RattacherContrat`, `QualifierDéclaration`) MUST validate business rules, update aggregates, and emit domain events. NO query logic in command handlers.
- **Query Side**: Queries (`RechercherDéclarations`, `ObtenirStatutDéclaration`) MUST read from optimized read models (projections). NO business logic in query handlers.
- **Read Models**: Projections MUST be built from domain events. Support multiple representations (list view, detail view, search index).
- **Eventual Consistency**: Clients MUST tolerate read-after-write delay (typically <1s). Use version/timestamp for optimistic concurrency if needed.
- **No Shared Models**: Command and query models MUST be separate. NO reuse of domain entities in API responses.

**Rationale**: CQRS optimizes write-heavy declaration workflows (complex validation, event emission) separately from read-heavy pilotage/reporting queries, enabling independent scaling and performance tuning per use case.

### IV. Immutability

**ALL domain objects MUST be immutable**:

- **Immutable Entities**: Once created, entity properties CANNOT be modified. State changes create new versions or emit events.
- **Immutable Value Objects**: Value objects MUST be immutable and equality-comparable by value (IdentifiantSinistre("SIN-2026-001") === IdentifiantSinistre("SIN-2026-001")).
- **Immutable Events**: Domain events MUST be immutable after publication. Event versioning handles schema evolution.
- **Functional Core**: Domain logic MUST be pure functions (input → output, no side effects). Side effects (persistence, messaging) isolated in infrastructure layer.
- **Copy-on-Write**: Modifications MUST create new instances. Use builder/with-methods pattern for complex objects.

**Rationale**: Immutability eliminates entire classes of bugs (race conditions, unintended mutations, temporal coupling), simplifies reasoning about code, enables safe concurrency, and aligns naturally with event sourcing (past events cannot change).

### V. Test-First Development (NON-NEGOTIABLE)

**TDD workflow MUST be followed strictly**:

1. **Write Tests FIRST**: Before ANY production code, write failing tests (unit, contract, integration).
2. **User Approval**: For new features, tests MUST be reviewed and approved by product owner before implementation begins.
3. **Red-Green-Refactor**: Tests MUST fail initially (RED) → Implement minimal code to pass (GREEN) → Refactor while keeping tests green.
4. **Contract Tests**: ALL published events MUST have contract tests verifying schema compliance with BCM definitions.
5. **Integration Tests**: ALL event handlers MUST have integration tests verifying idempotency and error handling.
6. **Coverage Gates**: Minimum 80% line coverage on domain layer, 60% overall. Pull requests failing coverage gates MUST be rejected.
7. **Property-Based Testing**: Complex domain invariants SHOULD use property-based tests (e.g., QuickCheck, Hypothesis) to validate edge cases.

**Rationale**: Test-first ensures requirements are testable, prevents regression, documents intended behavior, and de-risks refactoring. Critical for regulated insurance domain where errors have financial/legal consequences.

### VI. Clean Architecture

**Dependency rules MUST enforce layered architecture**:

```
┌─────────────────────────────────────────────────┐
│  Presentation (API, CLI, Event Handlers)        │ ← Adapters
├─────────────────────────────────────────────────┤
│  Application (Use Cases, Command Handlers)      │ ← Orchestration
├─────────────────────────────────────────────────┤
│  Domain (Aggregates, Value Objects, Events)     │ ← Business Rules
├─────────────────────────────────────────────────┤
│  Infrastructure (DB, Messaging, Logging)        │ ← Technical Concerns
└─────────────────────────────────────────────────┘
     ↓ Dependencies MUST flow inward only ↓
```

- **Hexagonal Architecture**: Domain layer MUST NOT depend on infrastructure. Use ports (interfaces) and adapters (implementations).
- **Dependency Inversion**: High-level domain logic MUST NOT depend on low-level technical details (databases, frameworks). Define abstractions (repositories, event publishers) in domain layer, implement in infrastructure.
- **Plugin Architecture**: Infrastructure components (PostgreSQL, Kafka, Redis) MUST be swappable without domain changes.
- **Framework Independence**: Domain logic MUST NOT depend on frameworks (FastAPI, Spring, etc.). Frameworks are delivery mechanisms, not architecture.

**Rationale**: Clean architecture isolates business logic from technical churn (database migrations, framework upgrades), enables testing domain logic in isolation, and future-proofs against technology changes.

### VII. Observability & Reliability

**Production-grade observability MUST be built-in**:

- **Structured Logging**: ALL logs MUST be structured JSON (timestamp, traceId, spanId, severity, event, context). NO unstructured string logs.
- **Distributed Tracing**: ALL requests/commands MUST propagate trace context (W3C Trace Context). Instrument critical paths (command handling, event publishing, external calls).
- **Metrics**: Expose Prometheus/OpenTelemetry metrics: command latency, event publish rate, error rate, queue depth, business KPIs (déclarations par heure).
- **Health Checks**: Implement liveness (`/health/live`) and readiness (`/health/ready`) endpoints. Readiness MUST verify dependencies (DB, event broker).
- **SLA Compliance**: 99.9% uptime target. p95 latency <500ms for declaration commands, <200ms for queries. Track SLIs/SLOs.
- **Circuit Breakers**: Protect against downstream failures (contract validation service, fraud detection). Fail gracefully with degraded functionality.
- **Audit Trail**: ALL commands MUST log audit trail (who, when, what, correlation ID) for regulatory compliance (RGPD, Solvabilité II).

**Rationale**: Insurance operations require strict SLA compliance, auditability, and operational visibility. Observability is not optional—it's a regulatory and business continuity requirement.

## Technical Constraints

### Language & Runtime
- **Python 3.12+** with type hints (mypy strict mode) OR **Kotlin 1.9+** with coroutines
- **Dependency Injection**: Use DI container (e.g., dependency-injector, Koin) for infrastructure components
- **Async I/O**: Non-blocking I/O for event processing and external calls (asyncio, kotlinx.coroutines)

### Data Storage
- **Event Store**: PostgreSQL with JSONB (event sourcing) OR purpose-built event store (EventStoreDB)
- **Read Models**: PostgreSQL (relational queries) + Redis (caching) OR Elasticsearch (full-text search)
- **Migrations**: Versioned schema migrations (Alembic, Flyway). NEVER manual schema changes.

### Messaging & Events
- **Event Backbone**: Apache Kafka (preferred) OR Azure Event Hubs with AMQP
- **Event Schema**: JSON Schema validation against BCM definitions. Schema registry mandatory (Confluent Schema Registry, Azure Schema Registry)
- **Outbox Pattern**: Transactional outbox for reliable event publishing (no dual-write problem)

### API & Contracts
- **REST API**: OpenAPI 3.1 spec-first design. Generated client SDKs for consumers.
- **Versioning**: URI versioning (`/api/v1/declarations`). Support N-1 version during migration window.
- **Authentication**: OAuth2/OIDC (JWT). Role-based access control (RBAC) for commands.

### Performance & Scale
- **Throughput**: Support 10,000 declarations/day (~7/min average, ~50/min peak)
- **Latency**: p95 <500ms command processing, <200ms query response
- **Concurrency**: Handle 100 concurrent declaration submissions without degradation

## Quality & Compliance Standards

### Code Quality
- **Linting**: Enforce strict linting (pylint, flake8, ktlint). Zero warnings policy.
- **Formatting**: Auto-format on commit (black, ktfmt). Reject non-formatted PRs.
- **Code Review**: ALL changes require peer review. Two approvals for domain logic changes.
- **Static Analysis**: Run static analysis (mypy, detekt) in CI. Block on type errors.

### Security
- **OWASP Top 10**: Address all OWASP vulnerabilities. Automated security scans in CI (bandit, safety).
- **RGPD Compliance**: Personal data (nom assuré, email, téléphone) MUST be encrypted at rest (AES-256) and redacted in logs.
- **Secret Management**: NO hardcoded secrets. Use secret manager (Azure Key Vault, AWS Secrets Manager).
- **Principle of Least Privilege**: Service accounts MUST have minimal permissions (read-only on reference data, write-only on events).

### CI/CD
- **Continuous Integration**: Automated build, test, lint on every commit. Block merge on failure.
- **Deployment Pipeline**: Automated deployment to DEV → UAT → PROD with manual approval gates.
- **Blue-Green Deployment**: Zero-downtime deployments. Rollback capability within 5 minutes.
- **Feature Flags**: Use feature flags for progressive rollout and A/B testing.

### Regulatory Compliance
- **Délai Légal**: Declaration acknowledgment MUST be sent within 5 business days (Code des Assurances).
- **Audit Trail**: Retain audit logs for 10 years (archivage probatoire).
- **Data Residency**: Personal data MUST remain in EU (RGPD Article 44).

## Governance

### Constitution Authority
This constitution supersedes all other development practices, coding guidelines, and tribal knowledge. In case of conflict, this document is the single source of truth.

### Amendment Process
1. Proposed amendments MUST be documented in Architecture Decision Record (ADR)
2. Amendments require approval from: Tech Lead, Product Owner, Enterprise Architect
3. MAJOR amendments (principle removal/change) require 2-week review period
4. MINOR amendments (new principle, expanded guidance) require 1-week review period
5. PATCH amendments (clarifications, examples) require 48-hour review period
6. Approved amendments MUST update constitution version (semver) and propagate to all templates

### Compliance Verification
- **Pull Request Gate**: ALL PRs MUST include "Constitution Check" section in description verifying compliance with relevant principles
- **Quarterly Review**: Constitution compliance MUST be audited quarterly (spot-check 10% of code, review metrics)
- **Complexity Justification**: Deviations from principles MUST be documented in ADR with explicit justification and mitigation plan
- **Training**: New team members MUST complete constitution training within first week

### Versioning Policy
- **MAJOR** (X.0.0): Backward-incompatible governance changes (principle removal, redefined invariants)
- **MINOR** (x.Y.0): New principles added, materially expanded guidance, new mandatory sections
- **PATCH** (x.y.Z): Clarifications, wording fixes, examples, non-semantic refinements

**Version**: 1.0.0 | **Ratified**: 2026-01-27 | **Last Amended**: 2026-01-27
