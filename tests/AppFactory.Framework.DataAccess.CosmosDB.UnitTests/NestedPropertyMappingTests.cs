using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using AppFactory.Framework.DataAccess.CosmosDB.Mapping;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AppFactory.Framework.DataAccess.CosmosDB.UnitTests
{
    // Id and partition key properties are ignored on the root model only.
    // Nested objects with properties of the same name must keep their values.
    public class NestedPropertyMappingTests
    {
        private static IModelMapper<Department> CreateDepartmentMapper()
        {
            var config = new CosmosDbModelConfig<Department>();
            config
                .ContainerName("Departments")
                .Id(d => d.Id)
                .IdPrefix("DEPT")
                .PartitionKey(d => d.TenantId).WithName("partitionKey").WithPrefix("TENANT");

            return new ModelMapper<Department>(config);
        }

        private static Department CreateDepartment() => new Department
        {
            Id = "D1",
            TenantId = "T1",
            Name = "Engineering",
            Head = new Person { Id = "P1", TenantId = "PT1", Name = "Ann" },
            Persons =
            {
                new Person { Id = "P2", TenantId = "PT2", Name = "Bob" },
                new Person { Id = "P3", TenantId = "PT3", Name = "Kate" }
            }
        };

        [Fact]
        public void MapToDocument_RootIdAndPartitionKeySource_AreNotStoredAsModelProperties()
        {
            var document = CreateDepartmentMapper().MapToDocument(CreateDepartment());

            Assert.Equal("DEPT#D1", document["id"]);
            Assert.Equal("TENANT#T1", document["partitionKey"]);
            Assert.False(document.ContainsKey("tenantId"));
        }

        [Fact]
        public void MapToDocument_NestedObject_KeepsIdAndPartitionKeyNamedProperties()
        {
            var document = CreateDepartmentMapper().MapToDocument(CreateDepartment());

            var head = (JObject)document["head"];
            Assert.Equal("P1", head["id"]!.ToString());
            Assert.Equal("PT1", head["tenantId"]!.ToString());
        }

        [Fact]
        public void MapToDocument_NestedCollection_KeepsIdAndPartitionKeyNamedProperties()
        {
            var document = CreateDepartmentMapper().MapToDocument(CreateDepartment());

            var persons = (JArray)document["persons"];
            Assert.Equal("P2", persons[0]["id"]!.ToString());
            Assert.Equal("PT2", persons[0]["tenantId"]!.ToString());
            Assert.Equal("P3", persons[1]["id"]!.ToString());
            Assert.Equal("PT3", persons[1]["tenantId"]!.ToString());
        }

        [Fact]
        public void RoundTrip_NestedObjectsKeepTheirValues()
        {
            var mapper = CreateDepartmentMapper();

            var result = mapper.MapFromDocument(mapper.MapToDocument(CreateDepartment()));

            Assert.Equal("D1", result.Id);
            Assert.Equal("T1", result.TenantId);
            Assert.Equal("P1", result.Head.Id);
            Assert.Equal("PT1", result.Head.TenantId);
            Assert.Collection(result.Persons,
                p => { Assert.Equal("P2", p.Id); Assert.Equal("PT2", p.TenantId); },
                p => { Assert.Equal("P3", p.Id); Assert.Equal("PT3", p.TenantId); });
        }

        // Contracts are resolved per type, so a nested object of the same type (or sharing a base class
        // that declares Id) as the root is the case a contract-resolver based ignore cannot handle.
        [Fact]
        public void RoundTrip_NestedObjectSharingBaseClassWithRoot_KeepsItsId()
        {
            var config = new CosmosDbModelConfig<Order>();
            config
                .ContainerName("Orders")
                .Id(o => o.Id)
                .PartitionKey(o => o.CustomerId);

            var mapper = new ModelMapper<Order>(config);

            var order = new Order
            {
                Id = "O1",
                CustomerId = "C1",
                Lines = { new OrderLine { Id = "L1", Sku = "SKU-1" } },
                Parent = new Order { Id = "O0", CustomerId = "C0" }
            };

            var document = mapper.MapToDocument(order);
            Assert.Equal("L1", ((JArray)document["lines"])[0]["id"]!.ToString());
            Assert.Equal("O0", ((JObject)document["parent"])["id"]!.ToString());
            Assert.Equal("C0", ((JObject)document["parent"])["customerId"]!.ToString());

            var result = mapper.MapFromDocument(document);
            Assert.Equal("O1", result.Id);
            Assert.Equal("C1", result.CustomerId);
            Assert.Equal("L1", result.Lines[0].Id);
            Assert.Equal("O0", result.Parent.Id);
            Assert.Equal("C0", result.Parent.CustomerId);
        }

        public class Department
        {
            public string Id { get; set; }
            public string TenantId { get; set; }
            public string Name { get; set; }
            public Person Head { get; set; }
            public List<Person> Persons { get; set; } = new List<Person>();
        }

        public class Person
        {
            public string Id { get; set; }
            public string TenantId { get; set; }
            public string Name { get; set; }
        }

        public abstract class EntityBase
        {
            public string Id { get; set; }
        }

        public class Order : EntityBase
        {
            public string CustomerId { get; set; }
            public List<OrderLine> Lines { get; set; } = new List<OrderLine>();
            public Order Parent { get; set; }
        }

        public class OrderLine : EntityBase
        {
            public string Sku { get; set; }
        }
    }
}
