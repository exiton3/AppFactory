using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;
using AppFactory.Framework.DataAccess.CosmosDB.Mapping;
using AppFactory.Framework.DataAccess.CosmosDB.Repositories;
using AppFactory.Framework.Domain.Repositories;
using AppFactory.Framework.Logging;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AppFactory.Framework.DataAccess.CosmosDB.IntegrationTests;

[Collection(CosmosDbEmulatorCollection.Name)]
[Trait("Category", "Integration")]
public sealed class ConcurrencyTokenIntegrationTests(CosmosDbEmulatorFixture emulator)
{
    private async Task<(AccountRepository Repository, Container Container)> CreateAccountsAsync()
    {
        var container = await emulator.CreateContainerAsync("/pk");
        var repository = emulator.CreateRepository<AccountRepository, Account>(new AccountConfig(container.Id));
        return (repository, container);
    }

    private static async Task<string> StoredETagAsync(Container container, string id, string partitionKey)
    {
        var response = await container.ReadItemAsync<JObject>(id, new PartitionKey(partitionKey));
        return response.ETag;
    }

    [Fact]
    public async Task Update_TwoLoads_FirstSucceedsSecondThrows()
    {
        var (repository, _) = await CreateAccountsAsync();
        await repository.Add(new Account { Id = "A1", Balance = 100 });

        var first = await repository.GetById("A1");
        var second = await repository.GetById("A1");

        first.Balance = 150;
        Assert.True(await repository.Update(first));

        second.Balance = 50;
        var exception = await Assert.ThrowsAsync<ConcurrencyConflictException>(() => repository.Update(second));

        Assert.Equal("A1", exception.Id);
        Assert.Equal(150, (await repository.GetById("A1")).Balance);
    }

    [Fact]
    public async Task GetById_TokenIsTheStoredETag()
    {
        var (repository, container) = await CreateAccountsAsync();
        await repository.Add(new Account { Id = "A1" });

        var account = await repository.GetById("A1");

        Assert.Equal(await StoredETagAsync(container, "A1", "A1"), account.ConcurrencyToken);
    }

    [Fact]
    public async Task Add_RefreshesToken_SameInstanceCanBeUpdated()
    {
        var (repository, container) = await CreateAccountsAsync();
        var account = new Account { Id = "A1" };

        await repository.Add(account);
        Assert.Equal(await StoredETagAsync(container, "A1", "A1"), account.ConcurrencyToken);

        account.Balance = 10;
        Assert.True(await repository.Update(account));
    }

    [Fact]
    public async Task Update_RefreshesToken_SameInstanceCanBeUpdatedAgain()
    {
        var (repository, container) = await CreateAccountsAsync();
        await repository.Add(new Account { Id = "A1" });
        var account = await repository.GetById("A1");
        var loadedToken = account.ConcurrencyToken;

        account.Balance = 10;
        await repository.Update(account);

        Assert.NotEqual(loadedToken, account.ConcurrencyToken);
        Assert.Equal(await StoredETagAsync(container, "A1", "A1"), account.ConcurrencyToken);

        account.Balance = 20;
        Assert.True(await repository.Update(account));
        Assert.Equal(20, (await repository.GetById("A1")).Balance);
    }

    [Fact]
    public async Task Upsert_StaleTokenThrows_NewModelIsCreated()
    {
        var (repository, _) = await CreateAccountsAsync();
        var created = new Account { Id = "A1" };
        Assert.True(await repository.Upsert(created));                   // no token: unconditional create
        Assert.NotNull(created.ConcurrencyToken);

        var first = await repository.GetById("A1");
        var second = await repository.GetById("A1");

        Assert.True(await repository.Upsert(first));
        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => repository.Upsert(second));
    }

    [Fact]
    public async Task Write_TokenIsNotStoredAsDataField()
    {
        var (repository, container) = await CreateAccountsAsync();
        await repository.Add(new Account { Id = "A1", ConcurrencyToken = "ignored" });

        var stored = (await container.ReadItemAsync<JObject>("A1", new PartitionKey("A1"))).Resource;

        Assert.Null(stored["concurrencyToken"]);
        Assert.NotNull(stored["_etag"]);
    }

    [Fact]
    public async Task ModelWithoutToken_LastWriteWins()
    {
        var container = await emulator.CreateContainerAsync("/pk");
        var repository = emulator.CreateRepository<NoteRepository, Note>(new NoteConfig(container.Id));
        await repository.Add(new Note { Id = "N1", Text = "a" });

        var first = await repository.GetById("N1");
        var second = await repository.GetById("N1");
        first.Text = "b";
        second.Text = "c";

        Assert.True(await repository.Update(first));
        Assert.True(await repository.Update(second));
        Assert.Equal("c", (await repository.GetById("N1")).Text);
    }

    public sealed class Account
    {
        public string? Id { get; set; }
        public decimal Balance { get; set; }
        public string? ConcurrencyToken { get; set; }
    }

    public sealed class AccountConfig(string containerName) : IModelConfig<Account>
    {
        public void Configure(IModelConfigOptions<Account> options) => options
            .ContainerName(containerName)
            .Id(a => a.Id)
            .PartitionKey(a => a.Id).WithName("pk")
            .ConcurrencyToken(a => a.ConcurrencyToken);
    }

    public sealed class AccountRepository(ICosmosDbClientFactory factory, ILogger logger, IModelMapRegistry registry)
        : RepositoryBase<Account>(factory, logger, registry);

    public sealed class Note
    {
        public string? Id { get; set; }
        public string? Text { get; set; }
    }

    public sealed class NoteConfig(string containerName) : IModelConfig<Note>
    {
        public void Configure(IModelConfigOptions<Note> options) => options
            .ContainerName(containerName)
            .Id(n => n.Id)
            .PartitionKey(n => n.Id).WithName("pk");
    }

    public sealed class NoteRepository(ICosmosDbClientFactory factory, ILogger logger, IModelMapRegistry registry)
        : RepositoryBase<Note>(factory, logger, registry);
}
