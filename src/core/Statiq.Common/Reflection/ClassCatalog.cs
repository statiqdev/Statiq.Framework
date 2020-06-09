using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    /// <summary>
    /// Contains all types in all referenced assemblies. The dictionary
    /// key is the full type name and the value is the type.
    /// </summary>
    public class ClassCatalog : IReadOnlyDictionary<string, Type>
    {
        // Key: full type name
        private readonly ConcurrentDictionary<string, Type> _types = new ConcurrentDictionary<string, Type>();

        private readonly ConcurrentQueue<string> _debugMessages = new ConcurrentQueue<string>();

        private readonly object _populateLock = new object();

        private Assembly[] _assemblies;

        /// <summary>
        /// Gets all loaded assemblies.
        /// </summary>
        /// <returns>A collection of available assemblies.</returns>
        public IReadOnlyList<Assembly> GetAssemblies()
        {
            Populate();
            return _assemblies;
        }

        /// <summary>
        /// Gets all types assignable to a specified type.
        /// </summary>
        /// <param name="assignableType">The type of classes to get.</param>
        /// <returns>All classes of the specified type.</returns>
        public IEnumerable<Type> GetTypesAssignableTo(Type assignableType)
        {
            _ = assignableType ?? throw new ArgumentNullException(nameof(assignableType));
            Populate();
            return _types.Values.Where(x => assignableType.IsAssignableFrom(x));
        }

        /// <summary>
        /// Gets all types from a specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly to get types from.</param>
        /// <returns>All types from the specified assembly.</returns>
        public IEnumerable<Type> GetTypesFromAssembly(Assembly assembly)
        {
            _ = assembly ?? throw new ArgumentNullException(nameof(assembly));
            Populate();
            return _types.Values.Where(x => x.Assembly.Equals(assembly));
        }

        /// <summary>
        /// Gets instances for all classes of a specified assignable type.
        /// </summary>
        /// <param name="assignableType">The type of instances to get.</param>
        /// <returns>Instances for all classes of the specified type.</returns>
        public IEnumerable<object> GetInstances(Type assignableType)
        {
            _ = assignableType ?? throw new ArgumentNullException(nameof(assignableType));
            Populate();
            return GetTypesAssignableTo(assignableType).Select(Activator.CreateInstance);
        }

        /// <summary>
        /// Gets a type for the specified full name.
        /// </summary>
        /// <param name="fullName">The full name of the type.</param>
        /// <returns>
        /// A <see cref="Type"/> that matches the specified full name or <c>null</c>
        /// if a corresponding type could not be found.
        /// </returns>
        public Type GetType(string fullName)
        {
            _ = fullName ?? throw new ArgumentNullException(nameof(fullName));
            Populate();
            return _types.TryGetValue(fullName, out Type type) ? type : default;
        }

        /// <summary>
        /// Gets an instance for a specified full name.
        /// </summary>
        /// <param name="fullName">The full name of the type.</param>
        /// <returns>
        /// An instance of the type that matches the full name or <c>null</c>
        /// if a corresponding type could not be found.
        /// </returns>
        public object GetInstance(string fullName)
        {
            _ = fullName ?? throw new ArgumentNullException(nameof(fullName));
            Populate();
            return _types.TryGetValue(fullName, out Type type)
                ? Activator.CreateInstance(type)
                : default;
        }

        /// <summary>
        /// Gets an instance of a specified assignable type and name.
        /// </summary>
        /// <param name="type">The type of instance to get.</param>
        /// <returns>
        /// An instance of the specified type, or the first class that can be assigned to the specified type, or <c>null</c>.
        /// </returns>
        public object GetInstance(Type type)
        {
            _ = type ?? throw new ArgumentNullException(nameof(type));
            Populate();
            if (!_types.TryGetValue(type.FullName, out Type match))
            {
                match = GetTypesAssignableTo(type).FirstOrDefault();
            }
            return match == null ? default : Activator.CreateInstance(type);
        }

        /// <summary>
        /// Gets an instance of a specified assignable type and name.
        /// </summary>
        /// <param name="assignableType">The assignable type of instance to get.</param>
        /// <param name="typeName">The name of the type.</param>
        /// <param name="ignoreCase">if set to <c>true</c> ignore the case of the type name.</param>
        /// <returns>
        /// An instance of the first class that matches the specified type and name or <c>null</c>
        /// if a corresponding type could not be found.
        /// </returns>
        public object GetInstance(Type assignableType, string typeName, bool ignoreCase = false)
        {
            _ = assignableType ?? throw new ArgumentNullException(nameof(assignableType));
            Populate();
            Type type = GetTypesAssignableTo(assignableType).FirstOrDefault(x => x.Name.Equals(
                typeName,
                ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));
            return type == null ? default : Activator.CreateInstance(type);
        }

        public void LogDebugMessagesTo(ILogger logger)
        {
            if (logger != null)
            {
                while (_debugMessages.TryDequeue(out string debugMessage))
                {
                    logger.LogDebug(debugMessage);
                }
            }
        }

        /// <summary>
        /// Populates the catalog. Population will only occur once and if the catalog has already been
        /// populated this method will immediately return.
        /// </summary>
        public void Populate()
        {
            lock (_populateLock)
            {
                if (_assemblies == null)
                {
                    // Add (and load) all assembly dependencies
                    HashSet<Assembly> assemblies;
                    try
                    {
                        assemblies = new HashSet<Assembly>(
                            DependencyContext.Default.RuntimeLibraries
                                .SelectMany(library => library.GetDefaultAssemblyNames(DependencyContext.Default))
                                .Select(Assembly.Load),
                            new AssemblyComparer());
                    }
                    catch
                    {
                        // The DependencyContext may not be available on all platforms so fall
                        // back to recursively loading all referenced assemblies of the entry assembly
                        assemblies = new HashSet<Assembly>(new AssemblyComparer());
                        LoadAssemblies(Assembly.GetEntryAssembly(), assemblies);
                    }

                    // Make sure we've also got all assemblies from the current domain
                    assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies());

                    // Load types in parallel
                    Parallel.ForEach(assemblies, assembly =>
                    {
                        _debugMessages.Enqueue($"Cataloging types in assembly {assembly.FullName}");
                        foreach (Type type in GetLoadableTypes(assembly).Where(x => x.IsPublic && !x.IsAbstract && x.IsClass))
                        {
                            _types.TryAdd(type.FullName, type);
                        }
                    });

                    _assemblies = assemblies.ToArray();
                }
            }
        }

        private void LoadAssemblies(Assembly assembly, HashSet<Assembly> visited)
        {
            if (visited.Contains(assembly))
            {
                return;
            }
            visited.Add(assembly);

            try
            {
                foreach (AssemblyName assemblyName in assembly.GetReferencedAssemblies())
                {
                    try
                    {
                        Assembly referencedAssembly = Assembly.Load(assemblyName);
                        LoadAssemblies(referencedAssembly, visited);
                    }
                    catch (Exception ex)
                    {
                        _debugMessages.Enqueue($"{ex.GetType().Name} exception while loading assembly {assemblyName.Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _debugMessages.Enqueue($"{ex.GetType().Name} exception while getting referenced assemblies from {assembly.FullName}: {ex.Message}");
            }
        }

        private Type[] GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (Exception loaderException in ex.LoaderExceptions)
                {
                    _debugMessages.Enqueue($"ReflectionTypeLoadException for assembly {assembly.FullName}: {loaderException.Message}");
                }
                return ex.Types.Where(t => t != null).ToArray();
            }
        }

        // IDictionary<string, Type>

        public IEnumerable<string> Keys => ((IReadOnlyDictionary<string, Type>)_types).Keys;

        public IEnumerable<Type> Values => ((IReadOnlyDictionary<string, Type>)_types).Values;

        public int Count => _types.Count;

        public Type this[string key] => _types[key];

        public bool ContainsKey(string key) => _types.ContainsKey(key);

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out Type value) => _types.TryGetValue(key, out value);

        public IEnumerator<KeyValuePair<string, Type>> GetEnumerator() => _types.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _types.GetEnumerator();
    }
}
