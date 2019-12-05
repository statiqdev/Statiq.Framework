using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public partial interface IShortcodeCollection
    {
        /// <summary>
        /// Adds a shortcode by type, inferring the name from the type name and removing a trailing "Shortcode" from the type name.
        /// </summary>
        /// <param name="type">The type of the shortcode to add (must implement <see cref="IShortcode"/>).</param>
        public void Add(Type type) => Add(type?.Name.RemoveEnd("Shortcode", StringComparison.OrdinalIgnoreCase), type);

        /// <summary>
        /// Adds a shortcode by type, inferring the name from the type name.
        /// </summary>
        /// <typeparam name="TShortcode">The type of the shortcode to add.</typeparam>
        public void Add<TShortcode>()
            where TShortcode : IShortcode =>
            Add<TShortcode>(typeof(TShortcode).Name.RemoveEnd("Shortcode", StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Adds a shortcode by type.
        /// </summary>
        /// <typeparam name="TShortcode">The type of the shortcode to add.</typeparam>
        /// <param name="name">The name of the shortcode.</param>
        public void Add<TShortcode>(string name)
            where TShortcode : IShortcode =>
            Add(name, () => Activator.CreateInstance<TShortcode>());

        /// <summary>
        /// Adds a shortcode by type.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="type">The type of the shortcode to add (must implement <see cref="IShortcode"/>).</param>
        public void Add(string name, Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            if (!typeof(IShortcode).IsAssignableFrom(type))
            {
                throw new ArgumentException("The type must implement " + nameof(IShortcode), nameof(type));
            }
            Add(name, () => (IShortcode)Activator.CreateInstance(type));
        }

        /// <summary>
        /// Adds a shortcode and uses a <see cref="Config{String}"/> to determine
        /// the shortcode result.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="shortcode">The content of the shortcode.</param>
        public void Add(string name, Config<string> shortcode) =>
            Add(name, async (_, __, doc, ctx) =>
            {
                string result = shortcode == null ? null : await shortcode.GetValueAsync(doc, ctx);
                return result != null ? ctx.CreateDocument(await ctx.GetContentProviderAsync(result)) : null;
            });

        /// <summary>
        /// Adds a shortcode that determines the result content using the declared content.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="shortcode">A function that has the declared content as an input and the result content as an output.</param>
        public void Add(string name, Func<string, string> shortcode) =>
            Add(name, async (_, content, __, ctx) =>
            {
                string result = shortcode?.Invoke(content);
                return result != null ? ctx.CreateDocument(await ctx.GetContentProviderAsync(result)) : null;
            });

        /// <summary>
        /// Adds a shortcode that determines the result content using the declared arguments.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="shortcode">A function that has the declared arguments as an input and the result content as an output.</param>
        public void Add(string name, Func<KeyValuePair<string, string>[], string> shortcode) =>
            Add(name, async (args, _, __, ctx) =>
            {
                string result = shortcode?.Invoke(args);
                return result != null ? ctx.CreateDocument(await ctx.GetContentProviderAsync(result)) : null;
            });

        /// <summary>
        /// Adds a shortcode that determines the result content using the declared arguments and content.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="shortcode">A function that has the declared arguments and content as inputs and the result content as an output.</param>
        public void Add(string name, Func<KeyValuePair<string, string>[], string, string> shortcode) =>
            Add(name, async (args, content, _, ctx) =>
            {
                string result = shortcode?.Invoke(args, content);
                return result != null ? ctx.CreateDocument(await ctx.GetContentProviderAsync(result)) : null;
            });

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared arguments and content and the current execution context.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="shortcode">
        /// A function that has the declared arguments and content and the current execution context as inputs
        /// and the result content as an output.
        /// </param>
        public void Add(string name, Func<KeyValuePair<string, string>[], string, IExecutionContext, string> shortcode) =>
            Add(name, async (args, content, _, ctx) =>
            {
                string result = shortcode?.Invoke(args, content, ctx);
                return result != null ? ctx.CreateDocument(await ctx.GetContentProviderAsync(result)) : null;
            });

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared arguments and content and the current document and execution context.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="shortcode">
        /// A function that has the declared arguments and content and the current document and execution context as inputs
        /// and the result content as an output.
        /// </param>
        public void Add(string name, Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, string> shortcode) =>
            Add(name, async (args, content, doc, ctx) =>
            {
                string result = shortcode?.Invoke(args, content, doc, ctx);
                return result != null ? ctx.CreateDocument(await ctx.GetContentProviderAsync(result)) : null;
            });

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared arguments and the current execution context.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="shortcode">
        /// A function that has the declared arguments and the current execution context as inputs
        /// and the result content as an output.
        /// </param>
        public void Add(string name, Func<KeyValuePair<string, string>[], IExecutionContext, string> shortcode) =>
            Add(name, async (args, _, __, ctx) =>
            {
                string result = shortcode?.Invoke(args, ctx);
                return result != null ? ctx.CreateDocument(await ctx.GetContentProviderAsync(result)) : null;
            });

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared arguments and the current document and execution context.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="shortcode">
        /// A function that has the declared arguments and the current document and execution context as inputs
        /// and the result content as an output.
        /// </param>
        public void Add(string name, Func<KeyValuePair<string, string>[], IDocument, IExecutionContext, string> shortcode) =>
            Add(name, async (args, _, doc, ctx) =>
            {
                string result = shortcode?.Invoke(args, doc, ctx);
                return result != null ? ctx.CreateDocument(await ctx.GetContentProviderAsync(result)) : null;
            });

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared content and the current execution context.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="shortcode">
        /// A function that has the declared content and the current execution context as inputs
        /// and the result content as an output.
        /// </param>
        public void Add(string name, Func<string, IExecutionContext, string> shortcode) =>
            Add(name, async (_, content, __, ctx) =>
            {
                string result = shortcode?.Invoke(content, ctx);
                return result != null ? ctx.CreateDocument(await ctx.GetContentProviderAsync(result)) : null;
            });

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared content and the current document and execution context.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="shortcode">
        /// A function that has the declared content and the current document and execution context as inputs
        /// and the result content as an output.
        /// </param>
        public void Add(string name, Func<string, IDocument, IExecutionContext, string> shortcode) =>
            Add(name, async (_, content, doc, ctx) =>
            {
                string result = shortcode?.Invoke(content, doc, ctx);
                return result != null ? ctx.CreateDocument(await ctx.GetContentProviderAsync(result)) : null;
            });

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared arguments and content and the current document and execution context.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="shortcode">
        /// A function that has the declared arguments and content and the current document and execution context as inputs
        /// and a <see cref="IDocument"/> as an output which allows the shortcode to add metadata to the document.
        /// </param>
        public void Add(string name, Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, Task<IDocument>> shortcode) =>
            Add(name, () => new FuncShortcode(shortcode));
    }
}
