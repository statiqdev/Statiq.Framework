using System;
using System.Collections.Generic;

namespace Wyam.Common.Configuration
{
    public interface IClassCatalog
    {
        /// <summary>
        /// Gets all types assignable from a specified type.
        /// </summary>
        /// <typeparam name="T">The type of classes to get.</typeparam>
        /// <returns>All classes of type <c>T</c>.</returns>
        IEnumerable<Type> GetAssignableFrom<T>();

        /// <summary>
        /// Gets instances for all classes of a specified type..
        /// </summary>
        /// <typeparam name="T">The type of instances to get.</typeparam>
        /// <returns>Instances for all classes of type <c>T</c>.</returns>
        IEnumerable<T> GetInstances<T>();

        /// <summary>
        /// Gets an instance for a class of a specified type and name.
        /// </summary>
        /// <typeparam name="T">The type of the instance to get.</typeparam>
        /// <param name="typeName">The name of the type.</param>
        /// <param name="ignoreCase">if set to <c>true</c> ignore the case of the type name.</param>
        /// <returns>
        /// An instance of the first class that matches the specified type and name.
        /// </returns>
        T GetInstance<T>(string typeName, bool ignoreCase = false);
    }
}
