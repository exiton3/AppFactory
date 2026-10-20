using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace AppFactory.Framework.DataAccess.CosmosDB.Configuration;

internal static class PropertyExpressionHelper
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

    /// <summary>
    /// Creates a setter that converts the value read from a Cosmos document (usually a string) to the property type.
    /// Returns null when the expression is not a settable property of <typeparamref name="TContainer"/>.
    /// </summary>
    public static Action<TContainer, object>? TryInitializeConvertingSetter<TContainer, TProperty>(Expression<Func<TContainer, TProperty>> getter)
    {
        if (getter.Body is not MemberExpression { Member: PropertyInfo propertyInfo } memberExpression
            || memberExpression.Expression is not ParameterExpression
            || propertyInfo.GetSetMethod(true) == null)
        {
            return null;
        }

        return InitializeConvertingSetter(getter);
    }

    /// <summary>
    /// Creates a setter that converts the value read from a Cosmos document (usually a string) to the property type.
    /// </summary>
    public static Action<TContainer, object> InitializeConvertingSetter<TContainer, TProperty>(Expression<Func<TContainer, TProperty>> getter)
    {
        var setter = InitializeSetter(getter);
        return (instance, value) => setter(instance, ConvertValue<TProperty>(value));
    }

    // Handles type conversion for values coming back from Newtonsoft.Json deserialization
    public static TProperty ConvertValue<TProperty>(object? value)
    {
        if (value is TProperty typedValue)
        {
            return typedValue;
        }

        if (value == null)
        {
            return default!;
        }

        var targetType = Nullable.GetUnderlyingType(typeof(TProperty)) ?? typeof(TProperty);

        if (targetType == typeof(Guid))
        {
            return (TProperty)(object)Guid.Parse(value.ToString()!);
        }

        if (targetType.IsEnum)
        {
            return (TProperty)Enum.Parse(targetType, value.ToString()!, ignoreCase: true);
        }

        return (TProperty)Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
    }

    public static string GetPropertyName<TModel, TKey>(Expression<Func<TModel, TKey>> expression) where TModel : class
    {
        
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        throw new ArgumentException("Expression must be a member expression");
    }
}