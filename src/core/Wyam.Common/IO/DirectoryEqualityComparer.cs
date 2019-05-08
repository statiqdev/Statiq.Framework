using System.Collections.Generic;

namespace Wyam.Common.IO
{
    /// <summary>
    /// Compares <see cref="IDirectory"/> equality.
    /// </summary>
    public class DirectoryEqualityComparer : IEqualityComparer<IDirectory>
    {
        /// <inheritdoc />
        public bool Equals(IDirectory x, IDirectory y)
        {
            return x.Path.Equals(y.Path);
        }

        /// <inheritdoc />
        public int GetHashCode(IDirectory obj)
        {
            return obj.Path.GetHashCode();
        }
    }
}
