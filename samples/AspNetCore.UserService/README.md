# ASP.NET Core User Service Sample

This sample demonstrates building a containerized user management API using ASP.NET Core Minimal APIs with the AppFactory framework. Perfect for Azure Container Apps, Kubernetes, or traditional hosting.

## 🏗️ Architecture

```
HTTP Request → Middleware → Minimal API Endpoint → CQRS Handler → Database
                  ↓
           AppFactory CQRS
```

## 📦 Features

- ✅ ASP.NET Core Minimal APIs
- ✅ CQRS pattern with commands and queries
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
├── Domain/
│   └── User.cs
├── Application/
│   ├── Commands/
│   │   └── CreateUserCommand.cs
│   ├── Queries/
│   │   └── GetUserByIdQuery.cs
│   ├── DTOs/
│   │   └── UserDto.cs
│   └── Processors/
│       ├── CreateUserProcessor.cs
│       └── GetUserProcessor.cs
├── Program.cs
├── Dockerfile
└── appsettings.json
```

## 🛠️ Local Development

### 1. Run Locally

```bash
cd samples/AspNetCore.UserService
dotnet run
```

Access Swagger UI: `http://localhost:8080/swagger`

### 2. Run with Docker

```bash
docker build -t user-service:latest -f samples/AspNetCore.UserService/Dockerfile .
docker run -p 8080:8080 user-service:latest
```

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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.MapCommand<CreateUserCommand, UserDto>("/api/users");
app.MapQuery<GetUserByIdQuery, UserDto>("/api/users/{userId}");

app.Run();
```

### Minimal API Endpoints

Fluent API for mapping CQRS handlers:

```csharp
app.MapCommand<CreateUserCommand, UserDto>("/api/users")
   .WithName("CreateUser")
   .WithOpenApi();

app.MapQuery<GetUserByIdQuery, UserDto>("/api/users/{userId}")
   .WithName("GetUser")
   .WithOpenApi();
```

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
public class CreateUserProcessor : IFunctionProcessor<CreateUserCommand, UserDto>
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

## 📚 Related Documentation

- [AppFactory Multi-Cloud Guide](../../MULTI_CLOUD_API_MIGRATION_GUIDE.md)
- [ASP.NET Core Package README](../../src/AppFactory.Framework.Api.AspNetCore/README.md)
- [Azure Container Apps Docs](https://learn.microsoft.com/en-us/azure/container-apps/)

## 🤝 Support

For issues or questions:
- [GitHub Issues](https://github.com/exiton3/AppFactory/issues)
- [AppFactory Documentation](../../README.md)
