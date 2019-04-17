using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Shortcodes
{
    public interface IShortcodeCollection : IReadOnlyShortcodeCollection
    {
        /// <summary>
        /// Adds a shortcode using a factory.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="shortcodeFactory">A factory that returns an <see cref="IShortcode"/>.</param>
        void Add(string name, Func<IShortcode> shortcodeFactory);
    }
}
