# AWS Lambda User Service Sample

This sample demonstrates building a serverless user management API using AWS Lambda, API Gateway, and DynamoDB with the AppFactory framework.

## 🏗️ Architecture

```
API Gateway → Lambda Function → DynamoDB
                ↓
         AppFactory CQRS
```

## 📦 Features

- ✅ AWS Lambda + API Gateway integration
- ✅ DynamoDB for data persistence
- ✅ CQRS pattern with commands and queries
- ✅ Automatic request parsing and validation
- ✅ Type-safe response building
- ✅ Structured logging with Serilog
- ✅ Serverless Framework deployment

## 🚀 Prerequisites

- .NET 10 SDK
- AWS CLI configured
- Serverless Framework: `npm install -g serverless`
- AWS account with appropriate permissions

## 📁 Project Structure

```
AWS.Lambda.UserService/
├── Domain/
│   └── User.cs                     # Domain entity
├── Application/
│   ├── Commands/
│   │   ├── CreateUserCommand.cs
│   │   └── CreateUserCommandHandler.cs
│   ├── DTOs/
│   │   └── UserDto.cs
│   └── Processors/
│       └── CreateUserProcessor.cs  # Platform-agnostic processor
├── Functions/
│   └── CreateUserFunction.cs       # Lambda entry point
├── Startup.cs                      # Dependency injection
└── serverless.yml                  # Deployment configuration
```

## 🛠️ Local Development

### 1. Build the Project

```bash
dotnet build
```

### 2. Run Tests

```bash
dotnet test
```

### 3. Package for Deployment

```bash
dotnet publish -c Release
cd bin/Release/net10.0/publish
zip -r ../AWS.Lambda.UserService.zip .
```

## 🚀 Deployment

### Deploy to AWS

```bash
serverless deploy --stage dev
```

### Deploy to Production

```bash
serverless deploy --stage prod
```

### Remove Stack

```bash
serverless remove --stage dev
```

## 🧪 Testing the API

### Create User

```bash
curl -X POST https://your-api-gateway-url/dev/users \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "name": "John Doe"
  }'
```

### Expected Response

```json
{
  "id": "abc123",
  "email": "user@example.com",
  "name": "John Doe",
  "createdAt": "2024-12-19T10:00:00Z",
  "isActive": true
}
```

### Error Response

```json
{
  "problem": "1 validation error detected:",
  "errors": [
    {
      "code": "EMAIL_REQUIRED",
      "message": "Email is required"
    }
  ]
}
```

## 📊 Key Components

### CreateUserCommand

Represents the user creation request with validation attributes:

```csharp
public class CreateUserCommand : ICommand
{
    [FromBody]
    public string Email { get; set; }

    [FromBody]
    public string Name { get; set; }
}
```

### CreateUserCommandHandler

Business logic for user creation:

```csharp
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    public async Task<CommandResult> Handle(CreateUserCommand command, CancellationToken ct)
    {
        // Validation
        // Business logic
        // Persistence
    }
}
```

### CreateUserProcessor

Platform-agnostic processor that works across AWS, Azure, and ASP.NET Core:

```csharp
public class CreateUserProcessor : IFunctionProcessor<CreateUserCommand, UserDto>
{
    public async Task<Result<UserDto>> Process(CreateUserCommand request, CancellationToken ct)
    {
        // Orchestrates command handling and DTO mapping
    }
}
```

### CreateUserFunction

AWS Lambda entry point:

```csharp
public class CreateUserFunction : LambdaFunctionHandlerBase<CreateUserCommand, UserDto>
{
    public async Task<APIGatewayProxyResponse> FunctionHandler(
        APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        return await Handle(request, context);
    }
}
```

## 🔧 Configuration

### Environment Variables

Set in `serverless.yml`:

```yaml
environment:
  DYNAMODB_TABLE: ${self:service}-${self:provider.stage}-users
  LOG_LEVEL: Information
```

### DynamoDB Table

Automatically created by Serverless Framework with:
- Partition Key: PK
- Sort Key: SK
- Billing Mode: PAY_PER_REQUEST

## 💰 Cost Estimation

- **Lambda**: ~$0.20 per million requests
- **DynamoDB**: Pay per request (typically < $1 for dev/test)
- **API Gateway**: ~$3.50 per million requests

## 🎯 Next Steps

1. Add query handlers for retrieving users
2. Implement update and delete operations
3. Add integration tests
4. Set up CI/CD pipeline
5. Add EventBridge integration for domain events
6. Implement authentication/authorization

## 📚 Related Documentation

- [AppFactory Multi-Cloud API Guide](../../MULTI_CLOUD_API_MIGRATION_GUIDE.md)
- [AWS Lambda Package README](../../src/AppFactory.Framework.Api.Aws/README.md)
- [CQRS Pattern Documentation](../../src/AppFactory.Framework.Application/README.md)

## 🤝 Support

For issues or questions:
- [GitHub Issues](https://github.com/exiton3/AppFactory/issues)
- [AppFactory Documentation](../../README.md)
