using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestRawAssemblyCollection : IRawAssemblyCollection
    {
        private readonly List<byte[]> _rawAssemblies = new List<byte[]>();

        public void Add(byte[] rawAssembly)
        {
            if (rawAssembly != null)
            {
                _rawAssemblies.Add(rawAssembly);
            }
        }

        public int Count => _rawAssemblies.Count;

        public IEnumerator<byte[]> GetEnumerator() => _rawAssemblies.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
