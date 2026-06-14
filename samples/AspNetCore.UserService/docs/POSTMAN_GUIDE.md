# Postman Configuration Guide - AspNetCore.UserService

## 🚨 Common Error: SSL WRONG_VERSION_NUMBER

If you see this error in Postman:
```
Error: 41283392:error:100000f7:SSL routines:OPENSSL_internal:WRONG_VERSION_NUMBER
```

**Cause:** You're trying to use `https://` on a port configured for HTTP only.

**Solution:** Use the correct Postman environment (see below).

---

## 📦 Postman Setup

We provide **1 collection** and **2 environments** (the proper Postman way):

### Files to Import:

1. **Collection:** `AspNetCore.UserService.postman_collection.json`
2. **Environment (HTTP):** `Postman-Environment-Local-HTTP.json`
3. **Environment (HTTPS):** `Postman-Environment-Local-HTTPS.json`

---

## 🎯 Which Environment to Use?

| How You're Running the App | Environment to Select | Base URL |
|----------------------------|----------------------|----------|
| **Command Line (`dotnet run`)** | Local HTTP (Command Line) | `http://localhost:8080` |
| **Docker** | Local HTTP (Command Line) | `http://localhost:8080` |
| **Visual Studio (F5)** | Local HTTPS (Visual Studio) | `https://localhost:64846` |

---

## 🔧 Setup Instructions

### Step 1: Import the Collection

1. Open Postman
2. Click **Import** button
3. Drag and drop `AspNetCore.UserService.postman_collection.json`
4. Collection appears in left sidebar

### Step 2: Import the Environments

1. Click **Environments** (left sidebar)
2. Click **Import**
3. Drag and drop both environment files:
   - `Postman-Environment-Local-HTTP.json`
   - `Postman-Environment-Local-HTTPS.json`

### Step 3: Select the Right Environment

1. Look at top-right corner of Postman
2. Click environment dropdown (says "No Environment")
3. Select:
   - **"Local HTTP (Command Line)"** if using `dotnet run` or Docker
   - **"Local HTTPS (Visual Studio)"** if using Visual Studio F5

### Step 4: Disable SSL Verification (for HTTPS only)

If using `https://localhost:64846` (Visual Studio):

1. Click **Settings** (gear icon in Postman)
2. Go to **General** tab
3. Turn **OFF**: "SSL certificate verification"
4. Click **Save**

This is safe for local development with self-signed certificates.

### Step 5: Run the Collection

1. Right-click the collection name
2. Select **Run collection**
3. Make sure correct environment is selected at top
4. Click **Run AspNetCore.UserService API**
5. Watch the tests execute

---

## 🧪 Testing Individual Requests

### Order of Execution:

1. **Service Info** - Verify app is running
2. **Health Check** - Check app health
3. **OpenAPI Specification** - View API schema (dev mode only)
4. **Create User** - Creates a user and saves ID to variable
5. **Get User by ID** - Uses saved ID from previous step

### Auto-Save User ID

The "Create User" request has a **test script** that automatically saves the created user's ID to a Postman variable:

```javascript
if (pm.response.code === 200 || pm.response.code === 201) {
    var jsonData = pm.response.json();
    pm.environment.set('userId', jsonData.id);
}
```

This means "Get User by ID" will automatically use the correct ID!

---

## 🔄 Switching Between Environments

To switch between HTTP and HTTPS:

1. Click environment dropdown (top-right)
2. Select different environment
3. All requests automatically use new URL

**Example:**
- Running from command line? → Select "Local HTTP (Command Line)"
- Switched to Visual Studio? → Select "Local HTTPS (Visual Studio)"

---

## 🎨 Custom Environments for Different Deployments

You can create additional environments for other scenarios:

### Docker Environment (if using different port)
```json
{
  "name": "Docker",
  "values": [
    {
      "key": "baseUrl",
      "value": "http://localhost:8080"
    }
  ]
}
```

### Azure Container Apps (Production)
```json
{
  "name": "Production",
  "values": [
    {
      "key": "baseUrl",
      "value": "https://my-app.azurecontainerapps.io"
    }
  ]
}
```

**To create custom environment:**
1. Click **Environments** → **+** button
2. Name it
3. Add `baseUrl` variable with your URL
4. Save

---

## 🐛 Troubleshooting

### Error: "Could not get response"

**Cause:** App isn't running or wrong port

**Solution:**
1. Verify app is running (check console output)
2. Check port in Postman matches app's port
3. Try `http://` instead of `https://` or vice versa

---

### Error: "SSL certificate problem"

**Cause:** Self-signed certificate not trusted

**Solution:**
1. **Option A:** Disable SSL verification in Postman Settings
2. **Option B:** Trust the dev certificate:
   ```powershell
   dotnet dev-certs https --trust
   ```
3. **Option C:** Use HTTP port instead: `http://localhost:64847`

---

### Error: "ECONNREFUSED"

**Cause:** App isn't listening on that port

**Solution:**
1. Check app console output for "Now listening on: ..."
2. Use that exact URL in Postman
3. Verify firewall isn't blocking the port

---

### Request Returns 404

**Cause:** Endpoint path is incorrect

**Solution:**
1. Check app console for registered routes
2. Verify OpenAPI spec: `GET /openapi/v1.json`
3. Ensure path matches exactly (case-sensitive)

---

## 📊 Expected Responses

### ✅ Service Info (200 OK)
```json
{
  "service": "User Service",
  "version": "1.0.0",
  "status": "running"
}
```

### ✅ Health Check (200 OK)
```
Healthy
```

### ✅ Create User (200 OK)
```json
{
  "id": "a1b2c3d4-e5f6-4g7h-8i9j-0k1l2m3n4o5p",
  "email": "john.doe@example.com",
  "name": "John Doe",
  "createdAt": "2026-06-13T10:30:00Z",
  "isActive": true
}
```

### ✅ Get User (200 OK)
```json
{
  "id": "a1b2c3d4-e5f6-4g7h-8i9j-0k1l2m3n4o5p",
  "email": "john.doe@example.com",
  "name": "John Doe",
  "createdAt": "2026-06-13T10:30:00Z",
  "isActive": true
}
```

---

## 🎯 Pro Tips

### Tip 1: Use Collection Runner for Automated Testing
1. Click **Runner** button
2. Select collection
3. Set iterations (e.g., 10)
4. Click **Run**
5. View pass/fail statistics

### Tip 2: Export Results
After running collection:
1. Click **Export Results**
2. Save as JSON for CI/CD integration

### Tip 3: Add Pre-Request Scripts
Add authentication, timestamps, or dynamic data:
```javascript
// Pre-request Script example
pm.variables.set("timestamp", Date.now());
pm.variables.set("random_email", `user${Date.now()}@example.com`);
```

### Tip 4: Chain Requests
Use test scripts to save data for subsequent requests:
```javascript
// In "Create User" test script
pm.test("Save user ID", function() {
    var jsonData = pm.response.json();
    pm.environment.set("userId", jsonData.id);
    pm.environment.set("userEmail", jsonData.email);
});
```

---

## 🔗 Quick Reference

| Scenario | Protocol | Port | Collection |
|----------|----------|------|------------|
| Visual Studio F5 | HTTPS | 64846 | VisualStudio.postman_collection.json |
| Visual Studio F5 (HTTP fallback) | HTTP | 64847 | VisualStudio.postman_collection.json (edit baseUrl) |
| dotnet run | HTTP | 8080 | AspNetCore.UserService.postman_collection.json |
| Docker | HTTP | 8080 | AspNetCore.UserService.postman_collection.json |

---

## 📚 Related Files

- `test-api.ps1` - PowerShell script for automated testing
- `test-requests.http` - VS Code REST Client format
- `API_TESTING_GUIDE.md` - Detailed API testing documentation
- `STARTUP_TROUBLESHOOTING.md` - App startup issues

---

## 💡 Need Help?

If you're still having issues:

1. Run the diagnostic script: `.\diagnose.ps1`
2. Check `STARTUP_TROUBLESHOOTING.md`
3. Verify app is running and check console output
4. Try the simpler PowerShell script: `.\test-api.ps1`

