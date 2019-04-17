using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Shortcodes;

namespace Wyam.Core.Shortcodes
{
    internal class ShortcodeCollection : IShortcodeCollection
    {
        private readonly Dictionary<string, Func<IShortcode>> _shortcodes =
            new Dictionary<string, Func<IShortcode>>(StringComparer.OrdinalIgnoreCase);

        public IShortcode CreateInstance(string name) => _shortcodes[name]();

        public void Add(string name, Func<IShortcode> shortcodeFactory)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Any(c => char.IsWhiteSpace(c)))
            {
                throw new ArgumentException(nameof(name));
            }
            _shortcodes[name] = shortcodeFactory ?? throw new ArgumentNullException(name);
        }

        public int Count => _shortcodes.Count;

        public bool Contains(string name) => _shortcodes.ContainsKey(name);

        public IEnumerator<string> GetEnumerator() => _shortcodes.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
