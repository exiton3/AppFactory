# AspNetCore.UserService - Startup Troubleshooting Guide

## Issue: Can't Access https://localhost:64846

### Step 1: Check Visual Studio Output Window

1. In Visual Studio, go to **View → Output**
2. Select **"Show output from: Debug"**
3. Look for any errors when the app starts

Common errors to look for:
- ❌ `System.InvalidOperationException` - Missing service registration
- ❌ `System.IO.IOException` - Port already in use
- ❌ `Microsoft.AspNetCore.Server.Kestrel` errors - Kestrel configuration issue

---

### Step 2: Verify Application Started

Look for these SUCCESS indicators in the Output window:

✅ **Application started successfully**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:64846
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:64847
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

❌ **If you DON'T see these messages**, the app crashed during startup.

---

### Step 3: Check Console Window

When running from Visual Studio (F5), a console window should appear showing:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:64846
```

❌ **If the console window closes immediately**, there's a startup error.

---

### Step 4: Common Fixes

#### Fix 1: Port Conflict
If port 64846 is already in use:

**Option A - Change Ports in launchSettings.json:**
```json
{
  "profiles": {
    "AspNetCore.UserService": {
      "commandName": "Project",
      "launchBrowser": true,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "applicationUrl": "https://localhost:5001;http://localhost:5000"
    }
  }
}
```

**Option B - Find and kill the process using the port:**
```powershell
# PowerShell - Run as Admin
Get-Process -Id (Get-NetTCPConnection -LocalPort 64846).OwningProcess | Stop-Process -Force
```

#### Fix 2: Missing Service Registration

If you see errors about missing services, check these are registered in **ServiceCollectionExtensions.cs**:

```csharp
services.AddSingleton<IJsonSerializer, DefaultJsonSerializer>();
services.AddSingleton<IPropertyMapperRegistry, PropertyMapperRegistry>();
services.AddSingleton<IParseModelMapRegistry>(sp => 
{
    var maps = sp.GetServices<IParseModelMap>();
    return new ParseModelMapRegistry(maps);
});
services.AddSingleton<IRequestParser, RequestParser>();
```

#### Fix 3: HTTPS Certificate Issue

If HTTPS isn't working, trust the dev certificate:
```powershell
dotnet dev-certs https --trust
```

Then restart Visual Studio.

#### Fix 4: Firewall Blocking

Windows Defender might block the app. When prompted, click **"Allow access"**.

Or manually add exception:
```powershell
# PowerShell - Run as Admin
New-NetFirewallRule -DisplayName "AspNetCore.UserService" -Direction Inbound -LocalPort 64846,64847 -Protocol TCP -Action Allow
```

---

### Step 5: Alternative Testing Methods

If HTTPS still doesn't work, try HTTP:

1. **Use HTTP port instead:**
   - Try: http://localhost:64847/
   
2. **Change to use appsettings.json port:**
   - Comment out `applicationUrl` in launchSettings.json
   - App will use port from appsettings.json: http://localhost:8080/

3. **Run from command line:**
   ```powershell
   cd samples\AspNetCore.UserService
   dotnet run --urls "http://localhost:5000"
   ```
   Then access: http://localhost:5000/

---

### Step 6: Enable Detailed Logging

Add to **Program.cs** at the very top:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add this for detailed startup logging
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Rest of your code...
```

---

### Step 7: Check These Files

Ensure these files are correctly configured:

**launchSettings.json:**
```json
{
  "profiles": {
    "AspNetCore.UserService": {
      "commandName": "Project",
      "launchBrowser": true,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "applicationUrl": "https://localhost:64846;http://localhost:64847"
    }
  }
}
```

**appsettings.json:**
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:8080"
      }
    }
  }
}
```

NOTE: launchSettings.json takes precedence when running from Visual Studio.

---

### Quick Test Commands

Once the app is running, test with:

**PowerShell:**
```powershell
# Test HTTPS (skip cert check)
Invoke-RestMethod https://localhost:64846/ -SkipCertificateCheck

# Test HTTP
Invoke-RestMethod http://localhost:64847/

# Test appsettings.json port
Invoke-RestMethod http://localhost:8080/
```

**Browser:**
- https://localhost:64846/ (may show cert warning - click "Advanced" → "Proceed")
- http://localhost:64847/

---

## What to Report

If still not working, provide:

1. **Full Output Window content** (View → Output → Debug)
2. **Console window output** (black window that appears)
3. **Any error dialogs** from Visual Studio
4. **Result of:** `netstat -ano | findstr "64846"`

---

## Expected Working State

When working correctly, you should see:

**Console Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:64846
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:64847
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

**Browser at https://localhost:64846/:**
```json
{
  "service": "User Service",
  "version": "1.0.0",
  "status": "running"
}
```

