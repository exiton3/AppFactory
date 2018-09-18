using Framework.Domain.Repositories;
using Framework.Domain.Specifications;

namespace Framework.Domain.Paging
{
    public class SearchRequest
    {
        public SearchRequest()
        {
            SortingSettings = new SortingSettings();
            PagingSettings = new PagingSettings();
        }

        public SearchRequest(SearchRequestModel data)
        {
            SortingSettings = data.SortingSettings;
            PagingSettings = data.PagingSettings;
        }

        public SortingSettings SortingSettings { get; set; }
        public PagingSettings PagingSettings { get; set; }
        public ISpecification Specification { get; set; }
    }
}