using System.Collections.Generic;

namespace AppFactory.Framework.Domain.Paging
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Results { get; set; }
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int PageSize { get; set; }
        public int RowCount { get; set; }
        public int TotalRecords { get; set; }
    }
}