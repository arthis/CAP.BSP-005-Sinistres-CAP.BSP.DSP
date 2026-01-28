#!/bin/bash

# Example API calls to test the Claims Declaration Service
# Make sure the service is running (./quickstart.sh) before running this script

BASE_URL="http://localhost:7071/api/v1"
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${BLUE}╔════════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║  CAP.BSP.DSP - API Example Calls                              ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════════════╝${NC}"
echo ""

# Generate correlation ID
CORRELATION_ID=$(uuidgen 2>/dev/null || cat /proc/sys/kernel/random/uuid)

echo -e "${YELLOW}1️⃣  POST /api/v1/declarations - Submit new claim${NC}"
echo ""
echo "Request:"
echo '{
  "identifiantContrat": "CTR-2026-001234",
  "dateSurvenance": "2026-01-20T10:30:00Z"
}'
echo ""

RESPONSE=$(curl -s -X POST "$BASE_URL/declarations" \
  -H "Content-Type: application/json" \
  -H "X-User-ID: user@example.com" \
  -H "X-Correlation-ID: $CORRELATION_ID" \
  -d '{
    "identifiantContrat": "CTR-2026-001234",
    "dateSurvenance": "2026-01-20T10:30:00Z"
  }')

echo -e "${GREEN}Response:${NC}"
echo "$RESPONSE" | jq . 2>/dev/null || echo "$RESPONSE"
echo ""

# Extract identifiantSinistre from response
IDENTIFIANT_SINISTRE=$(echo "$RESPONSE" | jq -r '.identifiantSinistre' 2>/dev/null)

if [ "$IDENTIFIANT_SINISTRE" != "null" ] && [ -n "$IDENTIFIANT_SINISTRE" ]; then
    echo -e "${GREEN}✅ Claim created successfully: $IDENTIFIANT_SINISTRE${NC}"
    echo ""
    
    # Wait a bit for eventual consistency
    sleep 2
    
    echo -e "${YELLOW}2️⃣  GET /api/v1/declarations/{id} - Get claim details${NC}"
    echo ""
    
    RESPONSE=$(curl -s "$BASE_URL/declarations/$IDENTIFIANT_SINISTRE")
    echo -e "${GREEN}Response:${NC}"
    echo "$RESPONSE" | jq . 2>/dev/null || echo "$RESPONSE"
    echo ""
else
    echo -e "${YELLOW}⚠️  Could not extract claim ID, skipping detail lookup${NC}"
    echo ""
fi

echo -e "${YELLOW}3️⃣  GET /api/v1/declarations - Search all declarations${NC}"
echo ""

RESPONSE=$(curl -s "$BASE_URL/declarations?limit=10&offset=0")
echo -e "${GREEN}Response:${NC}"
echo "$RESPONSE" | jq . 2>/dev/null || echo "$RESPONSE"
echo ""

echo -e "${YELLOW}4️⃣  GET /api/v1/declarations - Filter by contract${NC}"
echo ""

RESPONSE=$(curl -s "$BASE_URL/declarations?identifiantContrat=CTR-2026-001234&limit=10")
echo -e "${GREEN}Response:${NC}"
echo "$RESPONSE" | jq . 2>/dev/null || echo "$RESPONSE"
echo ""

echo -e "${BLUE}════════════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}✅ All API tests completed!${NC}"
echo ""
echo -e "💡 Tips:"
echo -e "   - View events in EventStoreDB: http://localhost:2113"
echo -e "   - View messages in RabbitMQ: http://localhost:15672 (guest/guest)"
echo -e "   - View read models: Connect MongoDB Compass to mongodb://admin:admin123@localhost:27017"
echo ""
