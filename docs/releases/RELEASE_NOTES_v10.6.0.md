# AppFactory v10.6.0 - Release Notes

**Release Date**: Jul 30, 2026   
**Status**: Production Ready  
**Focus**: ASP.NET Core Endpoint Configuration Model

---

## 🎉 What's New

**REPR-Style Endpoint Configuration for ASP.NET Core** - Build Minimal API endpoints using class-based endpoint configuration with staged fluent APIs, while keeping route declaration, request execution, and response mapping separated.

---

## 🚀 New Features

### 1. **EndpointConfig<TRequest, TResponse>**

ASP.NET Core endpoints can now be defined as dedicated configuration classes instead of inline Minimal API route declarations.

```csharp
public sealed class CreateUserEndpoint : EndpointConfig<CreateUserRequest, CreateUserResponse>
{
    protected override void Configure()
    {
        Post("/api/users")
            .Name("CreateUser")
            .Summary("Create a new user")
            .Description("Creates a new user with the specified email and name")
            .Tags("Users")
            .Security()
            .RequireAuthorization()
            .Produces<CreateUserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .ConfigureRoute(route => route.WithOpenApi());
    }
}
```

**Key Benefits:**
- Class-based endpoint declaration
- Feature-local route configuration
- Cleaner vertical slice structure
- More discoverable endpoint organization

### 2. **Staged Fluent Endpoint Configuration**

Endpoint configuration now uses staged fluent interfaces with compile-time guidance:

- **Route step**: `Get/Post/Put/Patch/Delete`
- **Metadata step**: `Name`, `Summary`, `Description`, `Tags`
- **Security step**: `Security().AllowAnonymous()` or `Security().RequireAuthorization(...)`
- **Final step**: `Produces(...)`, `ConfigureRoute(...)`

```csharp
Get("/api/users/{userId}")
    .Name("GetUser")
    .Summary("Get user by ID")
    .Tags("Users")
    .Security()
    .RequireAuthorization("Users.Read")
    .Produces<UserDto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);
```

### 3. **Authorization Configuration Helpers**

New endpoint security helpers for ASP.NET Core:

- `RequireAuthorization()`
- `RequireAuthorization(string policy)`
- `RequireAuthorization(Action<AuthorizationPolicyBuilder> policyBuilder)`
- `AllowAnonymous()`

Conflicting authorization configuration now throws `InvalidOperationException` instead of silently overriding previous settings.

### 4. **Assembly-Based Endpoint Discovery**

Endpoints can now be discovered and mapped from assemblies automatically:

```csharp
app.MapEndpointConfigs(typeof(Program).Assembly);
```

This removes manual endpoint registration and supports REPR-style endpoint organization.

### 5. **Separated ASP.NET Core Runtime Pipeline**

The ASP.NET Core runtime flow is now split into focused components:

- **Route mapping**: `MapEndpointRoute<TRequest, TResponse>()`
- **Request execution**: `IEndpointRequestHandler<TRequest, TResponse>`
- **Response translation**: `IEndpointResponseMapper<TResponse>`

This improves maintainability and aligns ASP.NET Core more closely with the multi-platform handler model used in AWS Lambda and Azure Functions.

### 6. **Improved Body Mapping for Multi-Property Requests**

Request body parsing was improved to correctly map multiple JSON body properties to request contracts.

Example request body:

```json
{
  "email": "john.doe@example.com",
  "name": "John Doe"
}
```

This now maps cleanly into request DTOs with multiple `FromBody()` property mappings.

---

## 📦 Package Updates

**Total Packages**: 24

### Updated Packages
All framework packages updated to **v10.6.0** for version consistency across direct package dependencies.

### Highlighted Package Changes
- `AppFactory.Framework.Api.AspNetCore` v10.6.0 - EndpointConfig model, staged fluent API, runtime pipeline split
- Other packages updated to v10.6.0 as part of coordinated release versioning

---

## 🎯 Key Benefits

- ✅ **Cleaner ASP.NET Core endpoint design**
- ✅ **Feature-local endpoint declaration**
- ✅ **Compile-time guided fluent configuration**
- ✅ **Separated route mapping and execution behavior**
- ✅ **Improved maintainability and extensibility**
- ✅ **Better alignment with AWS and Azure abstractions**
- ✅ **Safer authorization configuration**

---

## 💡 Quick Start

### Endpoint Registration

```csharp
builder.Services.AddAppFactoryApi(typeof(Program).Assembly);
app.MapEndpointConfigs(typeof(Program).Assembly);
```

### Direct Route Mapping

```csharp
app.MapEndpointRoute<CreateUserRequest, CreateUserResponse>("/api/users", "POST");
```

### Security Configuration

```csharp
Post("/api/users")
    .Name("CreateUser")
    .Security()
    .RequireAuthorization("AdminOnly")
    .Produces<CreateUserResponse>(StatusCodes.Status200OK);
```

---

## 🔄 Compatibility Notes

- `MapEndpoint<TRequest, TResponse>()` remains available as a backward-compatible alias for `MapEndpointRoute<TRequest, TResponse>()`
- Existing consumers can continue using current APIs
- New ASP.NET Core development should prefer `EndpointConfig<TRequest, TResponse>` and `MapEndpointConfigs(...)`

---

## 📚 Documentation

Updated documentation is available here:

- `src/AppFactory.Framework.Api.AspNetCore/README.md`
- `samples/AspNetCore.UserService/README.md`

---

## 🚀 Recommended Upgrade Path

For ASP.NET Core consumers:

1. Keep existing endpoints working as-is
2. Introduce `EndpointConfig<TRequest, TResponse>` for new endpoints
3. Migrate old inline mappings to assembly-discovered endpoint configs over time
4. Use `MapEndpointConfigs(...)` for automatic registration

---

## See Also

- `src/AppFactory.Framework.Api.AspNetCore/README.md`
- `samples/AspNetCore.UserService/README.md`
- `src/AppFactory.Framework.Api.Aws/README.md`
- `src/AppFactory.Framework.Api.Azure/README.md`
