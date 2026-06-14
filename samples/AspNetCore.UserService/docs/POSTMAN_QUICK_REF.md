# Postman Quick Reference

## 📦 Import These Files

**Drag and drop into Postman:**

```
✅ AspNetCore.UserService.postman_collection.json (Collection)
✅ Postman-Environment-Local-HTTP.json (Environment)
✅ Postman-Environment-Local-HTTPS.json (Environment)
```

**📖 Detailed import instructions:** See `POSTMAN_IMPORT_GUIDE.md`

## 🔄 Select Environment

**Top-right dropdown in Postman:**

```
Running dotnet run?     → Select "Local HTTP (Command Line)"
Running Docker?         → Select "Local HTTP (Command Line)"
Running Visual Studio?  → Select "Local HTTPS (Visual Studio)"
```

## ⚡ Quick Test

1. Select environment (top-right)
2. Send "Health Check" request
3. Should get: `Healthy`

## 🎯 Port Reference

| Environment | URL | When to Use |
|------------|-----|-------------|
| Local HTTP (Command Line) | `http://localhost:8080` | CLI, Docker |
| Local HTTPS (Visual Studio) | `https://localhost:64846` | VS F5 |

## 🔧 SSL Certificate Error?

**For HTTPS environment only:**
1. Postman **Settings** (gear icon)
2. Turn OFF "SSL certificate verification"
3. Save

## 📖 Full Guide

See `POSTMAN_GUIDE.md` for detailed instructions.
