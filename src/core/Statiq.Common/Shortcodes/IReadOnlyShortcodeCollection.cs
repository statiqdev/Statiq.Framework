using System.Collections.Generic;

namespace Statiq.Common.Shortcodes
{
    public interface IReadOnlyShortcodeCollection : IReadOnlyCollection<string>
    {
        bool Contains(string name);

        IShortcode CreateInstance(string name);
    }
}
