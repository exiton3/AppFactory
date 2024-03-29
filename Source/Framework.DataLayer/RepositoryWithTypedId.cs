﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppFactory.Framework.Domain.Entities;
using AppFactory.Framework.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AppFactory.Framework.DataLayer
{
    public abstract class RepositoryWithTypedId<TEntity, TId> : IRepositoryWithTypeId<TEntity, TId>
        where TEntity : EntityWithTypedId<TId>, new()
    {
        private readonly IUnitOfWorkFactory _unitOfWorkfactory;
        protected DbSet<TEntity> Set { get { return UnitOfWork.Context.Set<TEntity>(); } }
        protected IUnitOfWorkDbContext UnitOfWork { get { return _unitOfWorkfactory.Create(); } }

        protected RepositoryWithTypedId(IUnitOfWorkFactory unitOfWorkfactory)
        {
            _unitOfWorkfactory = unitOfWorkfactory;
            Debug.WriteLine("UoW - {0} in - {1} ", UnitOfWork.GetHashCode(), GetType().Name);
        }

        public virtual async Task<TEntity> Get(TId id)
        {
            return await Set.FindAsync(id);
        }

        public virtual void Add(TEntity entity)
        {
            Set.Add(entity);
        }

        public virtual void AddOrUpdate(TEntity entity)
        {
            UnitOfWork.Context.Entry(entity)
                .State = Equals(entity, default(TId))
                ? EntityState.Added
                : EntityState.Modified;
        }

        public virtual void Save()
        {
            UnitOfWork.Save();
        }

        public virtual Task SaveAsync()
        {
           return UnitOfWork.SaveAsync();
        }

        public virtual Task SaveAsync(CancellationToken cancellationToken)
        {
            return UnitOfWork.SaveAsync(cancellationToken);
        }

        public virtual void Delete(TEntity entity)
        {
            Set.Remove(entity);
        }

        public virtual void DeleteById(TId id)
        {
            var entity = new TEntity { Id = id };
            Set.Attach(entity);
            Set.Remove(entity);
        }

        public async Task<IEnumerable<TEntity>> GetAll()
        {
            return await Set.ToListAsync();
        }

        public IEnumerable<TEntity> GetList(IEnumerable<TId> idsList)
        {
            return GetByIdList(idsList).ToList();
        }

        protected IQueryable<TEntity> GetByIdList(IEnumerable<TId> idsList)
        {
            return Set.Where(x => idsList.Contains(x.Id));
        }

        public virtual IEnumerable<TEntity> GetByIdList(TId[] idList)
        {
            return Set
                .Where(entity => idList.Contains(entity.Id))
                .ToArray();
        }

        public void Update(TEntity entity)
        {
            UnitOfWork.Context.Entry((object)entity)
                .State = EntityState.Modified;
        }
        
        protected DbSet<T> GetSet<T>() where T : class
        {
            return UnitOfWork.Context.Set<T>();
        }

        protected void Update(object entity)
        {
            UnitOfWork.Context.Entry(entity)
                .State = EntityState.Modified;
        }
    }
}