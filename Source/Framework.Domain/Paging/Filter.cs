using System.Collections.Generic;
using AppFactory.Framework.Domain.Infrastructure;

namespace AppFactory.Framework.Domain.Paging
{
    public class Filter : IFilter
    {
        private string _operand;

        public Filter()
        {
            Values = new List<FilterItem>();
            _operand = FilterOperands.Equal;
        }

        public string FieldName { get; set; }

        public List<FilterItem> Values { get; set; }

        public string Operand
        {
            get { return _operand; }
            set
            {
                Check.NotNullOrEmpty(value, "The Operand must be defined.");
                _operand = value;
            }
        }
    }
}