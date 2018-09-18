using System;

namespace Framework.Domain.Infrastructure
{
    public static class Check
    {
        public static void NotNull<TArg>(TArg arg, string parameterName, string message) where TArg : class
        {
            if (arg == null)
            {
                throw new ArgumentNullException(parameterName, message);
            }
        }

        public static void NotNull<TArg>(TArg arg, string parameterName) where TArg : class
        {
            if (arg == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        public static void NotNull<TArg>(TArg arg) where TArg : class
        {
            if (arg == null)
            {
                throw new ArgumentNullException();
            }
        }

        public static void NotNullOrEmpty(string arg, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(arg))
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        public static void NotNullOrEmpty(string arg, string parameterName, string message)
        {
            if (string.IsNullOrWhiteSpace(arg))
            {
                throw new ArgumentNullException(parameterName, message);
            }
        }

        public static void NotEqual<TArg>(TArg value, string name, TArg restrictedValue, string message = null) where TArg : IComparable<TArg>
        {
            if (restrictedValue.CompareTo(value) == 0)
            {
                throw new ArgumentOutOfRangeException(name, value, message ?? $"The parameter value must not be equal to {restrictedValue}. Actual value is {value}.");
            }
        }

        public static void InRange<TArg>(TArg value, string name, TArg min, TArg max, string message = null) where TArg : IComparable<TArg>
        {
            if (min.CompareTo(value) > 0 || max.CompareTo(value) < 0)
            {
                throw new ArgumentOutOfRangeException(name, value, message ?? string.Format("The parameter value must be in range from {1} to {2} inclusively. Actual value is {0}.", value, min, max));
            }
        }
    }
}