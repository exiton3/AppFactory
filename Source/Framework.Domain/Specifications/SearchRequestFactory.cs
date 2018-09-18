using System.Linq;
using Framework.Domain.Infrastructure;
using Framework.Domain.Paging;

namespace Framework.Domain.Specifications
{
    public class SearchRequestFactory : ISearchRequestFactory
    {
        private readonly ISpecificationBuilder _specificationBuilder;

        public SearchRequestFactory(ISpecificationBuilder specificationBuilder)
        {
            _specificationBuilder = specificationBuilder;
        }

        public SearchRequest Create<TModel>(SearchRequestModel data) where TModel : class
        {
            Check.NotNull(data, "data");

            var searchSpecification = data.FilterSettings.Filters.Any()
                ? _specificationBuilder.Build<TModel>(data.FilterSettings)
                : null;

            var request = new SearchRequest
            {
                Specification = searchSpecification,
                SortingSettings = data.SortingSettings,
                PagingSettings = data.PagingSettings
            };
            return request;
        }
    }
}