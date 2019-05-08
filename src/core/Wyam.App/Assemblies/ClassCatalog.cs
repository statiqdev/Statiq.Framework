using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyModel;
using Wyam.Common.Configuration;
using Wyam.Common.Tracing;

namespace Wyam.App.Assemblies
{
    /// <summary>
    /// Responsible for iterating over a set of assemblies
    /// looking for implementations of predefined interfaces.
    /// </summary>
    internal class ClassCatalog : IClassCatalog
    {
        private readonly ConcurrentDictionary<string, Type> _types = new ConcurrentDictionary<string, Type>();

        private readonly object _populateLock = new object();

        private bool _populated;

        /// <inheritdoc/>
        public IEnumerable<Type> GetAssignableFrom<T>()
        {
            Populate();
            return _types.Values.Where(x => typeof(T).IsAssignableFrom(x));
        }

        /// <inheritdoc/>
        public IEnumerable<T> GetInstances<T>()
        {
            Populate();
            return GetAssignableFrom<T>().Select(x => (T)Activator.CreateInstance(x));
        }

        /// <inheritdoc/>
        public T GetInstance<T>(string typeName, bool ignoreCase = false)
        {
            Populate();
            Type type = GetAssignableFrom<T>().FirstOrDefault(x => x.Name.Equals(
                typeName,
                ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));
            return type == null ? default(T) : (T)Activator.CreateInstance(type);
        }

        public void Populate()
        {
            lock (_populateLock)
            {
                if (!_populated)
                {
                    Trace.Verbose($"Populating class catalog...");

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
                        Trace.Verbose($"Cataloging types in assembly {assembly.FullName}");
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
                        Trace.Verbose($"{ex.GetType().Name} exception while loading assembly {assemblyName.Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.Verbose($"{ex.GetType().Name} exception while getting referenced assemblies from {assembly.FullName}: {ex.Message}");
            }
        }

        private static Type[] GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (Exception loaderException in ex.LoaderExceptions)
                {
                    Trace.Verbose($"ReflectionTypeLoadException for assembly {assembly.FullName}: {loaderException.Message}");
                }
                return ex.Types.Where(t => t != null).ToArray();
            }
        }
    }
}
