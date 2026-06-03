# AWS SQS Message Handler Examples

## Complete Working Examples

### Example 1: Simple Order Processing

#### Message Definition
```csharp
using AppFactory.Framework.Messaging.Abstractions;

namespace OrderService.Messages;

public class OrderCreatedMessage : Message
{
    // Inherits: MessageId, Body, Properties, EnqueuedTimeUtc, DeliveryCount
}

public class OrderData
{
    public string OrderId { get; set; }
    public string CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItem> Items { get; set; }
    public DateTime OrderDate { get; set; }
}

public class OrderItem
{
    public string ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
```

#### Message Handler
```csharp
using AppFactory.Framework.Messaging.Abstractions;
using AppFactory.Framework.Logging;
using System.Text.Json;

namespace OrderService.Handlers;

public class OrderCreatedHandler : IMessageHandler<OrderCreatedMessage>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryService _inventoryService;
    private readonly IPaymentService _paymentService;
    private readonly INotificationService _notificationService;
    private readonly ILogger _logger;

    public OrderCreatedHandler(
        IOrderRepository orderRepository,
        IInventoryService inventoryService,
        IPaymentService paymentService,
        INotificationService notificationService,
        ILogger logger)
    {
        _orderRepository = orderRepository;
        _inventoryService = inventoryService;
        _paymentService = paymentService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task HandleAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
    {
        _logger.LogInfo($"Processing order created message {message.MessageId}");

        // Deserialize order data
        var orderData = JsonSerializer.Deserialize<OrderData>(message.Body);
        
        _logger.LogInfo($"Processing order {orderData.OrderId} for customer {orderData.CustomerId}");

        // Check idempotency
        if (await _orderRepository.ExistsAsync(orderData.OrderId, cancellationToken))
        {
            _logger.LogInfo($"Order {orderData.OrderId} already exists - skipping");
            return;
        }

        // Reserve inventory
        foreach (var item in orderData.Items)
        {
            await _inventoryService.ReserveAsync(
                item.ProductId, 
                item.Quantity, 
                orderData.OrderId, 
                cancellationToken);
        }

        // Process payment
        var paymentResult = await _paymentService.ChargeAsync(
            orderData.CustomerId,
            orderData.TotalAmount,
            orderData.OrderId,
            cancellationToken);

        if (!paymentResult.Success)
        {
            _logger.LogError($"Payment failed for order {orderData.OrderId}");
            await _inventoryService.ReleaseReservationAsync(orderData.OrderId, cancellationToken);
            throw new PaymentFailedException($"Payment failed: {paymentResult.Error}");
        }

        // Save order
        var order = new Order
        {
            Id = orderData.OrderId,
            CustomerId = orderData.CustomerId,
            Items = orderData.Items.Select(i => new OrderItemEntity(i)).ToList(),
            TotalAmount = orderData.TotalAmount,
            Status = OrderStatus.Confirmed,
            CreatedAt = orderData.OrderDate,
            PaymentId = paymentResult.PaymentId
        };

        await _orderRepository.SaveAsync(order, cancellationToken);

        // Send confirmation
        await _notificationService.SendOrderConfirmationAsync(
            orderData.CustomerId,
            orderData.OrderId,
            cancellationToken);

        _logger.LogInfo($"Order {orderData.OrderId} processed successfully");
    }
}
```

#### Lambda Function
```csharp
using AppFactory.Framework.Messaging.Aws.Handlers;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace OrderService.Lambda;

public class ProcessOrderCreatedFunction : SqsMessageHandlerBase<OrderCreatedMessage>
{
    public ProcessOrderCreatedFunction() : base(new Startup())
    {
    }

    protected override IStartup GetStartup() => new Startup();

    public async Task<SQSBatchResponse> FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
        => await Handle(sqsEvent, context);
}
```

#### Startup Configuration
```csharp
using AppFactory.Framework.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using AppFactory.Framework.Logging.Serilog;

namespace OrderService;

public class Startup : IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Message handler
        services.AddScoped<IMessageHandler<OrderCreatedMessage>, OrderCreatedHandler>();

        // Business services
        services.AddScoped<IOrderRepository, DynamoDbOrderRepository>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IPaymentService, StripePaymentService>();
        services.AddScoped<INotificationService, SnsNotificationService>();

        // AWS clients
        services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient());
        services.AddSingleton<IAmazonSimpleNotificationService>(_ => new AmazonSimpleNotificationServiceClient());

        // Logging
        services.AddSerilogLogging();
    }
}
```

### Example 2: Event-Driven Saga with Compensation

#### Message Handler with Saga Pattern
```csharp
public class OrderFulfilmentHandler : IMessageHandler<OrderFulfilmentMessage>
{
    private readonly ISagaOrchestrator _sagaOrchestrator;
    private readonly ILogger _logger;

    public OrderFulfilmentHandler(ISagaOrchestrator sagaOrchestrator, ILogger logger)
    {
        _sagaOrchestrator = sagaOrchestrator;
        _logger = logger;
    }

    public async Task HandleAsync(OrderFulfilmentMessage message, CancellationToken cancellationToken)
    {
        var orderData = JsonSerializer.Deserialize<OrderData>(message.Body);
        
        _logger.LogInfo($"Starting fulfilment saga for order {orderData.OrderId}");

        var saga = new OrderFulfilmentSaga(orderData);

        try
        {
            // Step 1: Reserve inventory
            await saga.ReserveInventoryAsync(cancellationToken);
            
            // Step 2: Charge payment
            await saga.ChargePaymentAsync(cancellationToken);
            
            // Step 3: Schedule shipping
            await saga.ScheduleShippingAsync(cancellationToken);
            
            // Step 4: Send confirmation
            await saga.SendConfirmationAsync(cancellationToken);

            await _sagaOrchestrator.CompleteAsync(saga, cancellationToken);
            
            _logger.LogInfo($"Fulfilment saga completed for order {orderData.OrderId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Saga failed for order {orderData.OrderId} - starting compensation");
            
            // Compensate in reverse order
            await saga.CompensateAsync(cancellationToken);
            
            throw;
        }
    }
}
```

### Example 3: Dead Letter Queue Processing

#### DLQ Message Handler
```csharp
public class DeadLetterQueueHandler : IMessageHandler<OrderCreatedMessage>
{
    private readonly IDeadLetterRepository _dlqRepository;
    private readonly IAlertingService _alertingService;
    private readonly ILogger _logger;

    public DeadLetterQueueHandler(
        IDeadLetterRepository dlqRepository,
        IAlertingService alertingService,
        ILogger logger)
    {
        _dlqRepository = dlqRepository;
        _alertingService = alertingService;
        _logger = logger;
    }

    public async Task HandleAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
    {
        _logger.LogWarning($"Processing DLQ message {message.MessageId}, delivery count: {message.DeliveryCount}");

        // Extract failure information
        var failureReason = message.Properties.GetValueOrDefault("FailureReason", "Unknown");
        var originalQueueArn = message.Properties.GetValueOrDefault("EventSourceARN");
        
        // Store for analysis
        var dlqEntry = new DeadLetterEntry
        {
            MessageId = message.MessageId,
            Body = message.Body,
            FailureReason = failureReason,
            DeliveryCount = message.DeliveryCount,
            OriginalQueue = originalQueueArn,
            ReceivedAt = DateTime.UtcNow,
            Properties = message.Properties
        };

        await _dlqRepository.SaveAsync(dlqEntry, cancellationToken);

        // Alert ops team
        await _alertingService.SendAlertAsync(
            $"DLQ Message: {message.MessageId}",
            $"Delivery attempts: {message.DeliveryCount}, Reason: {failureReason}",
            AlertLevel.Warning,
            cancellationToken);

        _logger.LogInfo($"DLQ message {message.MessageId} processed and stored");
    }
}
```

#### DLQ Lambda Function
```csharp
public class ProcessDlqFunction : SqsMessageHandlerBase<OrderCreatedMessage>
{
    public ProcessDlqFunction() : base(new DlqStartup())
    {
    }

    protected override IStartup GetStartup() => new DlqStartup();

    public async Task<SQSBatchResponse> FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
        => await Handle(sqsEvent, context);
}

public class DlqStartup : IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IMessageHandler<OrderCreatedMessage>, DeadLetterQueueHandler>();
        services.AddScoped<IDeadLetterRepository, DynamoDbDeadLetterRepository>();
        services.AddScoped<IAlertingService, SnsAlertingService>();
        services.AddSerilogLogging();
    }
}
```

### Example 4: FIFO Queue with Message Deduplication

#### FIFO Handler
```csharp
public class OrderStatusUpdateHandler : IMessageHandler<OrderStatusUpdateMessage>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger _logger;

    public OrderStatusUpdateHandler(IOrderRepository orderRepository, ILogger logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task HandleAsync(OrderStatusUpdateMessage message, CancellationToken cancellationToken)
    {
        // Extract FIFO metadata
        var messageGroupId = message.Properties.GetValueOrDefault("SQS_MessageGroupId");
        var sequenceNumber = message.Properties.GetValueOrDefault("SQS_SequenceNumber");
        var deduplicationId = message.Properties.GetValueOrDefault("SQS_MessageDeduplicationId");

        _logger.LogInfo($"Processing status update. Group: {messageGroupId}, Sequence: {sequenceNumber}");

        var statusUpdate = JsonSerializer.Deserialize<OrderStatusUpdate>(message.Body);

        // Messages in same group (same order) are processed in order
        await _orderRepository.UpdateStatusAsync(
            statusUpdate.OrderId,
            statusUpdate.NewStatus,
            statusUpdate.Timestamp,
            sequenceNumber,
            cancellationToken);

        _logger.LogInfo($"Order {statusUpdate.OrderId} status updated to {statusUpdate.NewStatus}");
    }
}
```

#### FIFO Queue Configuration
```yaml
resources:
  Resources:
    OrderStatusUpdatesQueue:
      Type: AWS::SQS::Queue
      Properties:
        QueueName: OrderStatusUpdates.fifo
        FifoQueue: true
        ContentBasedDeduplication: true  # Auto-deduplication based on message body
        DeduplicationScope: messageGroup  # Per message group
        FifoThroughputLimit: perMessageGroupId  # High throughput mode
```

### Example 5: Batch Processing with Custom Retry Logic

#### Batch Handler with Retry Strategy
```csharp
public class BulkOrderProcessorHandler : IMessageHandler<BulkOrderMessage>
{
    private readonly IOrderService _orderService;
    private readonly ILogger _logger;

    public BulkOrderProcessorHandler(IOrderService orderService, ILogger logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public async Task HandleAsync(BulkOrderMessage message, CancellationToken cancellationToken)
    {
        var deliveryCount = message.DeliveryCount;
        
        // Exponential backoff for retries
        if (deliveryCount > 1)
        {
            var delayMs = Math.Min(1000 * Math.Pow(2, deliveryCount - 1), 30000);
            _logger.LogInfo($"Retry #{deliveryCount} - waiting {delayMs}ms before processing");
            await Task.Delay((int)delayMs, cancellationToken);
        }

        var bulkOrder = JsonSerializer.Deserialize<BulkOrderData>(message.Body);

        try
        {
            // Process with timeout based on Lambda remaining time
            using var processingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            processingCts.CancelAfter(TimeSpan.FromSeconds(25)); // Leave 5s for Lambda overhead

            await _orderService.ProcessBulkOrderAsync(bulkOrder, processingCts.Token);
            
            _logger.LogInfo($"Bulk order {bulkOrder.BatchId} processed successfully");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning($"Processing timeout for bulk order {bulkOrder.BatchId}");
            throw; // Will retry
        }
        catch (TransientDatabaseException ex)
        {
            _logger.LogWarning($"Transient error (attempt {deliveryCount}): {ex.Message}");
            
            if (deliveryCount >= 5)
            {
                _logger.LogError("Max retries exceeded - moving to DLQ");
            }
            throw; // Will retry or go to DLQ
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, $"Validation error for bulk order {bulkOrder.BatchId} - won't retry");
            // Don't throw - permanent error, mark as processed
            await LogPermanentFailure(message, ex, cancellationToken);
        }
    }

    private async Task LogPermanentFailure(BulkOrderMessage message, Exception ex, CancellationToken ct)
    {
        // Log to separate tracking table instead of retrying
        await _orderService.LogFailedBulkOrderAsync(
            message.MessageId,
            message.Body,
            ex.Message,
            ct);
    }
}
```

### Example 6: Message Enrichment and Transformation

#### Enrichment Handler
```csharp
public class OrderEnrichmentHandler : IMessageHandler<OrderCreatedMessage>
{
    private readonly ICustomerService _customerService;
    private readonly IProductCatalogService _catalogService;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger _logger;

    public OrderEnrichmentHandler(
        ICustomerService customerService,
        IProductCatalogService catalogService,
        IMessagePublisher messagePublisher,
        ILogger logger)
    {
        _customerService = customerService;
        _catalogService = catalogService;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task HandleAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
    {
        var orderData = JsonSerializer.Deserialize<OrderData>(message.Body);
        
        _logger.LogInfo($"Enriching order {orderData.OrderId}");

        // Enrich with customer data
        var customer = await _customerService.GetCustomerAsync(orderData.CustomerId, cancellationToken);
        
        // Enrich with product details
        var enrichedItems = new List<EnrichedOrderItem>();
        foreach (var item in orderData.Items)
        {
            var product = await _catalogService.GetProductAsync(item.ProductId, cancellationToken);
            enrichedItems.Add(new EnrichedOrderItem
            {
                ProductId = item.ProductId,
                ProductName = product.Name,
                Category = product.Category,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TaxRate = product.TaxRate
            });
        }

        // Create enriched order
        var enrichedOrder = new EnrichedOrderData
        {
            OrderId = orderData.OrderId,
            Customer = new CustomerInfo
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Tier = customer.Tier
            },
            Items = enrichedItems,
            TotalAmount = orderData.TotalAmount,
            OrderDate = orderData.OrderDate
        };

        // Publish enriched order to next queue
        var enrichedMessage = new Message
        {
            Body = JsonSerializer.Serialize(enrichedOrder),
            Properties = new Dictionary<string, string>(message.Properties)
            {
                ["OriginalMessageId"] = message.MessageId,
                ["EnrichedAt"] = DateTime.UtcNow.ToString("O")
            }
        };

        await _messagePublisher.PublishAsync("enriched-orders-queue", enrichedMessage, cancellationToken);

        _logger.LogInfo($"Order {orderData.OrderId} enriched and published");
    }
}
```

### Example 7: Multi-Step Workflow with State Machine

```csharp
public class OrderWorkflowHandler : IMessageHandler<OrderWorkflowMessage>
{
    private readonly IWorkflowStateRepository _stateRepository;
    private readonly IStepExecutor _stepExecutor;
    private readonly ILogger _logger;

    public OrderWorkflowHandler(
        IWorkflowStateRepository stateRepository,
        IStepExecutor stepExecutor,
        ILogger logger)
    {
        _stateRepository = stateRepository;
        _stepExecutor = stepExecutor;
        _logger = logger;
    }

    public async Task HandleAsync(OrderWorkflowMessage message, CancellationToken cancellationToken)
    {
        var workflowData = JsonSerializer.Deserialize<WorkflowData>(message.Body);
        
        // Load or create workflow state
        var state = await _stateRepository.GetOrCreateAsync(workflowData.WorkflowId, cancellationToken)
            ?? new WorkflowState
            {
                WorkflowId = workflowData.WorkflowId,
                CurrentStep = WorkflowStep.ValidateOrder,
                Data = workflowData
            };

        _logger.LogInfo($"Executing workflow {state.WorkflowId}, step: {state.CurrentStep}");

        try
        {
            // Execute current step
            var result = await _stepExecutor.ExecuteAsync(state.CurrentStep, state.Data, cancellationToken);

            if (result.Success)
            {
                // Move to next step
                state.CurrentStep = result.NextStep;
                state.CompletedSteps.Add(result.CompletedStep);
                
                if (state.CurrentStep == WorkflowStep.Completed)
                {
                    _logger.LogInfo($"Workflow {state.WorkflowId} completed successfully");
                    await _stateRepository.MarkCompletedAsync(state, cancellationToken);
                }
                else
                {
                    // Save state and continue processing
                    await _stateRepository.SaveAsync(state, cancellationToken);
                    
                    // Could publish message to continue workflow asynchronously
                    // await PublishContinueWorkflowMessage(state, cancellationToken);
                }
            }
            else
            {
                _logger.LogWarning($"Workflow step {state.CurrentStep} failed: {result.Error}");
                throw new WorkflowStepFailedException(result.Error);
            }
        }
        catch (Exception ex)
        {
            state.Failures.Add(new WorkflowFailure
            {
                Step = state.CurrentStep,
                Error = ex.Message,
                Timestamp = DateTime.UtcNow,
                DeliveryCount = message.DeliveryCount
            });
            
            await _stateRepository.SaveAsync(state, cancellationToken);
            throw;
        }
    }
}
```

## serverless.yml Complete Example

```yaml
service: order-processing-service

provider:
  name: aws
  runtime: dotnet10
  region: us-east-1
  memorySize: 512
  timeout: 30
  environment:
    ASPNETCORE_ENVIRONMENT: ${opt:stage, 'dev'}
    LOG_LEVEL: Info
  iamRoleStatements:
    - Effect: Allow
      Action:
        - dynamodb:PutItem
        - dynamodb:GetItem
        - dynamodb:UpdateItem
        - dynamodb:Query
      Resource:
        - !GetAtt OrdersTable.Arn
    - Effect: Allow
      Action:
        - sqs:SendMessage
      Resource:
        - !GetAtt EnrichedOrdersQueue.Arn

functions:
  processOrderCreated:
    handler: OrderService.Lambda::OrderService.Lambda.ProcessOrderCreatedFunction::FunctionHandler
    events:
      - sqs:
          arn: !GetAtt OrderCreatedQueue.Arn
          batchSize: 10
          maximumBatchingWindowInSeconds: 5
          functionResponseType: ReportBatchItemFailures

  processEnrichedOrders:
    handler: OrderService.Lambda::OrderService.Lambda.ProcessEnrichedOrdersFunction::FunctionHandler
    events:
      - sqs:
          arn: !GetAtt EnrichedOrdersQueue.Arn
          batchSize: 5
          functionResponseType: ReportBatchItemFailures

  processDlq:
    handler: OrderService.Lambda::OrderService.Lambda.ProcessDlqFunction::FunctionHandler
    events:
      - sqs:
          arn: !GetAtt OrderCreatedDLQ.Arn
          batchSize: 1

resources:
  Resources:
    OrderCreatedQueue:
      Type: AWS::SQS::Queue
      Properties:
        QueueName: ${self:service}-order-created-${opt:stage, 'dev'}
        VisibilityTimeout: 60
        MessageRetentionPeriod: 345600  # 4 days
        ReceiveMessageWaitTimeSeconds: 20  # Long polling
        RedrivePolicy:
          deadLetterTargetArn: !GetAtt OrderCreatedDLQ.Arn
          maxReceiveCount: 3

    OrderCreatedDLQ:
      Type: AWS::SQS::Queue
      Properties:
        QueueName: ${self:service}-order-created-dlq-${opt:stage, 'dev'}
        MessageRetentionPeriod: 1209600  # 14 days

    EnrichedOrdersQueue:
      Type: AWS::SQS::Queue
      Properties:
        QueueName: ${self:service}-enriched-orders-${opt:stage, 'dev'}
        VisibilityTimeout: 60

    OrdersTable:
      Type: AWS::DynamoDB::Table
      Properties:
        TableName: ${self:service}-orders-${opt:stage, 'dev'}
        BillingMode: PAY_PER_REQUEST
        AttributeDefinitions:
          - AttributeName: orderId
            AttributeType: S
        KeySchema:
          - AttributeName: orderId
            KeyType: HASH
```

## Testing Examples

See [README.md](./README.md#testing) for comprehensive testing examples.
