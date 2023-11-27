using System.Collections.Generic;

namespace AppFactory.Framework.Domain.Paging
{
    public class FiltersSet
    {
        public FiltersSet()
        {
            Filters = new List<Filter>();
        }

        public List<Filter> Filters { get; set; }
    }
}