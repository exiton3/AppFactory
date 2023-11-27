using AppFactory.Framework.Domain.Paging;

namespace AppFactory.Framework.Domain.Specifications
{
    public interface ISearchRequestFactory
    {
        SearchRequest Create<TModel>(SearchRequestModel data) where  TModel:class;
    }
}