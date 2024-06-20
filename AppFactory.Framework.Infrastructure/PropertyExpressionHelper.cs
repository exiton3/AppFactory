using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace AppFactory.Framework.Infrastructure;

public static class PropertyExpressionHelper
{
    public static Func<TContainer, TProperty> InitializeGetter<TContainer, TProperty>(Expression<Func<TContainer, TProperty>> getterExpression)
    {
        return getterExpression.Compile();
    }

    public static Action<TContainer, TProperty> InitializeSetter<TContainer, TProperty>(Expression<Func<TContainer, TProperty>> getter)
    {
        var propertyInfo = (getter.Body as MemberExpression).Member as PropertyInfo;
        if (propertyInfo == null)
        {
            throw new ArgumentException("The expression is not a property", "getter");
        }
        var instance = Expression.Parameter(typeof(TContainer), "instance");
        var parameter = Expression.Parameter(typeof(TProperty), "param");

        return Expression.Lambda<Action<TContainer, TProperty>>(
            Expression.Call(instance, propertyInfo.GetSetMethod(true), parameter),
            new[] { instance, parameter }).Compile();
    }

    public static string GetPropertyName<TContainer, TProperty>(Expression<Func<TContainer, TProperty>> expression)
    {
        var unary = expression.Body as UnaryExpression;
        var member = expression.Body as MemberExpression ?? (unary != null ? unary.Operand as MemberExpression : null);
        if (member != null)
            return member.Member.Name;

        throw new ArgumentException("Expression is not a member access", "expression");
    }

    public static string GetPropertyNameExtended<TPropertySource>(Expression<Func<TPropertySource, object>> expression)
    {
        var lambda = expression as LambdaExpression;
        MemberExpression memberExpression;
        if (lambda.Body is UnaryExpression)
        {
            var unaryExpression = lambda.Body as UnaryExpression;
            memberExpression = unaryExpression.Operand as MemberExpression;
        }
        else
        {
            memberExpression = lambda.Body as MemberExpression;
        }

        Debug.Assert(memberExpression != null,
           "Please provide a lambda expression like 'n => n.PropertyName'");

        if (memberExpression != null)
        {
            var propertyInfo = memberExpression.Member as PropertyInfo;

            return propertyInfo.Name;
        }

        return null;
    }

    public static string GetPropertyName<T>(Expression<Func<T, object>> expression)
    {
        return GetPropertyName<T, object>(expression);
    }

    /// <summary>
    /// Convert a lambda expression for a getter into a setter
    /// </summary>
    public static Action<T, TProperty> GetPropertySetter<T, TProperty>(Expression<Func<T, TProperty>> expression)
    {
        var memberExpression = (MemberExpression)expression.Body;
        var property = (PropertyInfo)memberExpression.Member;
        var setMethod = property.GetSetMethod();

        var parameterT = Expression.Parameter(typeof(T), "x");
        var parameterTProperty = Expression.Parameter(typeof(TProperty), "y");

        var newExpression =
            Expression.Lambda<Action<T, TProperty>>(
                Expression.Call(parameterT, setMethod, parameterTProperty),
                parameterT,
                parameterTProperty
            );

        return newExpression.Compile();
    }

    /// <summary>
    /// Convert a lambda expression for a getter into a setter
    /// </summary>
    public static Action<T, TProperty> GetFieldSetter<T, TProperty>(Expression<Func<T, TProperty>> expression)
    {
        var memberExpression = (MemberExpression)expression.Body;
        var field = (FieldInfo)memberExpression.Member;
        Action<T, TProperty> action = (obj, x) => field.SetValue(obj, x);
        return action;
    }

}