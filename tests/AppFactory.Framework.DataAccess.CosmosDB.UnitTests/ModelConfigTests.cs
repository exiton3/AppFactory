using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using AppFactory.Framework.DataAccess.CosmosDB.Mapping;
using AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;
using Xunit;

namespace AppFactory.Framework.DataAccess.CosmosDB.UnitTests
{
   public class ModelConfigTests
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

            // Verify document structure
            Assert.NotNull(document);
            Assert.Equal("USER#123", document["id"]);
            Assert.Equal("TENANT#userID1", document["partitionKey"]);
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
                .PartitionKey(u => u.UserId).WithPropertyName("partitionKey").WithPrefix("TENANT");
        }
    }

   
}


