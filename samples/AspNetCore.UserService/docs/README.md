# AspNetCore.UserService Documentation

## 📚 Documentation Guide

All documentation files are organized in this folder for easy navigation.

---

## 🚀 **Quick Start**

**New to this project?** Start here:

1. **[../README.md](../README.md)** - Main project overview
2. **[TESTING_QUICK_START.md](TESTING_QUICK_START.md)** - Quick testing guide
3. **[POSTMAN_IMPORT_GUIDE.md](POSTMAN_IMPORT_GUIDE.md)** - Import Postman collection

---

## 📖 **Documentation by Category**

### 🧪 **API Testing**

| Document | Description | Use When |
|----------|-------------|----------|
| [TESTING_QUICK_START.md](TESTING_QUICK_START.md) | Quick reference for all testing methods | Want to test quickly |
| [API_TESTING_GUIDE.md](API_TESTING_GUIDE.md) | Comprehensive API testing guide | Need detailed testing info |
| [../test-api.ps1](../test-api.ps1) | PowerShell automated test script | Windows automated testing |
| [../test-api.sh](../test-api.sh) | Bash automated test script | Linux/Mac automated testing |
| [../test-requests.http](../test-requests.http) | VS Code REST Client format | Testing in VS Code |

### 📬 **Postman Setup**

| Document | Description | Use When |
|----------|-------------|----------|
| [POSTMAN_IMPORT_GUIDE.md](POSTMAN_IMPORT_GUIDE.md) ⭐ | Step-by-step import instructions | **First time using Postman** |
| [POSTMAN_IMPORT_VISUAL.md](POSTMAN_IMPORT_VISUAL.md) | Visual walkthrough with diagrams | Visual learner |
| [POSTMAN_QUICK_REF.md](POSTMAN_QUICK_REF.md) | One-page quick reference | Quick lookup |
| [POSTMAN_GUIDE.md](POSTMAN_GUIDE.md) | Complete Postman guide | Detailed help |
| [POSTMAN_SSL_FIX.md](POSTMAN_SSL_FIX.md) | SSL error troubleshooting | Getting SSL errors |

### 🔧 **Troubleshooting**

| Document | Description | Use When |
|----------|-------------|----------|
| [STARTUP_TROUBLESHOOTING.md](STARTUP_TROUBLESHOOTING.md) | App startup issues | Can't access the app |
| [../diagnose.ps1](../diagnose.ps1) | Diagnostic script | Automated problem detection |
| [POSTMAN_SSL_FIX.md](POSTMAN_SSL_FIX.md) | Postman SSL errors | Certificate errors |

### 📝 **Reference & Background**

| Document | Description |
|----------|-------------|
| [TESTING_QUICK_START.md](TESTING_QUICK_START.md) | Quick testing reference |
| [API_TESTING_GUIDE.md](API_TESTING_GUIDE.md) | Complete API testing guide |

---

## 🎯 **Common Tasks**

### "I want to..."

#### ✅ Test the API quickly
→ [TESTING_QUICK_START.md](TESTING_QUICK_START.md)

#### ✅ Set up Postman
→ [POSTMAN_IMPORT_GUIDE.md](POSTMAN_IMPORT_GUIDE.md)

#### ✅ Fix SSL errors in Postman
→ [POSTMAN_SSL_FIX.md](POSTMAN_SSL_FIX.md)

#### ✅ Troubleshoot app not starting
→ [STARTUP_TROUBLESHOOTING.md](STARTUP_TROUBLESHOOTING.md)

#### ✅ Understand all endpoints
→ [API_TESTING_GUIDE.md](API_TESTING_GUIDE.md)

---

## 📦 **Postman Files**

The following Postman files are in the **parent directory** (not in docs):

- `../AspNetCore.UserService.postman_collection.json` - API request collection
- `../Postman-Environment-Local-HTTP.json` - HTTP environment (CLI/Docker)
- `../Postman-Environment-Local-HTTPS.json` - HTTPS environment (Visual Studio)

**Import all 3 files to Postman!**

See [POSTMAN_IMPORT_GUIDE.md](POSTMAN_IMPORT_GUIDE.md) for instructions.

---

## 🎓 **Learning Path**

### For First-Time Users:

```
1. Read: ../README.md (overview)
2. Run: Application from Visual Studio (F5)
3. Test: Use POSTMAN_IMPORT_GUIDE.md
4. Reference: Keep TESTING_QUICK_START.md handy
```

### For Experienced Developers:

```
1. Quick Start: ../README.md → "Local Development"
2. Test: ../test-api.ps1 or Postman
3. Reference: POSTMAN_QUICK_REF.md
```

### For Troubleshooting:

```
1. App won't start? → STARTUP_TROUBLESHOOTING.md
2. SSL errors? → POSTMAN_SSL_FIX.md
3. Need diagnostics? → ../diagnose.ps1
```

---

## 📊 **File Organization**

```
AspNetCore.UserService/
├── README.md                           ← Start here!
├── Program.cs                          ← Application code
├── appsettings.json                    ← Configuration
├── Dockerfile                          ← Docker setup
│
├── docs/                               ← YOU ARE HERE
│   ├── README.md                       ← This file
│   ├── TESTING_QUICK_START.md          ← Quick testing
│   ├── API_TESTING_GUIDE.md            ← Detailed testing
│   ├── POSTMAN_IMPORT_GUIDE.md         ← Postman setup ⭐
│   ├── POSTMAN_QUICK_REF.md            ← Postman reference
│   ├── POSTMAN_GUIDE.md                ← Full Postman guide
│   ├── POSTMAN_SSL_FIX.md              ← SSL troubleshooting
│   ├── STARTUP_TROUBLESHOOTING.md      ← Startup issues
│   └── ... (other documentation)
│
├── test-api.ps1                        ← PowerShell tests
├── test-api.sh                         ← Bash tests
├── test-requests.http                  ← VS Code tests
├── diagnose.ps1                        ← Diagnostics
│
├── AspNetCore.UserService.postman_collection.json
├── Postman-Environment-Local-HTTP.json
└── Postman-Environment-Local-HTTPS.json
```

---

## 🔗 **External Links**

### AppFactory Framework Documentation

- [AppFactory Main README](../../README.md)
- [ASP.NET Core Package](../../src/AppFactory.Framework.Api.AspNetCore/README.md)
- [Multi-Cloud API Guide](../../MULTI_CLOUD_API_MIGRATION_GUIDE.md)

### Microsoft Documentation

- [ASP.NET Core](https://learn.microsoft.com/aspnet/core)
- [.NET 10](https://learn.microsoft.com/dotnet)
- [Postman Learning](https://learning.postman.com)

---

## ✅ **Documentation Checklist**

Before releasing, verify:

- [ ] All markdown files render correctly
- [ ] All internal links work
- [ ] All Postman files import successfully
- [ ] Scripts run without errors
- [ ] Port numbers are correct
- [ ] URLs are up to date

---

## 💡 **Contributing to Documentation**

When adding new documentation:

1. **Place in this `docs` folder**
2. **Update this README.md** with link
3. **Update ../README.md** if referenced there
4. **Test all links** to ensure they work
5. **Keep consistent formatting**

---

## 📞 **Need Help?**

- **Issues:** [GitHub Issues](https://github.com/exiton3/AppFactory/issues)
- **Questions:** See [../README.md](../README.md) for contacts
- **Framework Docs:** [../../README.md](../../README.md)

---

**Last Updated:** 2026-06-13

