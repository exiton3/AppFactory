using AppFactory.Framework.DataAccess.DynamoDB.DynamoDb;

namespace AppFactory.Framework.DataAccess.DynamoDB.Mapping;

internal interface IModelMapper<TModel> where TModel : class
{
    DynamoDbItem MapToAttributes(TModel model);
    TModel MapModelFromAttributes(DynamoDbItem item);
}