using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using AppFactory.Framework.DataAccess.CosmosDB.Mapping;
using AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;
using Xunit;

namespace AppFactory.Framework.DataAccess.CosmosDB.UnitTests
{
   public class ModelMapperTests
    {
        [Fact]
        public void UserModelConfig_ShouldConfigureCorrectly()
        {
            UserModelConfig config = new UserModelConfig();

            var cosmosDbModelConfig = new CosmosDbModelConfig<User>();
            config.Configure(cosmosDbModelConfig);

            IModelMapper<User> mapper = new ModelMapper<User>(cosmosDbModelConfig);

            var model = new User
            {
                Email = "some@mail.com",
                Name = "John Doe",
                Id = "123",
                UserId = "userID1"
            };

            var document = mapper.MapToDocument(model);

            Assert.NotNull(document);
            Assert.Equal("USER#123", document["id"]);
            Assert.Equal("USER#userID1", document["partitionKey"]);
        }

        [Fact]
        public void MultiplePartitionKeys_IsMappedToDocumentWithPrefixes()
        {
            UserModelConfig config = new UserModelConfig();

            var cosmosDbModelConfig = new CosmosDbModelConfig<User>();

            config.Configure(cosmosDbModelConfig);

            IModelMapper<User> mapper = new ModelMapper<User>(cosmosDbModelConfig);

            var model = new User
            {
                Email = "some@mail.com",
                Name = "John Doe",
                Id = "123",
                UserId = "userID1",
                TenantId = "tenantID1"
            };

            var document = mapper.MapToDocument(model);

            Assert.NotNull(document);
            Assert.Equal("USER#123", document["id"]);
            Assert.Equal("TENANT#tenantID1", document["tenantId"]);
            Assert.Equal("USER#userID1", document["partitionKey"]);
        }

        [Fact]
        public void MapFromDocument_EntityWithIdAndPrefix()
        {
            UserModelConfig config = new UserModelConfig();

            var cosmosDbModelConfig = new CosmosDbModelConfig<User>();
            config.Configure(cosmosDbModelConfig);

            IModelMapper<User> mapper = new ModelMapper<User>(cosmosDbModelConfig);

            var model = new User
            {
                Email = "some@mail.com",
                Name = "John Doe",
                Id = "123",
                UserId = "userID1"
            };

            var document = mapper.MapToDocument(model);

           var entity = mapper.MapFromDocument(document);

           Assert.NotNull(entity);
           Assert.Equal("123", entity.Id);
           Assert.Equal("userID1", entity.UserId);
        }

        [Fact]
        public void MapFromDocument_EntityWithMultiplePartitionKeyParts()
        {
            UserModelConfig config = new UserModelConfig();

            var cosmosDbModelConfig = new CosmosDbModelConfig<User>();
            config.Configure(cosmosDbModelConfig);

            IModelMapper<User> mapper = new ModelMapper<User>(cosmosDbModelConfig);

            var model = new User
            {
                Email = "some@mail.com",
                Name = "John Doe",
                Id = "123",
                UserId = "userID1",
                TenantId = "tenantID1"
            };

            var document = mapper.MapToDocument(model);

            var entity = mapper.MapFromDocument(document);

            Assert.NotNull(entity);
            Assert.Equal("123", entity.Id);
            Assert.Equal("userID1", entity.UserId);
            Assert.Equal("tenantID1", entity.TenantId);
        }

        [Fact]
        public void MapFromDocument_WithoutPrefixesAndCustomNames()
        {
            var options = new CosmosDbModelConfig<User>();
            options
                .ContainerName("Users")
                .Id(u => u.Id)
                .PartitionKey(u => u.TenantId)
                .PartitionKey(u => u.UserId);


            IModelMapper<User> mapper = new ModelMapper<User>(options);

            var model = new User
            {
                Email = "some@mail.com",
                Name = "John Doe",
                Id = "123",
                UserId = "userID1",
                TenantId = "tenantID1"
            };

            var document = mapper.MapToDocument(model);

            var entity = mapper.MapFromDocument(document);

            Assert.NotNull(entity);
            Assert.Equal("123", entity.Id);
            Assert.Equal("userID1", entity.UserId);
            Assert.Equal("tenantID1", entity.TenantId);
        }

        [Fact]
        public void MapToDocument_WithoutPrefixesAndCustomNames()
        {
            var options = new CosmosDbModelConfig<User>();
            options
                .ContainerName("Users")
                .Id(u => u.Id)
                .PartitionKey(u => u.TenantId)
                .PartitionKey(u => u.UserId);

            IModelMapper<User> mapper = new ModelMapper<User>(options);

            var model = new User
            {
                Email = "some@mail.com",
                Name = "John Doe",
                Id = "123",
                UserId = "userID1",
                TenantId = "tenantID1"
            };

            var document = mapper.MapToDocument(model);

            Assert.NotNull(document["id"]);
            Assert.Equal("123", document["id"]);
            Assert.Equal("userID1", document["userId"].ToString());
            Assert.Equal("tenantID1", document["tenantId"].ToString());
        }

        [Fact]
        public void MapToDocument_WithCustomNames()
        {
            var options = new CosmosDbModelConfig<User>();
            options
                .ContainerName("Users")
                .Id(u => u.Id)
                .PartitionKey(u => u.TenantId).WithPropertyName("customTenantId")
                .PartitionKey(u => u.UserId);

            IModelMapper<User> mapper = new ModelMapper<User>(options);

            var model = new User
            {
                Email = "some@mail.com",
                Name = "John Doe",
                Id = "123",
                UserId = "userID1",
                TenantId = "tenantID1"
            };

            var document = mapper.MapToDocument(model);

            Assert.False(document.ContainsKey("tenantId"));
            Assert.False(document.ContainsKey("TenantId"));
            Assert.Equal("tenantID1", document["customTenantId"].ToString());
            Assert.Equal("userID1", document["userId"].ToString());
        }

    }


    public class User
    {
        public string Id { get; set; }
        public string UserId { get; set; }
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
                .IdPrefix("USER")
                .PartitionKey(u => u.TenantId).WithPropertyName("tenantId").WithPrefix("TENANT")
                .PartitionKey(u => u.UserId).WithPropertyName("partitionKey").WithPrefix("USER");
        }
    }


    public class UserModelConfigWithoutPrefixWithoutCustomName : IModelConfig<User>
    {
        public void Configure(IModelConfigOptions<User> options)
        {
            options
                .ContainerName("Users")
                .Id(u => u.Id)
                .PartitionKey(u => u.TenantId)
                .PartitionKey(u => u.UserId);
        }
    }

}


