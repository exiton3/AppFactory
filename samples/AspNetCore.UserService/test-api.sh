#!/bin/bash
# AspNetCore.UserService API Test Script (Bash/curl)
# Make executable: chmod +x test-api.sh

# IMPORTANT: Set this based on how you're running the app:
# - Visual Studio (F5): Use https://localhost:64846
# - Command Line (dotnet run): Use http://localhost:8080
# - Docker: Use http://localhost:8080

BASE_URL="http://localhost:8080"  # Change this if using Visual Studio

echo "🧪 Testing AspNetCore.UserService API"
echo "Base URL: $BASE_URL"
echo ""

# Test 1: Service Info
echo "1️⃣  Testing GET / - Service Info"
curl -k -s "$BASE_URL/" | jq '.' || echo "❌ Failed"
echo ""

# Test 2: Health Check
echo "2️⃣  Testing GET /health - Health Check"
curl -k -s "$BASE_URL/health"
echo ""
echo ""

# Test 3: OpenAPI Specification
echo "3️⃣  Testing GET /openapi/v1.json - OpenAPI Spec"
curl -k -s "$BASE_URL/openapi/v1.json" | jq '.openapi' || echo "❌ Failed"
echo ""

# Test 4: Create User
echo "4️⃣  Testing POST /api/users - Create User"
RESPONSE=$(curl -k -s -X POST "$BASE_URL/api/users" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com",
    "name": "John Doe"
  }')

echo "$RESPONSE" | jq '.'
USER_ID=$(echo "$RESPONSE" | jq -r '.id')
echo ""

# Test 5: Get User by ID
echo "5️⃣  Testing GET /api/users/{userId} - Get User"
curl -k -s "$BASE_URL/api/users/$USER_ID" | jq '.' || echo "❌ Failed"
echo ""

echo "🎉 Testing Complete!"
