namespace AppFactory.Framework.Domain.Extensions;

public static class CollectionExtensions
{
    public static bool IsNullOrEmpty<TSource>(this IEnumerable<TSource> source)
    {
        return source?.Any() != true;
    }
}