using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using AppFactory.Framework.DataAccess.CosmosDB.Mapping;
using Xunit;

namespace AppFactory.Framework.DataAccess.CosmosDB.UnitTests
{
    // The Cosmos 'id' belongs to the property configured with Id(...).
    public class IdPropertyMappingTests
    {
        private static IModelMapper<TModel> CreateMapper<TModel>(Action<CosmosDbModelConfig<TModel>> configure) where TModel : class
        {
            var config = new CosmosDbModelConfig<TModel>();
            configure(config);
            return new ModelMapper<TModel>(config);
        }

        [Fact]
        public void RoundTrip_IdPropertyNotNamedId_IsRestored()
        {
            var mapper = CreateMapper<EmployeeNoId>(c => c
                .ContainerName("Employees")
                .Id(e => e.EmployeeNumber).IdPrefix("EMP")
                .PartitionKey(e => e.TenantId));

            var document = mapper.MapToDocument(new EmployeeNoId { EmployeeNumber = "E-100", TenantId = "T1", Name = "Ann" });
            var result = mapper.MapFromDocument(document);

            Assert.Equal("EMP#E-100", document["id"]);
            Assert.False(document.ContainsKey("employeeNumber"));
            Assert.Equal("E-100", result.EmployeeNumber);
            Assert.Equal("T1", result.TenantId);
        }

        [Fact]
        public void RoundTrip_ModelIdEqualToConfiguredId_BothRestoredWithSameValue()
        {
            var mapper = CreateMapper<Employee>(c => c
                .ContainerName("Employees")
                .Id(e => e.EmployeeNumber).IdPrefix("EMP")
                .PartitionKey(e => e.TenantId));

            var document = mapper.MapToDocument(new Employee { Id = "E-100", EmployeeNumber = "E-100", TenantId = "T1" });
            var result = mapper.MapFromDocument(document);

            Assert.Equal("EMP#E-100", document["id"]);
            Assert.Equal("E-100", result.Id);
            Assert.Equal("E-100", result.EmployeeNumber);
        }

        [Fact]
        public void RoundTrip_ModelIdNull_BothRestoredWithConfiguredIdValue()
        {
            var mapper = CreateMapper<Employee>(c => c
                .ContainerName("Employees")
                .Id(e => e.EmployeeNumber)
                .PartitionKey(e => e.TenantId));

            var result = mapper.MapFromDocument(
                mapper.MapToDocument(new Employee { Id = null, EmployeeNumber = "E-100", TenantId = "T1" }));

            Assert.Equal("E-100", result.Id);
            Assert.Equal("E-100", result.EmployeeNumber);
        }

        [Fact]
        public void MapToDocument_ModelIdDiffersFromConfiguredId_Throws()
        {
            var mapper = CreateMapper<Employee>(c => c
                .ContainerName("Employees")
                .Id(e => e.EmployeeNumber)
                .PartitionKey(e => e.TenantId));

            var exception = Assert.Throws<InvalidOperationException>(() =>
                mapper.MapToDocument(new Employee { Id = "a1b2", EmployeeNumber = "E-100", TenantId = "T1" }));

            Assert.Contains("Employee.Id ('a1b2')", exception.Message);
            Assert.Contains("Employee.EmployeeNumber ('E-100')", exception.Message);
        }

        [Fact]
        public void RoundTrip_GuidIdPropertyNotNamedId_IsConverted()
        {
            var mapper = CreateMapper<GuidKeyModel>(c => c
                .ContainerName("Items")
                .Id(m => m.Key).IdPrefix("ITEM")
                .PartitionKey(m => m.TenantId));

            var key = Guid.NewGuid();
            var result = mapper.MapFromDocument(mapper.MapToDocument(new GuidKeyModel { Key = key, TenantId = "T1" }));

            Assert.Equal(key, result.Key);
        }

        [Fact]
        public void RoundTrip_IntIdPropertyNotNamedId_IsConverted()
        {
            var mapper = CreateMapper<IntKeyModel>(c => c
                .ContainerName("Items")
                .Id(m => m.Number)
                .PartitionKey(m => m.TenantId));

            var result = mapper.MapFromDocument(mapper.MapToDocument(new IntKeyModel { Number = 42, TenantId = "T1" }));

            Assert.Equal(42, result.Number);
        }

        [Fact]
        public void RoundTrip_IdPropertyWithPrivateSetter_IsRestored()
        {
            var mapper = CreateMapper<PrivateSetterModel>(c => c
                .ContainerName("Items")
                .Id(m => m.Code)
                .PartitionKey(m => m.TenantId));

            var result = mapper.MapFromDocument(mapper.MapToDocument(new PrivateSetterModel("C-1", "T1")));

            Assert.Equal("C-1", result.Code);
        }

        public class Employee
        {
            public string? Id { get; set; }
            public string? EmployeeNumber { get; set; }
            public string? TenantId { get; set; }
        }

        public class EmployeeNoId
        {
            public string? EmployeeNumber { get; set; }
            public string? TenantId { get; set; }
            public string? Name { get; set; }
        }

        public class GuidKeyModel
        {
            public Guid Key { get; set; }
            public string? TenantId { get; set; }
        }

        public class IntKeyModel
        {
            public int Number { get; set; }
            public string? TenantId { get; set; }
        }

        public class PrivateSetterModel
        {
            public PrivateSetterModel(string code, string tenantId)
            {
                Code = code;
                TenantId = tenantId;
            }

            public string Code { get; private set; }
            public string TenantId { get; private set; }
        }
    }
}
