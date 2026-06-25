using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using AppFactory.Framework.DataAccess.CosmosDB.Mapping;
using AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;
using AppFactory.Framework.TestExtensions;
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
                .PartitionKey(u => u.TenantId).WithName("customTenantId")
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


        [Fact]
        public void MapToDocument_IdWithCustomNames()
        {
            var options = new CosmosDbModelConfig<User>();
            options
                .ContainerName("Users")
                .Id(u => u.UserId)
                .PartitionKey(u => u.TenantId).WithName("customTenantId")
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

            document.ContainsKey("id").ShouldBeTrue();
            document.ContainsKey("userId").ShouldBeTrue();
            document["userId"].ToString().ShouldBeEqualTo("userID1");
            Assert.Equal("userID1", document["id"].ToString());
        }

        [Fact]
        public void MapBack_IdWithCustomNames()
        {
            var options = new CosmosDbModelConfig<User>();
            options
                .ContainerName("Users")
                .Id(u => u.UserId)
                .PartitionKey(u => u.TenantId).WithName("customTenantId")
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

           var mappedModel = mapper.MapFromDocument(document);

          mappedModel.UserId.ShouldBeEqualTo("userID1");


        }

        [Fact] 
        public void Map_PartitionKeyThatNotInTheMode()
        {
            var options = new CosmosDbModelConfig<UserNoTenantId>();
            options
                .ContainerName("Users")
                .Id(u => u.UserId)
                .PartitionKey("tenantId").UseResolver<TenantIdValueResolver>()
                .PartitionKey(u => u.UserId);

            IModelMapper<UserNoTenantId> mapper = new ModelMapper<UserNoTenantId>(options);

            var model = new UserNoTenantId
            {
                Email = "some@mail.com",
                Name = "John Doe",
                Id = "123",
                UserId = "userID1",
            };

            var document = mapper.MapToDocument(model);

            document.ContainsKey("tenantId").ShouldBeTrue();
            document["tenantId"].ToString().ShouldBeEqualTo("TenantIDFromResolver");

        }

        [Fact]
        public void PropertyWithCustomResolverShouldbeIgnoredWhenMappedBack()
        {
            var options = new CosmosDbModelConfig<UserNoTenantId>();
            options
                .ContainerName("Users")
                .Id(u => u.Id)
                .PartitionKey("tenantId").UseResolver<TenantIdValueResolver>()
                .PartitionKey(u => u.UserId);

            IModelMapper<UserNoTenantId> mapper = new ModelMapper<UserNoTenantId>(options);

            var model = new UserNoTenantId
            {
                Email = "some@mail.com",
                Name = "John Doe",
                Id = "123",
                UserId = "userID1",
            };

            var document = mapper.MapToDocument(model);

            var result = mapper.MapFromDocument(document);

            result.UserId.ShouldBeEqualTo("userID1");
            result.Id.ShouldBeEqualTo("123");
        }

        [Fact]
        public void GuidIdShouldBeDeserializedInPartitionKey()
        {
            var options = new CosmosDbModelConfig<User2>();
            options
                .ContainerName("Users")
                .Id(u => u.Id)
                .PartitionKey(u => u.Id).WithName("id");

            IModelMapper<User2> mapper = new ModelMapper<User2>(options);

            var newGuid = Guid.NewGuid();
            var model = new User2
            {
                Email = "some@mail.com",
                Name = "John Doe",
                Id = newGuid,
                UserId = "userID1",
            };

            var document = mapper.MapToDocument(model);
            document["id"] = newGuid.ToString();
            var result = mapper.MapFromDocument(document);

            result.UserId.ShouldBeEqualTo("userID1");
            result.Id.ShouldBeEqualTo(newGuid);
        }

        [Fact]
        public void ReportDefinitionSerialization()
        {
            var options = new ReportDefinitionModelConfig();
            CosmosDbModelConfig<ReportDefinition> config = new CosmosDbModelConfig<ReportDefinition>();
            
            options.Configure(config);

            ModelMapper<ReportDefinition> mapper = new ModelMapper<ReportDefinition>(config);

            // Create a test report with complex nested objects to verify serialization
            var testReport = ReportDefinition.Create(
                id: "probeId",
                name: "Serialization Test Report",
                description: "Test report to verify AppFactory 10.5.5 serialization with private setters",
                category: ReportCategory.Tax,
                catalogType: CatalogType.ServiceBureau,
                legacyCode: "TEST-SER-001",
                outputFormats: ["PDF", "Excel", "CSV"],
                tags:
                [
                    new ReportTag(ReportTagType.OutputType, "Standard"),
                    new ReportTag(ReportTagType.OutputType, "Detailed"),
                    new ReportTag(ReportTagType.ReportType, "By As Of Date"),
                    new ReportTag(ReportTagType.ReportType, "Summary")
                ],
                parameters:
                [
                    new ParameterDefinition
                    {
                        Label = "Start Date",
                        ControlType = ParameterControlType.DatePicker,
                        IsRequired = true,
                        DisplayOrder = 1,
                        DefaultValue = "2024-01-01",
                        MutualExclusionGroup = "DateRange"
                    },
                    new ParameterDefinition
                    {
                        Label = "End Date",
                        ControlType = ParameterControlType.DatePicker,
                        IsRequired = true,
                        DisplayOrder = 2,
                        DefaultValue = "2024-12-31",
                        MutualExclusionGroup = "DateRange"
                    },
                    new ParameterDefinition
                    {
                        Label = "Department",
                        ControlType = ParameterControlType.Dropdown,
                        IsRequired = false,
                        DisplayOrder = 3,
                        DataProviderKey = "DepartmentProvider"
                    }
                ]);

                var document = mapper.MapToDocument(testReport);

                document["id"].ShouldBeEqualTo("probeId");

               var result = mapper.MapFromDocument(document);

               result.Id.ShouldBeEqualTo("probeId");
               result.Parameters.Count.ShouldBeEqualTo(3);
        }

    }

    class TenantIdValueResolver: IPartitionKeyValueResolver 
    {
        public object GetValue()
        {
            return "TenantIDFromResolver";
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

    public class User2
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string TenantId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
    }

    public class UserNoTenantId
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        
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
                .PartitionKey(u => u.TenantId).WithName("tenantId").WithPrefix("TENANT")
                .PartitionKey(u => u.UserId).WithName("partitionKey").WithPrefix("USER");
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


public class ReportDefinitionModelConfig : IModelConfig<ReportDefinition>
{
    public void Configure(IModelConfigOptions<ReportDefinition> options)
    {
        options
            .ContainerName("report-catalog")
            .Id(r => r.Id)
            .PartitionKey(r => r.Id)
            .WithName("id");
    }
}

public class ReportDefinition
{
    public string Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public ReportCategory Category { get; set; }
    public CatalogType CatalogType { get; set; }
    public string LegacyCode { get; set; } = default!;
    public bool IsActive { get; set; }
    public List<ParameterDefinition> Parameters { get; set; } = [];
    public List<string> OutputFormats { get; set; } = [];
    public List<ReportTag> Tags { get; set; } = [];

    public ReportDefinition() { }

    public static ReportDefinition Create(
        string name,
        string description,
        ReportCategory category,
        CatalogType catalogType,
        string legacyCode,
        IReadOnlyList<string> outputFormats,
        IReadOnlyList<ReportTag> tags,
        IReadOnlyList<ParameterDefinition> parameters,
        string? id = null)
    {
        return new ReportDefinition
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Name = name,
            Description = description,
            Category = category,
            CatalogType = catalogType,
            LegacyCode = legacyCode,
            IsActive = true,
            OutputFormats = new List<string>(outputFormats),
            Tags = new List<ReportTag>(tags),
            Parameters = new List<ParameterDefinition>(parameters),
        };
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}

public enum CatalogType
{
    ServiceBureau,
    Client
}

public enum ParameterControlType
{
    TextInput, DatePicker, DateRangePicker, Checkbox,
    Dropdown, MultiSelectDropdown,
    SearchableDropdown, SearchableMultiSelectDropdown,
    TreeView, AutocompleteSearch
}

public class ParameterDefinition
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Label { get; init; } = default!;
    public ParameterControlType ControlType { get; init; }
    public bool IsRequired { get; init; }
    public int DisplayOrder { get; init; }
    public string? DefaultValue { get; init; }
    public string? MutualExclusionGroup { get; init; }
    public string? DataProviderKey { get; init; }
}

public enum ReportCategory
{
    Tax, PEO, Billing, Treasury, Benefits, PayrollHR, Finance, ClientAdmin
}

public enum ReportTagType { OutputType, ReportType }

public record ReportTag(ReportTagType TagType, string Value);
