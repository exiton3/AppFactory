using System.Text;

namespace Framework.Domain.Infrastructure.Extensions
{
    public static class IntExtension
    {

        public static string BuildDigitsSequence(this int length)
        {
            var result = new StringBuilder(length);
            for (int i = 1; i <= length; i++)
            {
                result.Append(i % 10);
            }
            return result.ToString();
        }

    }
}