using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Framework.Domain.ReadModel;
using Microsoft.EntityFrameworkCore;

namespace Framework.DataLayer.ReadModel
{
    public abstract class ReadModelRepository<TReadModel> : IReadModelRepository<TReadModel>
        where TReadModel : class, IReadModel
    {
        protected readonly IContextFactory ContextFactory;

        protected ReadModelRepository(IContextFactory contextFactory)
        {
            ContextFactory = contextFactory;
           
        }

        public async Task<IEnumerable<TReadModel>> GetAllAsync()
        {
            using (var context = ContextFactory.Create())
            {
                return await context.Set<TReadModel>().ToListAsync();
            }
        }

        public virtual TReadModel Get(int id)
        {
            using (var context = ContextFactory.Create())
            {
                return context.Set<TReadModel>().Find(id);
            }
        }

        public virtual async Task<TReadModel> GetAsync(int id)
        {
            using (var context = ContextFactory.Create())
            {
                return await context.Set<TReadModel>().FindAsync(id);
            }
        }

        public virtual async Task DeleteAsync(Expression<Func<TReadModel, bool>> predicate)
        {
            using (var context = ContextFactory.Create())
            {
                var entity = await context.Set<TReadModel>().FirstOrDefaultAsync(predicate, CancellationToken.None);
                if (entity == null) return;

                context.Set<TReadModel>().Remove(entity);
                await context.SaveChangesAsync();
            }
        }

        public virtual async Task DeleteAsync(TReadModel entity)
        {
            using (var context = ContextFactory.Create())
            {
                context.Set<TReadModel>().Remove(entity);
                await context.SaveChangesAsync();
            }
        }

        public virtual async Task DeleteAsync(int id)
        {
            using (var context = ContextFactory.Create())
            {
                var entity = await context.Set<TReadModel>().FindAsync(id);
                if (entity != null)
                {
                    context.Set<TReadModel>().Remove(entity);
                    await context.SaveChangesAsync();
                }
            }
        }

        public virtual void Add(TReadModel entity)
        {
            using (var context = ContextFactory.Create())
            {
                context.Set<TReadModel>().Add(entity);
                context.SaveChanges();
            }
        }

      
        public virtual void Delete(TReadModel entity)
        {
            using (var context = ContextFactory.Create())
            {
                context.Set<TReadModel>().Remove(entity);
                context.SaveChanges();
            }
        }

        public virtual void DeleteById(int id)
        {
            Delete(Get(id));
        }

        public void Update(TReadModel entity)
        {
            using (var context = ContextFactory.Create())
            {
                context.Entry((object) entity)
                    .State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public virtual async Task AddAsync(TReadModel entity)
        {
            using (var context = ContextFactory.Create())
            {
                context.Set<TReadModel>().Add(entity);
                 await context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(TReadModel entity)
        {
            using (var context = ContextFactory.Create())
            {
                context.Entry((object) entity)
                    .State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
        }
    }
}