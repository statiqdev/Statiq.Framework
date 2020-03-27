using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class IBootstrapperShortcodeExtensions
    {
        public static TBootstrapper AddShortcode<TBootstrapper>(this TBootstrapper bootstrapper, Type type)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add(type));

        public static TBootstrapper AddShortcode<TBootstrapper>(this TBootstrapper bootstrapper, string name, Type type)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add(name, type));

        public static TBootstrapper AddShortcode<TBootstrapper>(this TBootstrapper bootstrapper, string name, Config<string> shortcode)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add(name, shortcode));

        public static TBootstrapper AddShortcode<TBootstrapper>(this TBootstrapper bootstrapper, string name, Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, string> shortcode)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add(name, shortcode));

        public static TBootstrapper AddShortcode<TBootstrapper>(this TBootstrapper bootstrapper, string name, Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, Task<string>> shortcode)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add(name, shortcode));

        public static TBootstrapper AddShortcode<TBootstrapper>(this TBootstrapper bootstrapper, string name, Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, IDocument> shortcode)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add(name, shortcode));

        public static TBootstrapper AddShortcode<TBootstrapper>(this TBootstrapper bootstrapper, string name, Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, IEnumerable<IDocument>> shortcode)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add(name, shortcode));

        public static TBootstrapper AddShortcode<TBootstrapper>(this TBootstrapper bootstrapper, string name, Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, Task<IDocument>> shortcode)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add(name, shortcode));

        public static TBootstrapper AddShortcode<TBootstrapper>(this TBootstrapper bootstrapper, string name, Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, Task<IEnumerable<IDocument>>> shortcode)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add(name, shortcode));
    }
}
