namespace Statiq.Common
{
    public static class NormalizedPathExtensions
    {
        /// <summary>
        /// Gets a string representation of the path that's guaranteed non-null, used primarily for trace messages.
        /// </summary>
        /// <remarks>
        /// This is an extension so that it can also return a valid string when the normalized path is null.
        /// </remarks>
        /// <returns>A string representation of the path.</returns>
        public static string ToDisplayString(this NormalizedPath path) => path?.ToString() ?? "[null]";
    }
}
