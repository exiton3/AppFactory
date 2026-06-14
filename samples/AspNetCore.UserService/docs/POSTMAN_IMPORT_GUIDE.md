# How to Import Postman Collection and Environments

## 📦 Step-by-Step Import Guide

### Step 1: Open Postman

1. Launch **Postman** application
2. Make sure you're in a workspace (create one if needed)

---

### Step 2: Import the Collection

#### Method A: Drag and Drop (Easiest)

1. Open File Explorer and navigate to:
   ```
   C:\Projects\Architecture\AppFactory\samples\AspNetCore.UserService\
   ```

2. Find the file:
   ```
   AspNetCore.UserService.postman_collection.json
   ```

3. **Drag and drop** this file into the Postman window

4. You'll see an import dialog → Click **Import**

5. ✅ Collection appears in left sidebar under "Collections"

#### Method B: Using Import Button

1. In Postman, click **Import** button (top-left)

2. Click **"Upload Files"** or drag file into the dialog

3. Select:
   ```
   AspNetCore.UserService.postman_collection.json
   ```

4. Click **Open**

5. Click **Import** to confirm

6. ✅ Collection appears in left sidebar

---

### Step 3: Import the Environments

#### Option 1: Drag and Drop Both Files

1. In File Explorer, navigate to:
   ```
   C:\Projects\Architecture\AppFactory\samples\AspNetCore.UserService\
   ```

2. Select **both** environment files (Ctrl+Click):
   ```
   ✅ Postman-Environment-Local-HTTP.json
   ✅ Postman-Environment-Local-HTTPS.json
   ```

3. **Drag and drop** both files into Postman window

4. Click **Import** in the dialog

5. ✅ Both environments imported!

#### Option 2: Import One at a Time

1. Click **Environments** icon in left sidebar (icon looks like a gear/eye)

2. Click **Import** button

3. Select first file:
   ```
   Postman-Environment-Local-HTTP.json
   ```

4. Click **Open** → **Import**

5. Repeat for second file:
   ```
   Postman-Environment-Local-HTTPS.json
   ```

6. ✅ Both environments now visible in Environments tab

---

### Step 4: Select an Environment

1. Look at **top-right corner** of Postman

2. You'll see a dropdown that says **"No Environment"**

3. Click the dropdown

4. You should see:
   ```
   ⚪ No Environment
   🌐 Local HTTP (Command Line)
   🌐 Local HTTPS (Visual Studio)
   ```

5. **Select the environment** based on how you're running the app:
   - Running `dotnet run` or Docker? → Select **"Local HTTP (Command Line)"**
   - Running Visual Studio F5? → Select **"Local HTTPS (Visual Studio)"**

6. ✅ Selected environment name appears in top-right corner

---

### Step 5: Verify Import

#### Check Collection:

1. Click **Collections** in left sidebar

2. You should see:
   ```
   📁 AspNetCore.UserService API
      └── 📄 Service Info
      └── 📄 Health Check
      └── 📄 OpenAPI Specification
      └── 📄 Create User
      └── 📄 Get User by ID
   ```

#### Check Environments:

1. Click **Environments** in left sidebar

2. You should see two environments:
   ```
   🌐 Local HTTP (Command Line)
   🌐 Local HTTPS (Visual Studio)
   ```

3. Click on each environment to see the `baseUrl` variable:
   - **Local HTTP:** `http://localhost:8080`
   - **Local HTTPS:** `https://localhost:64846`

---

### Step 6: Test It!

1. Make sure the **correct environment is selected** (top-right dropdown)

2. In the collection, click on **"Health Check"** request

3. Click **Send** button

4. You should get response:
   ```
   Healthy
   ```

5. ✅ Success! You're ready to test all endpoints

---

## 🎯 Quick Visual Checklist

After importing, your Postman should look like this:

```
┌─────────────────────────────────────────────────┐
│ Postman                    [Local HTTP ▼] 👈    │ Environment dropdown
├─────────────────────────────────────────────────┤
│ Collections │ Environments │ History │          │
├─────────────┴──────────────┴─────────┴──────────┤
│                                                  │
│ 📁 AspNetCore.UserService API                   │ Your collection
│    └── 📄 Service Info                          │
│    └── 📄 Health Check                          │
│    └── 📄 OpenAPI Specification                 │
│    └── 📄 Create User                           │
│    └── 📄 Get User by ID                        │
│                                                  │
└──────────────────────────────────────────────────┘
```

**Environments Tab:**
```
┌─────────────────────────────────────────────────┐
│ Environments                                     │
├─────────────────────────────────────────────────┤
│ 🌐 Local HTTP (Command Line)                    │
│    baseUrl: http://localhost:8080               │
│                                                  │
│ 🌐 Local HTTPS (Visual Studio)                  │
│    baseUrl: https://localhost:64846             │
└──────────────────────────────────────────────────┘
```

---

## 🔧 Troubleshooting Import Issues

### Issue: "Import failed" or file not recognized

**Solution:**
- Make sure you're importing `.json` files (not `.md` files)
- Try copying file contents and using "Raw text" import option

### Issue: Environment variables not showing

**Solution:**
1. Click environment name in Environments tab
2. Check if `baseUrl` variable exists
3. If missing, manually add it:
   - Variable: `baseUrl`
   - Initial Value: `http://localhost:8080` (or your URL)
   - Current Value: `http://localhost:8080`

### Issue: Can't see environments in dropdown

**Solution:**
1. Click **Environments** in left sidebar
2. Check if environments exist there
3. If yes, close and reopen Postman
4. Try selecting from top-right dropdown again

---

## 💡 Pro Tips

### Tip 1: Check Active Environment
The active environment is shown in **top-right corner**. Make sure it matches how you're running the app!

### Tip 2: Quick Environment Switch
- Visual Studio F5 → HTTPS environment
- Stop VS, run `dotnet run` → Switch to HTTP environment
- Just click dropdown and select!

### Tip 3: Verify Environment Variables
Before testing:
1. Click environment dropdown (top-right)
2. Click the "eye" icon 👁️ next to environment name
3. Verify `baseUrl` shows correct URL
4. Close tooltip

### Tip 4: Test with Simple Request First
Always test with **"Health Check"** or **"Service Info"** first:
- These are GET requests (simpler)
- No request body needed
- Should return quickly
- Confirms app is running and environment is correct

---

## 🎯 What to Do Next

After successful import:

### 1️⃣ Start Your Application

**From Visual Studio:**
```
Press F5 → Select "Local HTTPS (Visual Studio)" environment
```

**From Command Line:**
```powershell
cd samples\AspNetCore.UserService
dotnet run
```
→ Select **"Local HTTP (Command Line)"** environment

### 2️⃣ Test Endpoints in Order

1. **Service Info** → Verify app responds
2. **Health Check** → Verify health endpoint works
3. **OpenAPI Specification** → View API schema (dev mode only)
4. **Create User** → Test POST endpoint (saves userId to variable)
5. **Get User by ID** → Test GET endpoint (uses saved userId)

### 3️⃣ Run Full Collection

1. Right-click collection name
2. Select **"Run collection"**
3. Select environment (top of runner)
4. Click **"Run AspNetCore.UserService API"**
5. Watch all tests execute automatically!

---

## 📚 Files Reference

Files you imported:

```
📄 AspNetCore.UserService.postman_collection.json
   → Collection with all API requests

📄 Postman-Environment-Local-HTTP.json
   → Environment for dotnet run / Docker
   → baseUrl: http://localhost:8080

📄 Postman-Environment-Local-HTTPS.json
   → Environment for Visual Studio F5
   → baseUrl: https://localhost:64846
```

---

## 🆘 Need More Help?

- **SSL Errors:** See `POSTMAN_SSL_FIX.md`
- **Detailed Guide:** See `POSTMAN_GUIDE.md`
- **Quick Reference:** See `POSTMAN_QUICK_REF.md`
- **General API Testing:** See `API_TESTING_GUIDE.md`

---

## ✅ Success Checklist

After import, you should have:

- ✅ Collection visible in "Collections" tab
- ✅ Two environments visible in "Environments" tab
- ✅ Environment dropdown shows both options (top-right)
- ✅ Can select environment and it shows in top-right corner
- ✅ "Health Check" request returns `Healthy`

**All good? You're ready to test the API!** 🎉

