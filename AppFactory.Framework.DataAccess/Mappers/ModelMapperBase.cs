using AppFactory.Framework.DataAccess.Models;
using AppFactory.Framework.Domain.Entities;

namespace AppFactory.Framework.DataAccess.Mappers;

public abstract class ModelMapperBase<TEntity, TModel> : IModelMapper<TEntity, TModel> where TEntity : Entity where TModel : ModelBase
{
    public TModel MapToModel(TEntity entity)
    {
        var model = MapTo(entity);

        model.Id = entity.Id;

        return model;
    }

    protected abstract TModel MapTo(TEntity entity);

    public TEntity MapFromModel(TModel model)
    {
        var entity = MapFrom(model);

        entity.Id = model.Id;

        return entity;
    }

    protected abstract TEntity MapFrom(TModel model);
}