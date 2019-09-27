using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.App
{
    /// <summary>
    /// Responsible for iterating over a set of assemblies
    /// looking for implementations of predefined interfaces.
    /// </summary>
    internal class ClassCatalog : IClassCatalog
    {
        private readonly ConcurrentDictionary<string, Type> _types = new ConcurrentDictionary<string, Type>();

        private readonly ConcurrentQueue<string> _debugMessages = new ConcurrentQueue<string>();

        private readonly object _populateLock = new object();

        private bool _populated;

        /// <inheritdoc/>
        public IEnumerable<Type> GetTypesAssignableTo(Type assignableType)
        {
            _ = assignableType ?? throw new ArgumentNullException(nameof(assignableType));
            Populate();
            return _types.Values.Where(x => assignableType.IsAssignableFrom(x));
        }

        /// <inheritdoc/>
        public IEnumerable<Type> GetTypesFromAssembly(Assembly assembly)
        {
            _ = assembly ?? throw new ArgumentNullException(nameof(assembly));
            Populate();
            return _types.Values.Where(x => x.Assembly.Equals(assembly));
        }

        /// <inheritdoc/>
        public IEnumerable<object> GetInstances(Type assignableType)
        {
            _ = assignableType ?? throw new ArgumentNullException(nameof(assignableType));
            Populate();
            return GetTypesAssignableTo(assignableType).Select(Activator.CreateInstance);
        }

        /// <inheritdoc/>
        public Type GetType(string fullName)
        {
            _ = fullName ?? throw new ArgumentNullException(nameof(fullName));
            Populate();
            return _types.TryGetValue(fullName, out Type type) ? type : default;
        }

        public object GetInstance(string fullName)
        {
            _ = fullName ?? throw new ArgumentNullException(nameof(fullName));
            Populate();
            return _types.TryGetValue(fullName, out Type type)
                ? Activator.CreateInstance(type)
                : default;
        }

        /// <inheritdoc/>
        public object GetInstance(Type assignableType, string typeName, bool ignoreCase = false)
        {
            _ = assignableType ?? throw new ArgumentNullException(nameof(assignableType));
            Populate();
            Type type = GetTypesAssignableTo(assignableType).FirstOrDefault(x => x.Name.Equals(
                typeName,
                ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));
            return type == null ? default : Activator.CreateInstance(type);
        }

        internal void LogDebugMessages(ILogger logger)
        {
            if (logger != null)
            {
                while (_debugMessages.TryDequeue(out string debugMessage))
                {
                    logger.LogDebug(debugMessage);
                }
            }
        }

        public void Populate()
        {
            lock (_populateLock)
            {
                if (!_populated)
                {
                    Assembly[] assemblies;
                    try
                    {
                        assemblies = DependencyContext.Default.RuntimeLibraries
                            .SelectMany(library => library.GetDefaultAssemblyNames(DependencyContext.Default))
                            .Select(Assembly.Load)
                            .Distinct(new AssemblyComparer())
                            .ToArray();
                    }
                    catch
                    {
                        // The DependencyContext may not be available on all platforms so fall
                        // back to recursively loading all referenced assemblies of the entry assembly
                        HashSet<Assembly> visited = new HashSet<Assembly>(new AssemblyComparer());
                        LoadAssemblies(Assembly.GetEntryAssembly(), visited);
                        assemblies = visited.ToArray();
                    }

                    // Load types
                    Parallel.ForEach(assemblies, assembly =>
                    {
                        _debugMessages.Enqueue($"Cataloging types in assembly {assembly.FullName}");
                        foreach (Type type in GetLoadableTypes(assembly).Where(x => x.IsPublic && !x.IsAbstract && x.IsClass))
                        {
                            _types.TryAdd(type.FullName, type);
                        }
                    });
                }
                _populated = true;
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
    }
}
