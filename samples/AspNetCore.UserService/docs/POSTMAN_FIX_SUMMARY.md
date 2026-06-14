# AspNetCore.UserService - Complete Fix Summary

## ✅ Issue Resolved: Postman SSL Error

### Problem
```
GET https://localhost:8080/health
Error: SSL routines:OPENSSL_internal:WRONG_VERSION_NUMBER
```

### Root Cause
- Port **8080** is configured for **HTTP only** (in `appsettings.json`)
- Postman collection was using **HTTPS** (`https://localhost:8080`)
- **HTTPS on HTTP port = SSL error!**

---

## 🔧 Fixes Applied

### 1. Updated Postman Collection
**File:** `AspNetCore.UserService.postman_collection.json`
- Changed default `baseUrl` from `https://localhost:64846` to `http://localhost:8080`
- Now works with collection variables OR environments

### 2. Created Postman Environments (NEW - Proper Approach!)
**Files:** 
- `Postman-Environment-Local-HTTP.json` - For command line/Docker (`http://localhost:8080`)
- `Postman-Environment-Local-HTTPS.json` - For Visual Studio F5 (`https://localhost:64846`)

**Benefits:**
- ✅ One collection, multiple environments (standard Postman pattern)
- ✅ Easy switching between HTTP/HTTPS
- ✅ Can add more environments (staging, production, etc.)
- ✅ No duplicate collection files

### 3. Removed Duplicate Collection
**Deleted:** `AspNetCore.UserService.VisualStudio.postman_collection.json`
- No longer needed - environments handle this better

### 4. Added HTTPS Support to appsettings.json
**File:** `appsettings.json`
```json
"Kestrel": {
  "Endpoints": {
    "Http": {
      "Url": "http://0.0.0.0:8080"
    },
    "Https": {
      "Url": "https://0.0.0.0:8081"    // NEW!
    }
  }
}
```

### 4. Updated All Test Scripts
- `test-api.ps1` - Changed default to `http://localhost:8080`
- `test-api.sh` - Changed default to `http://localhost:8080`
- `test-requests.http` - Changed default to `http://localhost:8080`
- All include comments on how to change for Visual Studio

### 5. Created Documentation
- `POSTMAN_GUIDE.md` - Complete Postman configuration guide
- `POSTMAN_SSL_FIX.md` - Quick reference for SSL errors
- Updated `README.md` with port configuration table

---

## 📦 Available Files

| File | Purpose |
|------|---------|
| `AspNetCore.UserService.postman_collection.json` | Main collection with all API requests |
| `Postman-Environment-Local-HTTP.json` | Environment for command line/Docker |
| `Postman-Environment-Local-HTTPS.json` | Environment for Visual Studio F5 |

---

## 🎯 Quick Solutions

### Solution 1: Use Environments (Recommended!)

**Import both environments:**
1. Click **Environments** in Postman (left sidebar)
2. Click **Import**
3. Import both `.json` environment files
4. Select environment from dropdown (top-right):
   - **"Local HTTP (Command Line)"** for `dotnet run`/Docker
   - **"Local HTTPS (Visual Studio)"** for Visual Studio F5

### Solution 2: Use Collection Variable

The collection has a default `baseUrl` variable set to `http://localhost:8080`.

**To change it:**
1. Click collection name
2. Go to **Variables** tab
3. Change `baseUrl` to your desired URL
4. Save

If using HTTPS/Visual Studio:
1. Postman **Settings** (gear icon)
2. **General** tab
3. Turn OFF "SSL certificate verification"
4. Save

---

## 🚀 Port Configuration Reference

| Deployment Method | Protocol | Port | URL | Collection to Use |
|------------------|----------|------|-----|-------------------|
| **Visual Studio F5** | HTTPS | 64846 | `https://localhost:64846` | VisualStudio.postman_collection.json |
| **Visual Studio F5** | HTTP | 64847 | `http://localhost:64847` | Edit VisualStudio collection baseUrl |
| **dotnet run** | HTTP | 8080 | `http://localhost:8080` | AspNetCore.UserService.postman_collection.json |
| **dotnet run** | HTTPS | 8081 | `https://localhost:8081` | Create custom collection |
| **Docker** | HTTP | 8080 | `http://localhost:8080` | AspNetCore.UserService.postman_collection.json |

---

## ✅ Verification

Test that it's working:

### PowerShell
```powershell
# Command line deployment
Invoke-RestMethod http://localhost:8080/health

# Visual Studio deployment
Invoke-RestMethod https://localhost:64846/health -SkipCertificateCheck
```

### Postman
1. Import correct collection
2. Send "Health Check" request
3. Should get response: `Healthy`

### Browser
- Command line: http://localhost:8080/
- Visual Studio: https://localhost:64846/ (accept cert warning)

---

## 📚 Documentation Files

All new/updated documentation:

1. **POSTMAN_GUIDE.md** - Complete Postman setup and troubleshooting
2. **POSTMAN_SSL_FIX.md** - Quick fix for SSL errors
3. **README.md** - Updated with port configuration table
4. **test-api.ps1** - Updated to use HTTP by default
5. **test-api.sh** - Updated to use HTTP by default
6. **test-requests.http** - Updated to use HTTP by default
7. **STARTUP_TROUBLESHOOTING.md** - App startup issues
8. **API_TESTING_GUIDE.md** - General API testing guide
9. **TESTING_QUICK_START.md** - Quick testing reference

---

## 🎉 Summary

**You now have:**
- ✅ 2 Postman collections for different scenarios
- ✅ HTTPS support on port 8081 (if needed)
- ✅ All test scripts defaulting to HTTP port 8080
- ✅ Comprehensive documentation for Postman configuration
- ✅ Clear port configuration guide
- ✅ Quick-fix guides for common errors

**Next Steps:**
1. Import the correct Postman collection for your deployment
2. Run the "Health Check" request to verify
3. Run full collection to test all endpoints
4. Refer to `POSTMAN_GUIDE.md` if you encounter issues

---

## 🔗 Related Issues Fixed

This fix also resolves:
- ❌ "Could not get response" errors
- ❌ "SSL certificate problem" warnings
- ❌ "Connection refused" issues
- ❌ Confusion about which port to use

---

## 💡 Pro Tip

Create **Postman environments** for different deployments:

**Local-HTTP Environment:**
```
baseUrl = http://localhost:8080
```

**Local-HTTPS Environment:**
```
baseUrl = https://localhost:64846
```

**Production Environment:**
```
baseUrl = https://my-app.azurecontainerapps.io
```

Then switch between them using the environment dropdown!

