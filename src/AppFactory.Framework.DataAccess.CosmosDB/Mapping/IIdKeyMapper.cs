using AppFactory.Framework.DataAccess.CosmosDB.Configuration;

namespace AppFactory.Framework.DataAccess.CosmosDB.Mapping;

internal interface IIdKeyMapper<T>
{
    KeyValuePair<string,string> MapId(T model);
    
}

internal class IdKeyMapper<T>(CosmosDbModelConfig<T> config) : IIdKeyMapper<T>
    where T : class
{
    public KeyValuePair<string, string> MapId(T model)
    {
        var documentKey = config.GetDocumentKey(model);

        return new KeyValuePair<string, string>(CosmosDbConstants.Id, documentKey.Id);
    }
}