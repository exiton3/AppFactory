using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;
using AppFactory.Framework.DataAccess.CosmosDB.Mapping;
using AppFactory.Framework.DataAccess.CosmosDB.Repositories;
using AppFactory.Framework.Logging;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AppFactory.Framework.DataAccess.CosmosDB.IntegrationTests;

[Collection(CosmosDbEmulatorCollection.Name)]
[Trait("Category", "Integration")]
public sealed class PolymorphicMappingIntegrationTests(CosmosDbEmulatorFixture emulator)
{
    [Fact]
    public async Task RoundTrip_HierarchiesAndNestedIds_SurviveRealStorage()
    {
        var container = await emulator.CreateContainerAsync("/pk");
        var repository = emulator.CreateRepository<DepartmentRepository, Department>(new DepartmentConfig(container.Id));

        await repository.Add(new Department
        {
            Id = "D1",
            Head = new Manager { Id = "P1", Name = "Ann", Reports = { new Person { Id = "P2", Name = "Bob" } } },
            Persons = { new Person { Id = "P3" }, new Manager { Id = "P4" } },
            Budget = new Card { Number = "4111" }
        });

        var stored = (await container.ReadItemAsync<JObject>("D1", new PartitionKey("D1"))).Resource;
        Assert.Equal("manager", stored["head"]!["type"]!.ToString());
        Assert.Equal("card", stored["budget"]!["kind"]!.ToString());

        var result = await repository.GetById("D1");

        var head = Assert.IsType<Manager>(result.Head);
        Assert.Equal("P1", head.Id);                                    // nested Id kept (10.8.0 fix)
        Assert.Equal("P2", Assert.IsType<Person>(Assert.Single(head.Reports)).Id);
        Assert.IsType<Person>(result.Persons[0]);
        Assert.IsType<Manager>(result.Persons[1]);
        Assert.Equal("4111", Assert.IsType<Card>(result.Budget).Number);
    }

    public sealed class Department
    {
        public string? Id { get; set; }
        public Person? Head { get; set; }
        public List<Person> Persons { get; set; } = new();
        public Payment? Budget { get; set; }
    }

    public class Person
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
    }

    public sealed class Manager : Person
    {
        public List<Person> Reports { get; set; } = new();
    }

    public abstract class Payment
    {
    }

    public sealed class Card : Payment
    {
        public string? Number { get; set; }
    }

    public sealed class DepartmentConfig(string containerName) : IModelConfig<Department>
    {
        public void Configure(IModelConfigOptions<Department> options)
        {
            options.ContainerName(containerName).Id(d => d.Id).PartitionKey(d => d.Id).WithName("pk");

            options.Polymorphic<Person>().DiscriminateOn("type").WithValue<Person>("person").WithValue<Manager>("manager");
            options.Polymorphic<Payment>().DiscriminateOn("kind").WithValue<Card>("card");
        }
    }

    public sealed class DepartmentRepository(ICosmosDbClientFactory factory, ILogger logger, IModelMapRegistry registry)
        : RepositoryBase<Department>(factory, logger, registry);
}
