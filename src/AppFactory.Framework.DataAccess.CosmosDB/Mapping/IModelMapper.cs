using AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;

namespace AppFactory.Framework.DataAccess.CosmosDB.Mapping;

public interface IModelMapper<TModel> where TModel : class
{
    CosmosDbDocument MapToDocument(TModel model);
    TModel MapFromDocument(CosmosDbDocument document);
}
