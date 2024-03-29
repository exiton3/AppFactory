﻿using AppFactory.Framework.Domain.Entities;

namespace AppFactory.Framework.Domain.Repositories
{
    public interface IRepository<TEntity> : IRepositoryWithTypeId<TEntity, int> where TEntity : Entity
    {
    }
}