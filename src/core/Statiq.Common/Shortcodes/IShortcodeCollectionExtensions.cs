using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class IShortcodeCollectionExtensions
    {
        /// <summary>
        /// Adds a shortcode by type, inferring the name from the type name and removing a trailing "Shortcode" from the type name.
        /// </summary>
        /// <param name="shortcodes">The shortcodes.</param>
        /// <param name="type">The type of the shortcode to add (must implement <see cref="IShortcode"/>).</param>
        public static void Add(this IShortcodeCollection shortcodes, Type type) =>
            shortcodes.Add(type?.Name.RemoveEnd("Shortcode", StringComparison.OrdinalIgnoreCase), type);

        /// <summary>
        /// Adds a shortcode by type, inferring the name from the type name.
        /// </summary>
        /// <typeparam name="TShortcode">The type of the shortcode to add.</typeparam>
        /// <param name="shortcodes">The shortcodes.</param>
        public static void Add<TShortcode>(this IShortcodeCollection shortcodes)
            where TShortcode : IShortcode =>
            shortcodes.Add<TShortcode>(typeof(TShortcode).Name.RemoveEnd("Shortcode", StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Adds a shortcode by type.
        /// </summary>
        /// <typeparam name="TShortcode">The type of the shortcode to add.</typeparam>
        /// <param name="shortcodes">The shortcodes.</param>
        /// <param name="name">The name of the shortcode.</param>
        public static void Add<TShortcode>(this IShortcodeCollection shortcodes, string name)
            where TShortcode : IShortcode
        {
            _ = shortcodes ?? throw new ArgumentNullException(nameof(shortcodes));
            shortcodes.Add(name, () => Activator.CreateInstance<TShortcode>());
        }

        /// <summary>
        /// Adds a shortcode by type.
        /// </summary>
        /// <param name="shortcodes">The shortcodes.</param>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="type">The type of the shortcode to add (must implement <see cref="IShortcode"/>).</param>
        public static void Add(this IShortcodeCollection shortcodes, string name, Type type)
        {
            _ = shortcodes ?? throw new ArgumentNullException(nameof(shortcodes));
            _ = type ?? throw new ArgumentNullException(nameof(type));
            if (!typeof(IShortcode).IsAssignableFrom(type))
            {
                throw new ArgumentException("The type must implement " + nameof(IShortcode), nameof(type));
            }
            shortcodes.Add(name, () => (IShortcode)Activator.CreateInstance(type));
        }

        /// <summary>
        /// Adds a shortcode and uses a <see cref="Config{String}"/> to determine
        /// the shortcode result.
        /// </summary>
        /// <param name="shortcodes">The shortcodes.</param>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="shortcode">The content of the shortcode.</param>
        public static void Add(this IShortcodeCollection shortcodes, string name, Config<string> shortcode) =>
            shortcodes.Add(name, async (_, __, doc, ctx) =>
            {
                string result = shortcode == null ? null : await shortcode.GetValueAsync(doc, ctx);
                return result != null ? ctx.CreateDocument(await ctx.GetContentProviderAsync(result)) : null;
            });

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared arguments and content and the current document and execution context.
        /// </summary>
        /// <param name="shortcodes">The shortcodes.</param>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="shortcode">
        /// A function that has the declared arguments and content and the current document and execution context as inputs
        /// and the result content as an output.
        /// </param>
        public static void Add(
            this IShortcodeCollection shortcodes,
            string name,
            Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, string> shortcode) =>
            shortcodes.Add(name, async (args, content, doc, ctx) =>
            {
                string result = shortcode?.Invoke(args, content, doc, ctx);
                return result != null ? ctx.CreateDocument(await ctx.GetContentProviderAsync(result)) : null;
            });

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared arguments and content and the current document and execution context.
        /// </summary>
        /// <param name="shortcodes">The shortcodes.</param>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="shortcode">
        /// A function that has the declared arguments and content and the current document and execution context as inputs
        /// and the result content as an output.
        /// </param>
        public static void Add(
            this IShortcodeCollection shortcodes,
            string name,
            Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, Task<string>> shortcode)
        {
            _ = shortcodes ?? throw new ArgumentNullException(nameof(shortcodes));
            shortcodes.Add(name, async (args, content, doc, ctx) =>
            {
                string result = await shortcode?.Invoke(args, content, doc, ctx);
                return result != null ? ctx.CreateDocument(await ctx.GetContentProviderAsync(result)) : null;
            });
        }

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared arguments and content and the current document and execution context.
        /// </summary>
        /// <param name="shortcodes">The shortcodes.</param>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="shortcode">
        /// A function that has the declared arguments and content and the current document and execution context as inputs
        /// and a <see cref="IDocument"/> as an output which allows the shortcode to add metadata to the document.
        /// </param>
        public static void Add(
            this IShortcodeCollection shortcodes,
            string name,
            Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, IDocument> shortcode)
        {
            _ = shortcodes ?? throw new ArgumentNullException(nameof(shortcodes));
            shortcodes.Add(name, (args, content, doc, ctx) => Task.FromResult(shortcode?.Invoke(args, content, doc, ctx)));
        }

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared arguments and content and the current document and execution context.
        /// </summary>
        /// <param name="shortcodes">The shortcodes.</param>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="shortcode">
        /// A function that has the declared arguments and content and the current document and execution context as inputs
        /// and a <see cref="IDocument"/> as an output which allows the shortcode to add metadata to the document.
        /// </param>
        public static void Add(
            this IShortcodeCollection shortcodes,
            string name,
            Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, IEnumerable<IDocument>> shortcode)
        {
            _ = shortcodes ?? throw new ArgumentNullException(nameof(shortcodes));
            shortcodes.Add(name, (args, content, doc, ctx) => Task.FromResult(shortcode?.Invoke(args, content, doc, ctx)));
        }

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared arguments and content and the current document and execution context.
        /// </summary>
        /// <param name="shortcodes">The shortcodes.</param>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="shortcode">
        /// A function that has the declared arguments and content and the current document and execution context as inputs
        /// and a <see cref="IDocument"/> as an output which allows the shortcode to add metadata to the document.
        /// </param>
        public static void Add(
            this IShortcodeCollection shortcodes,
            string name,
            Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, Task<IDocument>> shortcode)
        {
            _ = shortcodes ?? throw new ArgumentNullException(nameof(shortcodes));
            shortcodes.Add(name, () => new FuncShortcode(shortcode));
        }

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared arguments and content and the current document and execution context.
        /// </summary>
        /// <param name="shortcodes">The shortcodes.</param>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="shortcode">
        /// A function that has the declared arguments and content and the current document and execution context as inputs
        /// and a <see cref="IDocument"/> collection as an output which allows the shortcode to add metadata to the document.
        /// </param>
        public static void Add(
            this IShortcodeCollection shortcodes,
            string name,
            Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, Task<IEnumerable<IDocument>>> shortcode)
        {
            _ = shortcodes ?? throw new ArgumentNullException(nameof(shortcodes));
            shortcodes.Add(name, () => new FuncShortcode(shortcode));
        }
    }
}
