using System.Collections.Generic;

namespace AppFactory.Framework.Domain.Paging
{
    public interface IFilter
    {
        string FieldName { get; set; }
        
        List<FilterItem> Values { get; set; }

        string Operand { get; set; }
    }
}
