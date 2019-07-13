using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// Compares <see cref="IFile"/> equality.
    /// </summary>
    public class FileEqualityComparer : IEqualityComparer<IFile>
    {
        /// <inheritdoc />
        public bool Equals(IFile x, IFile y)
        {
            return x.Path.Equals(y.Path);
        }

        /// <inheritdoc />
        public int GetHashCode(IFile obj)
        {
            return obj.Path.GetHashCode();
        }
    }
}