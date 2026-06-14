# AspNetCore.UserService - Testing Quick Reference

## 🚀 Start the Application

### From Visual Studio (Recommended)
1. Press **F5**
2. App runs at: `https://localhost:64846` and `http://localhost:64847`

### From Command Line
```powershell
cd samples\AspNetCore.UserService
dotnet run
```
App runs at: `http://localhost:8080`

---

## 🧪 5 Ways to Test

### 1️⃣ PowerShell Script (Windows - Easiest)
```powershell
.\test-api.ps1
```
✅ Tests all endpoints automatically  
✅ Color-coded output  
✅ JSON formatted responses

### 2️⃣ VS Code REST Client (Developer Favorite)
1. Install "REST Client" extension
2. Open `test-requests.http`
3. Click "Send Request" above any request

✅ IntelliSense support  
✅ Response history  
✅ Environment variables

### 3️⃣ Postman/Thunder Client (Team Testing)
1. Import `AspNetCore.UserService.postman_collection.json`
2. Click "Run Collection"

✅ Automated tests  
✅ Share with team  
✅ CI/CD integration

### 4️⃣ Browser (Quick Check)
- Service Info: https://localhost:64846/
- Health: https://localhost:64846/health  
- OpenAPI: https://localhost:64846/openapi/v1.json

✅ No tools needed  
✅ Quick verification

### 5️⃣ Manual curl/PowerShell
```powershell
# Service Info
Invoke-RestMethod https://localhost:64846/ -SkipCertificateCheck

# Create User
$body = @{ email = "test@example.com"; name = "Test User" } | ConvertTo-Json
Invoke-RestMethod https://localhost:64846/api/users -Method Post -Body $body -ContentType "application/json" -SkipCertificateCheck

# Get User
Invoke-RestMethod https://localhost:64846/api/users/{userId} -SkipCertificateCheck
```

---

## 📋 Endpoint Cheatsheet

| What | Method | Endpoint | Body |
|------|--------|----------|------|
| **Service Info** | GET | `/` | - |
| **Health Check** | GET | `/health` | - |
| **OpenAPI Spec** | GET | `/openapi/v1.json` | - |
| **Create User** | POST | `/api/users` | `{"email":"...","name":"..."}` |
| **Get User** | GET | `/api/users/{userId}` | - |

---

## ✅ Expected Responses

### Create User (Success)
```json
{
  "id": "guid-here",
  "email": "john.doe@example.com",
  "name": "John Doe",
  "createdAt": "2026-06-13T10:30:00Z",
  "isActive": true
}
```

### Get User (Success)
```json
{
  "id": "guid-here",
  "email": "john.doe@example.com",
  "name": "John Doe",
  "createdAt": "2026-06-13T10:30:00Z",
  "isActive": true
}
```

### Validation Error (400)
```json
{
  "errors": [
    { "code": "001", "message": "Email is required" }
  ]
}
```

### Not Found (404)
```json
{
  "error": "User not found"
}
```

---

## 🔧 Troubleshooting

**SSL Certificate Error?**
```powershell
# PowerShell
-SkipCertificateCheck

# curl
-k
```

**Port in use?**
- Change in `launchSettings.json` or `appsettings.json`

**Can't connect?**
- Check Windows Defender/Firewall
- Verify app is running (check console output)

---

## 📚 Files

- `test-api.ps1` - PowerShell test script
- `test-api.sh` - Bash test script
- `test-requests.http` - VS Code REST Client format
- `AspNetCore.UserService.postman_collection.json` - Postman collection
- `API_TESTING_GUIDE.md` - Detailed testing guide

---

## 🎯 Recommended Workflow

1. **Start**: Press F5 in Visual Studio
2. **Quick Check**: Open browser → `https://localhost:64846/`
3. **Test All**: Run `.\test-api.ps1`
4. **Develop**: Use `test-requests.http` in VS Code
5. **Share**: Export Postman collection for team

Happy Testing! 🚀
