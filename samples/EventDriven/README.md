# Event-Driven Microservices Samples

Complete examples demonstrating event-driven architecture using AppFactory across AWS, Azure, and on-premises platforms.

## 📁 Available Samples

### 1. E-Commerce Event-Driven Microservices (`samples/EventDriven.ECommerce`)
Complete event-driven e-commerce system with:
- **User Service** - User registration and management
- **Order Service** - Order processing
- **Inventory Service** - Stock management
- **Notification Service** - Email notifications

**Events Flow:**
```
UserRegistered → Send Welcome Email
OrderPlaced → Reserve Inventory → Process Payment → Create Shipment
OrderCancelled → Release Inventory → Refund Payment
```

### 2. AWS Lambda Event-Driven Sample (`samples/EventDriven.Aws.UserService`)
Demonstrates:
- Publishing events to EventBridge
- Consuming events in Lambda functions
- Cross-service event handling

### 3. Azure Functions Event-Driven Sample (`samples/EventDriven.Azure.UserService`)
Demonstrates:
- Publishing events to Event Grid
- Consuming events in Azure Functions
- Event Grid subscriptions

## 🎯 Key Patterns Demonstrated

### 1. **Event Publishing**
```csharp
await _eventPublisher.PublishAsync(new UserCreatedEvent
{
    EventType = "com.appfactory.user.created",
    Source = "user-service",
    Data = new { UserId = user.Id, Email = user.Email }
});
```

### 2. **Event Handling**
```csharp
public class SendWelcomeEmailHandler : IEventHandler<UserCreatedEvent>
{
    public async Task HandleAsync(UserCreatedEvent @event, CancellationToken ct)
    {
        // Send welcome email
    }
}
```

### 3. **Cross-Service Communication**
- Decoupled services communicate through events
- Each service can be deployed independently
- Events provide audit trail and temporal decoupling

## 🚀 Running the Samples

### AWS Lambda
```bash
cd samples/EventDriven.Aws.UserService
serverless deploy
```

### Azure Functions
```bash
cd samples/EventDriven.Azure.UserService
func azure functionapp publish MyFunctionApp
```

## 📚 Documentation

- [Event-Driven Architecture Guide](../../EVENT_DRIVEN_ARCHITECTURE_GUIDE.md)
- [CloudEvents Specification](../../CLOUDEVENTS_GUIDE.md)
- [Multi-Cloud Events Guide](../../MULTI_CLOUD_EVENTS_GUIDE.md)

## 🔗 Related

- [Multi-Cloud API Samples](../README.md)
- [Main Documentation](../../README.md)
