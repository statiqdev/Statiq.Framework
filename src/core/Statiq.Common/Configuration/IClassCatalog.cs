using System;
using System.Collections.Generic;
using System.Reflection;

namespace Statiq.Common
{
    public interface IClassCatalog
    {
        /// <summary>
        /// Gets all types assignable to a specified type.
        /// </summary>
        /// <param name="assignableType">The type of classes to get.</param>
        /// <returns>All classes of the specified type.</returns>
        IEnumerable<Type> GetTypesAssignableTo(Type assignableType);

        /// <summary>
        /// Gets all types from a specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly to get types from.</param>
        /// <returns>All types from the specified assembly.</returns>
        IEnumerable<Type> GetTypesFromAssembly(Assembly assembly);

        /// <summary>
        /// Gets instances for all classes of a specified assignable type.
        /// </summary>
        /// <param name="assignableType">The type of instances to get.</param>
        /// <returns>Instances for all classes of the specified type.</returns>
        IEnumerable<object> GetInstances(Type assignableType);

        /// <summary>
        /// Gets a type for the specified full name.
        /// </summary>
        /// <param name="fullName">The full name of the type.</param>
        /// <returns>
        /// A <see cref="Type"/> that matches the specified full name or <c>null</c>
        /// if a corresponding type could not be found.
        /// </returns>
        Type GetType(string fullName);

        /// <summary>
        /// Gets an instance for a specified full name.
        /// </summary>
        /// <param name="fullName">The full name of the type.</param>
        /// <returns>
        /// An instance of the type that matches the full name or <c>null</c>
        /// if a corresponding type could not be found.
        /// </returns>
        object GetInstance(string fullName);

        /// <summary>
        /// Gets an instance for a class of a specified assignable type and name.
        /// </summary>
        /// <param name="assignableType">The assignable type of instance to get.</param>
        /// <param name="typeName">The name of the type.</param>
        /// <param name="ignoreCase">if set to <c>true</c> ignore the case of the type name.</param>
        /// <returns>
        /// An instance of the first class that matches the specified type and name or <c>null</c>
        /// if a corresponding type could not be found.
        /// </returns>
        object GetInstance(Type assignableType, string typeName, bool ignoreCase = false);
    }
}
