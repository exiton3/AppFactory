# AppFactory Sample Applications

This folder contains complete working examples demonstrating how to build serverless and containerized APIs using the AppFactory framework across different platforms.

## 📁 Available Samples

### 1. AWS Lambda User Service (`samples/AWS.Lambda.UserService`)
Complete AWS Lambda + API Gateway application demonstrating:
- CQRS command and query handlers
- DynamoDB integration
- EventBridge event publishing
- Serverless Framework deployment

### 2. Azure Functions User Service (`samples/Azure.Functions.UserService`)
Complete Azure Functions v4 application demonstrating:
- CQRS command and query handlers
- Azure Cosmos DB integration
- Azure Service Bus messaging
- Azure Functions Core Tools deployment

### 3. ASP.NET Core User Service (`samples/AspNetCore.UserService`)
Complete ASP.NET Core minimal API application demonstrating:
- CQRS command and query handlers
- Cosmos DB / DynamoDB integration
- Swagger/OpenAPI documentation
- Health checks
- Docker containerization
- Azure Container Apps deployment

## 🚀 Quick Start

Each sample includes:
- ✅ Complete source code
- ✅ README with step-by-step instructions
- ✅ Deployment configurations
- ✅ Unit tests
- ✅ Integration tests

## 📚 Common Features

All samples demonstrate:
- **CQRS Pattern** - Separated command and query handlers
- **Domain-Driven Design** - Rich domain models with business logic
- **Repository Pattern** - Data access abstraction
- **Result Pattern** - Type-safe error handling
- **Dependency Injection** - Clean, testable architecture
- **Logging** - Structured logging with performance tracking

## 🔗 Resources

- [Multi-Cloud API Migration Guide](../MULTI_CLOUD_API_MIGRATION_GUIDE.md)
- [Multi-Cloud API Quick Reference](../MULTI_CLOUD_API_QUICK_REFERENCE.md)
- [Main Documentation](../README.md)

## 💡 Learning Path

1. **Start with AWS Lambda** - Simplest serverless example
2. **Try Azure Functions** - Similar to Lambda with Azure services
3. **Explore ASP.NET Core** - Full-featured API with containers

## 🎯 Next Steps

After exploring the samples:
- Modify the code to fit your domain
- Add your own commands and queries
- Deploy to your cloud environment
- Share your feedback!
