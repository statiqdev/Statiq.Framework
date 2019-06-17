using System;
using System.Collections.Generic;
using System.Text;

namespace Statiq.Common.IO
{
    public static class NormalizedPathExtensions
    {
        /// <summary>
        /// Gets a string representation of the path that's guaranteed non-null, used primarily for trace messages.
        /// </summary>
        /// <returns>A string representation of the path.</returns>
        public static string ToDisplayString(this NormalizedPath path) => path?.ToString() ?? "[null]";
    }
}
