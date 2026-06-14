# Diagnostic Script for AspNetCore.UserService
# Run this to diagnose startup issues

Write-Host "🔍 AspNetCore.UserService Diagnostic Tool" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Check 1: Port Availability
Write-Host "1️⃣  Checking if ports are available..." -ForegroundColor Yellow

$ports = @(64846, 64847, 8080)
foreach ($port in $ports) {
    $connection = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
    if ($connection) {
        $process = Get-Process -Id $connection.OwningProcess -ErrorAction SilentlyContinue
        Write-Host "   ❌ Port $port is IN USE by: $($process.ProcessName) (PID: $($process.Id))" -ForegroundColor Red
        Write-Host "      To free it, run: Stop-Process -Id $($process.Id) -Force" -ForegroundColor Gray
    } else {
        Write-Host "   ✅ Port $port is available" -ForegroundColor Green
    }
}
Write-Host ""

# Check 2: .NET SDK
Write-Host "2️⃣  Checking .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "   ✅ .NET SDK installed: $dotnetVersion" -ForegroundColor Green
    
    $sdks = dotnet --list-sdks | Select-String "10\."
    if ($sdks) {
        Write-Host "   ✅ .NET 10 SDK found" -ForegroundColor Green
    } else {
        Write-Host "   ⚠️  .NET 10 SDK not found - this project requires .NET 10" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ❌ .NET SDK not found or not in PATH" -ForegroundColor Red
}
Write-Host ""

# Check 3: Project File
Write-Host "3️⃣  Checking project files..." -ForegroundColor Yellow
$projectPath = "AspNetCore.UserService.csproj"
if (Test-Path $projectPath) {
    Write-Host "   ✅ Project file found: $projectPath" -ForegroundColor Green
} else {
    Write-Host "   ❌ Project file not found. Are you in the correct directory?" -ForegroundColor Red
    Write-Host "      Current directory: $(Get-Location)" -ForegroundColor Gray
}

$programPath = "Program.cs"
if (Test-Path $programPath) {
    Write-Host "   ✅ Program.cs found" -ForegroundColor Green
} else {
    Write-Host "   ❌ Program.cs not found" -ForegroundColor Red
}

$launchSettingsPath = "Properties\launchSettings.json"
if (Test-Path $launchSettingsPath) {
    Write-Host "   ✅ launchSettings.json found" -ForegroundColor Green
    $launchSettings = Get-Content $launchSettingsPath -Raw | ConvertFrom-Json
    $appUrl = $launchSettings.profiles.'AspNetCore.UserService'.applicationUrl
    Write-Host "      Configured URLs: $appUrl" -ForegroundColor Gray
} else {
    Write-Host "   ⚠️  launchSettings.json not found" -ForegroundColor Yellow
}
Write-Host ""

# Check 4: HTTPS Certificate
Write-Host "4️⃣  Checking HTTPS development certificate..." -ForegroundColor Yellow
try {
    $certCheck = dotnet dev-certs https --check 2>&1
    if ($certCheck -like "*valid certificate*" -or $certCheck -like "*trusted*") {
        Write-Host "   ✅ HTTPS certificate is trusted" -ForegroundColor Green
    } else {
        Write-Host "   ⚠️  HTTPS certificate may not be trusted" -ForegroundColor Yellow
        Write-Host "      Run: dotnet dev-certs https --trust" -ForegroundColor Gray
    }
} catch {
    Write-Host "   ⚠️  Could not verify HTTPS certificate" -ForegroundColor Yellow
}
Write-Host ""

# Check 5: Build Status
Write-Host "5️⃣  Attempting to build project..." -ForegroundColor Yellow
try {
    $buildOutput = dotnet build --no-restore 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✅ Project builds successfully" -ForegroundColor Green
    } else {
        Write-Host "   ❌ Project has build errors:" -ForegroundColor Red
        Write-Host $buildOutput -ForegroundColor Gray
    }
} catch {
    Write-Host "   ❌ Build failed: $_" -ForegroundColor Red
}
Write-Host ""

# Check 6: Firewall
Write-Host "6️⃣  Checking Windows Firewall..." -ForegroundColor Yellow
if (Get-Command Get-NetFirewallRule -ErrorAction SilentlyContinue) {
    $firewallRules = Get-NetFirewallRule | Where-Object { 
        $_.DisplayName -like "*AspNetCore*" -or $_.DisplayName -like "*dotnet*" 
    }
    if ($firewallRules) {
        Write-Host "   ✅ Found firewall rules for .NET applications" -ForegroundColor Green
    } else {
        Write-Host "   ⚠️  No firewall rules found - you may need to allow access when prompted" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ⚠️  Cannot check firewall (requires admin)" -ForegroundColor Yellow
}
Write-Host ""

# Summary
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "📋 Diagnostic Summary" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Fix any ❌ red issues above" -ForegroundColor White
Write-Host "2. Press F5 in Visual Studio to run the app" -ForegroundColor White
Write-Host "3. Watch the Output window for startup messages" -ForegroundColor White
Write-Host "4. Look for: 'Now listening on: https://localhost:64846'" -ForegroundColor White
Write-Host "5. Test with: Invoke-RestMethod https://localhost:64846/ -SkipCertificateCheck" -ForegroundColor White
Write-Host ""
Write-Host "If issues persist, check STARTUP_TROUBLESHOOTING.md for detailed help" -ForegroundColor Gray
Write-Host ""
