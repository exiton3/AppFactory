# Quick Fix: Postman SSL Error

## ❌ Error You're Seeing
```
Error: SSL routines:OPENSSL_internal:WRONG_VERSION_NUMBER
```

## ✅ Solution

You're using **HTTPS** on a port that only supports **HTTP**.

### Fix 1: Select Correct Environment

**In Postman:**
1. Look at top-right corner
2. Click environment dropdown
3. Select the right environment:
   - **"Local HTTP (Command Line)"** for `dotnet run` or Docker
   - **"Local HTTPS (Visual Studio)"** for Visual Studio F5

### Fix 2: Import Both Environments

If you don't see the environments:

1. Click **Environments** (left sidebar)
2. Click **Import**
3. Import both files:
   - `Postman-Environment-Local-HTTP.json`
   - `Postman-Environment-Local-HTTPS.json`
4. Select the correct one from dropdown (top-right)

### Fix 3: Manually Change URL

Change your request URL from:
```
https://localhost:8080/health
```

To:
```
http://localhost:8080/health
```

---

## 🎯 Port Reference

| How You Run App | Protocol | Port | URL |
|-----------------|----------|------|-----|
| **Visual Studio F5** | HTTPS | 64846 | `https://localhost:64846` |
| **Visual Studio F5** | HTTP | 64847 | `http://localhost:64847` |
| **dotnet run** | HTTP | 8080 | `http://localhost:8080` |
| **dotnet run with appsettings** | HTTPS | 8081 | `https://localhost:8081` |

---

## ⚡ Quick Test

Run this in PowerShell to verify:

```powershell
# Test HTTP (should work)
Invoke-RestMethod http://localhost:8080/health

# Test HTTPS (only if running from Visual Studio)
Invoke-RestMethod https://localhost:64846/health -SkipCertificateCheck
```

---

## 📖 Detailed Help

See `POSTMAN_GUIDE.md` for complete Postman configuration guide.
