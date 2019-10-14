using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ConcurrentCollections;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestNamespacesCollection : INamespacesCollection
    {
        private readonly ConcurrentHashSet<string> _namespaces = new ConcurrentHashSet<string>();

        public TestNamespacesCollection()
        {
            // This is the default set of namespaces that should brought in scope during configuration and in other dynamic modules
            _namespaces.AddRange(new[]
            {
                "System",
                "System.Threading.Tasks",
                "System.Collections.Generic",
                "System.Linq",
            });

            // Add all public namespaces from Statiq.Common
            _namespaces.AddRange(typeof(IEngine).Assembly.GetTypes()
                .Where(x => x.IsPublic)
                .Select(x => x.Namespace)
                .Distinct());
        }

        public TestNamespacesCollection(params string[] namespaces)
        {
            _namespaces.AddRange(namespaces);
        }

        public bool Add(string ns) => _namespaces.Add(ns);

        public void AddRange(IEnumerable<string> namespaces) => _namespaces.AddRange(namespaces);

        public int Count => _namespaces.Count;

        public IEnumerator<string> GetEnumerator() => _namespaces.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
