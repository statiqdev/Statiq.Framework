using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.App
{
    public static class BoostrapperShortcodeExtensions
    {
        public static Bootstrapper AddShortcode(this Bootstrapper bootstrapper, Type type) =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add(type));

        public static Bootstrapper AddShortcode<TShortcode>(this Bootstrapper bootstrapper)
            where TShortcode : IShortcode =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add<TShortcode>());

        public static Bootstrapper AddShortcode<TShortcode>(this Bootstrapper bootstrapper, string name)
            where TShortcode : IShortcode =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add<TShortcode>(name));

        public static Bootstrapper AddShortcode(this Bootstrapper bootstrapper, string name, Type type) =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add(name, type));

        public static Bootstrapper AddShortcode(this Bootstrapper bootstrapper, string name, Config<string> shortcode) =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add(name, shortcode));

        public static Bootstrapper AddShortcode(this Bootstrapper bootstrapper, string name, Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, string> shortcode) =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add(name, shortcode));

        public static Bootstrapper AddShortcode(this Bootstrapper bootstrapper, string name, Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, Task<string>> shortcode) =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add(name, shortcode));

        public static Bootstrapper AddShortcode(this Bootstrapper bootstrapper, string name, Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, IDocument> shortcode) =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add(name, shortcode));

        public static Bootstrapper AddShortcode(this Bootstrapper bootstrapper, string name, Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, IEnumerable<IDocument>> shortcode) =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add(name, shortcode));

        public static Bootstrapper AddShortcode(this Bootstrapper bootstrapper, string name, Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, Task<IDocument>> shortcode) =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add(name, shortcode));

        public static Bootstrapper AddShortcode(this Bootstrapper bootstrapper, string name, Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, Task<IEnumerable<IDocument>>> shortcode) =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add(name, shortcode));
    }
}
