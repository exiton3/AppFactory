using Framework.Domain.Paging;

namespace Framework.Domain.Specifications
{
    public interface ISearchRequestFactory
    {
        SearchRequest Create<TModel>(SearchRequestModel data) where  TModel:class;
    }
}