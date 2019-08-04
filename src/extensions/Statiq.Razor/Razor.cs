using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;
using Trace = Statiq.Common.Trace;

namespace Statiq.Razor
{
    /// <summary>
    /// Parses, compiles, and renders Razor templates.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Razor is the template language used by ASP.NET MVC. This module can parse and compile Razor
    /// templates and then render them to HTML. While a bit
    /// outdated, <a href="http://haacked.com/archive/2011/01/06/razor-syntax-quick-reference.aspx/">this guide</a>
    /// is a good quick reference for the Razor language syntax. This module uses the Razor engine from ASP.NET Core.
    /// </para>
    /// <para>
    /// Whenever possible, the same conventions as the Razor engine in ASP.NET MVC were used. It's
    /// important to keep in mind however, that this is <em>not</em> ASP.NET MVC. Many features you may
    /// be used to will not work (like most of the <c>HtmlHelper</c> extensions) and others just don't
    /// make sense (like the concept of <em>actions</em> and <em>controllers</em>). Also, while property names and
    /// classes in the two engines have similar names(such as <c>HtmlHelper</c>) they are not the same,
    /// and code intended to extend the capabilities of Razor in ASP.NET MVC probably won't work.
    /// That said, a lot of functionality does function the same as it does in ASP.NET MVC.
    /// </para>
    /// </remarks>
    /// Used to determine if the source file name contains the ignore prefix.
    /// <category>Templates</category>
    public class Razor : IModule
    {
        private static readonly RazorService RazorService = new RazorService();
        private static Guid _executionId = Guid.Empty;

        private readonly Type _basePageType;
        private Config<FilePath> _viewStartPath;
        private Config<FilePath> _layoutPath;
        private Config<object> _model;
        private string _ignorePrefix = "_";

        /// <summary>
        /// Parses Razor templates in each input document and outputs documents with rendered HTML content.
        /// If <c>basePageType</c> is specified, it will be used as the base type for Razor pages. The new base
        /// type must derive from <c>StatiqRazorPage&lt;TModel&gt;</c>.
        /// </summary>
        /// <param name="basePageType">Type of the base Razor page class, or <c>null</c> for the default base class.</param>
        public Razor(Type basePageType = null)
        {
            if (basePageType != null && !IsSubclassOfRawGeneric(typeof(StatiqRazorPage<>), basePageType))
            {
                throw new ArgumentException($"The Razor base page type must derive from {nameof(StatiqRazorPage<object>)}.");
            }
            _basePageType = basePageType;
        }

        // From http://stackoverflow.com/a/457708/807064
        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                Type current = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == current)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        /// <summary>
        /// Specifies an alternate ViewStart file to use for all Razor pages processed by this module. This
        /// lets you specify a different ViewStart file for each document. For example, you could return a
        /// ViewStart based on document location or document metadata. Returning <c>null</c> from the
        /// function reverts back to the default ViewStart search behavior for that document.
        /// </summary>
        /// <param name="path">A delegate that should return the ViewStart path as a <c>FilePath</c>,
        /// or <c>null</c> for the default ViewStart search behavior.</param>
        /// <returns>The current module instance.</returns>
        public Razor WithViewStart(Config<FilePath> path)
        {
            _viewStartPath = path;
            return this;
        }

        /// <summary>
        /// Specifies a layout file to use for all Razor pages processed by this module. This
        /// lets you specify a different layout file for each document.
        /// </summary>
        /// <param name="path">A delegate that should return the layout path as a <c>FilePath</c>.</param>
        /// <returns>The current module instance.</returns>
        public Razor WithLayout(Config<FilePath> path)
        {
            _layoutPath = path;
            return this;
        }

        /// <summary>
        /// Specifies a model to use for each page based on the current input
        /// document and context.
        /// </summary>
        /// <param name="model">A delegate that returns the model.</param>
        /// <returns>The current module instance.</returns>
        public Razor WithModel(Config<object> model)
        {
            _model = model;
            return this;
        }

        /// <summary>
        /// Specifies a file prefix to ignore. If a document has a metadata value for <c>SourceFileName</c> and
        /// that metadata value starts with the specified prefix, that document will not be processed or
        /// output by the module. By default, the Razor module ignores all documents prefixed with
        /// an underscore (_). Specifying <c>null</c> will result in no documents being ignored.
        /// </summary>
        /// <param name="prefix">The file prefix to ignore.</param>
        /// <returns>The current module instance.</returns>
        public Razor IgnorePrefix(string prefix)
        {
            _ignorePrefix = prefix;
            return this;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDocument>> ExecuteAsync(IExecutionContext context)
        {
            // Expire the internal Razor cache if this is a new execution
            // This needs to be done so that layouts/partials can be re-rendered if they've changed,
            // otherwise Razor will just use the previously cached version of them
            if (_executionId != Guid.Empty && _executionId != context.ExecutionId)
            {
                RazorService.ExpireChangeTokens();
            }
            _executionId = context.ExecutionId;

            // Eliminate input documents that we shouldn't process
            ImmutableArray<IDocument> validInputs = context.QueryInputs()
                .Where(x => _ignorePrefix == null || x.Source?.FileName.FullPath.StartsWith(_ignorePrefix) != true)
                .ToImmutableArray();

            if (validInputs.Length < context.Inputs.Length)
            {
                Trace.Information($"Ignoring {context.Inputs.Length - validInputs.Length} inputs due to source file name prefix");
            }

            // Compile and evaluate the pages in parallel
            return await validInputs.AsParallel().SelectAsync(RenderDocumentAsync);

            async Task<IDocument> RenderDocumentAsync(IDocument input)
            {
                Trace.Verbose("Processing Razor for {0}", input.ToSafeDisplayString());

                using (Stream contentStream = await context.GetContentStreamAsync())
                {
                    using (Stream inputStream = await input.GetStreamAsync())
                    {
                        FilePath viewStartLocationPath = _viewStartPath == null ? null : await _viewStartPath.GetValueAsync(input, context);
                        string layoutPath = _layoutPath == null ? null : (await _layoutPath.GetValueAsync(input, context))?.FullPath;

                        RenderRequest request = new RenderRequest
                        {
                            Input = inputStream,
                            Output = contentStream,
                            BaseType = _basePageType,
                            Context = context,
                            Document = input,
                            LayoutLocation = layoutPath,
                            ViewStartLocation = viewStartLocationPath != null ? await GetRelativePathAsync(viewStartLocationPath, context) : null,
                            RelativePath = await GetRelativePathAsync(input, context),
                            Model = _model == null ? input : await _model.GetValueAsync(input, context)
                        };

                        await RazorService.RenderAsync(request);
                    }

                    return input.Clone(context.GetContentProvider(contentStream));
                }
            }
        }

        private async Task<string> GetRelativePathAsync(IDocument document, IExecutionContext context)
        {
            // Use the pre-calculated relative file path if available
            FilePath relativePath = document.Source?.GetRelativeInputPath(context);
            return relativePath != null ? $"/{relativePath.FullPath}" : await GetRelativePathAsync(document.Source, context);
        }

        private async Task<string> GetRelativePathAsync(FilePath path, IExecutionContext context)
        {
            // Calculate a relative path from the input path(s) (or root) to the provided path
            if (path != null)
            {
                DirectoryPath inputPath = await context.FileSystem.GetContainingInputPathAsync(path) ?? new DirectoryPath("/");
                if (path.IsRelative)
                {
                    // If the path is relative, combine it with the input path to make it absolute
                    path = inputPath.CombineFile(path);
                }
                return $"/{inputPath.GetRelativePath(path).FullPath}";
            }

            // If there's no path, give this document a placeholder name
            return $"/{Path.GetRandomFileName()}.cshtml";
        }
    }
}
