# AspNetCore.UserService API Test Script
# Run this script to test all API endpoints

$baseUrl = "https://localhost:64846"

Write-Host "🧪 Testing AspNetCore.UserService API" -ForegroundColor Cyan
Write-Host "Base URL: $baseUrl" -ForegroundColor Yellow
Write-Host ""

# Test 1: Service Info
Write-Host "1️⃣  Testing GET / - Service Info" -ForegroundColor Green
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/" -Method Get -SkipCertificateCheck
    Write-Host "✅ Success:" -ForegroundColor Green
    $response | ConvertTo-Json
} catch {
    Write-Host "❌ Error: $_" -ForegroundColor Red
}
Write-Host ""

# Test 2: Health Check
Write-Host "2️⃣  Testing GET /health - Health Check" -ForegroundColor Green
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/health" -Method Get -SkipCertificateCheck
    Write-Host "✅ Success: $response" -ForegroundColor Green
} catch {
    Write-Host "❌ Error: $_" -ForegroundColor Red
}
Write-Host ""

# Test 3: OpenAPI Specification
Write-Host "3️⃣  Testing GET /openapi/v1.json - OpenAPI Spec" -ForegroundColor Green
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/openapi/v1.json" -Method Get -SkipCertificateCheck
    Write-Host "✅ Success: OpenAPI spec retrieved (version: $($response.openapi))" -ForegroundColor Green
} catch {
    Write-Host "❌ Error: $_" -ForegroundColor Red
}
Write-Host ""

# Test 4: Create User
Write-Host "4️⃣  Testing POST /api/users - Create User" -ForegroundColor Green
$newUser = @{
    email = "john.doe@example.com"
    name = "John Doe"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/users" `
        -Method Post `
        -ContentType "application/json" `
        -Body $newUser `
        -SkipCertificateCheck
    
    Write-Host "✅ Success: User created" -ForegroundColor Green
    $response | ConvertTo-Json
    $userId = $response.id
} catch {
    Write-Host "❌ Error: $_" -ForegroundColor Red
    $userId = "123" # Use a test ID for next step
}
Write-Host ""

# Test 5: Get User by ID
Write-Host "5️⃣  Testing GET /api/users/{userId} - Get User" -ForegroundColor Green
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/users/$userId" -Method Get -SkipCertificateCheck
    Write-Host "✅ Success: User retrieved" -ForegroundColor Green
    $response | ConvertTo-Json
} catch {
    Write-Host "❌ Error: $_" -ForegroundColor Red
}
Write-Host ""

Write-Host "🎉 Testing Complete!" -ForegroundColor Cyan
