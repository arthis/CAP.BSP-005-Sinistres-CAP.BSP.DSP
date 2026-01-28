#!/bin/bash
set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Project root directory
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo -e "${BLUE}╔════════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║  CAP.BSP.DSP - Claims Declaration Service - Quick Start       ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════════════╝${NC}"
echo ""

cd docker && docker-compose down 2>/dev/null || true
cd "$PROJECT_ROOT"

# Check prerequisites
echo -e "${YELLOW}📋 Checking prerequisites...${NC}"

if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}❌ .NET SDK not found. Please install .NET 8.0 SDK${NC}"
    exit 1
fi
echo -e "${GREEN}✅ .NET SDK found: $(dotnet --version)${NC}"

if ! command -v docker &> /dev/null; then
    echo -e "${RED}❌ Docker not found. Please install Docker${NC}"
    exit 1
fi
echo -e "${GREEN}✅ Docker found${NC}"

if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
    echo -e "${RED}❌ Docker Compose not found. Please install Docker Compose${NC}"
    exit 1
fi
echo -e "${GREEN}✅ Docker Compose found${NC}"

if ! command -v func &> /dev/null; then
    echo -e "${YELLOW}⚠️  Azure Functions Core Tools not found. Installing recommended...${NC}"
    echo -e "${YELLOW}   You can install it with: npm install -g azure-functions-core-tools@4${NC}"
    exit 1
fi
echo -e "${GREEN}✅ Azure Functions Core Tools found: $(func --version)${NC}"

echo ""

# Check port availability
echo -e "${YELLOW}🔍 Checking port availability...${NC}"

REQUIRED_PORTS=(
    "2113:EventStoreDB HTTP"
    "1113:EventStoreDB TCP"
    "27017:MongoDB"
    "5672:RabbitMQ AMQP"
    "15672:RabbitMQ Management"
    "16686:Jaeger UI"
    "14268:Jaeger Collector HTTP"
    "14250:Jaeger Collector gRPC"
    "4317:OTLP gRPC"
    "4318:OTLP HTTP"
    "9090:Prometheus"
    "7071:Azure Functions"
)

check_port() {
    local port=$1
    local in_use=false
    
    # Method 1: lsof (most reliable)
    if command -v lsof &> /dev/null; then
        if lsof -i :$port -sTCP:LISTEN &> /dev/null || lsof -i :$port &> /dev/null; then
            in_use=true
        fi
    # Method 2: netstat
    elif command -v netstat &> /dev/null; then
        if netstat -tuln 2>/dev/null | grep -E "[:.]$port[[:space:]]" &> /dev/null; then
            in_use=true
        fi
    # Method 3: ss (modern alternative)
    elif command -v ss &> /dev/null; then
        if ss -tuln 2>/dev/null | grep -E ":$port[[:space:]]" &> /dev/null; then
            in_use=true
        fi
    fi
    
    # Method 4: Check for Docker containers (running or stopped) using this port
    if command -v docker &> /dev/null; then
        if docker ps -a --format '{{.Ports}}' 2>/dev/null | grep -E "0\.0\.0\.0:$port|:::$port" &> /dev/null; then
            in_use=true
        fi
    fi
    
    $in_use && return 0 || return 1
}

get_port_usage() {
    local port=$1
    if command -v lsof &> /dev/null; then
        lsof -i :$port 2>/dev/null | tail -n +2
    elif command -v netstat &> /dev/null; then
        netstat -tulnp 2>/dev/null | grep ":$port"
    elif command -v ss &> /dev/null; then
        ss -tulnp 2>/dev/null | grep ":$port"
    fi
}

PORT_CONFLICTS=()
PORT_DETAILS=()

for port_info in "${REQUIRED_PORTS[@]}"; do
    port="${port_info%%:*}"
    service="${port_info#*:}"
    
    if check_port "$port"; then
        PORT_CONFLICTS+=("Port $port ($service)")
        usage=$(get_port_usage "$port")
        if [ -n "$usage" ]; then
            PORT_DETAILS+=("Port $port:\n$usage")
        fi
    fi
done

if [ ${#PORT_CONFLICTS[@]} -gt 0 ]; then
    echo -e "${RED}❌ The following ports are already in use:${NC}"
    for conflict in "${PORT_CONFLICTS[@]}"; do
        echo -e "${RED}   - $conflict${NC}"
    done
    echo ""
    
    if [ ${#PORT_DETAILS[@]} -gt 0 ]; then
        echo -e "${YELLOW}📋 Port usage details:${NC}"
        for detail in "${PORT_DETAILS[@]}"; do
            echo -e "${YELLOW}$detail${NC}"
            echo ""
        done
    fi
    
    # Check for stopped Docker containers
    echo -e "${YELLOW}🔍 Checking for Docker containers that may be holding ports...${NC}"
    stopped_containers=$(docker ps -a --filter "status=exited" --filter "name=cap-bsp-dsp" --format "{{.Names}}" 2>/dev/null)
    if [ -n "$stopped_containers" ]; then
        echo -e "${YELLOW}   Found stopped containers:${NC}"
        echo "$stopped_containers" | while read container; do
            echo -e "${YELLOW}   - $container${NC}"
        done
        echo ""
        echo -e "${YELLOW}💡 Run the following to remove stopped containers:${NC}"
        echo -e "${YELLOW}   cd docker && docker-compose down${NC}"
    fi
    
    echo ""
    echo -e "${YELLOW}💡 To find what's using a port: lsof -i :<port> or docker ps -a${NC}"
    echo -e "${YELLOW}💡 To stop all project containers: cd docker && docker-compose down${NC}"
    exit 1
fi

echo -e "${GREEN}✅ All required ports are available${NC}"
echo ""

# Step 1: Start infrastructure
echo -e "${BLUE}🐳 Starting infrastructure services (EventStoreDB, MongoDB, RabbitMQ, Jaeger, Prometheus)...${NC}"
cd "$PROJECT_ROOT/docker"

# Check if services are already running
if docker-compose ps | grep -q "Up"; then
    echo -e "${YELLOW}⚠️  Some services are already running. Restarting...${NC}"
    docker-compose down
fi

docker-compose up -d

echo -e "${YELLOW}⏳ Waiting for services to be healthy (this may take 30-60 seconds)...${NC}"

# Wait for services to be healthy
max_attempts=60

check_service() {
    local service_name=$1
    local container_name=$2
    local check_command=$3
    local attempt=0
    
    # First check if container exists and is running
    if ! docker ps --filter "name=$container_name" --filter "status=running" --format "{{.Names}}" 2>/dev/null | grep -q "^${container_name}$"; then
        echo -e "\n${RED}❌ Container $container_name is not running${NC}"
        echo -e "${YELLOW}📋 Container status:${NC}"
        docker ps -a --filter "name=$container_name" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" 2>/dev/null
        echo ""
        echo -e "${YELLOW}📋 Last 20 lines of logs:${NC}"
        docker logs --tail 20 "$container_name" 2>&1 || echo "Could not retrieve logs"
        return 1
    fi
    
    # Wait for Docker's native healthcheck to pass (if configured)
    while [ $attempt -lt $max_attempts ]; do
        health_status=$(docker inspect --format='{{.State.Health.Status}}' "$container_name" 2>/dev/null)
        
        # If container has healthcheck, wait for it
        if [ -n "$health_status" ] && [ "$health_status" != "<no value>" ]; then
            if [ "$health_status" = "healthy" ]; then
                # Container is healthy according to Docker, now verify with our check
                if eval "$check_command" 2>/dev/null; then
                    echo -e "\n${GREEN}✅ $service_name is ready${NC}"
                    return 0
                fi
                # If our check fails but Docker says healthy, give it a bit more time
                sleep 2
            fi
        else
            # No Docker healthcheck, rely on our check command
            if eval "$check_command" 2>/dev/null; then
                echo -e "\n${GREEN}✅ $service_name is ready${NC}"
                return 0
            fi
        fi
        
        attempt=$((attempt + 1))
        sleep 1
        echo -n "."
    done
    
    echo -e "\n${RED}❌ $service_name failed to become healthy after ${max_attempts}s${NC}"
    echo -e "${YELLOW}📋 Container status:${NC}"
    docker ps -a --filter "name=$container_name" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" 2>/dev/null
    echo ""
    
    # Show health status if available
    health_status=$(docker inspect --format='{{.State.Health.Status}}' "$container_name" 2>/dev/null)
    if [ -n "$health_status" ] && [ "$health_status" != "<no value>" ]; then
        echo -e "${YELLOW}📋 Docker Health Status: $health_status${NC}"
    fi
    
    echo -e "${YELLOW}📋 Last 30 lines of logs:${NC}"
    docker logs --tail 30 "$container_name" 2>&1 || echo "Could not retrieve logs"
    echo ""
    echo -e "${YELLOW}💡 Try manually: curl -v http://localhost:2113/health/live${NC}"
    echo -e "${YELLOW}💡 Or follow logs: docker logs -f $container_name${NC}"
    return 1
}

echo -n "Waiting for EventStoreDB"
if ! check_service "EventStoreDB" "cap-bsp-dsp-eventstoredb" "curl -f -s --max-time 5 http://localhost:2113/health/live"; then
    exit 1
fi

echo -n "Waiting for MongoDB"
if ! check_service "MongoDB" "cap-bsp-dsp-mongodb" "docker exec cap-bsp-dsp-mongodb mongosh --quiet --eval 'db.adminCommand(\"ping\")'"; then
    exit 1
fi

echo -n "Waiting for RabbitMQ"
if ! check_service "RabbitMQ" "cap-bsp-dsp-rabbitmq" "curl -f -s --max-time 5 -u guest:guest http://localhost:15672/api/overview"; then
    exit 1
fi

echo ""
echo -e "${GREEN}✅ All infrastructure services are healthy!${NC}"
echo ""

# Step 2: Build the solution
echo -e "${BLUE}🔨 Building .NET solution...${NC}"
cd "$PROJECT_ROOT"
dotnet build --nologo --verbosity quiet

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Build successful${NC}"
else
    echo -e "${RED}❌ Build failed${NC}"
    exit 1
fi
echo ""

# Step 3: Display service URLs
echo -e "${BLUE}🌐 Infrastructure Services:${NC}"
echo -e "   ${GREEN}EventStoreDB UI:${NC}      http://localhost:2113"
echo -e "   ${GREEN}MongoDB:${NC}              mongodb://admin:admin123@localhost:27017"
echo -e "   ${GREEN}RabbitMQ Management:${NC}  http://localhost:15672 (guest/guest)"
echo -e "   ${GREEN}Jaeger UI:${NC}            http://localhost:16686"
echo -e "   ${GREEN}Prometheus:${NC}           http://localhost:9090"
echo ""

# Step 4: Start Azure Functions
echo -e "${BLUE}🚀 Starting Azure Functions...${NC}"
echo -e "${YELLOW}   API will be available at: http://localhost:7071${NC}"
echo ""
echo -e "${YELLOW}📝 Available endpoints:${NC}"
echo -e "   POST   http://localhost:7071/api/v1/declarations"
echo -e "   GET    http://localhost:7071/api/v1/declarations"
echo -e "   GET    http://localhost:7071/api/v1/declarations/{id}"
echo ""
echo -e "${YELLOW}💡 Press Ctrl+C to stop the Functions host${NC}"
echo -e "${YELLOW}💡 To stop infrastructure: cd docker && docker-compose down${NC}"
echo ""
echo -e "${GREEN}════════════════════════════════════════════════════════════════${NC}"
echo ""

cd "$PROJECT_ROOT/src/CAP.BSP.DSP.Functions"
func start
