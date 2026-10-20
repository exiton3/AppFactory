using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;
using AppFactory.Framework.DataAccess.CosmosDB.Mapping;
using AppFactory.Framework.DataAccess.CosmosDB.Repositories;
using AppFactory.Framework.Domain.Repositories;
using AppFactory.Framework.Logging;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AppFactory.Framework.DataAccess.CosmosDB.UnitTests
{
    public class ConcurrencyTokenTests
    {
        private readonly InMemoryCosmosDbClientFactory _factory = new InMemoryCosmosDbClientFactory();

        private InMemoryCosmosDbClient Client => _factory.Client;

        private Repository<TModel> CreateRepository<TModel>(IModelConfig<TModel> config) where TModel : class
        {
            var provider = new ServiceCollection()
                .AddSingleton(config)
                .AddSingleton<IModelMapRegistry, ModelMapRegistry>()
                .BuildServiceProvider();

            return new Repository<TModel>(_factory, new NullLogger(), provider.GetRequiredService<IModelMapRegistry>());
        }

        private Repository<Account> CreateAccountRepository() => CreateRepository<Account>(new AccountConfig());

        [Fact]
        public async Task Update_SecondLoadAfterFirstUpdate_ThrowsConcurrencyConflict()
        {
            var repository = CreateAccountRepository();
            await repository.Add(new Account { Id = "A1", TenantId = "T1", Balance = 100 });

            var first = await repository.GetById("A1");
            var second = await repository.GetById("A1");

            first.Balance = 150;
            Assert.True(await repository.Update(first));

            second.Balance = 50;
            var exception = await Assert.ThrowsAsync<ConcurrencyConflictException>(() => repository.Update(second));

            Assert.Equal("Account", exception.EntityType);
            Assert.Equal("ACCOUNT#A1", exception.Id);
            Assert.Equal(150, (await repository.GetById("A1")).Balance);
        }

        [Fact]
        public async Task GetById_ReadsETagIntoConcurrencyToken()
        {
            var repository = CreateAccountRepository();
            await repository.Add(new Account { Id = "A1", TenantId = "T1" });

            var account = await repository.GetById("A1");

            Assert.Equal(Client.Stored("ACCOUNT#A1")[CosmosDbConstants.ETag].ToString(), account.ConcurrencyToken);
        }

        [Fact]
        public async Task Query_ReadsETagIntoConcurrencyToken()
        {
            var repository = CreateAccountRepository();
            await repository.Add(new Account { Id = "A1", TenantId = "T1" });

            var account = Assert.Single(await repository.QueryAll());

            Assert.Equal(Client.Stored("ACCOUNT#A1")[CosmosDbConstants.ETag].ToString(), account.ConcurrencyToken);
        }

        [Fact]
        public async Task Add_RefreshesTokenSoTheSameInstanceCanBeUpdated()
        {
            var repository = CreateAccountRepository();
            var account = new Account { Id = "A1", TenantId = "T1" };

            await repository.Add(account);
            Assert.Equal(Client.Stored("ACCOUNT#A1")[CosmosDbConstants.ETag].ToString(), account.ConcurrencyToken);

            account.Balance = 10;
            Assert.True(await repository.Update(account));
        }

        [Fact]
        public async Task Update_RefreshesTokenSoTheSameInstanceCanBeUpdatedAgain()
        {
            var repository = CreateAccountRepository();
            await repository.Add(new Account { Id = "A1", TenantId = "T1" });
            var account = await repository.GetById("A1");
            var loadedToken = account.ConcurrencyToken;

            account.Balance = 10;
            await repository.Update(account);
            Assert.NotEqual(loadedToken, account.ConcurrencyToken);

            account.Balance = 20;
            Assert.True(await repository.Update(account));
            Assert.Equal(20, (await repository.GetById("A1")).Balance);
        }

        [Fact]
        public async Task Upsert_WithStaleToken_ThrowsConcurrencyConflict_AndRefreshesOnSuccess()
        {
            var repository = CreateAccountRepository();
            await repository.Add(new Account { Id = "A1", TenantId = "T1" });
            var first = await repository.GetById("A1");
            var second = await repository.GetById("A1");

            Assert.True(await repository.Upsert(first));
            Assert.Equal(Client.Stored("ACCOUNT#A1")[CosmosDbConstants.ETag].ToString(), first.ConcurrencyToken);

            await Assert.ThrowsAsync<ConcurrencyConflictException>(() => repository.Upsert(second));
        }

        [Fact]
        public async Task Upsert_NewModelWithoutToken_IsCreatedWithoutCondition()
        {
            var repository = CreateAccountRepository();
            var account = new Account { Id = "A1", TenantId = "T1" };

            Assert.True(await repository.Upsert(account));

            Assert.Null(Assert.Single(Client.IfMatchEtags));
            Assert.NotNull(account.ConcurrencyToken);
        }

        [Fact]
        public async Task Write_TokenIsNeverStoredAsDataField()
        {
            var repository = CreateAccountRepository();
            var account = new Account { Id = "A1", TenantId = "T1", ConcurrencyToken = "\"stale\"" };

            await repository.Upsert(new Account { Id = "A1", TenantId = "T1" });
            await Assert.ThrowsAsync<ConcurrencyConflictException>(() => repository.Upsert(account));

            var stored = Client.Stored("ACCOUNT#A1");
            Assert.False(stored.ContainsKey("concurrencyToken"));
            Assert.True(stored.ContainsKey(CosmosDbConstants.ETag));
        }

        [Fact]
        public async Task Update_SendsIfMatchOnlyWhenTokenIsSet()
        {
            var repository = CreateAccountRepository();
            await repository.Add(new Account { Id = "A1", TenantId = "T1" });
            var loaded = await repository.GetById("A1");

            await repository.Update(new Account { Id = "A1", TenantId = "T1" });   // no token: unconditional
            await Assert.ThrowsAsync<ConcurrencyConflictException>(() => repository.Update(loaded)); // now stale

            Assert.Equal(2, Client.IfMatchEtags.Count);
            Assert.Null(Client.IfMatchEtags[0]);
            Assert.Equal(loaded.ConcurrencyToken, Client.IfMatchEtags[1]);
        }

        [Fact]
        public async Task ModelWithoutConcurrencyToken_IsUnchanged()
        {
            var repository = CreateRepository<Note>(new NoteConfig());
            await repository.Add(new Note { Id = "N1", TenantId = "T1", Text = "a" });

            var first = await repository.GetById("N1");
            var second = await repository.GetById("N1");
            first.Text = "b";
            second.Text = "c";

            Assert.True(await repository.Update(first));
            Assert.True(await repository.Update(second));   // last write wins, as before

            Assert.All(Client.IfMatchEtags, Assert.Null);
            Assert.Equal("c", (await repository.GetById("N1")).Text);
        }

        [Fact]
        public void PrivateSetterToken_IsSupported()
        {
            var config = new CosmosDbModelConfig<PrivateTokenAccount>();
            config.ContainerName("accounts").Id(a => a.Id).PartitionKey(a => a.TenantId).ConcurrencyToken(a => a.Version);
            var mapper = new ModelMapper<PrivateTokenAccount>(config);

            var document = mapper.MapToDocument(new PrivateTokenAccount { Id = "A1", TenantId = "T1" });
            document[CosmosDbConstants.ETag] = "\"etag-7\"";

            Assert.Equal("\"etag-7\"", mapper.MapFromDocument(document).Version);
        }

        [Fact]
        public void Configuration_GetOnlyTokenProperty_Throws()
        {
            var config = new CosmosDbModelConfig<GetOnlyTokenAccount>();

            var exception = Assert.Throws<ArgumentException>(() => config.ConcurrencyToken(a => a.Version));
            Assert.Contains("setter", exception.Message);
        }

        [Fact]
        public void Configuration_ConfiguredTwice_Throws()
        {
            var config = new CosmosDbModelConfig<Account>();
            config.ConcurrencyToken(a => a.ConcurrencyToken);

            Assert.Throws<InvalidOperationException>(() => config.ConcurrencyToken(a => a.ConcurrencyToken));
        }

        public class Repository<TModel> : RepositoryBase<TModel> where TModel : class
        {
            public Repository(ICosmosDbClientFactory factory, ILogger logger, IModelMapRegistry registry)
                : base(factory, logger, registry)
            {
            }

            public Task<IEnumerable<TModel>> QueryAll() => Query("SELECT * FROM c");
        }

        public class Account
        {
            public string? Id { get; set; }
            public string? TenantId { get; set; }
            public decimal Balance { get; set; }
            public string? ConcurrencyToken { get; set; }
        }

        public class AccountConfig : IModelConfig<Account>
        {
            public void Configure(IModelConfigOptions<Account> options)
            {
                options
                    .ContainerName("accounts")
                    .Id(a => a.Id).IdPrefix("ACCOUNT")
                    .PartitionKey(a => a.TenantId)
                    .ConcurrencyToken(a => a.ConcurrencyToken);
            }
        }

        public class Note
        {
            public string? Id { get; set; }
            public string? TenantId { get; set; }
            public string? Text { get; set; }
        }

        public class NoteConfig : IModelConfig<Note>
        {
            public void Configure(IModelConfigOptions<Note> options)
            {
                options.ContainerName("notes").Id(n => n.Id).PartitionKey(n => n.TenantId);
            }
        }

        public class PrivateTokenAccount
        {
            public string? Id { get; set; }
            public string? TenantId { get; set; }
            public string? Version { get; private set; }
        }

        public class GetOnlyTokenAccount
        {
            public string? Version => null;
        }
    }
}
