# AppFactory Framework - Release Guide

## Quick Release Process

### 1. Prepare the Release

```bash
# 1. Make sure all changes are committed
git status

# 2. Update CHANGELOG.md with release notes

# 3. Update version in Directory.Build.props (optional, mainly for documentation)
# Change <Version>10.1.0-dev</Version> to <Version>10.2.0-dev</Version> for next version
```

### 2. Create and Push Tag

```bash
# For version 10.1.0
git tag v10.1.0 -a -m "Release v10.1.0 - Assembly Scanning Framework"
git push origin master
git push origin v10.1.0
```

### 3. Monitor GitHub Actions

- Go to: https://github.com/exiton3/AppFactory/actions
- Watch the "Release Package Version" workflow
- It will:
  1. Run all unit tests
  2. Build all projects
  3. Pack all NuGet packages with version from tag
  4. Publish to NuGet.org
  5. Upload artifacts

### 4. Verify on NuGet.org

Check packages are published:
- https://www.nuget.org/packages/AppFactory.Framework.Application/
- https://www.nuget.org/packages/AppFactory.Framework.DataAccess.CosmosDB/
- etc.

---

## Version Numbering Guidelines

### When to bump MAJOR (X.0.0)
- Remove public APIs
- Rename public types/methods
- Change method signatures
- Any breaking change that requires consumer code changes

**Example:** `11.0.0`
```csharp
// Breaking: Removed method
// Old: void DoSomething(string param)
// New: Method removed completely
```

### When to bump MINOR (0.X.0)
- Add new public APIs
- Add new features
- Add new packages
- Enhance existing functionality (backward compatible)

**Example:** `10.1.0` (this release)
```csharp
// New: Assembly scanning framework
services.Scan(scan => scan
    .FromAssembliesOf(typeof(Program))
    .AddClasses(classes => classes.AssignableTo<IMyService>())
        .AsImplementedInterfaces()
        .WithScopedLifetime());
```

### When to bump PATCH (0.0.X)
- Bug fixes
- Performance improvements
- Documentation updates
- Internal refactoring (no public API changes)

**Example:** `10.0.7`
```csharp
// Fixed: NullReferenceException in RepositoryBase.GetById()
```

---

## Release Checklist

### Pre-Release
- [ ] All tests pass locally: `dotnet test`
- [ ] All changes committed to `master` branch
- [ ] CHANGELOG.md updated with release notes
- [ ] README.md updated if needed
- [ ] Breaking changes documented (if any)
- [ ] Version number decided (MAJOR.MINOR.PATCH)

### Release
- [ ] Create annotated tag: `git tag v10.1.0 -a -m "Release message"`
- [ ] Push commits: `git push origin master`
- [ ] Push tag: `git push origin v10.1.0`
- [ ] Monitor GitHub Actions workflow
- [ ] Verify all tests pass in CI
- [ ] Verify all packages published

### Post-Release
- [ ] Verify packages on NuGet.org
- [ ] Test package installation: `dotnet add package AppFactory.Framework.Application --version 10.1.0`
- [ ] Create GitHub Release with notes
- [ ] Update documentation site (if applicable)
- [ ] Announce release (if applicable)

---

## Tag Format

**Always use `v` prefix:**
```bash
✅ git tag v10.1.0
❌ git tag 10.1.0
```

**For pre-release versions:**
```bash
git tag v10.2.0-beta.1
git tag v10.2.0-rc.1
git tag v10.2.0-alpha.1
```

---

## Rolling Back a Release

If you need to remove a bad release:

### 1. Delete the Tag
```bash
# Delete local tag
git tag -d v10.1.0

# Delete remote tag
git push origin :refs/tags/v10.1.0
```

### 2. Unlist Package on NuGet.org
- Go to NuGet.org
- Navigate to package
- Click "Manage Package" → "Unlist"
- **Note:** Cannot delete, only unlist

### 3. Create Fixed Version
```bash
# Fix the issues, then release a patch
git tag v10.1.1 -a -m "Fix: Critical bug in v10.1.0"
git push origin v10.1.1
```

---

## Common Scenarios

### Scenario 1: New Feature (Backward Compatible)
```bash
# Current: v10.0.6
# New feature: Add assembly scanning
# Next version: v10.1.0

git tag v10.1.0 -a -m "Release v10.1.0 - Add assembly scanning framework"
git push origin v10.1.0
```

### Scenario 2: Bug Fix
```bash
# Current: v10.1.0
# Fix: Repository null handling
# Next version: v10.1.1

git tag v10.1.1 -a -m "Release v10.1.1 - Fix repository null handling"
git push origin v10.1.1
```

### Scenario 3: Breaking Change
```bash
# Current: v10.1.0
# Breaking: Rename IRepository to IDataRepository
# Next version: v11.0.0

git tag v11.0.0 -a -m "Release v11.0.0 - BREAKING: Rename IRepository interface"
git push origin v11.0.0
```

### Scenario 4: Pre-release (Beta/RC)
```bash
# Testing major changes before official release
git tag v11.0.0-beta.1 -a -m "Release v11.0.0-beta.1 - Testing new architecture"
git push origin v11.0.0-beta.1

# After testing, official release
git tag v11.0.0 -a -m "Release v11.0.0 - New architecture"
git push origin v11.0.0
```

---

## Troubleshooting

### Workflow doesn't trigger
- **Check:** Tag must match pattern `v*.*.*`
- **Check:** Tag must be pushed from `master` branch
- **Check:** GitHub Actions enabled in repository settings

### Tests fail in CI but pass locally
- **Check:** Environment variables
- **Check:** .NET SDK version matches
- **Check:** All dependencies restored

### Package push fails
- **Check:** NuGet API key in GitHub Secrets
- **Check:** Package version doesn't already exist
- **Check:** Package metadata is valid

### Wrong version in packages
- **Check:** Tag format is correct (`v10.1.0` not `10.1.0`)
- **Check:** Regex extraction in workflow
- **Check:** `PackageVersion` parameter in pack command

---

## Package Dependencies

When you release, all packages use the **same version** for project references:

```xml
<!-- In AppFactory.Framework.Application.nupkg -->
<dependencies>
  <dependency id="AppFactory.Framework.Domain" version="10.1.0" />
  <dependency id="AppFactory.Framework.DependencyInjection" version="10.1.0" />
</dependencies>
```

This is **automatic** because you use `<ProjectReference>` in `.csproj` files.

---

## Next Steps After This Release (v10.1.0)

1. **Tag and Release:**
   ```bash
   git tag v10.1.0 -a -m "Release v10.1.0 - Assembly Scanning Framework"
   git push origin master
   git push origin v10.1.0
   ```

2. **Update for Next Version:**
   - Update `Directory.Build.props`: `<Version>10.2.0-dev</Version>`
   - Start planning next features

3. **Monitor:**
   - Watch GitHub Actions: https://github.com/exiton3/AppFactory/actions
   - Check NuGet.org for published packages
