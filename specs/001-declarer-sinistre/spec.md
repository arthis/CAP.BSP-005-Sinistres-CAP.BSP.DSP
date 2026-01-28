# Feature Specification: SinistreDéclaré Event & DeclarerSinistre Command

**Feature Branch**: `001-declarer-sinistre`  
**Created**: 2026-01-27  
**Status**: Draft  
**Input**: Implement SinistreDéclaré event and DeclarerSinistre command for claims declaration with automatic field generation, database validation, and business rule enforcement

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Submit Valid Claim Declaration (Priority: P1)

An insured submits a claim declaration through any channel (web, mobile, phone, email) with valid contract reference and occurrence date, and receives immediate acknowledgment that the claim has been recorded.

**Why this priority**: This is the core MVP functionality - the absolute minimum needed for the capability to provide value. Without this, no claims can be entered into the system. This represents the "happy path" that 95% of declarations follow.

**Independent Test**: Can be fully tested by submitting a claim with valid `identifiantContrat` and `dateSurvenance` (in the past), then verifying that a `SinistreDéclaré` event is emitted with auto-generated `identifiantSinistre` and system-set `dateDeclaration`, and the claim is retrievable via query.

**Acceptance Scenarios**:

1. **Given** an active insurance contract (identifiantContrat: "CONTRAT-2026-001"), **When** an insured declares a claim with typeSinistre "ACCIDENT_CORPOREL" and dateSurvenance "2026-01-20", **Then** the system generates a unique identifiantSinistre (e.g., "SIN-2026-000001"), sets dateDeclaration to current timestamp, validates typeSinistre against reference database, and emits a SinistreDéclaré event.

2. **Given** a claim declaration is in progress, **When** the system processes the DeclarerSinistre command, **Then** identifiantSinistre is automatically generated (not settable by caller), dateDeclaration is set to now (not settable by caller), and both are included in the emitted event.

3. **Given** a valid DeclarerSinistre command, **When** the system emits the SinistreDéclaré event, **Then** the event conforms to the BCM schema (identifiantSinistre, identifiantContrat, typeSinistre, dateSurvenance, dateDeclaration, statut) and is published to the event backbone (Kafka/Event Hubs).

---

### User Story 2 - Reject Invalid Claim Type (Priority: P2)

The system prevents claim submission when the typeSinistre value provided doesn't exist in the reference database, providing clear error feedback to the user.

**Why this priority**: Data quality is critical for downstream processing (fraud detection, indemnification). Invalid claim types would break routing rules and SLA tracking. This is essential but can be deferred after P1 if needed for MVP.

**Independent Test**: Can be tested by attempting to submit a claim with an invalid typeSinistre value (e.g., "INVALID_TYPE") and verifying that the command is rejected with a clear validation error before any event is emitted.

**Acceptance Scenarios**:

1. **Given** a claim declaration with typeSinistre "UNKNOWN_TYPE", **When** the DeclarerSinistre command is validated, **Then** the system checks the reference database, finds no match, and rejects the command with error "TypeSinistre invalide: 'UNKNOWN_TYPE' n'existe pas dans le référentiel".

2. **Given** an invalid typeSinistre, **When** the command fails validation, **Then** NO SinistreDéclaré event is emitted and NO identifiantSinistre is generated.

3. **Given** the reference database contains types ["ACCIDENT_CORPOREL", "DEGATS_MATERIELS", "RESPONSABILITE_CIVILE"], **When** a claim is submitted with typeSinistre "ACCIDENT_CORPOREL", **Then** validation passes and the event is emitted.

---

### User Story 3 - Reject Future Occurrence Dates (Priority: P2)

The system prevents claim submission when the dateSurvenance (occurrence date) is in the future, as claims can only be declared for events that have already occurred.

**Why this priority**: Business rule enforcement - you cannot claim an accident that hasn't happened yet. This prevents fraud and data quality issues. Essential for production but can be added after P1 for initial testing.

**Independent Test**: Can be tested by submitting a claim with dateSurvenance set to tomorrow's date and verifying rejection with a clear error message.

**Acceptance Scenarios**:

1. **Given** today's date is "2026-01-27", **When** a claim is submitted with dateSurvenance "2026-01-28" (tomorrow), **Then** the command is rejected with error "DateSurvenance invalide: la date de survenance ne peut pas être dans le futur".

2. **Given** today's date is "2026-01-27", **When** a claim is submitted with dateSurvenance "2026-01-27" (today), **Then** validation passes (same-day occurrence is allowed).

3. **Given** today's date is "2026-01-27", **When** a claim is submitted with dateSurvenance "2026-01-20" (past), **Then** validation passes.

---

### User Story 4 - Require Contract Reference (Priority: P1)

The system enforces that every claim declaration MUST include a valid identifiantContrat, as claims cannot exist without linkage to an insurance contract.

**Why this priority**: Fundamental business invariant - a claim without a contract has no guarantee coverage, no premium payer, and cannot be processed. This is non-negotiable for P1.

**Independent Test**: Can be tested by attempting to submit a DeclarerSinistre command without identifiantContrat and verifying rejection.

**Acceptance Scenarios**:

1. **Given** a DeclarerSinistre command with missing identifiantContrat, **When** the command is validated, **Then** it is rejected with error "IdentifiantContrat obligatoire: toute déclaration doit être rattachée à un contrat".

2. **Given** a DeclarerSinistre command with identifiantContrat "", **When** the command is validated, **Then** it is rejected (empty string not allowed).

3. **Given** a DeclarerSinistre command with identifiantContrat "CONTRAT-2026-001", **When** the command is processed, **Then** the identifiantContrat is included in the SinistreDéclaré event.

---

### Edge Cases

- **What happens when typeSinistre validation database is temporarily unavailable?**  
  System should fail gracefully with a 503 Service Unavailable error (circuit breaker pattern), NOT accept the claim with unvalidated data. Command retry can be attempted after service recovery.

- **What happens when two claims are submitted simultaneously for the same contract?**  
  Both should succeed independently with different identifiantSinistre values. The system must handle concurrent submissions without ID collisions (use database sequence or UUID generation).

- **What happens when dateDeclaration or identifiantSinistre are explicitly provided in the command?**  
  System MUST ignore these values and overwrite them with system-generated values. These fields are not settable from outside to prevent tampering.

- **What happens if dateSurvenance is far in the past (e.g., 10 years ago)?**  
  Validation should pass (past dates are allowed), but downstream processing may flag this for review based on contract prescription periods. That's outside the scope of this capability.

- **What happens if the same claim is submitted twice (duplicate detection)?**  
  This specification covers command validation and event emission. Duplicate detection is a separate concern, likely handled by CAP.BSP.OED (Case Opening) which receives the event. For idempotency at the event level, use event ID as idempotency key.

## Requirements *(mandatory)*

### Functional Requirements

**Command: DeclarerSinistre**

- **FR-001**: System MUST accept a DeclarerSinistre command with mandatory field `identifiantContrat` (string, non-empty)
- **FR-002**: System MUST accept a DeclarerSinistre command with mandatory field `typeSinistre` (string, must exist in reference database)
- **FR-003**: System MUST accept a DeclarerSinistre command with mandatory field `dateSurvenance` (date, must be in the past or today)
- **FR-004**: System MUST automatically generate `identifiantSinistre` (unique identifier) when processing the command - this field is NOT settable by the caller
- **FR-005**: System MUST automatically set `dateDeclaration` to the current server timestamp when processing the command - this field is NOT settable by the caller
- **FR-006**: System MUST validate `typeSinistre` against a reference database of valid claim types before accepting the command
- **FR-007**: System MUST reject commands where `dateSurvenance` is in the future with validation error
- **FR-008**: System MUST reject commands where `identifiantContrat` is missing or empty with validation error
- **FR-009**: System MUST reject commands where `typeSinistre` does not exist in the reference database with validation error
- **FR-010**: System MUST ignore any values provided for `identifiantSinistre` or `dateDeclaration` in the command payload and overwrite them with system-generated values

**Event: SinistreDéclaré**

- **FR-011**: System MUST emit a `SinistreDéclaré` event upon successful command processing
- **FR-012**: SinistreDéclaré event MUST conform to BCM schema from `events-BSP-005-Sinistres & Prestations.yaml`
- **FR-013**: SinistreDéclaré event MUST include: `identifiantSinistre`, `identifiantContrat`, `typeSinistre`, `dateSurvenance`, `dateDeclaration`, `statut`
- **FR-014**: SinistreDéclaré event MUST be published to the event backbone (Kafka/Azure Event Hubs) as part of the transactional boundary (outbox pattern)
- **FR-015**: SinistreDéclaré event MUST be immutable after publication (no updates allowed)
- **FR-016**: SinistreDéclaré event MUST include a unique event ID for idempotency tracking by consumers

**Validation & Business Rules**

- **FR-017**: System MUST validate all command inputs before persisting any state or emitting events (fail-fast principle)
- **FR-018**: System MUST use circuit breaker pattern when accessing typeSinistre reference database to handle temporary unavailability
- **FR-019**: System MUST generate unique `identifiantSinistre` values with no collisions even under concurrent load (use database sequence or UUID v4)
- **FR-020**: System MUST set initial `statut` field in the event to "DECLARE" (declared state)

**Audit & Observability**

- **FR-021**: System MUST log all DeclarerSinistre command attempts (success and failure) with structured logging (JSON format)
- **FR-022**: System MUST include correlation ID (trace context) in all logs and events for distributed tracing
- **FR-023**: System MUST track metrics: command processing latency (p50, p95, p99), command success/failure rate, event publish rate

### Key Entities *(mandatory - feature involves data)*

- **DéclarationSinistre** (Aggregate Root): Represents a claim declaration  
  Key attributes (implementation-agnostic):
  - identifiantSinistre (unique identifier, system-generated)
  - identifiantContrat (reference to insurance contract)
  - typeSinistre (claim type classification)
  - dateSurvenance (date the covered event occurred)
  - dateDeclaration (timestamp when claim was declared, system-set)
  - statut (declaration lifecycle status: DECLARE, VALIDE, ANNULE)

- **TypeSinistre** (Value Object): Represents a valid claim type from reference database  
  Key attributes:
  - code (e.g., "ACCIDENT_CORPOREL", "DEGATS_MATERIELS")
  - libelle (human-readable label)
  - categorie (grouping for business rules)

- **IdentifiantSinistre** (Value Object): Unique immutable identifier for a claim  
  Format: "SIN-{YEAR}-{SEQUENCE}" or UUID v4

- **DateSurvenance** (Value Object): Date of occurrence with validation rules  
  Rules: must be <= today, must be valid date

- **Command: DeclarerSinistre**: Input structure for claim declaration  
  Fields:
  - identifiantContrat: string (required)
  - typeSinistre: string (required, validated against reference DB)
  - dateSurvenance: date (required, must be <= today)
  Note: identifiantSinistre and dateDeclaration are NOT part of command input

- **Event: SinistreDéclaré**: Output event published to event backbone  
  Fields: as per BCM schema (identifiantSinistre, identifiantContrat, typeSinistre, dateSurvenance, dateDeclaration, statut)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Command processing completes in under 500ms at p95 (95th percentile latency)
- **SC-002**: System correctly validates 100% of invalid typeSinistre values against reference database (no false negatives)
- **SC-003**: System correctly rejects 100% of future dateSurvenance values (no false positives allowing future dates)
- **SC-004**: System generates unique identifiantSinistre for 10,000 concurrent claim submissions with zero collisions
- **SC-005**: Event publication succeeds with 99.9% reliability (transactional outbox ensures eventual delivery even if initial publish fails)
- **SC-006**: All emitted SinistreDéclaré events conform to BCM schema (100% schema validation pass rate)
- **SC-007**: System handles reference database unavailability gracefully with circuit breaker (fails commands with 503, not corrupted data)
- **SC-008**: Audit trail captures 100% of command attempts with correlation ID for tracing

## Assumptions *(optional - document known assumptions)*

- **A-001**: Reference database of typeSinistre values exists and is accessible via query interface (repository or service)
- **A-002**: Event backbone (Kafka/Azure Event Hubs) is operational and configured with appropriate topics for SinistreDéclaré events
- **A-003**: Transactional outbox infrastructure exists for reliable event publishing
- **A-004**: Contract validation (checking if identifiantContrat references an active contract) is NOT part of this capability - that's handled by anti-corruption layer or downstream processing
- **A-005**: User authentication and authorization are handled by API gateway/middleware - command handler receives authenticated user context
- **A-006**: Identifiant generation uses database sequence for ordered IDs OR UUID v4 for distributed generation (to be determined in planning phase)
- **A-007**: System clock is synchronized (NTP) for accurate dateDeclaration timestamps across distributed nodes

## Scope Boundaries *(optional - clarify what's OUT of scope)*

**In Scope for this Feature:**
- DeclarerSinistre command handling and validation
- SinistreDéclaré event emission conforming to BCM schema
- TypeSinistre validation against reference database
- Date validation (dateSurvenance <= today)
- Automatic field generation (identifiantSinistre, dateDeclaration)
- Event publication to event backbone with transactional guarantees

**Out of Scope (handled elsewhere):**
- Contract existence validation (assume identifiantContrat references a valid contract - ACL handles translation)
- Guarantee/coverage validation (checking if typeSinistre is covered by the contract - handled by CAP.BSP.IND Investigation)
- Duplicate claim detection (handled by CAP.BSP.OED Case Opening)
- User notification/acknowledgment email (handled by CAP.BSP.006 Client Interaction)
- Fraud scoring/detection (triggered by event consumption in CAP.BSP.DLC Fraud Detection)
- Read model updates / query projections (separate from command processing, built from events)
- Case/dossier creation (handled by CAP.BSP.OED which subscribes to SinistreDéclaré)
