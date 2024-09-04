using AppFactory.Framework.DataAccess.AmazonDbServices;

namespace AppFactory.Framework.DataAccess.Mapping;

internal interface IModelMapper<TModel> where TModel : class
{
    DynamoDbItem MapToAttributes(TModel model);
    TModel MapModelFromAttributes(DynamoDbItem item);
}