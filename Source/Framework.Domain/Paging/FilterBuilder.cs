using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Framework.Domain.Infrastructure;

namespace Framework.Domain.Paging
{
   public class FilterBuilder
    {
        private readonly Filter _filter;
        private FilterBuilder(string fieldName)
        {
            _filter = new Filter {FieldName = fieldName};
        }
        public static  FilterBuilder CreateFor<TModel>(Expression<Func<TModel, object>> property)
        {

            return new FilterBuilder(PropertyExpressionHelper.GetPropertyName(property));
        }

        public FilterBuilder Operand(string operand)
        {
            _filter.Operand = operand;
            return this;
        }

        public FilterBuilder EqualOperand()
        {
            _filter.Operand = FilterOperands.Equal;
            return this;
        }

        public FilterBuilder AddValue(object value, string displayValue)
        {
            _filter.Values.Add(new FilterItem{ Value = value, DisplayValue = displayValue}); 
            return this;
        }

        public FilterBuilder AddValue(object value)
        {
            return AddValue(value, string.Empty);
        }

        public Filter Build()
        {
            return _filter;
        }

        public FilterBuilder AddValues<TValue>(IEnumerable<TValue> values)
        {
            foreach (var value in values)
            {
                AddValue(value);
            }
            return this;
        }
    }
}