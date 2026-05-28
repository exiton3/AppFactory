using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using AppFactory.Framework.DataAccess.CosmosDB.Mapping;
using AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;
using Xunit;

namespace AppFactory.Framework.DataAccess.CosmosDB.UnitTests;

public class IdPrefixStrippingTests
{
    [Fact]
    public void MapFromDocument_WithIdPrefix_ShouldDeserializeCorrectId()
    {
        // Arrange
        var config = new CosmosDbModelConfig<TestEntity>();
        config
            .ContainerName("TestEntities")
            .Id(e => e.Id)
            .IdPrefix("ENTITY")
            .PartitionKey(e => e.TenantId).WithName("partitionKey").WithPrefix("TENANT");

        var mapper = new ModelMapper<TestEntity>(config);

        var model = new TestEntity
        {
            Id = "123",
            TenantId = "tenant-abc",
            Name = "Test Entity"
        };

        // Act - Map to document and back
        var document = mapper.MapToDocument(model);
        var mappedModel = mapper.MapFromDocument(document);

        // Assert
        Assert.Equal("123", mappedModel.Id); // ID prefix should be stripped
        Assert.Equal("tenant-abc", mappedModel.TenantId); // Original value preserved
        Assert.Equal("Test Entity", mappedModel.Name);

        // Verify document has prefixes
        Assert.Equal("ENTITY#123", document["id"]);
        Assert.Equal("TENANT#tenant-abc", document["partitionKey"]);
    }

    [Fact]
    public void MapFromDocument_WithHierarchicalPartitionKeys_ShouldPreserveModelProperties()
    {
        // Arrange
        var config = new CosmosDbModelConfig<TestEntity>();
        config
            .ContainerName("TestEntities")
            .Id(e => e.Id)
            .IdPrefix("ENTITY")
            .PartitionKey(e => e.TenantId).WithName("tenantId").WithPrefix("TENANT")
            .PartitionKey(e => e.Category).WithName("category");

        var mapper = new ModelMapper<TestEntity>(config);

        var model = new TestEntity
        {
            Id = "123",
            TenantId = "tenant-abc",
            Category = "Electronics",
            Name = "Test Entity"
        };

        // Act - Map to document and back
        var document = mapper.MapToDocument(model);
        var mappedModel = mapper.MapFromDocument(document);

        // Assert
        Assert.Equal("123", mappedModel.Id); // ID prefix stripped
        Assert.Equal("tenant-abc", mappedModel.TenantId); // Original value preserved
        Assert.Equal("Electronics", mappedModel.Category); // Original value preserved
        Assert.Equal("Test Entity", mappedModel.Name);

        // Verify document has partition key properties with prefixes
        Assert.Equal("ENTITY#123", document["id"]);
        Assert.Equal("TENANT#tenant-abc", document["tenantId"]);
        Assert.Equal("Electronics", document["category"]);
    }

    public class TestEntity
    {
        public string Id { get; set; }
        public string TenantId { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
    }
}
