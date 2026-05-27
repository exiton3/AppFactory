# AppFactory.Framework.DataAccess.CosmosDB

A comprehensive data access layer for Azure Cosmos DB following the Repository pattern with fluent configuration.

## Overview

This package provides a complete abstraction over Azure Cosmos DB, enabling you to work with Cosmos DB using the same patterns as the DynamoDB implementation. Designed for serverless, event-driven applications on Azure.

## Features

- **🎯 Generic Repository Pattern** - Type-safe data access with minimal boilerplate
- **🔧 Fluent Configuration** - Intuitive fluent API for model configuration
- **📦 Partition Key Support** - Single and hierarchical partition keys (up to 3 levels)
- **🌳 Hierarchical Partition Keys** - Improved query performance and data distribution
- **⚡ Batch Operations** - Transactional batches with automatic partition grouping
- **🔍 SQL Query Support** - Parameterized SQL queries
- **⏱️ TTL Support** - Document-level Time-To-Live
- **🔄 Automatic Serialization** - camelCase JSON conventions
- **♻️ Singleton Client** - Optimized CosmosClient pattern
- **📊 Performance Logging** - Built-in tracking

## Installation

```bash
dotnet add package AppFactory.Framework.DataAccess.CosmosDB
```

## Quick Start

### 1. Configuration (appsettings.json)

```json
{
  "CosmosDb": {
    "ConnectionString": "AccountEndpoint=https://...;AccountKey=...;",
    "DatabaseName": "YourDatabase"
  }
}
```

### 2. Register Services

```csharp
using AppFactory.Framework.DataAccess.CosmosDB;

services.RegisterCosmosDbPersistence();
services.RegisterModelConfig<UserModelConfig, User>();
```

### 3. Define Model & Configuration

```csharp
public class User
{
    public string Id { get; set; }
    public string TenantId { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
}

public class UserModelConfig : IModelConfig<User>
{
    public void Configure(IModelConfigOptions<User> options)
    {
        options
            .ContainerName("Users")
            .Id(u => u.Id)
            .PartitionKey(u => u.TenantId)
            .IdPrefix("USER")
            .PartitionKeyPrefix("TENANT");
    }
}
```

### 4. Create Repository

```csharp
public class UserRepository : RepositoryBase<User>
{
    public UserRepository(
        ICosmosDbClientFactory cosmosDbFactory,
        ILogger logger,
        IModelConfig<User> modelConfig)
        : base(cosmosDbFactory, logger, modelConfig)
    {
    }

    public async Task<User> GetByEmail(string email)
    {
        var query = $"SELECT * FROM c WHERE c.email = '{email}'";
        return await QuerySingle(query);
    }
}
```

## Usage Examples

### CRUD Operations

```csharp
// Create
var user = new User { Id = Guid.NewGuid().ToString(), Email = "user@example.com" };
await userRepository.Add(user);

// Read
var user = await userRepository.GetById(userId);

// Update
user.Name = "Updated Name";
await userRepository.Update(user);

// Upsert
await userRepository.Upsert(user);

// Delete
await userRepository.Delete(userId);

// Batch
var users = new List<User> { user1, user2, user3 };
await userRepository.BatchAddItems(users);
```

### Querying

```csharp
// Simple query
var users = await Query("SELECT * FROM c WHERE c.isActive = true");

// Parameterized query
var query = CreateQuery("SELECT * FROM c WHERE c.email = @email")
    .WithParameter("@email", "user@example.com");
var user = await QuerySingle(query);

// Partition-specific (faster)
var tenantUsers = await Query(
    "SELECT * FROM c WHERE c.isActive = true",
    partitionKey: "tenant-123");
```

## Configuration Options

```csharp
options
    .ContainerName("Users")              // Required
    .Id(u => u.Id)                        // Required
    .PartitionKey(u => u.TenantId)       // Required
    .IdPrefix("USER")                     // Optional
    .PartitionKeyPrefix("TENANT")        // Optional
    .PartitionKeyPath("/partitionKey")   // Optional (default: /partitionKey)
    .TimeToLive(3600);                   // Optional (seconds)
```

### Hierarchical Partition Keys (NEW)

Azure Cosmos DB supports up to **3 hierarchical partition key paths** for improved query performance and better data distribution.

**Benefits:**
- Better data distribution across partitions
- More efficient querying with multiple filters
- Reduced hot partitions

**Example Configuration:**

```csharp
public class Order
{
    public string Id { get; set; }
    public string TenantId { get; set; }
    public string UserId { get; set; }
    public string Category { get; set; }
    public decimal Amount { get; set; }
}

public class OrderModelConfig : IModelConfig<Order>
{
    public void Configure(IModelConfigOptions<Order> options)
    {
        options
            .ContainerName("Orders")
            .Id(o => o.Id)
            .IdPrefix("ORDER")
            // Define partition keys with fluent interface - property names auto-derived
            .PartitionKey(o => o.TenantId).WithPropertyName("tenantId").WithPrefix("TENANT")
            .PartitionKey(o => o.UserId).WithPropertyName("userId").WithPrefix("USER")
            .PartitionKey(o => o.Category).WithPropertyName("category");
    }
}
```

**Resulting Document:**

```json
{
  "id": "ORDER#12345",
  "tenantId": "TENANT#tenant-abc",
  "userId": "USER#user-123",
  "category": "Electronics",
  "amount": 299.99
}
```

**Querying with Hierarchical Partition Keys:**

```csharp
// Query within specific partition (most efficient)
var orders = await Query(
    "SELECT * FROM c WHERE c.amount > 100",
    partitionKey: "tenant-abc"); // Uses first level

// Cross-partition query (less efficient)
var allOrders = await Query("SELECT * FROM c WHERE c.category = 'Electronics'");

// Get by ID with hierarchical partition key
var documentKey = _config.GetDocumentKey(orderId, "tenant-abc", "user-123", "Electronics");
var order = await GetByDocumentKey(documentKey);
```

**Single vs Hierarchical Partition Keys:**

```csharp
// Single partition key
options
    .PartitionKey(u => u.TenantId).WithPropertyName("partitionKey").WithPrefix("TENANT");

// Hierarchical partition keys (up to 3 levels)
options
    .PartitionKey(u => u.TenantId).WithPropertyName("tenantId").WithPrefix("TENANT")
    .PartitionKey(u => u.UserId).WithPropertyName("userId").WithPrefix("USER")
    .PartitionKey(u => u.Category).WithPropertyName("category");
```

### Prefix Pattern

```csharp
// With prefixes:
// id: "USER#123"
// partitionKey: "TENANT#tenant-abc"
```

## Document Structure

```json
{
  "id": "USER#123",
  "partitionKey": "TENANT#tenant-abc",
  "email": "user@example.com",
  "name": "John Doe",
  "ttl": 86400
}
```

## Best Practices

### Partition Key Design

```csharp
// ✅ Good: High cardinality
options.PartitionKey(u => u.TenantId);

// ❌ Bad: Low cardinality
options.PartitionKey(u => u.IsActive);
```

### Query Optimization

```csharp
// ✅ Optimized: Include partition key
var users = await Query(query, partitionKey: tenantId);

// ⚠️ Less optimal: Cross-partition
var users = await Query(query);
```

### Use Parameterized Queries

```csharp
// ✅ Safe
var query = CreateQuery("SELECT * FROM c WHERE c.email = @email")
    .WithParameter("@email", email);

// ❌ Unsafe
var query = $"SELECT * FROM c WHERE c.email = '{email}'";
```

## DynamoDB vs CosmosDB

| Feature | DynamoDB | CosmosDB |
|---------|----------|----------|
| **Primary Key** | PK + SK | id + partitionKey |
| **Config** | `PK()`, `SK()` | `Id()`, `PartitionKey()` |
| **Model** | Single table | Container per type |
| **Query** | Key conditions | SQL syntax |
| **Indexes** | GSI/LSI | Automatic |
| **TTL** | Table-level | Document-level |
| **Batch** | 25 items | 100 per partition |

## Error Handling

```csharp
using Microsoft.Azure.Cosmos;

try
{
    await userRepository.Add(user);
}
catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
{
    // Duplicate document
}
catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
{
    // Rate limiting (429)
    await Task.Delay(ex.RetryAfter ?? TimeSpan.FromSeconds(1));
}
```

## Repository Methods

### From IRepository<TModel>

- `GetById<TKey>(TKey key)`
- `Add(TModel model)`
- `Update(TModel model)`
- `Delete<TKey>(TKey key)`
- `BatchAddItems(IEnumerable<TModel> models)`

### Additional in RepositoryBase<TModel>

- `Upsert(TModel model)`
- `Query(string query, string partitionKey = null)`
- `Query(QueryDefinition queryDef, string partitionKey = null)`
- `QuerySingle(string query, string partitionKey = null)`
- `CreateQuery(string query)`

## Advanced Example

```csharp
public class OrderRepository : RepositoryBase<Order>
{
    public OrderRepository(
        ICosmosDbClientFactory factory,
        ILogger logger,
        IModelConfig<Order> config)
        : base(factory, logger, config)
    {
    }

    public async Task<IEnumerable<Order>> GetRecentOrders(
        string customerId,
        int days)
    {
        var query = CreateQuery(@"
            SELECT * FROM c 
            WHERE c.orderDate >= @startDate
            ORDER BY c.orderDate DESC")
            .WithParameter("@startDate", DateTime.UtcNow.AddDays(-days));

        return await Query(query, partitionKey: customerId);
    }
}
```

## License

Copyright © Sergey Kichuk. All rights reserved. Licensed under the MIT License.

## Related

- [AppFactory.Framework.DataAccess.DynamoDB](../AppFactory.Framework.DataAccess.DynamoDB) - AWS DynamoDB
- [AppFactory](../../README.md) - Main framework documentation

---

**Built with ❤️ for Azure serverless applications**
