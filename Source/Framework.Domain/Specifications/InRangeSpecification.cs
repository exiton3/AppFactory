using System;
using System.Linq.Expressions;

namespace AppFactory.Framework.Domain.Specifications
{
    public class InRangeSpecification<T> : SpecificationBase<T> where T : class 
    {
        private readonly object _left;
        private readonly object _right;

        public InRangeSpecification(object left, object right, string propertyName)
        {
            _left = left;
            _right = right;
            Property = propertyName;
        }

        public string Property { get; private set; }

        public override Expression<Func<T, bool>> GetExpression()
        {
            var first = new LessThanSpecification<T>(_right, Property);
            var second = new GreaterThanSpecification<T>(_left, Property);
            return first.And(second).GetExpression();
        }
    }
}