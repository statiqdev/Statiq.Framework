using System;
using System.Text.RegularExpressions;

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

        public static string ToKebab(this string str) =>
            Regex.Replace(
                str,
                "([a-z])([A-Z])",
                "$1-$2")
                .ToLower();

        /// <summary>
        /// Allocates a <see cref="Span{T}"/> and copies the characters of the string into it.
        /// </summary>
        /// <param name="str">The string to copy to a <see cref="Span{T}"/>.</param>
        /// <returns>A new <see cref="Span{T}"/> with the characters of the string.</returns>
        public static Span<char> ToSpan(this string str)
        {
            Span<char> chars = new char[str.Length];
            str.AsSpan().CopyTo(chars);
            return chars;
        }

        /// <summary>
        /// Allocates a <see cref="Memory{T}"/> and copies the characters of the string into it.
        /// </summary>
        /// <param name="str">The string to copy to a <see cref="Memory{T}"/>.</param>
        /// <returns>A new <see cref="Memory{T}"/> with the characters of the string.</returns>
        public static Memory<char> ToMemory(this string str)
        {
            Memory<char> chars = new char[str.Length];
            str.AsMemory().CopyTo(chars);
            return chars;
        }
    }
}
