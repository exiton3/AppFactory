using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;
using AppFactory.Framework.DataAccess.CosmosDB.Mapping;
using AppFactory.Framework.DataAccess.CosmosDB.Repositories;
using AppFactory.Framework.Logging;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AppFactory.Framework.DataAccess.CosmosDB.UnitTests
{
    public class ModelMapRegistryTests
    {
        private static (ServiceProvider Provider, CountingCustomerConfig Config) CreateServices()
        {
            var config = new CountingCustomerConfig();
            var services = new ServiceCollection();
            services.AddSingleton<IModelConfig<Customer>>(config);
            services.AddSingleton<IModelMapRegistry, ModelMapRegistry>();
            services.AddSingleton<ICosmosDbClientFactory, InMemoryCosmosDbClientFactory>();
            services.AddSingleton<ILogger, NullLogger>();
            services.AddScoped<CustomerRepository>();

            return (services.BuildServiceProvider(), config);
        }

        [Fact]
        public void Get_CalledRepeatedly_ReturnsSameMapAndConfiguresOnce()
        {
            var (provider, config) = CreateServices();
            var registry = provider.GetRequiredService<IModelMapRegistry>();

            var first = registry.Get<Customer>();
            var second = registry.Get<Customer>();

            Assert.Same(first, second);
            Assert.Same(first.Mapper, second.Mapper);
            Assert.Equal(1, config.ConfigureCalls);
        }

        [Fact]
        public void Get_CalledConcurrently_ConfiguresOnce()
        {
            var (provider, config) = CreateServices();
            var registry = provider.GetRequiredService<IModelMapRegistry>();

            var maps = new CosmosDbModelMap<Customer>[64];
            Parallel.For(0, maps.Length, i => maps[i] = registry.Get<Customer>());

            Assert.All(maps, map => Assert.Same(maps[0], map));
            Assert.Equal(1, config.ConfigureCalls);
        }

        [Fact]
        public void Get_ModelConfigNotRegistered_Throws()
        {
            var (provider, _) = CreateServices();
            var registry = provider.GetRequiredService<IModelMapRegistry>();

            Assert.Throws<InvalidOperationException>(() => registry.Get<NotConfiguredModel>());
        }

        [Fact]
        public void Repository_CreatedPerScope_ConfiguresModelOnce()
        {
            var (provider, config) = CreateServices();

            for (var i = 0; i < 3; i++)
            {
                using var scope = provider.CreateScope();
                scope.ServiceProvider.GetRequiredService<CustomerRepository>();
            }

            Assert.Equal(1, config.ConfigureCalls);
        }

#pragma warning disable CS0618 // the obsolete constructor is tested on purpose
        [Fact]
        public void ObsoleteRepositoryConstructor_ConfiguresModelOnEveryInstance()
        {
            var config = new CountingCustomerConfig();
            var factory = new InMemoryCosmosDbClientFactory();

            _ = new LegacyCustomerRepository(factory, new NullLogger(), config);
            _ = new LegacyCustomerRepository(factory, new NullLogger(), config);

            Assert.Equal(2, config.ConfigureCalls);
        }

        [Fact]
        public async Task Repository_NewAndObsoleteConstructors_WriteSameDocument()
        {
            var (provider, _) = CreateServices();
            var customer = new Customer { Id = "C1", TenantId = "T1", Name = "Ann" };

            var registryFactory = new InMemoryCosmosDbClientFactory();
            await new CustomerRepository(registryFactory, new NullLogger(), provider.GetRequiredService<IModelMapRegistry>()).Add(customer);

            var legacyFactory = new InMemoryCosmosDbClientFactory();
            await new LegacyCustomerRepository(legacyFactory, new NullLogger(), new CountingCustomerConfig()).Add(customer);

            var document = Assert.Single(registryFactory.Client.Created);
            Assert.Equal("customers", registryFactory.Client.LastContainer);
            Assert.Equal("CUSTOMER#C1", document["id"]);
            Assert.Equal("T1", document["tenantId"]);
            Assert.Equal(Assert.Single(legacyFactory.Client.Created).ToJson(), document.ToJson());
        }
#pragma warning restore CS0618

        [Fact]
        public void RegisterCosmosDbPersistence_RegistersRegistryOnceAsSingleton()
        {
            var services = new ServiceCollection();
            services.RegisterCosmosDbPersistence();
            services.RegisterCosmosDbPersistence();

            var registration = Assert.Single(services, s => s.ServiceType == typeof(IModelMapRegistry));
            Assert.Equal(ServiceLifetime.Singleton, registration.Lifetime);
        }

        public class Customer
        {
            public string? Id { get; set; }
            public string? TenantId { get; set; }
            public string? Name { get; set; }
        }

        public class NotConfiguredModel
        {
            public string? Id { get; set; }
        }

        public class CountingCustomerConfig : IModelConfig<Customer>
        {
            private int _configureCalls;

            public int ConfigureCalls => _configureCalls;

            public void Configure(IModelConfigOptions<Customer> config)
            {
                Interlocked.Increment(ref _configureCalls);

                config
                    .ContainerName("customers")
                    .Id(c => c.Id).IdPrefix("CUSTOMER")
                    .PartitionKey(c => c.TenantId);
            }
        }

        public class CustomerRepository : RepositoryBase<Customer>
        {
            public CustomerRepository(ICosmosDbClientFactory factory, ILogger logger, IModelMapRegistry registry)
                : base(factory, logger, registry)
            {
            }
        }

        public class LegacyCustomerRepository : RepositoryBase<Customer>
        {
#pragma warning disable CS0618
            public LegacyCustomerRepository(ICosmosDbClientFactory factory, ILogger logger, IModelConfig<Customer> config)
                : base(factory, logger, config)
            {
            }
#pragma warning restore CS0618
        }
    }
}
