using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;
using AppFactory.Framework.DataAccess.CosmosDB.Mapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AppFactory.Framework.DataAccess.CosmosDB.UnitTests
{
    public class PolymorphicMappingTests
    {
        private static IModelMapper<Department> CreateMapper(Action<CosmosDbModelConfig<Department>>? configure = null)
        {
            var config = new CosmosDbModelConfig<Department>();
            config
                .ContainerName("Departments")
                .Id(d => d.Id)
                .PartitionKey(d => d.TenantId);

            config.Polymorphic<Person>()
                .DiscriminateOn("type")
                .WithValue<Person>("person")
                .WithValue<Manager>("manager");

            config.Polymorphic<Payment>()
                .DiscriminateOn("kind")
                .WithValue<Card>("card")
                .WithValue<BankTransfer>("ach");

            configure?.Invoke(config);

            return new ModelMapper<Department>(config);
        }

        // Simulates storing in and reading from Cosmos: the document goes through JSON text
        private static Department RoundTrip(IModelMapper<Department> mapper, Department department, out JObject stored)
        {
            var json = mapper.MapToDocument(department).ToJson();
            stored = JObject.Parse(json);
            return mapper.MapFromDocument(new CosmosDbDocument(json));
        }

        private static Department NewDepartment() => new Department { Id = "D1", TenantId = "T1" };

        [Fact]
        public void AbstractProperty_RoundTrip_RestoresConcreteType()
        {
            var department = NewDepartment();
            department.Payment = new Card { Number = "4111" };

            var result = RoundTrip(CreateMapper(), department, out var stored);

            Assert.Equal("card", stored["payment"]!["kind"]!.ToString());
            var card = Assert.IsType<Card>(result.Payment);
            Assert.Equal("4111", card.Number);
        }

        [Fact]
        public void ConcreteBaseProperty_RoundTrip_RestoresBaseAndDerivedTypes()
        {
            var department = NewDepartment();
            department.Head = new Manager { Id = "P1", Name = "Ann", Level = 3 };
            department.Deputy = new Person { Id = "P2", Name = "Bob" };

            var result = RoundTrip(CreateMapper(), department, out var stored);

            Assert.Equal("manager", stored["head"]!["type"]!.ToString());
            Assert.Equal("person", stored["deputy"]!["type"]!.ToString());
            var head = Assert.IsType<Manager>(result.Head);
            Assert.Equal(3, head.Level);
            Assert.Equal("P1", head.Id);
            Assert.IsType<Person>(result.Deputy);
            Assert.Equal("Bob", result.Deputy.Name);
        }

        [Fact]
        public void Collection_RoundTrip_RestoresEachItemType()
        {
            var department = NewDepartment();
            department.Persons.Add(new Person { Id = "P1" });
            department.Persons.Add(new Manager { Id = "P2", Level = 1 });

            var result = RoundTrip(CreateMapper(), department, out var stored);

            Assert.Equal(new[] { "person", "manager" }, stored["persons"]!.Select(p => p["type"]!.ToString()));
            Assert.Collection(result.Persons,
                p => { Assert.IsType<Person>(p); Assert.Equal("P1", p.Id); },
                p => { Assert.Equal(1, Assert.IsType<Manager>(p).Level); Assert.Equal("P2", p.Id); });
        }

        [Fact]
        public void ArrayAndDictionaryValues_RoundTrip_RestoreConcreteTypes()
        {
            var department = NewDepartment();
            department.PastHeads = new Person[] { new Manager { Id = "M1" }, new Person { Id = "P1" } };
            department.ByRole["lead"] = new Manager { Id = "M2" };
            department.Payments.Add(new BankTransfer { Iban = "DE89" });

            var result = RoundTrip(CreateMapper(), department, out _);

            Assert.IsType<Manager>(result.PastHeads[0]);
            Assert.IsType<Person>(result.PastHeads[1]);
            Assert.Equal("M2", Assert.IsType<Manager>(result.ByRole["lead"]).Id);
            Assert.Equal("DE89", Assert.IsType<BankTransfer>(Assert.Single(result.Payments)).Iban);
        }

        [Fact]
        public void NestedHierarchy_RoundTrip_RestoresTypesAtAnyDepth()
        {
            var department = NewDepartment();
            var junior = new Person { Id = "P3", Mentor = new Manager { Id = "M2" } };
            department.Head = new Manager
            {
                Id = "M1",
                Reports = { junior, new Manager { Id = "M3", Reports = { new Person { Id = "P4" } } } }
            };

            var result = RoundTrip(CreateMapper(), department, out _);

            var head = Assert.IsType<Manager>(result.Head);
            Assert.IsType<Person>(head.Reports[0]);
            Assert.Equal("M2", Assert.IsType<Manager>(head.Reports[0].Mentor).Id);
            var nestedManager = Assert.IsType<Manager>(head.Reports[1]);
            Assert.Equal("P4", Assert.IsType<Person>(Assert.Single(nestedManager.Reports)).Id);
        }

        [Fact]
        public void PropertyMap_OverridesTypeLevelForThatPropertyOnly()
        {
            var mapper = CreateMapper(c => c.Map(d => d.Persons)
                .DiscriminateOn("personType")
                .WithValue<Person>("p")
                .WithValue<Manager>("m"));

            var department = NewDepartment();
            department.Head = new Manager { Id = "M1" };
            department.Persons.Add(new Manager { Id = "M2" });

            var result = RoundTrip(mapper, department, out var stored);

            var item = (JObject)stored["persons"]![0]!;
            Assert.Equal("m", item["personType"]!.ToString());
            Assert.Null(item["type"]);
            Assert.Equal("manager", stored["head"]!["type"]!.ToString());
            Assert.IsType<Manager>(Assert.Single(result.Persons));
            Assert.IsType<Manager>(result.Head);
        }

        [Fact]
        public void PropertyMap_WithoutTypeLevelConfiguration_Works()
        {
            var config = new CosmosDbModelConfig<Department>();
            config.ContainerName("Departments").Id(d => d.Id).PartitionKey(d => d.TenantId);
            config.Map(d => d.Payment).DiscriminateOn("kind").WithValue<Card>("card");
            var mapper = new ModelMapper<Department>(config);

            var department = NewDepartment();
            department.Payment = new Card { Number = "1" };

            var result = RoundTrip(mapper, department, out _);

            Assert.IsType<Card>(result.Payment);
        }

        [Fact]
        public void StoredWithoutDiscriminator_UsesDefaultType()
        {
            var config = new CosmosDbModelConfig<Department>();
            config.ContainerName("Departments").Id(d => d.Id).PartitionKey(d => d.TenantId);
            config.Polymorphic<Payment>().DiscriminateOn("kind").WithValue<Card>("card").DefaultTo<Card>();
            var mapper = new ModelMapper<Department>(config);

            var result = mapper.MapFromDocument(new CosmosDbDocument(
                "{\"id\":\"D1\",\"tenantId\":\"T1\",\"payment\":{\"number\":\"4111\"}}"));

            Assert.Equal("4111", Assert.IsType<Card>(result.Payment).Number);
        }

        [Fact]
        public void StoredWithoutDiscriminator_ConcretePropertyType_UsesPropertyType()
        {
            var result = CreateMapper().MapFromDocument(new CosmosDbDocument(
                "{\"id\":\"D1\",\"tenantId\":\"T1\",\"head\":{\"id\":\"P1\"},\"persons\":[{\"id\":\"P2\"}]}"));

            Assert.IsType<Person>(result.Head);
            Assert.Equal("P2", Assert.IsType<Person>(Assert.Single(result.Persons)).Id);
        }

        [Fact]
        public void StoredWithoutDiscriminator_AbstractPropertyWithoutDefault_Throws()
        {
            var exception = Assert.Throws<JsonSerializationException>(() => CreateMapper().MapFromDocument(new CosmosDbDocument(
                "{\"id\":\"D1\",\"tenantId\":\"T1\",\"payment\":{\"number\":\"4111\"}}")));

            Assert.Contains("DefaultTo<T>()", exception.Message);
        }

        [Fact]
        public void UnknownDiscriminatorValue_Throws()
        {
            var exception = Assert.Throws<JsonSerializationException>(() => CreateMapper().MapFromDocument(new CosmosDbDocument(
                "{\"id\":\"D1\",\"tenantId\":\"T1\",\"payment\":{\"kind\":\"paypal\"}}")));

            Assert.Contains("unknown kind 'paypal'", exception.Message);
            Assert.Contains("card, ach", exception.Message);
        }

        [Fact]
        public void UnregisteredRuntimeType_ThrowsOnWrite()
        {
            var department = NewDepartment();
            department.Payment = new GiftCard();

            var exception = Assert.Throws<JsonSerializationException>(() => CreateMapper().MapToDocument(department));

            Assert.Contains("GiftCard", exception.Message);
            Assert.Contains("WithValue<GiftCard>", exception.Message);
        }

        [Fact]
        public void DiscriminatorMappingToTypeNotAssignableToProperty_Throws()
        {
            // "person" is valid for Person properties, but not for a Manager property
            var exception = Assert.Throws<JsonSerializationException>(() => CreateMapper().MapFromDocument(new CosmosDbDocument(
                "{\"id\":\"D1\",\"tenantId\":\"T1\",\"director\":{\"type\":\"person\"}}")));

            Assert.Contains("not assignable to Manager", exception.Message);
        }

        [Fact]
        public void NestedIdAndPartitionKeyNamedProperties_AreKeptInPolymorphicObjects()
        {
            var department = NewDepartment();
            department.Head = new Manager { Id = "M1", TenantId = "MT1" };

            var result = RoundTrip(CreateMapper(), department, out _);

            Assert.Equal("M1", result.Head!.Id);
            Assert.Equal("MT1", result.Head.TenantId);
        }

        [Fact]
        public void SameModelConfiguredDifferently_EachMapperUsesItsOwnConfiguration()
        {
            var first = CreateMapper();
            var second = CreateMapper(c => c.Map(d => d.Head).DiscriminateOn("headType").WithValue<Manager>("boss").WithValue<Person>("staff"));

            var department = NewDepartment();
            department.Head = new Manager();

            RoundTrip(first, department, out var firstStored);
            RoundTrip(second, department, out var secondStored);

            Assert.Equal("manager", firstStored["head"]!["type"]!.ToString());
            Assert.Equal("boss", secondStored["head"]!["headType"]!.ToString());
        }

        [Fact]
        public void Configuration_MissingDiscriminateOn_ThrowsWhenMapperIsCreated()
        {
            var config = new CosmosDbModelConfig<Department>();
            config.ContainerName("Departments").Id(d => d.Id);
            config.Polymorphic<Payment>();

            var exception = Assert.Throws<InvalidOperationException>(() => new ModelMapper<Department>(config));
            Assert.Contains("DiscriminateOn", exception.Message);
        }

        [Fact]
        public void Configuration_NoValues_ThrowsWhenMapperIsCreated()
        {
            var config = new CosmosDbModelConfig<Department>();
            config.ContainerName("Departments").Id(d => d.Id);
            config.Polymorphic<Payment>().DiscriminateOn("kind");

            var exception = Assert.Throws<InvalidOperationException>(() => new ModelMapper<Department>(config));
            Assert.Contains("WithValue", exception.Message);
        }

        [Fact]
        public void Configuration_InvalidRegistrations_Throw()
        {
            var config = new CosmosDbModelConfig<Department>();
            var options = config.Polymorphic<Payment>().DiscriminateOn("kind").WithValue<Card>("card");

            Assert.Throws<ArgumentException>(() => options.WithValue<Payment>("payment"));       // abstract
            Assert.Throws<InvalidOperationException>(() => options.WithValue<BankTransfer>("card")); // duplicate value
            Assert.Throws<InvalidOperationException>(() => options.WithValue<Card>("card2"));        // duplicate type
            Assert.Throws<InvalidOperationException>(() => config.Polymorphic<Payment>());           // configured twice
            Assert.Throws<ArgumentException>(() => config.Map(d => d.Persons)
                .DiscriminateOn("t").WithValue<Card>("card"));                                       // not a Person
        }

        [Fact]
        public void Configuration_DiscriminatorClashesWithProperty_ThrowsWhenMapperIsCreated()
        {
            var config = new CosmosDbModelConfig<Department>();
            config.ContainerName("Departments").Id(d => d.Id);
            config.Polymorphic<Person>().DiscriminateOn("name").WithValue<Person>("person");

            var exception = Assert.Throws<InvalidOperationException>(() => new ModelMapper<Department>(config));
            Assert.Contains("clashes with a property of Person", exception.Message);
        }

        public class Department
        {
            public string? Id { get; set; }
            public string? TenantId { get; set; }
            public Person? Head { get; set; }
            public Person? Deputy { get; set; }
            public Manager? Director { get; set; }
            public List<Person> Persons { get; set; } = new List<Person>();
            public Person[] PastHeads { get; set; } = Array.Empty<Person>();
            public Dictionary<string, Person> ByRole { get; set; } = new Dictionary<string, Person>();
            public Payment? Payment { get; set; }
            public IList<Payment> Payments { get; set; } = new List<Payment>();
        }

        public class Person
        {
            public string? Id { get; set; }
            public string? TenantId { get; set; }
            public string? Name { get; set; }
            public Person? Mentor { get; set; }
        }

        public class Manager : Person
        {
            public int Level { get; set; }
            public List<Person> Reports { get; set; } = new List<Person>();
        }

        public abstract class Payment
        {
        }

        public class Card : Payment
        {
            public string? Number { get; set; }
        }

        public class BankTransfer : Payment
        {
            public string? Iban { get; set; }
        }

        public class GiftCard : Payment
        {
        }
    }
}
