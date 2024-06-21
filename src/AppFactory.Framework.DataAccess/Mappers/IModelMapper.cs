using AppFactory.Framework.DataAccess.Models;
using AppFactory.Framework.Domain.Entities;

namespace AppFactory.Framework.DataAccess.Mappers;

public interface IModelMapper<TEntity, TModel> where TEntity : Entity where TModel : ModelBase
{
    TModel MapToModel(TEntity entity);
    TEntity MapFromModel(TModel model);
}