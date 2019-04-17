using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Shortcodes
{
    public static class IShortcodeCollectionExtensions
    {
        /// <summary>
        /// Adds a shortcode by type, inferring the name from the type name.
        /// </summary>
        /// <param name="collection">The shortcode collection.</param>
        /// <param name="type">The type of the shortcode to add (must implement <see cref="IShortcode"/>).</param>
        public static void Add(this IShortcodeCollection collection, Type type) =>
            collection.Add(type?.Name, type);

        /// <summary>
        /// Adds a shortcode by type, inferring the name from the type name.
        /// </summary>
        /// <param name="collection">The shortcode collection.</param>
        /// <typeparam name="TShortcode">The type of the shortcode to add.</typeparam>
        public static void Add<TShortcode>(this IShortcodeCollection collection)
            where TShortcode : IShortcode =>
            collection.Add<TShortcode>(typeof(TShortcode).Name);

        /// <summary>
        /// Adds a shortcode by type.
        /// </summary>
        /// <param name="collection">The shortcode collection.</param>
        /// <typeparam name="TShortcode">The type of the shortcode to add.</typeparam>
        /// <param name="name">The name of the shortcode.</param>
        public static void Add<TShortcode>(this IShortcodeCollection collection, string name)
            where TShortcode : IShortcode =>
            collection.Add(name, () => Activator.CreateInstance<TShortcode>());

        /// <summary>
        /// Adds a shortcode by type.
        /// </summary>
        /// <param name="collection">The shortcode collection.</param>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="type">The type of the shortcode to add (must implement <see cref="IShortcode"/>).</param>
        public static void Add(
            this IShortcodeCollection collection,
            string name,
            Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            if (!typeof(IShortcode).IsAssignableFrom(type))
            {
                throw new ArgumentException("The type must implement " + nameof(IShortcode), nameof(type));
            }
            collection.Add(name, () => (IShortcode)Activator.CreateInstance(type));
        }

        /// <summary>
        /// Adds a shortcode and uses a <see cref="DocumentConfig{String}"/> to determine
        /// the shortcode result.
        /// </summary>
        /// <param name="collection">The shortcode collection.</param>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="documentConfig">A delegate that should return a <see cref="string"/>.</param>
        public static void Add(
            this IShortcodeCollection collection,
            string name,
            DocumentConfig<string> documentConfig) =>
            collection.Add(name, async (args, content, doc, ctx) =>
            {
                string result = documentConfig == null ? null : await documentConfig.GetValueAsync(doc, ctx);
                return result != null ? await ctx.GetShortcodeResultAsync(result) : null;
            });

        /// <summary>
        /// Adds a shortcode that determines the result content using the declared content.
        /// </summary>
        /// <param name="collection">The shortcode collection.</param>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="func">A function that has the declared content as an input and the result content as an output.</param>
        public static void Add(
            this IShortcodeCollection collection,
            string name,
            Func<string, string> func) =>
            collection.Add(name, async (args, content, doc, ctx) =>
            {
                string result = func?.Invoke(content);
                return result != null ? await ctx.GetShortcodeResultAsync(result) : null;
            });

        /// <summary>
        /// Adds a shortcode that determines the result content using the declared arguments.
        /// </summary>
        /// <param name="collection">The shortcode collection.</param>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="func">A function that has the declared arguments as an input and the result content as an output.</param>
        public static void Add(
            this IShortcodeCollection collection,
            string name,
            Func<KeyValuePair<string, string>[], string> func) =>
            collection.Add(name, async (args, content, doc, ctx) =>
            {
                string result = func?.Invoke(args);
                return result != null ? await ctx.GetShortcodeResultAsync(result) : null;
            });

        /// <summary>
        /// Adds a shortcode that determines the result content using the declared arguments and content.
        /// </summary>
        /// <param name="collection">The shortcode collection.</param>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="func">A function that has the declared arguments and content as inputs and the result content as an output.</param>
        public static void Add(
            this IShortcodeCollection collection,
            string name,
            Func<KeyValuePair<string, string>[], string, string> func) =>
            collection.Add(name, async (args, content, doc, ctx) =>
            {
                string result = func?.Invoke(args, content);
                return result != null ? await ctx.GetShortcodeResultAsync(result) : null;
            });

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared arguments and content and the current execution context.
        /// </summary>
        /// <param name="collection">The shortcode collection.</param>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="func">
        /// A function that has the declared arguments and content and the current execution context as inputs
        /// and the result content as an output.
        /// </param>
        public static void Add(
            this IShortcodeCollection collection,
            string name,
            Func<KeyValuePair<string, string>[], string, IExecutionContext, string> func) =>
            collection.Add(name, async (args, content, doc, ctx) =>
            {
                string result = func?.Invoke(args, content, ctx);
                return result != null ? await ctx.GetShortcodeResultAsync(result) : null;
            });

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared arguments and content and the current document and execution context.
        /// </summary>
        /// <param name="collection">The shortcode collection.</param>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="func">
        /// A function that has the declared arguments and content and the current document and execution context as inputs
        /// and the result content as an output.
        /// </param>
        public static void Add(
            this IShortcodeCollection collection,
            string name,
            Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, string> func) =>
            collection.Add(name, async (args, content, doc, ctx) =>
            {
                string result = func?.Invoke(args, content, doc, ctx);
                return result != null ? await ctx.GetShortcodeResultAsync(result) : null;
            });

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared arguments and the current execution context.
        /// </summary>
        /// <param name="collection">The shortcode collection.</param>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="func">
        /// A function that has the declared arguments and the current execution context as inputs
        /// and the result content as an output.
        /// </param>
        public static void Add(
            this IShortcodeCollection collection,
            string name,
            Func<KeyValuePair<string, string>[], IExecutionContext, string> func) =>
            collection.Add(name, async (args, content, doc, ctx) =>
            {
                string result = func?.Invoke(args, ctx);
                return result != null ? await ctx.GetShortcodeResultAsync(result) : null;
            });

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared arguments and the current document and execution context.
        /// </summary>
        /// <param name="collection">The shortcode collection.</param>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="func">
        /// A function that has the declared arguments and the current document and execution context as inputs
        /// and the result content as an output.
        /// </param>
        public static void Add(
            this IShortcodeCollection collection,
            string name,
            Func<KeyValuePair<string, string>[], IDocument, IExecutionContext, string> func) =>
            collection.Add(name, async (args, content, doc, ctx) =>
            {
                string result = func?.Invoke(args, doc, ctx);
                return result != null ? await ctx.GetShortcodeResultAsync(result) : null;
            });

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared content and the current execution context.
        /// </summary>
        /// <param name="collection">The shortcode collection.</param>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="func">
        /// A function that has the declared content and the current execution context as inputs
        /// and the result content as an output.
        /// </param>
        public static void Add(
            this IShortcodeCollection collection,
            string name,
            Func<string, IExecutionContext, string> func) =>
            collection.Add(name, async (args, content, doc, ctx) =>
            {
                string result = func?.Invoke(content, ctx);
                return result != null ? await ctx.GetShortcodeResultAsync(result) : null;
            });

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared content and the current document and execution context.
        /// </summary>
        /// <param name="collection">The shortcode collection.</param>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="func">
        /// A function that has the declared content and the current document and execution context as inputs
        /// and the result content as an output.
        /// </param>
        public static void Add(
            this IShortcodeCollection collection,
            string name,
            Func<string, IDocument, IExecutionContext, string> func) =>
            collection.Add(name, async (args, content, doc, ctx) =>
            {
                string result = func?.Invoke(content, doc, ctx);
                return result != null ? await ctx.GetShortcodeResultAsync(result) : null;
            });

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared arguments and content and the current document and execution context.
        /// </summary>
        /// <param name="collection">The shortcode collection.</param>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="func">
        /// A function that has the declared arguments and content and the current document and execution context as inputs
        /// and a <see cref="IShortcodeResult"/> as an output which allows the shortcode to add metadata to the document.
        /// </param>
        public static void Add(
            this IShortcodeCollection collection,
            string name,
            Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, Task<IShortcodeResult>> func) =>
            collection.Add(name, () => new FuncShortcode(func));
    }
}
