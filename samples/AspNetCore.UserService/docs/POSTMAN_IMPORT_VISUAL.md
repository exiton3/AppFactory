# Postman Import - Visual Walkthrough

## 🎯 Quick 3-Step Process

```
Step 1: Drag collection file → Postman
Step 2: Drag both environment files → Postman  
Step 3: Select environment from dropdown (top-right)
```

**That's it!** 🎉

---

## 📸 Visual Guide

### Before Import

```
Your Postman workspace (empty):
┌─────────────────────────────────────┐
│ Postman            [No Environment ▼]│
├─────────────────────────────────────┤
│ Collections │ Environments │        │
├─────────────────────────────────────┤
│                                     │
│   No collections yet...             │
│   Click Import to get started       │
│                                     │
└─────────────────────────────────────┘
```

### After Importing Collection

```
┌─────────────────────────────────────┐
│ Postman            [No Environment ▼]│
├─────────────────────────────────────┤
│ Collections │ Environments │        │
├─────────────────────────────────────┤
│ 📁 AspNetCore.UserService API   ✅  │ ← Collection imported!
│    └── Service Info                 │
│    └── Health Check                 │
│    └── OpenAPI Specification        │
│    └── Create User                  │
│    └── Get User by ID               │
└─────────────────────────────────────┘
```

### After Importing Environments

```
Click Environments tab:
┌─────────────────────────────────────┐
│ Environments                        │
├─────────────────────────────────────┤
│ 🌐 Local HTTP (Command Line)    ✅ │ ← Environment 1
│    Variables: 1                     │
│                                     │
│ 🌐 Local HTTPS (Visual Studio)  ✅ │ ← Environment 2
│    Variables: 1                     │
└─────────────────────────────────────┘
```

### Selecting Environment

```
Top-right dropdown:
┌─────────────────────────────────────┐
│ Postman   [Local HTTP (Command... ▼]│ ← Click here!
├─────────────────────────────────────┤
│                                     │
│  Select environment:                │
│  ○ No Environment                   │
│  ● Local HTTP (Command Line)    ✅  │ ← Selected
│  ○ Local HTTPS (Visual Studio)      │
│                                     │
└─────────────────────────────────────┘
```

### Ready to Test!

```
With environment selected:
┌─────────────────────────────────────────────────┐
│ Postman      [Local HTTP (Command Line) ▼] ✅  │
├─────────────────────────────────────────────────┤
│ GET http://localhost:8080/health                │
│ ───────────────────────────────────             │
│                                    [Send] 👈     │
├─────────────────────────────────────────────────┤
│ Response                                        │
│ Status: 200 OK                                  │
│ Time: 45 ms                                     │
│ ───────────────────────────────────             │
│ Healthy                                         │
└─────────────────────────────────────────────────┘
```

---

## 🎬 Step-by-Step Actions

### 1. Import Collection

```
Action: Drag file into Postman
File:   AspNetCore.UserService.postman_collection.json

Where: Anywhere in Postman window

What you'll see:
┌──────────────────────────┐
│ Import                   │
├──────────────────────────┤
│ ✅ AspNetCore.UserSer... │
│                          │
│         [Import] 👈      │
└──────────────────────────┘

Result: Collection appears in left sidebar
```

### 2. Import Environments

```
Action: Drag both files into Postman
Files:  
  - Postman-Environment-Local-HTTP.json
  - Postman-Environment-Local-HTTPS.json

Where: Anywhere in Postman window

What you'll see:
┌──────────────────────────┐
│ Import                   │
├──────────────────────────┤
│ ✅ Postman-Environmen... │
│ ✅ Postman-Environmen... │
│                          │
│         [Import] 👈      │
└──────────────────────────┘

Result: 2 environments available
```

### 3. Select Environment

```
Action: Click dropdown (top-right)
Location: Top-right corner next to gear icon

Options shown:
  ○ No Environment
  ○ Local HTTP (Command Line)
  ○ Local HTTPS (Visual Studio)

Choose based on how you're running:
  - dotnet run or Docker    → Local HTTP
  - Visual Studio F5        → Local HTTPS
```

### 4. Verify Setup

```
Check 1 - Collections Tab:
┌─────────────────────────┐
│ 📁 AspNetCore.UserSe... │ ✅ Present?
└─────────────────────────┘

Check 2 - Environments Tab:
┌─────────────────────────┐
│ 🌐 Local HTTP (Comma... │ ✅ Present?
│ 🌐 Local HTTPS (Visua...│ ✅ Present?
└─────────────────────────┘

Check 3 - Environment Selected:
┌─────────────────────────┐
│ [Local HTTP ▼] (top-r...)│ ✅ Shows name?
└─────────────────────────┘
```

---

## 🔍 What Each File Does

### Collection File
```
📄 AspNetCore.UserService.postman_collection.json

Contains:
  - 5 API requests
  - Request headers
  - Request bodies
  - Test scripts
  - Variable placeholders ({{baseUrl}})

Purpose: Defines what to test
```

### HTTP Environment File
```
📄 Postman-Environment-Local-HTTP.json

Contains:
  - baseUrl = http://localhost:8080

Purpose: Provides values for CLI/Docker
```

### HTTPS Environment File
```
📄 Postman-Environment-Local-HTTPS.json

Contains:
  - baseUrl = https://localhost:64846

Purpose: Provides values for Visual Studio
```

---

## 🎯 Complete Import Checklist

```
□ Open Postman
□ Drag collection.json into Postman
□ Click [Import] button
□ See collection in left sidebar ✅

□ Drag both environment.json files into Postman
□ Click [Import] button
□ Click Environments tab
□ See 2 environments listed ✅

□ Click environment dropdown (top-right)
□ See both environments in dropdown ✅
□ Select appropriate environment
□ See environment name in top-right ✅

□ Open "Health Check" request
□ Click [Send]
□ Get "Healthy" response ✅
```

**All checked? You're ready!** 🎉

---

## 💡 Common Import Scenarios

### Scenario 1: First Time User

**You have:** Nothing in Postman yet
**Do this:**
1. Import collection file
2. Import both environment files
3. Select HTTP environment (safer to start with)
4. Test "Health Check"

### Scenario 2: Already Using Postman

**You have:** Other collections and environments
**Do this:**
1. Import just adds to existing (doesn't replace)
2. Switch between collections as needed
3. Use environment dropdown to switch contexts

### Scenario 3: Team Member Import

**You have:** Shared by teammate
**Do this:**
1. Copy all 3 files to your machine
2. Import all 3 at once (drag together)
3. Everyone has same setup

### Scenario 4: Multiple Projects

**You have:** Multiple API projects
**Do this:**
1. Each project gets its own collection
2. Each project can have multiple environments
3. Switch between projects via Collections tab
4. Switch between environments via dropdown

---

## 🆘 Troubleshooting Import

### Import Button Grayed Out?
- Make sure you're in a workspace
- Try creating a new workspace first

### Files Not Recognized?
- Confirm files end with `.json`
- Open file in text editor to verify it's valid JSON
- Try copying file contents and using "Raw text" import

### Environments Not Showing?
- Click **Environments** tab (left sidebar)
- Look for environments there
- If missing, re-import environment files

### Can't Select Environment?
- Make sure environment is imported (check Environments tab)
- Try closing and reopening Postman
- Click environment name in Environments tab to verify `baseUrl` variable exists

### Wrong URL in Requests?
- Check which environment is selected (top-right)
- Hover over `{{baseUrl}}` in request to see actual value
- Click eye icon 👁️ next to environment name to see all variables

---

## 📚 Related Documentation

- `POSTMAN_QUICK_REF.md` - One-page quick reference
- `POSTMAN_GUIDE.md` - Complete Postman guide
- `POSTMAN_SSL_FIX.md` - SSL error fixes
- `API_TESTING_GUIDE.md` - General API testing
- `README.md` - Main project documentation

---

## ✅ Success Indicators

You'll know import was successful when:

1. **Collections Tab**
   - Shows "AspNetCore.UserService API"
   - Can expand to see 5 requests

2. **Environments Tab**
   - Shows 2 environments
   - Each has `baseUrl` variable

3. **Top-Right Dropdown**
   - Shows environment names
   - Can select and switch between them

4. **Test Request**
   - "Health Check" returns `Healthy`
   - Status shows `200 OK`

**Seeing all 4? Perfect! You're all set!** 🚀

