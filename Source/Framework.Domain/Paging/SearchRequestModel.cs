using AppFactory.Framework.Domain.Repositories;

namespace AppFactory.Framework.Domain.Paging
{
    public class SearchRequestModel
    {
        public SearchRequestModel()
        {
            SortingSettings = new SortingSettings();
            PagingSettings = new PagingSettings();
            FilterSettings = new FiltersSet();
        }

        public PagingSettings PagingSettings { get; set; }

        public SortingSettings SortingSettings { get; set; }

        public FiltersSet FilterSettings { get; set; }
    }
}