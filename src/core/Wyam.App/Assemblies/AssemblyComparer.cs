using System.Collections.Generic;
using System.Reflection;

namespace Wyam.App.Assemblies
{
    /// <summary>
    /// Compares two assemblies for equality by comparing at their full names.
    /// </summary>
    internal class AssemblyComparer : IEqualityComparer<Assembly>
    {
        /// <inheritdoc/>
        public bool Equals(Assembly x, Assembly y) => x?.FullName.Equals(y?.FullName) ?? false;

        /// <inheritdoc/>
        public int GetHashCode(Assembly obj) => obj?.GetHashCode() ?? 0;
    }
}