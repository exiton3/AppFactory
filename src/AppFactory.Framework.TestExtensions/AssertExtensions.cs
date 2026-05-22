using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Xunit;

[assembly: ExcludeFromCodeCoverage]

namespace AppFactory.Framework.TestExtensions; 

[DebuggerStepThrough]
public static class AssertExtensions
{
    private const string Empty = "";

    public static void ShouldBeEqualTo<T>(this T actual, T expected, string message = Empty)
    {
        Assert.Equal(expected, actual);
    }

    public static void ShouldNotBeEqualTo<T>(this T actual, T expected, string message = Empty)
    {
        Assert.NotEqual(expected, actual);
    }

    public static void ShouldBeGreaterThen<T>(this T actual, T expected, string message = Empty) where T : IComparable
    {
        Assert.True(actual.CompareTo(expected) > 0, message);
    }

    public static void ShouldBeLessThen<T>(this T actual, T expected, string message = Empty) where T : IComparable
    {
        Assert.True(actual.CompareTo(expected) < 0, message);
    }

    public static void ShouldBeTrue(this bool actual, string message = Empty)
    {
        Assert.True(actual, message);
    }

    public static void ShouldBeFalse(this bool actual, string message = Empty)
    {
        Assert.False(actual, message);
    }

    public static void ShouldBeNull<T>(this T actual, string message = Empty)
    {
        Assert.Null(actual);
    }

    public static void ShouldBeNotNull<T>(this T actual, string message = Empty)
    {
        Assert.NotNull(actual);
    }

    public static void ShouldBeInstanceOf<TInstance>(this object actual, string message = Empty)
    {
        Assert.IsType<TInstance>(actual);
    }


    public static void ShouldBeEqualTo<T>(this IEnumerable<T> actual, IEnumerable<T> expected)
    {
        Assert.Equal(expected, actual);
    }

    public static void ShouldNotBeEqualTo<T>(this IEnumerable<T> actual, IEnumerable<T> expected)
    {
        Assert.NotEqual(expected, actual);
    }


    public static void ShouldContain<T>(this IEnumerable<T> actual, T expected, string message = Empty)
    {
        Assert.Contains(expected, actual);
    }

    public static void ShouldContain(this string actual, string expected, string message = Empty)
    {
        Assert.Contains(expected, actual);
    }

    public static void ShouldContain<T>(this IEnumerable<T> actual, Func<T, bool> condition, string message = Empty)
    {
        ShouldBeNotNull(actual, message);
        actual.Any(condition).ShouldBeTrue(message);
    }

    public static void ShouldNotContain<T>(this IEnumerable<T> actual, T expected, string message = Empty)
    {
        Assert.DoesNotContain(expected, actual);
    }

    public static void ShouldNotContain<T>(this IEnumerable<T> actual, Func<T, bool> condition, string message = Empty)
    {
        ShouldBeNotNull(actual, message);
        actual.Any(condition).ShouldBeFalse(message);
    }

    public static void ShouldBeEmpty(this string actual, string message = Empty)
    {
        Assert.Equal(string.Empty, actual);
    }

    public static void ShouldNotBeEmpty(this string actual)
    {
        Assert.NotEqual(string.Empty, actual);
    }

    public static void ShouldAllMeetTheCondition<T>(this IEnumerable<T> collection, Func<T, bool> condition)
    {
        var errors = new List<string>();

        foreach (var element in collection)
            if (!condition(element))
                errors.Add(element.ToString());

        errors.Count.ShouldBeEqualTo(0, $"Condition failed for elements <{typeof(T)}> :" + string.Join(",", errors));
    }

    public static void ShouldThrow<TException>(this Func<object> action) where TException : Exception
    {
        Assert.Throws<ArgumentNullException>(action);
    }
}