using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppFactory.Framework.Domain.Entities;
using AppFactory.Framework.Domain.Paging;
using AppFactory.Framework.Domain.Repositories;
using AppFactory.Framework.Domain.Specifications;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AppFactory.Framework.DataLayer
{
    public abstract class SearchRepository<TEntity> : Repository<TEntity>, ISearchRepository<TEntity> where TEntity : Entity, new()
    {
        protected IMapper Mapper { get; }

        protected SearchRepository(IUnitOfWorkFactory unitOfWork, IMapper mapper) : base(unitOfWork)
        {
            Mapper = mapper;
        }


        public IEnumerable<TEntityDto> GetAll<TEntityDto>() where TEntityDto : class
        {
            var entityDtos = Set.ProjectTo<TEntityDto>(Mapper.ConfigurationProvider);
          //  Console.WriteLine(entityDtos.ToString());
            var resultItems = entityDtos.ToList();

            return resultItems;
        }

        [Obsolete("Use GetAllOrderedAsync instead")]
        public PagedResult<TEntityDto> GetAll<TEntityDto>(SearchRequest request) where TEntityDto : class
        {
            var projectTo = Set.ProjectTo<TEntityDto>(Mapper.ConfigurationProvider);

            if (request.Specification != null)
            {
                var specification = (ISpecification<TEntityDto>)request.Specification;
                projectTo = projectTo.Where(specification.GetExpression());
            }

            var entityDtos = projectTo.OrderByFields(request.SortingSettings);

            if (request.PagingSettings != null)
                entityDtos = entityDtos.ToPage(request.PagingSettings);

            //Console.WriteLine(entityDtos.ToString());
            var resultItems = entityDtos.ToList();

            var result = MakePagedResult(request.PagingSettings, resultItems, projectTo.Count());
            return result;
        }

        public async Task<TableInfo> GetTotalRowsAsync(string tableName)
        {
            var sql = @"select sum (spart.rows) as [RowCount] " +
                      "from sys.partitions spart " +
                      $"where spart.object_id = object_id('{tableName}')" +
                      " and spart.index_id < 2";

            var result = await UnitOfWork.Context.Database.SqlQueryRaw<TableInfo>(sql).SingleAsync();

            return result;
        }


        public async Task<int> GetTotalMatchesAsync<TDto>(SearchRequest request) where TDto : class
        {
            var projectTo = Set.ProjectTo<TDto>(Mapper.ConfigurationProvider);

            if (request.Specification != null)
            {
                var specification = (ISpecification<TDto>)request.Specification;
                projectTo = projectTo.Where(specification.GetExpression());
            }

            return await projectTo.CountAsync();
        }

        public async Task<SimplePagedResult<TEntityDto>> GetAllAsync<TEntityDto>(SearchRequest request) where TEntityDto : class
        {
            request.SortingSettings = new SortingSettings(new SortingRule{Field = "Id", SortOrder = SortOrder.Asc});

            return await SearchOnAsync(Set.ProjectTo<TEntityDto>(Mapper.ConfigurationProvider), request);
        }

        public async Task<SimplePagedResult<TEntityDto>> GetAllOrderedAsync<TEntityDto>(SearchRequest request) where TEntityDto : class
        {
            return await SearchOnAsync(Set.ProjectTo<TEntityDto>(Mapper.ConfigurationProvider), request);
        }

        protected async Task<SimplePagedResult<TDto>> SearchOnAsync<TDto>(IQueryable<TDto> source, SearchRequest request) where TDto : class
        {
            if (request.Specification is ISpecification<TDto> specification)
            {
                source = source.Where(specification.GetExpression());
            }
            if (request.SortingSettings != null)
            {
                source = source.OrderByFields(request.SortingSettings);
            }
            if (request.PagingSettings != null)
            {
                source = source.ToPage(request.PagingSettings);
            }
            var resultItems = await source.ToListAsync();
            var result = MakeSimplePagedResult(request.PagingSettings, resultItems);
            return result;
        }

        private PagedResult<TEntityDto> MakePagedResult<TEntityDto>(PagingSettings pagingSettings, IEnumerable<TEntityDto> resultItems, int rowCount) where TEntityDto : class
        {
            var totalRecords = Set.Count();
            var result = new PagedResult<TEntityDto>
            {
                CurrentPage = pagingSettings.PageNumber,
                PageSize = pagingSettings.PageSize,
                RowCount = rowCount,
                Results = resultItems,
                TotalRecords = totalRecords
            };
            var pageCount = (double)result.RowCount / pagingSettings.PageSize;
            result.PageCount = (int)Math.Ceiling(pageCount);
            return result;
        }

        private SimplePagedResult<TEntityDto> MakeSimplePagedResult<TEntityDto>(PagingSettings pagingSettings, IEnumerable<TEntityDto> resultItems) where TEntityDto : class
        {
            var result = new SimplePagedResult<TEntityDto>
            {
                CurrentPage = pagingSettings.PageNumber,
                PageSize = pagingSettings.PageSize,
                Results = resultItems,
            };
            return result;
        }

        public IEnumerable<TEntityDto> GetAll<TEntityDto>(ISpecification specification, int limit = 10000) where TEntityDto : class
        {
            var projectTo = Set.ProjectTo<TEntityDto>(Mapper.ConfigurationProvider);

            if (specification != null)
            {
                projectTo = projectTo.Where(((ISpecification<TEntityDto>)specification).GetExpression());
            }

            var entityDtos = projectTo;

            Console.WriteLine(entityDtos.ToString());

            var resultItems = entityDtos.Take(limit).ToList();

            return resultItems;
        }

    }
}