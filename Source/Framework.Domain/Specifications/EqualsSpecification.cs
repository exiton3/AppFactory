using System;
using System.Linq.Expressions;

namespace Framework.Domain.Specifications
{
   public class EqualsSpecification<T> : SpecificationBase<T> where T : class 
    {
        private  object _value;

        public EqualsSpecification(object filterValue, string propertyName)
        {
            _value = filterValue;
            Property = propertyName;
        }

        public string Property { get; }


        public override Expression<Func<T, bool>> GetExpression()
        {
            ParameterExpression paramExp = Expression.Parameter(typeof(T), "x");
            MemberExpression memberExp = Expression.Property(paramExp, Property);
            Expression expression = memberExp;
            var type = memberExp.Type;
            if (type.IsEnum)
            {
                expression = Expression.Convert(memberExp, typeof(int));
                _value = Convert.ToInt32(_value);
            }

            if (_value is long)
            {
                _value = Convert.ToInt32(_value);
            }

            var equalExpression = Expression.Equal(Expression.Constant(_value), expression);

            return Expression.Lambda<Func<T, bool>>(equalExpression, paramExp);
        }
    }
}