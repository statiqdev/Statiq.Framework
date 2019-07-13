using System;

namespace Statiq.Common
{
    public static class StringExtensions
    {
        public static string RemoveStart(this string str, string value) =>
            str != null && value != null && str.StartsWith(value) ? str.Substring(value.Length) : str;

        public static string RemoveStart(this string str, string value, StringComparison comparisonType) =>
            str != null && value != null && str.StartsWith(value, comparisonType) ? str.Substring(value.Length) : str;

        public static string RemoveEnd(this string str, string value) =>
            str != null && value != null && str.EndsWith(value) ? str.Substring(0, str.Length - value.Length) : str;

        public static string RemoveEnd(this string str, string value, StringComparison comparisonType) =>
            str != null && value != null && str.EndsWith(value, comparisonType) ? str.Substring(0, str.Length - value.Length) : str;
    }
}
