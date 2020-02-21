using System;

namespace Statiq.Common
{
    public static class SpanExtensions
    {
        // See https://github.com/dotnet/runtime/issues/29758#issuecomment-498645607

        public static bool Replace(
            this Span<char> str,
            char oldChar,
            char newChar)
        {
            bool replaced = false;
            for (int c = 0; c < str.Length; c++)
            {
                if (str[c] == oldChar)
                {
                    str[c] = newChar;
                    replaced = true;
                }
            }
            return replaced;
        }

        public static bool Replace(
            this Span<char> str,
            char[] oldChars,
            char newChar)
        {
            bool replaced = false;
            for (int c = 0; c < str.Length; c++)
            {
                foreach (char oldChar in oldChars)
                {
                    if (str[c] == oldChar)
                    {
                        str[c] = newChar;
                        replaced = true;
                        break;
                    }
                }
            }
            return replaced;
        }
    }
}