# API Testing Guide - AspNetCore.UserService

## Quick Start

After starting the application (F5 in Visual Studio), the API will be available at:
- **HTTPS**: https://localhost:64846
- **HTTP**: http://localhost:64847

---

## 📍 Endpoints

### 1. Service Info
**Request:**
```http
GET https://localhost:64846/
```

**Expected Response (200 OK):**
```json
{
  "service": "User Service",
  "version": "1.0.0",
  "status": "running"
}
```

---

### 2. Health Check
**Request:**
```http
GET https://localhost:64846/health
```

**Expected Response (200 OK):**
```
Healthy
```

---

### 3. OpenAPI Specification (Dev Mode Only)
**Request:**
```http
GET https://localhost:64846/openapi/v1.json
```

**Expected Response (200 OK):**
```json
{
  "openapi": "3.0.1",
  "info": {
    "title": "AspNetCore.UserService",
    "version": "1.0.0"
  },
  "paths": {
    "/api/users": { ... },
    "/api/users/{userId}": { ... }
  }
}
```

---

### 4. Create User
**Request:**
```http
POST https://localhost:64846/api/users
Content-Type: application/json

{
  "email": "john.doe@example.com",
  "name": "John Doe"
}
```

**Expected Response (200 OK):**
```json
{
  "id": "a1b2c3d4-e5f6-4g7h-8i9j-0k1l2m3n4o5p",
  "email": "john.doe@example.com",
  "name": "John Doe",
  "createdAt": "2026-06-13T10:30:00Z",
  "isActive": true
}
```

**Validation Errors (400 Bad Request):**
```json
{
  "errors": [
    {
      "code": "001",
      "message": "Email is required"
    }
  ]
}
```

---

### 5. Get User by ID
**Request:**
```http
GET https://localhost:64846/api/users/{userId}
```

**Example:**
```http
GET https://localhost:64846/api/users/a1b2c3d4-e5f6-4g7h-8i9j-0k1l2m3n4o5p
```

**Expected Response (200 OK):**
```json
{
  "id": "a1b2c3d4-e5f6-4g7h-8i9j-0k1l2m3n4o5p",
  "email": "john.doe@example.com",
  "name": "John Doe",
  "createdAt": "2026-06-13T10:30:00Z",
  "isActive": true
}
```

**Not Found (404):**
```json
{
  "error": "User not found"
}
```

---

## 🧪 Testing Tools

### Option 1: PowerShell Script
```powershell
.\test-api.ps1
```

### Option 2: Bash/curl Script
```bash
chmod +x test-api.sh
./test-api.sh
```

### Option 3: Postman/Thunder Client
1. Import `AspNetCore.UserService.postman_collection.json`
2. Run the collection

### Option 4: Browser (for GET endpoints)
- Service Info: https://localhost:64846/
- Health: https://localhost:64846/health
- OpenAPI: https://localhost:64846/openapi/v1.json

### Option 5: Visual Studio Code REST Client Extension
Install the "REST Client" extension and use the requests in `test-requests.http`

---

## 🔧 Troubleshooting

### SSL Certificate Warnings
When using curl or PowerShell with HTTPS, you may need to skip certificate validation:

**PowerShell:**
```powershell
Invoke-RestMethod -Uri "https://localhost:64846/" -SkipCertificateCheck
```

**curl:**
```bash
curl -k https://localhost:64846/
```

### Port Already in Use
If ports 64846/64847 are in use, you can change them in:
- `launchSettings.json` (for Visual Studio)
- `appsettings.json` (for dotnet run)

### CORS Issues (if testing from browser/web app)
CORS is not configured in this sample. To enable:
```csharp
// In Program.cs
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

app.UseCors();
```

---

## 📊 Expected Behavior

### Create User Flow
1. Send POST request with email and name
2. Server validates the request
3. Command handler processes the command
4. User is created (in-memory for this sample)
5. Response includes generated ID and timestamp

### Get User Flow
1. Send GET request with userId in path
2. Server parses the userId from route
3. Query handler retrieves the user
4. Response includes user details or 404 if not found

---

## 🎯 Next Steps

- Add authentication (JWT Bearer tokens)
- Add rate limiting
- Add request validation middleware
- Connect to real database (CosmosDB)
- Add integration tests
- Deploy to Azure Container Apps

