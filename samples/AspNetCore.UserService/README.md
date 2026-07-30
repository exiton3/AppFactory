# ASP.NET Core User Service Sample

This sample demonstrates building a containerized user management API using ASP.NET Core Minimal APIs with the AppFactory framework. Perfect for Azure Container Apps, Kubernetes, or traditional hosting.

## 🏗️ Architecture

```
HTTP Request
  → Middleware
  → Endpoint Config Route
  → EndpointRequestHandler<TRequest, TResponse>
  → IFunctionProcessor<TRequest, TResponse>
  → EndpointResponseMapper<TResponse>
```

## 📦 Features

- ✅ ASP.NET Core Minimal APIs
- ✅ Endpoint config classes (REPR style)
- ✅ Staged fluent endpoint configuration
- ✅ Generic request handler pipeline
- ✅ Separated response mapper pipeline
- ✅ Swagger/OpenAPI documentation
- ✅ Health checks
- ✅ Global exception handling
- ✅ Request logging and performance tracking
- ✅ Docker containerization
- ✅ Azure Container Apps ready

## 🚀 Prerequisites

- .NET 10 SDK
- Docker (optional)
- Azure CLI (for Azure deployment)

## 📁 Project Structure

```
AspNetCore.UserService/
├── Contracts/
│   └── Users/
│       └── UserDto.cs
├── Domain/
│   └── Users/
│       ├── IUserRepository.cs
│       └── User.cs
├── Features/
│   └── Users/
│       ├── CreateUser/
│       │   ├── CreateUserContracts.cs
│       │   ├── CreateUserEndpoint.cs
│       │   ├── CreateUserRequestMap.cs
│       │   ├── CreateUserProcessor.cs
│       │   └── CreateUserCommandHandler.cs
│       └── GetUserById/
│           ├── GetUserByIdEndpoint.cs
│           ├── GetUserByIdQuery.cs
│           ├── GetUserByIdQueryMap.cs
│           └── GetUserByIdProcessor.cs
├── Infrastructure/
│   └── Persistence/
│       └── UserRepository.cs
├── Program.cs
├── Dockerfile
└── appsettings.json
```

## 🛠️ Local Development

### 1. Run from Visual Studio

1. Open `AppFactory.sln` in Visual Studio
2. Set `AspNetCore.UserService` as the startup project
3. Press **F5** to run
4. Application will start at:
   - HTTPS: `https://localhost:64846`
   - HTTP: `http://localhost:64847`

### 2. Run from Command Line

```bash
cd samples/AspNetCore.UserService
dotnet run
```

Application will start at: `http://localhost:8080`

### 3. Run with Docker

```bash
docker build -t user-service:latest -f samples/AspNetCore.UserService/Dockerfile .
docker run -p 8080:8080 user-service:latest
```

## 🧪 Testing the API

### Quick Test Options

1. **PowerShell Script** (Windows)
   ```powershell
   # Edit $baseUrl in test-api.ps1 to match how you're running the app
   .\test-api.ps1
   ```

2. **Bash Script** (Linux/Mac)
   ```bash
   # Edit BASE_URL in test-api.sh to match how you're running the app
   chmod +x test-api.sh
   ./test-api.sh
   ```

3. **VS Code REST Client**
   - Install "REST Client" extension
   - Open `test-requests.http`
   - Edit `@baseUrl` to match how you're running the app
   - Click "Send Request" above each request

4. **Postman/Thunder Client**
   - **Import:** See detailed instructions in [docs/POSTMAN_IMPORT_GUIDE.md](docs/POSTMAN_IMPORT_GUIDE.md)
   - **Quick:** Drag and drop these 3 files into Postman:
     - `AspNetCore.UserService.postman_collection.json` (collection)
     - `Postman-Environment-Local-HTTP.json` (for CLI/Docker)
     - `Postman-Environment-Local-HTTPS.json` (for Visual Studio)
   - **Select environment** from dropdown (top-right) based on how you're running
   - **Having SSL errors?** See [docs/POSTMAN_SSL_FIX.md](docs/POSTMAN_SSL_FIX.md)

5. **Browser** (for GET endpoints)
   - **Visual Studio:** `https://localhost:64846/` (accept cert warning)
   - **Command Line:** `http://localhost:8080/`

### Port Configuration

| How You Run | Protocol | Port | URL |
|-------------|----------|------|-----|
| **Visual Studio F5** | HTTPS | 64846 | `https://localhost:64846` |
| **Visual Studio F5** | HTTP | 64847 | `http://localhost:64847` |
| **dotnet run** | HTTP | 8080 | `http://localhost:8080` |

### API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/` | GET | Service information |
| `/health` | GET | Health check endpoint |
| `/openapi/v1.json` | GET | OpenAPI specification |
| `/api/users` | POST | Create a new user |
| `/api/users/{userId}` | GET | Get user by ID |

### Example Requests

**Create User:**
```bash
curl -X POST https://localhost:64846/api/users \
  -H "Content-Type: application/json" \
  -d '{"email":"john.doe@example.com","name":"John Doe"}' \
  -k
```

**Get User:**
```bash
curl https://localhost:64846/api/users/{userId} -k
```

📖 **See [docs/API_TESTING_GUIDE.md](docs/API_TESTING_GUIDE.md) for detailed testing instructions**

### 3. Run Tests

```bash
dotnet test
```

## 🚀 Deployment

### Deploy to Azure Container Apps

#### 1. Build and Push to Azure Container Registry

```bash
az acr build \
  --registry myregistry \
  --image user-service:latest \
  --file samples/AspNetCore.UserService/Dockerfile .
```

#### 2. Create Container App Environment

```bash
az containerapp env create \
  --name my-environment \
  --resource-group MyResourceGroup \
  --location eastus
```

#### 3. Deploy Container App

```bash
az containerapp create \
  --name user-service \
  --resource-group MyResourceGroup \
  --environment my-environment \
  --image myregistry.azurecr.io/user-service:latest \
  --target-port 8080 \
  --ingress external \
  --registry-server myregistry.azurecr.io \
  --cpu 0.5 \
  --memory 1Gi \
  --min-replicas 1 \
  --max-replicas 10
```

### Deploy to Kubernetes (AKS)

```bash
kubectl apply -f kubernetes/deployment.yaml
kubectl apply -f kubernetes/service.yaml
```

## 🧪 Testing the API

### Create User

```bash
curl -X POST http://localhost:8080/api/users \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "name": "John Doe"
  }'
```

### Get User

```bash
curl http://localhost:8080/api/users/{userId}
```

### Health Check

```bash
curl http://localhost:8080/health
```

## 📊 Key Components

### Program.cs - Application Entry Point

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAppFactoryApi(typeof(Program).Assembly);
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.MapEndpointConfigs(typeof(Program).Assembly);

app.Run();
```

### Endpoint Config Class

Class-based REPR-style endpoint declaration:

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
      .AllowAnonymous()
      .Produces<CreateUserResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .ConfigureRoute(route => route.WithOpenApi());
  }
}
```

### Runtime Flow

1. Endpoint config declares route and metadata.
2. MapEndpointConfigs discovers and maps configs at startup.
3. MapEndpointRoute binds HTTP route to generic handler delegate.
4. EndpointRequestHandler parses request and executes processor.
5. EndpointResponseMapper maps service result to HTTP response.

### Middleware

- **ExceptionHandlingMiddleware**: Global error handling with problem details
- **RequestLoggingMiddleware**: Performance tracking and structured logging

## 🔧 Configuration

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:8080"
      }
    }
  }
}
```

### Environment Variables

For Azure Container Apps:

```bash
az containerapp update \
  --name user-service \
  --resource-group MyResourceGroup \
  --set-env-vars \
    "ASPNETCORE_ENVIRONMENT=Production" \
    "CosmosDb__Endpoint=https://..." \
    "CosmosDb__Key=..."
```

## 📈 Monitoring

### Application Insights

Add to `Program.cs`:

```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### Health Checks

Extended health checks:

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("database", () => HealthCheckResult.Healthy())
    .AddCheck("external-api", () => HealthCheckResult.Healthy());

app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");
```

## 💰 Cost Comparison

### Azure Container Apps
- **Always On**: ~$50-100/month for basic workload
- **Consumption**: Pay for actual usage
- **No Cold Start**: Instant response times

### AWS Lambda
- **Per Request**: ~$0.20 per million requests
- **Cold Start**: 200-500ms initial latency

## 🎯 When to Use ASP.NET Core vs Serverless

### Use ASP.NET Core When:
- ✅ High-traffic, low-latency requirements
- ✅ WebSocket or SignalR needed
- ✅ Long-running connections
- ✅ Predictable pricing
- ✅ Full control over runtime

### Use Serverless (Lambda/Functions) When:
- ✅ Sporadic traffic patterns
- ✅ Event-driven architecture
- ✅ Pay-per-use cost optimization
- ✅ Auto-scaling without config

## 🔄 Platform Portability

The beauty of AppFactory: **same business logic works everywhere!**

```csharp
public class CreateUserProcessor : IFunctionProcessor<CreateUserRequest, CreateUserResponse>
{
    // This processor works in:
    // ✅ AWS Lambda
    // ✅ Azure Functions
    // ✅ ASP.NET Core
    // ✅ Any .NET hosting environment
}
```

## 🎯 Next Steps

1. Add authentication/authorization
2. Implement remaining CRUD operations
3. Add integration tests
4. Set up CI/CD pipeline
5. Configure auto-scaling rules
6. Add distributed caching

## 📚 Documentation

All documentation is organized in the [`docs/`](docs/) folder:

- **[Quick Start Testing](docs/TESTING_QUICK_START.md)** - Fast testing guide
- **[Postman Setup](docs/POSTMAN_IMPORT_GUIDE.md)** - Import Postman collection
- **[API Testing Guide](docs/API_TESTING_GUIDE.md)** - Complete API documentation
- **[Troubleshooting](docs/STARTUP_TROUBLESHOOTING.md)** - Fix common issues
- **[Full Documentation Index](docs/DOCUMENTATION_INDEX.md)** - All docs

## 📚 Related Documentation

- [AppFactory Multi-Cloud Guide](../../MULTI_CLOUD_API_MIGRATION_GUIDE.md)
- [ASP.NET Core Package README](../../src/AppFactory.Framework.Api.AspNetCore/README.md)
- [Azure Container Apps Docs](https://learn.microsoft.com/en-us/azure/container-apps/)

## 🤝 Support

For issues or questions:
- [GitHub Issues](https://github.com/exiton3/AppFactory/issues)
- [AppFactory Documentation](../../README.md)
