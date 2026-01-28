# Quickstart Guide: Local Development Setup

**Feature**: 001-declarer-sinistre (Claims Declaration)  
**Stack**: .NET 8 + EventStoreDB + MongoDB + RabbitMQ + Azure Functions  
**Last Updated**: 2026-01-27

## Overview

This guide helps you set up a **fully local development environment** with all infrastructure running in Docker containers. You'll be able to:

- Run EventStoreDB, MongoDB, RabbitMQ locally (no cloud dependencies)
- Build and test .NET code with hot reload
- Debug Azure Functions locally with VS Code
- Run integration tests with Testcontainers
- Observe traces and metrics in Jaeger and Prometheus

**Time to first run**: ~10 minutes

---

## Prerequisites

### Required Software

| Tool | Version | Purpose | Installation |
|------|---------|---------|--------------|
| **Docker** | 20.10+ | Run infrastructure containers | [Get Docker](https://docs.docker.com/get-docker/) |
| **Docker Compose** | 2.0+ | Orchestrate multi-container setup | Included with Docker Desktop |
| **.NET SDK** | 8.0+ | Build and run .NET code | [Download .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0) |
| **Azure Functions Core Tools** | 4.x | Run Azure Functions locally | `npm install -g azure-functions-core-tools@4` |
| **Git** | 2.x | Version control | [Get Git](https://git-scm.com/) |

### Optional Tools

| Tool | Purpose |
|------|---------|
| **VS Code** | IDE with debugging support for Azure Functions |
| **MongoDB Compass** | GUI for MongoDB (alternative to mongo shell) |
| **curl** / **Postman** | Test HTTP endpoints |

### System Requirements

- **OS**: Linux, macOS, Windows (with WSL2 for Docker)
- **RAM**: 8 GB minimum (16 GB recommended for running all services + IDE)
- **Disk**: 5 GB free space for Docker images

---

## Step 1: Clone Repository and Navigate to Project

```bash
cd /home/yoann/yoann/foodaroo/BCM/Business\ Service\ Production/CAP.BSP-005-Sinistres/CAP.BSP.DSP

# Verify you're on the correct feature branch
git branch --show-current
# Expected output: 001-declarer-sinistre
```

---

## Step 2: Start Infrastructure with Docker Compose

```bash
# From project root, start all infrastructure services
docker-compose -f docker/docker-compose.yml up -d

# Verify all containers are running
docker-compose -f docker/docker-compose.yml ps

# Expected output (all STATUS = "Up"):
# NAME                   STATUS
# eventstore             Up (healthy)
# mongodb                Up (healthy)
# rabbitmq               Up (healthy)
# jaeger                 Up
# prometheus             Up
```

**What gets started:**

1. **EventStoreDB** (port 2113) - Event sourcing database
2. **MongoDB** (port 27017) - Read model + reference data
3. **RabbitMQ** (port 5672, management 15672) - Event bus
4. **Jaeger** (port 16686) - Distributed tracing UI
5. **Prometheus** (port 9090) - Metrics collection

**Startup time**: ~30 seconds (first run may take 2-3 minutes to download images)

---

## Step 3: Verify Infrastructure Health

### EventStoreDB

```bash
# Access EventStoreDB UI
open http://localhost:2113

# Login credentials:
# Username: admin
# Password: changeit

# Expected: Dashboard showing "0 streams" (empty database)
```

**Verify via curl**:
```bash
curl -u admin:changeit http://localhost:2113/stats
# Expected: JSON with "proc" statistics
```

---

### MongoDB

```bash
# Access MongoDB and verify seed data
docker exec -it mongodb mongosh

# In mongo shell:
use cap_bsp_dsp
db.typeSinistreReference.find()

# Expected output (3 reference types):
# { _id: "ACCIDENT_CORPOREL", libelle: "Accident corporel", ... }
# { _id: "DEGATS_MATERIELS", libelle: "Dégâts matériels", ... }
# { _id: "RESPONSABILITE_CIVILE", libelle: "Responsabilité civile", ... }

exit
```

**Verify sequence collection** (for ID generation):
```bash
docker exec -it mongodb mongosh cap_bsp_dsp --eval \
  "db.sequences.findOne({_id: 'identifiantSinistre'})"

# Expected: { _id: "identifiantSinistre", year: 2026, seq: 0 }
```

---

### RabbitMQ

```bash
# Access RabbitMQ Management UI
open http://localhost:15672

# Login credentials:
# Username: guest
# Password: guest

# Expected: Overview showing "0 queues, 0 exchanges" (empty broker)
```

**Verify via curl**:
```bash
curl -u guest:guest http://localhost:15672/api/overview
# Expected: JSON with "rabbitmq_version": "3.13.x"
```

---

## Step 4: Build .NET Solution

```bash
# Restore NuGet packages
dotnet restore

# Build all projects (Domain, Application, Infrastructure, Functions)
dotnet build

# Expected output: "Build succeeded. 0 Warning(s). 0 Error(s)."
```

**Verify project structure**:
```bash
find src -name "*.csproj"

# Expected output:
# src/CAP.BSP.DSP.Domain/CAP.BSP.DSP.Domain.csproj
# src/CAP.BSP.DSP.Application/CAP.BSP.DSP.Application.csproj
# src/CAP.BSP.DSP.Infrastructure/CAP.BSP.DSP.Infrastructure.csproj
# src/CAP.BSP.DSP.Functions/CAP.BSP.DSP.Functions.csproj
```

---

## Step 5: Run Unit Tests

```bash
# Run Domain tests (pure business logic, no infrastructure)
dotnet test tests/CAP.BSP.DSP.Domain.Tests/

# Run Application tests (command handlers, use cases)
dotnet test tests/CAP.BSP.DSP.Application.Tests/

# Expected: All tests pass (green output)
```

**Example test run**:
```
Test run for CAP.BSP.DSP.Domain.Tests.dll (.NET 8.0)
Total tests: 15
     Passed: 15
      Time: 0.5s
```

---

## Step 6: Run Integration Tests (with Testcontainers)

```bash
# Integration tests spin up Docker containers automatically
dotnet test tests/CAP.BSP.DSP.Integration.Tests/ --logger "console;verbosity=detailed"

# What happens:
# 1. Testcontainers starts EventStoreDB, MongoDB, RabbitMQ containers
# 2. Tests execute against real infrastructure
# 3. Containers are torn down after tests complete

# Expected: All tests pass (~30s execution time)
```

**Troubleshooting**:
- If tests fail with "Docker not found", ensure Docker daemon is running
- If tests timeout, increase Docker memory limit (8GB minimum)

---

## Step 7: Start Azure Functions Locally

```bash
# Navigate to Functions project
cd src/CAP.BSP.DSP.Functions

# Configure local settings (create local.settings.json if not exists)
cat > local.settings.json <<EOF
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "EventStoreDb__ConnectionString": "esdb://admin:changeit@localhost:2113?tls=false",
    "MongoDB__ConnectionString": "mongodb://localhost:27017",
    "MongoDB__DatabaseName": "cap_bsp_dsp",
    "RabbitMQ__HostName": "localhost",
    "RabbitMQ__UserName": "guest",
    "RabbitMQ__Password": "guest"
  }
}
EOF

# Start Functions runtime
func start

# Expected output:
# Azure Functions Core Tools
# Core Tools Version: 4.x
# 
# Functions:
#   DeclarerSinistreFunction: [POST] http://localhost:7071/api/v1/declarations
#   GetDeclarationFunction: [GET] http://localhost:7071/api/v1/declarations/{id}
#   ListDeclarationsFunction: [GET] http://localhost:7071/api/v1/declarations
#   HealthLivenessFunction: [GET] http://localhost:7071/api/health/liveness
#   HealthReadinessFunction: [GET] http://localhost:7071/api/health/readiness
```

**Keep this terminal running** - Functions are now listening on http://localhost:7071

---

## Step 8: Test the API

### Health Check

```bash
# In a new terminal, verify service is healthy
curl http://localhost:7071/api/health/readiness | jq

# Expected response:
# {
#   "status": "UP",
#   "checks": {
#     "eventStoreDb": "UP",
#     "mongoDb": "UP",
#     "rabbitMq": "UP"
#   }
# }
```

---

### Declare a Claim

```bash
# Submit a claim declaration command
curl -X POST http://localhost:7071/api/v1/declarations \
  -H "Content-Type: application/json" \
  -d '{
    "identifiantContrat": "CONTRAT-2026-001",
    "typeSinistre": "ACCIDENT_CORPOREL",
    "dateSurvenance": "2026-01-20"
  }' | jq

# Expected response (201 Created):
# {
#   "identifiantSinistre": "SIN-2026-000001",
#   "dateDeclaration": "2026-01-27T14:30:00.000Z"
# }
```

---

### Query the Declaration

```bash
# Retrieve the declaration we just created
curl http://localhost:7071/api/v1/declarations/SIN-2026-000001 | jq

# Expected response (200 OK):
# {
#   "identifiantSinistre": "SIN-2026-000001",
#   "identifiantContrat": "CONTRAT-2026-001",
#   "typeSinistre": "ACCIDENT_CORPOREL",
#   "typeSinistreLibelle": "Accident corporel",
#   "dateSurvenance": "2026-01-20",
#   "dateDeclaration": "2026-01-27T14:30:00.000Z",
#   "statut": "DECLARE",
#   "version": 1
# }
```

---

### Verify Event in EventStoreDB

```bash
# Check that SinistreDéclaré event was persisted
curl -u admin:changeit \
  "http://localhost:2113/streams/declaration-sinistre-SIN-2026-000001" | jq

# Expected: JSON array with 1 event of type "SinistreDeclare"
```

**Or use EventStoreDB UI**:
1. Open http://localhost:2113
2. Navigate to "Stream Browser"
3. Search for `declaration-sinistre-SIN-2026-000001`
4. Inspect event data

---

### Verify Event in RabbitMQ

```bash
# Check that event was published to RabbitMQ
# In RabbitMQ Management UI (http://localhost:15672):
# 1. Go to "Exchanges" tab
# 2. Click "bsp.events" exchange
# 3. See "Publish rate" chart showing 1 message

# Or via curl:
curl -u guest:guest http://localhost:15672/api/exchanges/%2F/bsp.events | jq '.message_stats'
```

---

## Step 9: Observe with Jaeger (Distributed Tracing)

```bash
# Open Jaeger UI
open http://localhost:16686

# Select service: "CAP.BSP.DSP"
# Click "Find Traces"
# Expected: Trace showing POST /api/v1/declarations with spans:
#   - HTTP request
#   - Command validation
#   - TypeSinistre validation (MongoDB call)
#   - ID generation (MongoDB sequence)
#   - Event persistence (EventStoreDB)
#   - Event publishing (RabbitMQ)
```

**Trace timeline example**:
```
POST /api/v1/declarations [450ms]
  ├─ Validate command [2ms]
  ├─ Validate TypeSinistre [50ms] (MongoDB)
  ├─ Generate IdentifiantSinistre [30ms] (MongoDB)
  ├─ Persist event [200ms] (EventStoreDB)
  └─ Publish to RabbitMQ [150ms]
```

---

## Step 10: Debug with VS Code

### Configure Launch Settings

Create `.vscode/launch.json`:
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Attach to .NET Functions",
      "type": "coreclr",
      "request": "attach",
      "processId": "${command:pickProcess}"
    }
  ]
}
```

### Start Debugging

1. Start Functions with `func start` (Step 7)
2. In VS Code, press F5
3. Select the `func` process from the list
4. Set breakpoints in command handlers (e.g., `DeclarerSinistreCommandHandler.cs`)
5. Send HTTP request (Step 8)
6. Debugger pauses at breakpoints

---

## Architecture Tests

Verify Clean Architecture rules:

```bash
# Run architecture compliance tests (ArchUnitNET)
dotnet test tests/CAP.BSP.DSP.Architecture.Tests/

# What it checks:
# - Domain has no dependencies on Application/Infrastructure
# - Application depends only on Domain
# - Infrastructure depends on Application + Domain
# - No circular dependencies
# - Value objects are immutable (readonly properties)

# Expected: All tests pass
```

---

## Common Issues & Troubleshooting

### Port Conflicts

**Symptom**: `docker-compose up` fails with "port already allocated"

**Solution**:
```bash
# Find process using port (e.g., 2113 for EventStoreDB)
lsof -i :2113

# Kill process or change port in docker-compose.yml
```

---

### MongoDB Connection Refused

**Symptom**: Integration tests fail with "MongoConnectionException"

**Solution**:
```bash
# Ensure MongoDB container is running
docker ps | grep mongodb

# Restart MongoDB if unhealthy
docker-compose -f docker/docker-compose.yml restart mongodb
```

---

### EventStoreDB Authentication Error

**Symptom**: `401 Unauthorized` when calling EventStoreDB

**Solution**:
- Verify credentials in `local.settings.json`: `admin:changeit`
- Check connection string includes `?tls=false` for local setup

---

### Functions Not Starting

**Symptom**: `func start` hangs or crashes

**Solution**:
```bash
# Ensure .NET 8 SDK is installed
dotnet --version
# Expected: 8.0.x

# Rebuild solution
dotnet clean && dotnet build

# Clear Functions cache
rm -rf bin/ obj/
```

---

## Next Steps

✅ Local environment working? Great! Now you can:

1. **Implement new features**:
   - Add value objects in `src/CAP.BSP.DSP.Domain/ValueObjects/`
   - Write command handlers in `src/CAP.BSP.DSP.Application/Commands/`
   - Add Azure Functions in `src/CAP.BSP.DSP.Functions/`

2. **Write tests**:
   - Unit tests in `tests/CAP.BSP.DSP.Domain.Tests/`
   - Integration tests in `tests/CAP.BSP.DSP.Integration.Tests/`
   - Contract tests in `tests/CAP.BSP.DSP.Contract.Tests/` (JSON Schema validation)

3. **Explore observability**:
   - View traces in Jaeger (http://localhost:16686)
   - Query metrics in Prometheus (http://localhost:9090)
   - Monitor EventStoreDB streams (http://localhost:2113)

4. **Read design docs**:
   - [Data Model](data-model.md) - DDD tactical patterns
   - [Research](research.md) - Technical decisions
   - [API Contracts](contracts/openapi.yaml) - HTTP endpoints

---

## Clean Up

Stop all infrastructure:
```bash
docker-compose -f docker/docker-compose.yml down

# Remove volumes (deletes all data):
docker-compose -f docker/docker-compose.yml down -v
```

Remove .NET build artifacts:
```bash
dotnet clean
rm -rf **/bin **/obj
```

---

## Reference: Container Ports

| Service | Port(s) | Purpose |
|---------|---------|---------|
| EventStoreDB | 2113 (HTTP), 1113 (TCP) | Event store + admin UI |
| MongoDB | 27017 | Database |
| RabbitMQ | 5672 (AMQP), 15672 (HTTP) | Message broker + management UI |
| Jaeger | 16686 (UI), 14268 (collector) | Distributed tracing |
| Prometheus | 9090 | Metrics scraping + UI |
| Azure Functions | 7071 | Local HTTP API |

---

**Last Updated**: 2026-01-27  
**Maintainer**: Foodaroo BCM Team  
**Questions?**: See [README.md](../../README.md) or open an issue
