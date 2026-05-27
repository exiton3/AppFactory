using System.Text.Json;
using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using AppFactory.Framework.DataAccess.CosmosDB.Mapping;
using Xunit;

namespace AppFactory.Framework.DataAccess.CosmosDB.UnitTests;

public class HierarchicalPartitionKeyTests
{
    [Fact]
    public void HierarchicalPartitionKey_ShouldMapCorrectly()
    {
        // Arrange
        var config = new OrderModelConfig();
        var cosmosDbModelConfig = new CosmosDbModelConfig<Order>();
        config.Configure(cosmosDbModelConfig);

        IModelMapper<Order> mapper = new ModelMapper<Order>(cosmosDbModelConfig);

        var order = new Order
        {
            Id = "12345",
            TenantId = "tenant-abc",
            UserId = "user-123",
            Category = "Electronics",
            Amount = 299.99m
        };

        // Act
        var document = mapper.MapToDocument(order);

        // Assert
        Assert.Equal("ORDER#12345", document["id"]);
        Assert.Equal("TENANT#tenant-abc", document["tenantId"]);
        Assert.Equal("USER#user-123", document["userId"]);
        Assert.Equal("Electronics", document["category"]);

        var amountElement = (JsonElement)document["amount"];
        Assert.Equal(299.99m, amountElement.GetDecimal());
    }

    [Fact]
    public void HierarchicalPartitionKey_DocumentKey_ShouldHaveMultipleValues()
    {
        // Arrange
        var config = new OrderModelConfig();
        var cosmosDbModelConfig = new CosmosDbModelConfig<Order>();
        config.Configure(cosmosDbModelConfig);

        var order = new Order
        {
            Id = "12345",
            TenantId = "tenant-abc",
            UserId = "user-123",
            Category = "Electronics"
        };

        // Act
        var documentKey = cosmosDbModelConfig.GetDocumentKey(order);

        // Assert
        Assert.Equal("ORDER#12345", documentKey.Id);
        Assert.Equal(3, documentKey.PartitionKeyValues.Count);
        Assert.Equal("TENANT#tenant-abc", documentKey.PartitionKeyValues[0]);
        Assert.Equal("USER#user-123", documentKey.PartitionKeyValues[1]);
        Assert.Equal("Electronics", documentKey.PartitionKeyValues[2]);
    }

    [Fact]
    public void HierarchicalPartitionKey_ToPartitionKey_ShouldBuildCorrectly()
    {
        // Arrange
        var config = new OrderModelConfig();
        var cosmosDbModelConfig = new CosmosDbModelConfig<Order>();
        config.Configure(cosmosDbModelConfig);

        var order = new Order
        {
            Id = "12345",
            TenantId = "tenant-abc",
            UserId = "user-123",
            Category = "Electronics"
        };

        var documentKey = cosmosDbModelConfig.GetDocumentKey(order);

        // Act
        var partitionKey = documentKey.ToPartitionKey();

        // Assert
        Assert.NotNull(partitionKey);
    }

    [Fact]
    public void HierarchicalPartitionKey_MapFromDocument_ShouldRemovePartitionKeyProperties()
    {
        // Arrange
        var config = new OrderModelConfig();
        var cosmosDbModelConfig = new CosmosDbModelConfig<Order>();
        config.Configure(cosmosDbModelConfig);

        IModelMapper<Order> mapper = new ModelMapper<Order>(cosmosDbModelConfig);

        var order = new Order
        {
            Id = "12345",
            TenantId = "tenant-abc",
            UserId = "user-123",
            Category = "Electronics",
            Amount = 299.99m
        };

        var document = mapper.MapToDocument(order);

        // Act
        var mappedOrder = mapper.MapFromDocument(document);

        // Assert
        Assert.Equal(order.Id, mappedOrder.Id);
        Assert.Equal(order.TenantId, mappedOrder.TenantId);
        Assert.Equal(order.UserId, mappedOrder.UserId);
        Assert.Equal(order.Category, mappedOrder.Category);
        Assert.Equal(order.Amount, mappedOrder.Amount);
    }

    [Fact]
    public void HierarchicalPartitionKey_IsHierarchical_ShouldBeTrue()
    {
        // Arrange
        var config = new OrderModelConfig();
        var cosmosDbModelConfig = new CosmosDbModelConfig<Order>();
        config.Configure(cosmosDbModelConfig);

        // Assert
        Assert.True(cosmosDbModelConfig.IsHierarchicalPartitionKey);
        Assert.Equal(3, cosmosDbModelConfig.PartitionKeyPaths.Count);
        Assert.Equal("/tenantId", cosmosDbModelConfig.PartitionKeyPaths[0]);
        Assert.Equal("/userId", cosmosDbModelConfig.PartitionKeyPaths[1]);
        Assert.Equal("/category", cosmosDbModelConfig.PartitionKeyPaths[2]);
    }

    [Fact]
    public void SinglePartitionKey_IsHierarchical_ShouldBeFalse()
    {
        // Arrange
        var config = new UserModelConfig();
        var cosmosDbModelConfig = new CosmosDbModelConfig<User>();
        config.Configure(cosmosDbModelConfig);

        // Assert
        Assert.False(cosmosDbModelConfig.IsHierarchicalPartitionKey);
    }

    [Fact]
    public void HierarchicalPartitionKey_ExceedsThreeLevels_ShouldThrowException()
    {
        // Arrange
        var cosmosDbModelConfig = new CosmosDbModelConfig<Order>();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            cosmosDbModelConfig
                .AddPartitionKey(o => o.TenantId)
                .AddPartitionKey(o => o.UserId)
                .AddPartitionKey(o => o.Category)
                .AddPartitionKey(o => o.Id); // 4th level - should throw
        });

        Assert.Contains("up to 3", exception.Message);
    }


    [Fact]
    public void GetDocumentKeyWithValues_ShouldCreateCorrectKey()
    {
        // Arrange
        var config = new OrderModelConfig();
        var cosmosDbModelConfig = new CosmosDbModelConfig<Order>();
        config.Configure(cosmosDbModelConfig);

        // Act
        var documentKey = cosmosDbModelConfig.GetDocumentKey("12345", "tenant-abc", "user-123", "Electronics");

        // Assert
        Assert.Equal("ORDER#12345", documentKey.Id);
        Assert.Equal(3, documentKey.PartitionKeyValues.Count);
        Assert.Equal("TENANT#tenant-abc", documentKey.PartitionKeyValues[0]);
        Assert.Equal("USER#user-123", documentKey.PartitionKeyValues[1]);
        Assert.Equal("Electronics", documentKey.PartitionKeyValues[2]);
    }

    [Fact]
    public void FluentPartitionKeyConfig_WithMultipleKeys_ShouldConfigureCorrectly()
    {
        // Arrange
        var cosmosDbModelConfig = new CosmosDbModelConfig<User>();

        // Act - Using fluent interface for each partition key
        cosmosDbModelConfig
            .ContainerName("Users")
            .Id(u => u.Id)
            .IdPrefix("USER")
            .PartitionKey(u => u.TenantId).WithPropertyName("tenantId").WithPrefix("TENANT")
            .PartitionKey(u => u.UserId).WithPropertyName("userId").WithPrefix("USER");

        var mapper = new ModelMapper<User>(cosmosDbModelConfig);
        var user = new User
        {
            Id = "123",
            TenantId = "tenant-abc",
            UserId = "user-456",
            Email = "test@test.com",
            Name = "Test User"
        };

        // Act
        var document = mapper.MapToDocument(user);

        // Assert
        Assert.Equal("USER#123", document["id"]);
        Assert.Equal("TENANT#tenant-abc", document["tenantId"]);
        Assert.Equal("USER#user-456", document["userId"]);

        // Verify it's hierarchical
        Assert.True(cosmosDbModelConfig.IsHierarchicalPartitionKey);
    }

    [Fact]
    public void FluentPartitionKeyConfig_SingleKey_ShouldConfigureCorrectly()
    {
        // Arrange
        var cosmosDbModelConfig = new CosmosDbModelConfig<User>();

        // Act - Using fluent interface for single partition key
        cosmosDbModelConfig
            .ContainerName("Users")
            .Id(u => u.Id)
            .IdPrefix("USER")
            .PartitionKey(u => u.TenantId).WithPropertyName("partitionKey").WithPrefix("TENANT");

        var mapper = new ModelMapper<User>(cosmosDbModelConfig);
        var user = new User
        {
            Id = "123",
            TenantId = "tenant-abc",
            Email = "test@test.com",
            Name = "Test User"
        };

        // Act
        var document = mapper.MapToDocument(user);

        // Assert
        Assert.Equal("USER#123", document["id"]);
        Assert.Equal("TENANT#tenant-abc", document["partitionKey"]);

        // Single partition key is still hierarchical with 1 part
        Assert.True(cosmosDbModelConfig.IsHierarchicalPartitionKey);
    }
}

// Hierarchical partition key test models
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
            .PartitionKey(o => o.TenantId).WithPropertyName("tenantId").WithPrefix("TENANT")
            .PartitionKey(o => o.UserId).WithPropertyName("userId").WithPrefix("USER")
            .PartitionKey(o => o.Category).WithPropertyName("category");
    }
}
