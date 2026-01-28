# Tasks: SinistreDéclaré Event & DeclarerSinistre Command

**Feature**: 001-declarer-sinistre  
**Input**: Design documents from [specs/001-declarer-sinistre/](.)  
**Prerequisites**: ✅ plan.md, spec.md, research.md, data-model.md, contracts/  
**Tech Stack**: .NET 8 (C# 12), EventStoreDB, MongoDB, RabbitMQ, Azure Functions, Docker  
**Generated**: 2026-01-27

## Format: `- [ ] [TaskID] [P?] [Story?] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3, US4)
- **Note**: Tests are NOT requested in specification, so no test tasks included

## User Story Mapping

From [spec.md](spec.md):

- **US1** (P1): Submit Valid Claim Declaration - Core MVP functionality
- **US2** (P2): Reject Invalid Claim Type - Data quality validation
- **US3** (P2): Reject Future Occurrence Dates - Business rule enforcement
- **US4** (P1): Require Contract Reference - Fundamental business invariant

**MVP Scope**: US1 + US4 (core "happy path" + mandatory contract reference)

---

## Phase 1: Setup (Project Initialization) ✅ COMPLETE

**Purpose**: Create .NET solution structure and configure foundational tooling

- [X] T001 Create .NET solution file CAP.BSP.DSP.sln in repository root
- [X] T002 [P] Create Domain project: src/CAP.BSP.DSP.Domain/CAP.BSP.DSP.Domain.csproj with .NET 8 target framework
- [X] T003 [P] Create Application project: src/CAP.BSP.DSP.Application/CAP.BSP.DSP.Application.csproj
- [X] T004 [P] Create Infrastructure project: src/CAP.BSP.DSP.Infrastructure/CAP.BSP.DSP.Infrastructure.csproj
- [X] T005 [P] Create Azure Functions project: src/CAP.BSP.DSP.Functions/CAP.BSP.DSP.Functions.csproj with Microsoft.Azure.Functions.Worker 1.21.x
- [X] T006 [P] Create Domain.Tests project: tests/CAP.BSP.DSP.Domain.Tests/CAP.BSP.DSP.Domain.Tests.csproj with xUnit 2.6.x
- [X] T007 [P] Create Application.Tests project: tests/CAP.BSP.DSP.Application.Tests/CAP.BSP.DSP.Application.Tests.csproj
- [X] T008 [P] Create Integration.Tests project: tests/CAP.BSP.DSP.Integration.Tests/CAP.BSP.DSP.Integration.Tests.csproj with Testcontainers.NET 3.7.x
- [X] T009 [P] Create Contract.Tests project: tests/CAP.BSP.DSP.Contract.Tests/CAP.BSP.DSP.Contract.Tests.csproj
- [X] T010 [P] Create Architecture.Tests project: tests/CAP.BSP.DSP.Architecture.Tests/CAP.BSP.DSP.Architecture.Tests.csproj with ArchUnitNET 0.10.x
- [X] T011 Configure Directory.Build.props with shared MSBuild properties (C# 12, nullable enable, TreatWarningsAsErrors)
- [X] T012 [P] Create .editorconfig with code style rules (4-space indents, UTF-8, LF line endings)
- [X] T013 [P] Create docker/docker-compose.yml with EventStoreDB 23.x, MongoDB 7.x, RabbitMQ 3.13, Jaeger, Prometheus services
- [X] T014 [P] Create docker/init-scripts/mongo-init.js with typeSinistre reference data (ACCIDENT_CORPOREL, DEGATS_MATERIELS, RESPONSABILITE_CIVILE)
- [X] T015 [P] Create infrastructure/local/eventstoredb.yaml with EventStoreDB stream naming config
- [X] T016 Add project references: Application → Domain, Infrastructure → Application + Domain, Functions → Infrastructure

**Checkpoint**: ✅ Solution structure created, compiles successfully with `dotnet build` (0 warnings, 0 errors)

---

## Phase 2: Foundational (Blocking Prerequisites) ✅ COMPLETE

**Purpose**: Core infrastructure that MUST be complete before ANY user story implementation

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T017 [P] Create IDomainEvent interface in src/CAP.BSP.DSP.Domain/Events/IDomainEvent.cs
- [X] T018 [P] Create AggregateRoot base class in src/CAP.BSP.DSP.Domain/Aggregates/AggregateRoot.cs with event sourcing pattern (Apply, AddDomainEvent methods)
- [X] T019 [P] Create ICommand interface in src/CAP.BSP.DSP.Application/Commands/ICommand.cs
- [X] T020 [P] Create CommandResult record in src/CAP.BSP.DSP.Application/Commands/CommandResult.cs (Success/Failure result pattern)
- [X] T021 Install MediatR 12.2.x in Application project via NuGet
- [X] T022 Install FluentValidation 11.9.x in Application project via NuGet
- [X] T023 [P] Create IDeclarationRepository port in src/CAP.BSP.DSP.Application/Ports/IDeclarationRepository.cs (Save, GetById methods)
- [X] T024 [P] Create IEventPublisher port in src/CAP.BSP.DSP.Application/Ports/IEventPublisher.cs (Publish method for RabbitMQ)
- [X] T025 [P] Create IDeclarationReadModelRepository port in src/CAP.BSP.DSP.Application/Ports/IDeclarationReadModelRepository.cs (Add, FindById, FindByContract methods)
- [X] T026 [P] Create ITypeSinistreReferenceRepository port in src/CAP.BSP.DSP.Application/Ports/ITypeSinistreReferenceRepository.cs (Exists, GetByCode methods)
- [X] T027 Install EventStore.Client 23.x in Infrastructure project via NuGet
- [X] T028 Install MongoDB.Driver 2.25.x in Infrastructure project via NuGet
- [X] T029 Install RabbitMQ.Client 6.8.x in Infrastructure project via NuGet
- [X] T030 Install Polly 8.x in Infrastructure project via NuGet
- [X] T031 Create MongoDbContext in src/CAP.BSP.DSP.Infrastructure/Persistence/MongoDB/MongoDbContext.cs (connection, collections: declarationReadModel, typeSinistreReference, sequences)
- [X] T032 Create EventStoreConnection in src/CAP.BSP.DSP.Infrastructure/Persistence/EventStore/EventStoreConnection.cs (EventStoreClient configuration)
- [X] T033 Create RabbitMqConnection in src/CAP.BSP.DSP.Infrastructure/Messaging/RabbitMqConnection.cs (IConnection factory, channel pool)
- [X] T034 Create CircuitBreakerPolicies in src/CAP.BSP.DSP.Infrastructure/Resilience/CircuitBreakerPolicies.cs (MongoDB validation circuit breaker)
- [X] T035 Install Serilog in Infrastructure project via NuGet, create SerilogConfiguration in src/CAP.BSP.DSP.Infrastructure/Observability/SerilogConfiguration.cs
- [X] T036 Create Program.cs in src/CAP.BSP.DSP.Functions/ with DI container setup (MediatR, repositories, validators, circuit breakers)
- [X] T037 Create host.json in src/CAP.BSP.DSP.Functions/ with Azure Functions configuration (logging, concurrency, timeout)
- [X] T038 Create local.settings.json template in src/CAP.BSP.DSP.Functions/ with EventStoreDB, MongoDB, RabbitMQ connection strings

**Checkpoint**: ✅ Foundation ready - infrastructure wired, DI configured, services can resolve dependencies, builds with 0 warnings/errors

---

## Phase 3: User Story 4 - Require Contract Reference (Priority: P1) 🎯 MVP ✅ COMPLETE

**Goal**: Enforce that every claim declaration MUST include a valid identifiantContrat (fundamental business invariant)

**Independent Test**: Submit DeclarerSinistre command without identifiantContrat and verify rejection with validation error

### Implementation for User Story 4

- [X] T039 [P] [US4] Create IdentifiantContrat value object in src/CAP.BSP.DSP.Domain/ValueObjects/IdentifiantContrat.cs with static Create method validating non-empty
- [X] T040 [P] [US4] Create IdentifiantContratManquantException in src/CAP.BSP.DSP.Domain/Exceptions/IdentifiantContratManquantException.cs
- [X] T041 [US4] Add IdentifiantContrat property to DeclarerSinistreCommand in src/CAP.BSP.DSP.Application/Commands/DeclarerSinistre/DeclarerSinistreCommand.cs
- [X] T042 [US4] Add IdentifiantContrat validation rule in DeclarerSinistreCommandValidator in src/CAP.BSP.DSP.Application/Commands/DeclarerSinistre/DeclarerSinistreCommandValidator.cs (RuleFor: NotEmpty)
- [X] T043 [US4] Update DeclarationSinistre aggregate in src/CAP.BSP.DSP.Domain/Aggregates/DeclarationSinistre/DeclarationSinistre.cs to enforce identifiantContrat not null in Declarer factory method
- [X] T044 [US4] Add IdentifiantContrat to SinistreDeclare event in src/CAP.BSP.DSP.Domain/Events/SinistreDeclare.cs

**Checkpoint**: ✅ Commands without identifiantContrat are rejected with validation error "IdentifiantContrat obligatoire"

---

## Phase 4: User Story 1 - Submit Valid Claim Declaration (Priority: P1) 🎯 MVP

**Goal**: Core MVP - insured submits claim with valid contract and occurrence date, receives acknowledgment with auto-generated identifiantSinistre

**Independent Test**: Submit claim with valid identifiantContrat and dateSurvenance (past), verify SinistreDéclaré event emitted with auto-generated identifiantSinistre and system-set dateDeclaration, claim retrievable via query

### Implementation for User Story 1

- [X] T045 [P] [US1] Create IdentifiantSinistre value object in src/CAP.BSP.DSP.Domain/ValueObjects/IdentifiantSinistre.cs with regex validation (^SIN-\d{4}-\d{6}$)
- [X] T046 [P] [US1] Create DateSurvenance value object in src/CAP.BSP.DSP.Domain/ValueObjects/DateSurvenance.cs with ≤ today validation in static Create method
- [X] T047 [P] [US1] Create DateDeclaration value object in src/CAP.BSP.DSP.Domain/ValueObjects/DateDeclaration.cs with static Now() factory method
- [X] T048 [P] [US1] Create StatutDeclaration enum in src/CAP.BSP.DSP.Domain/ValueObjects/StatutDeclaration.cs (Declare, Valide, Annule)
- [X] T049 [P] [US1] Create DateSurvenanceFutureException in src/CAP.BSP.DSP.Domain/Exceptions/DateSurvenanceFutureException.cs
- [X] T050 [P] [US1] Create IIdentifiantSinistreGenerator service interface in src/CAP.BSP.DSP.Domain/Services/IIdentifiantSinistreGenerator.cs (GenerateNext method)
- [X] T051 [US1] Create DeclarationSinistreId value object in src/CAP.BSP.DSP.Domain/Aggregates/DeclarationSinistre/DeclarationSinistreId.cs (GUID wrapper)
- [X] T052 [US1] Create SinistreDeclare domain event in src/CAP.BSP.DSP.Domain/Events/SinistreDeclare.cs with properties: EventId, OccurredAt, IdentifiantSinistre, IdentifiantContrat, DateSurvenance, DateDeclaration, Statut, CorrelationId, CausationId, UserId
- [X] T053 [US1] Create DeclarationSinistre aggregate root in src/CAP.BSP.DSP.Domain/Aggregates/DeclarationSinistre/DeclarationSinistre.cs with Declarer factory method and Apply(SinistreDeclare) event handler
- [X] T054 [US1] Create DeclarerSinistreCommand in src/CAP.BSP.DSP.Application/Commands/DeclarerSinistre/DeclarerSinistreCommand.cs with properties: IdentifiantContrat, DateSurvenance, CorrelationId, UserId (NO identifiantSinistre or dateDeclaration)
- [X] T055 [US1] Create DeclarerSinistreCommandValidator in src/CAP.BSP.DSP.Application/Commands/DeclarerSinistre/DeclarerSinistreCommandValidator.cs with FluentValidation rules (NotEmpty for identifiantContrat, LessThanOrEqualTo today for dateSurvenance)
- [X] T056 [US1] Create SequentialIdentifiantSinistreGenerator in src/CAP.BSP.DSP.Infrastructure/DomainServices/SequentialIdentifiantSinistreGenerator.cs implementing IIdentifiantSinistreGenerator (MongoDB findAndModify on sequences collection for atomic increment)
- [X] T057 [US1] Create EventStoreDeclarationRepository in src/CAP.BSP.DSP.Infrastructure/Persistence/EventStore/EventStoreDeclarationRepository.cs implementing IDeclarationRepository (AppendToStreamAsync with stream name "declaration-sinistre-{id}")
- [X] T058 [US1] Create EventSerializer in src/CAP.BSP.DSP.Infrastructure/Persistence/EventStore/EventSerializer.cs (serialize domain events to EventStoreDB EventData with JSON payload)
- [X] T059 [US1] Create DeclarerSinistreCommandHandler in src/CAP.BSP.DSP.Application/Commands/DeclarerSinistre/DeclarerSinistreCommandHandler.cs implementing IRequestHandler<DeclarerSinistreCommand, CommandResult> (calls IIdentifiantSinistreGenerator, DeclarationSinistre.Declarer, IDeclarationRepository.Save)
- [X] T060 [US1] Create DeclarerSinistreFunction in src/CAP.BSP.DSP.Functions/Commands/DeclarerSinistreFunction.cs with [Function("DeclarerSinistre")] HTTP POST trigger, sends command via MediatR, returns 201 Created with identifiantSinistre + dateDeclaration
- [X] T061 [US1] Create RabbitMqEventPublisher in src/CAP.BSP.DSP.Infrastructure/Messaging/RabbitMqEventPublisher.cs implementing IEventPublisher (publish to "bsp.events" topic exchange with routing key "sinistre.declare")
- [X] T062 [US1] Wire up event publishing in DeclarerSinistreCommandHandler (call IEventPublisher.Publish after repository save for transactional outbox pattern)
- [X] T063 [P] [US1] Create DeclarationListProjection MongoDB document in src/CAP.BSP.DSP.Infrastructure/Persistence/MongoDB/Projections/DeclarationListProjection.cs
- [X] T064 [P] [US1] Create DeclarationDetailProjection MongoDB document in src/CAP.BSP.DSP.Infrastructure/Persistence/MongoDB/Projections/DeclarationDetailProjection.cs
- [X] T065 [US1] Create MongoDeclarationReadModelRepository in src/CAP.BSP.DSP.Infrastructure/Persistence/MongoDB/MongoDeclarationReadModelRepository.cs implementing IDeclarationReadModelRepository (Insert, FindOne, Find methods)
- [X] T066 [US1] Create SinistreDeclareEventHandler in src/CAP.BSP.DSP.Application/EventHandlers/SinistreDeclareEventHandler.cs implementing INotificationHandler<SinistreDeclare> (projects event to MongoDB declarationReadModel collection)
- [X] T067 [P] [US1] Create RechercherDeclarationsQuery in src/CAP.BSP.DSP.Application/Queries/RechercherDeclarations/RechercherDeclarationsQuery.cs with filters: IdentifiantContrat, Statut, Limit, Offset
- [X] T068 [P] [US1] Create DeclarationListItemDto in src/CAP.BSP.DSP.Application/Queries/RechercherDeclarations/DeclarationListItemDto.cs
- [X] T069 [US1] Create RechercherDeclarationsQueryHandler in src/CAP.BSP.DSP.Application/Queries/RechercherDeclarations/RechercherDeclarationsQueryHandler.cs implementing IRequestHandler (queries MongoDB read model)
- [X] T070 [US1] Create RechercherDeclarationsFunction in src/CAP.BSP.DSP.Functions/Queries/RechercherDeclarationsFunction.cs with HTTP GET trigger on /api/v1/declarations
- [X] T071 [P] [US1] Create ObtenirStatutDeclarationQuery in src/CAP.BSP.DSP.Application/Queries/ObtenirStatutDeclaration/ObtenirStatutDeclarationQuery.cs with IdentifiantSinistre parameter
- [X] T072 [P] [US1] Create DeclarationDetailDto in src/CAP.BSP.DSP.Application/Queries/ObtenirStatutDeclaration/DeclarationDetailDto.cs
- [X] T073 [US1] Create ObtenirStatutDeclarationQueryHandler in src/CAP.BSP.DSP.Application/Queries/ObtenirStatutDeclaration/ObtenirStatutDeclarationQueryHandler.cs implementing IRequestHandler
- [X] T074 [US1] Create ObtenirStatutDeclarationFunction in src/CAP.BSP.DSP.Functions/Queries/ObtenirStatutDeclarationFunction.cs with HTTP GET trigger on /api/v1/declarations/{identifiantSinistre}

**Checkpoint**: User Story 1 fully functional - can declare claim via POST, receive identifiantSinistre, query via GET

---

## Phase 5: User Story 2 - Reject Invalid Claim Type (Priority: P2)

**Goal**: Prevent claim submission when typeSinistre doesn't exist in reference database (data quality enforcement)

**Independent Test**: Submit claim with invalid typeSinistre (e.g., "INVALID_TYPE"), verify rejection with validation error before event emission

### Implementation for User Story 2

- [ ] T075 [P] [US2] Create TypeSinistre value object in src/CAP.BSP.DSP.Domain/ValueObjects/TypeSinistre.cs with static Create method (accepts code, validation deferred to service)
- [ ] T076 [P] [US2] Create TypeSinistreInvalideException in src/CAP.BSP.DSP.Domain/Exceptions/TypeSinistreInvalideException.cs
- [ ] T077 [P] [US2] Create ITypeSinistreValidator service interface in src/CAP.BSP.DSP.Domain/Services/ITypeSinistreValidator.cs (IsValid method)
- [ ] T078 [US2] Add TypeSinistre property to DeclarerSinistreCommand in src/CAP.BSP.DSP.Application/Commands/DeclarerSinistre/DeclarerSinistreCommand.cs
- [ ] T079 [US2] Add TypeSinistre validation rule in DeclarerSinistreCommandValidator (NotEmpty rule)
- [ ] T080 [US2] Create MongoTypeSinistreReferenceRepository in src/CAP.BSP.DSP.Infrastructure/Persistence/MongoDB/MongoTypeSinistreReferenceRepository.cs implementing ITypeSinistreReferenceRepository (queries typeSinistreReference collection)
- [ ] T081 [US2] Create MongoTypeSinistreValidator in src/CAP.BSP.DSP.Infrastructure/DomainServices/MongoTypeSinistreValidator.cs implementing ITypeSinistreValidator with in-memory cache (60min TTL) and Polly circuit breaker fallback
- [ ] T082 [US2] Update DeclarationSinistre.Declarer factory method to call ITypeSinistreValidator.IsValid before creating aggregate, throw TypeSinistreInvalideException if invalid
- [ ] T083 [US2] Update DeclarerSinistreCommandHandler to inject ITypeSinistreValidator and pass to aggregate factory
- [ ] T084 [US2] Add TypeSinistre property to SinistreDeclare event
- [ ] T085 [US2] Update DeclarationListProjection and DeclarationDetailProjection to include typeSinistreLibelle field (denormalized from reference data)
- [ ] T086 [US2] Update SinistreDeclareEventHandler to populate typeSinistreLibelle from ITypeSinistreReferenceRepository when projecting

**Checkpoint**: Commands with invalid typeSinistre rejected with error "TypeSinistre invalide: '{code}' n'existe pas dans le référentiel"

---

## Phase 6: User Story 3 - Reject Future Occurrence Dates (Priority: P2)

**Goal**: Prevent claim submission when dateSurvenance is in the future (business rule: claims only for past events)

**Independent Test**: Submit claim with dateSurvenance = tomorrow, verify rejection with validation error

### Implementation for User Story 3

- [ ] T087 [US3] Enhance DateSurvenance.Create method in src/CAP.BSP.DSP.Domain/ValueObjects/DateSurvenance.cs to throw DateSurvenanceFutureException if value > DateTime.UtcNow.Date
- [ ] T088 [US3] Add DateSurvenance validation in DeclarerSinistreCommandValidator with custom validation: LessThanOrEqualTo(DateTime.UtcNow.Date) with message "DateSurvenance ne peut pas être dans le futur"
- [ ] T089 [US3] Update DeclarationSinistre.Declarer factory method to validate DateSurvenance.Create (which throws if future date)

**Checkpoint**: Commands with future dateSurvenance rejected with error "DateSurvenance invalide: la date de survenance ne peut pas être dans le futur"

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Production-readiness improvements affecting multiple user stories

- [ ] T090 [P] Create HealthCheckFunction in src/CAP.BSP.DSP.Functions/Health/HealthCheckFunction.cs with /api/health/liveness (returns 200 OK always) and /api/health/readiness (checks EventStoreDB, MongoDB, RabbitMQ connectivity)
- [ ] T091 [P] Install OpenTelemetry.Extensions.Hosting in Functions project, create OpenTelemetryConfiguration in src/CAP.BSP.DSP.Infrastructure/Observability/OpenTelemetryConfiguration.cs with Jaeger exporter
- [ ] T092 [P] Create MetricsCollector in src/CAP.BSP.DSP.Infrastructure/Observability/MetricsCollector.cs with Prometheus metrics (command_processing_latency_ms, command_success_total, command_failure_total, event_publish_total)
- [ ] T093 [P] Add correlation ID propagation in DeclarerSinistreCommandHandler (set CorrelationId, CausationId on events)
- [ ] T094 [P] Add structured logging in DeclarerSinistreCommandHandler with Serilog (log command attempts, validation failures, event emissions)
- [ ] T095 [P] Create RetryPolicies in src/CAP.BSP.DSP.Infrastructure/Resilience/RetryPolicies.cs (MongoDB transient failures, RabbitMQ publish retries)
- [ ] T096 [P] Add error handling middleware in Program.cs for Azure Functions (catch exceptions, return ProblemDetails JSON)
- [ ] T097 Update docker/docker-compose.yml to include MongoDB indexes: { identifiantSinistre: 1 } unique, { identifiantContrat: 1, dateDeclaration: -1 }, { typeSinistre: 1, statut: 1 }
- [ ] T098 Create infrastructure/azure/main.bicep with Azure Functions Premium Plan, EventStoreDB VM, MongoDB Atlas connection, RabbitMQ CloudAMQP, Key Vault for secrets
- [ ] T099 [P] Create .github/workflows/ci.yml with build, test, lint steps (dotnet build, dotnet test, dotnet format)
- [ ] T100 [P] Create .github/workflows/cd.yml with Azure Functions deployment via `func azure functionapp publish`
- [ ] T101 [P] Create Dockerfile in docker/Dockerfile with multi-stage build (build .NET, runtime with Alpine Linux)
- [ ] T102 Validate quickstart.md by running docker-compose up, dotnet build, func start, curl POST /api/v1/declarations
- [ ] T103 Create ADR-DSP-001 in specs/001-declarer-sinistre/adrs/ADR-DSP-001-dotnet-stack.md documenting .NET stack deviation from constitution (Python/Kotlin)
- [ ] T104 Update README.md with instructions to run local environment, deploy to Azure, access observability tools

**Checkpoint**: All user stories complete, infrastructure production-ready, documented, deployable

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - start immediately  
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories  
- **US4 (Phase 3)**: Depends on Foundational completion  
- **US1 (Phase 4)**: Depends on Foundational + US4 (needs IdentifiantContrat)  
- **US2 (Phase 5)**: Depends on US1 (extends TypeSinistre validation)  
- **US3 (Phase 6)**: Depends on US1 (extends DateSurvenance validation)  
- **Polish (Phase 7)**: Depends on all desired user stories being complete

### User Story Completion Order

**MVP (minimum viable product)**:
1. Setup (Phase 1)
2. Foundational (Phase 2)
3. US4: Require Contract Reference (Phase 3)
4. US1: Submit Valid Claim Declaration (Phase 4)
5. **STOP HERE** - Deploy MVP with core functionality

**Incremental Delivery**:
1. MVP deployed
2. Add US2: Reject Invalid Claim Type (Phase 5) → Deploy
3. Add US3: Reject Future Occurrence Dates (Phase 6) → Deploy
4. Add Polish (Phase 7) → Production-ready deployment

### User Story Dependencies

- **US4 (P1)**: Independent - can start after Foundational  
- **US1 (P1)**: Depends on US4 (IdentifiantContrat is required for US1)  
- **US2 (P2)**: Depends on US1 (extends command/event with TypeSinistre)  
- **US3 (P2)**: Depends on US1 (extends command validation for DateSurvenance)  

**Cannot parallelize user stories** due to dependencies (US1 needs US4, US2/US3 need US1)

### Within Each User Story

**US4 (Require Contract Reference)**:
- T039-T040 (value object + exception) can run in parallel
- T041-T042 (command + validator) must follow T039
- T043-T044 (aggregate + event) must follow T039

**US1 (Submit Valid Claim Declaration)**:
- T045-T050 (value objects + domain services) can run in parallel
- T051-T053 (aggregate components) sequential
- T054-T055 (command + validator) can run in parallel
- T056-T058 (infrastructure adapters) can run in parallel after T050, T051
- T059 (command handler) depends on T053, T056, T057
- T060 (Azure Function) depends on T059
- T063-T064 (projections) can run in parallel
- T067-T068, T071-T072 (query DTOs) can run in parallel

**US2 (Reject Invalid Claim Type)**:
- T075-T077 (value object + exception + interface) can run in parallel
- T078-T079 (extend command/validator) sequential after T075
- T080-T081 (repository + validator impl) can run in parallel after T077
- T082-T083 (aggregate + handler) sequential after T081

**US3 (Reject Future Occurrence Dates)**:
- T087-T089 sequential (enhances existing DateSurvenance)

### Parallel Opportunities

**Phase 1 (Setup)**: T002-T005 (projects), T006-T010 (test projects), T012-T015 (config files) can all run in parallel

**Phase 2 (Foundational)**: T017-T020 (interfaces), T023-T026 (ports), T031-T033 (infrastructure connections), T035-T038 (observability + config) can run in parallel

**Phase 4 (US1)**: Value objects (T045-T048), domain services (T050), infrastructure adapters (T056-T058), projections (T063-T064), query components (T067-T068, T071-T072) can run in parallel

**Phase 7 (Polish)**: Most tasks marked [P] (health checks, observability, CI/CD, docs) can run in parallel

---

## Parallel Example: User Story 1 (Core Implementation)

```bash
# After foundational phase, launch all value objects in parallel:
Task T045: Create IdentifiantSinistre value object
Task T046: Create DateSurvenance value object
Task T047: Create DateDeclaration value object
Task T048: Create StatutDeclaration enum
Task T049: Create DateSurvenanceFutureException
Task T050: Create IIdentifiantSinistreGenerator interface

# Then sequentially build aggregate (depends on value objects):
Task T051: Create DeclarationSinistreId
Task T052: Create SinistreDeclare event
Task T053: Create DeclarationSinistre aggregate root

# Parallel infrastructure adapters:
Task T056: Create SequentialIdentifiantSinistreGenerator (MongoDB)
Task T057: Create EventStoreDeclarationRepository (EventStoreDB)
Task T058: Create EventSerializer

# Command handler ties it together:
Task T059: Create DeclarerSinistreCommandHandler
Task T060: Create DeclarerSinistreFunction (HTTP trigger)
```

---

## Implementation Strategy

### MVP First (US4 + US1 Only) ✅ RECOMMENDED

1. Complete Phase 1: Setup (~2 hours)
2. Complete Phase 2: Foundational (~4 hours)
3. Complete Phase 3: US4 (Require Contract) (~1 hour)
4. Complete Phase 4: US1 (Submit Valid Claim) (~6 hours)
5. **STOP and VALIDATE**: Test via POST /api/v1/declarations, verify event in EventStoreDB, query via GET
6. Deploy to dev environment
7. **MVP DELIVERED** - Core claim declaration working

### Incremental Delivery (Add US2, US3, Polish)

1. MVP deployed (US4 + US1)
2. Add Phase 5: US2 (Reject Invalid Type) (~3 hours) → Deploy
3. Add Phase 6: US3 (Reject Future Dates) (~1 hour) → Deploy
4. Add Phase 7: Polish (observability, health checks, CI/CD) (~4 hours) → Production deployment

### Parallel Team Strategy (if available)

**NOT RECOMMENDED** due to strong dependencies between user stories (US1 needs US4, US2/US3 need US1)

**Sequential execution preferred**: One developer implements US4 → US1 → US2 → US3 → Polish

---

## Task Count Summary

| Phase | Task Count | Est. Time |
|-------|------------|-----------|
| Phase 1: Setup | 16 tasks (T001-T016) | 2 hours |
| Phase 2: Foundational | 22 tasks (T017-T038) | 4 hours |
| Phase 3: US4 (P1) | 6 tasks (T039-T044) | 1 hour |
| Phase 4: US1 (P1) | 30 tasks (T045-T074) | 6 hours |
| Phase 5: US2 (P2) | 12 tasks (T075-T086) | 3 hours |
| Phase 6: US3 (P2) | 3 tasks (T087-T089) | 1 hour |
| Phase 7: Polish | 15 tasks (T090-T104) | 4 hours |
| **TOTAL** | **104 tasks** | **~21 hours** |

**MVP Scope**: Phase 1-4 (54 tasks, ~13 hours) delivers core claim declaration functionality

---

## Notes

- [P] = Parallelizable (different files, no dependencies)
- [Story] = Maps task to specific user story for traceability (US1, US2, US3, US4)
- Tests NOT included (not requested in specification per FR-021 observability logging only)
- Each user story independently testable via HTTP API (POST command, GET query, verify EventStoreDB/MongoDB/RabbitMQ)
- Tasks include exact file paths following plan.md Clean Architecture structure
- ADR-DSP-001 required before code implementation (Phase 7, T103) to document .NET stack deviation
- Commit after logical groups (value objects, aggregate, command handler, function)
- Validate at each checkpoint before proceeding to next phase

---

**Generated by**: `/speckit.tasks` command  
**Feature Branch**: 001-declarer-sinistre  
**References**: [spec.md](spec.md), [plan.md](plan.md), [data-model.md](data-model.md), [contracts/](contracts/), [research.md](research.md)  
**Ready for Implementation**: ✅ YES (pending ADR-DSP-001 approval)
