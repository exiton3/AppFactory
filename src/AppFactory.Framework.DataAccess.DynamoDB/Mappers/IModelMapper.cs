using AppFactory.Framework.DataAccess.DynamoDB.Models;
using AppFactory.Framework.Domain.Entities;

namespace AppFactory.Framework.DataAccess.DynamoDB.Mappers;

public interface IModelMapper<TEntity, TModel> where TEntity : Entity where TModel : ModelBase
{
    TModel MapToModel(TEntity entity);
    TEntity MapFromModel(TModel model);
}