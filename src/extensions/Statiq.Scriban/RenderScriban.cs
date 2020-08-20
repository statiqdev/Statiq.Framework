using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Scriban;
using Scriban.Runtime;
using Statiq.Common;

namespace Statiq.Scriban
{
    /// <summary>
    /// Parses, compiles, and renders Scriban and Liquid templates.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Scriban is a fast, powerful, safe and lightweight text templating language and engine for .NET, with a compatibility mode for parsing Liquid templates.
    /// See <a href="https://github.com/lunet-io/scriban/blob/master/doc/language.md">this guide</a> for an introduction to Scriban.
    /// See <a href="https://shopify.github.io/liquid/">this guide</a> for an introduction to Liquid.
    /// </para>
    /// <para>
    /// This module user <a href="https://github.com/lunet-io/scriban">Scriban</a> to render Scriban and liquid templates.
    /// </para>
    /// </remarks>
    /// <category>Templates</category>
    public class RenderScriban : ParallelModule
    {
        private readonly string _sourceKey;
        private readonly string _destinationKey;

        private Config<object> _model;
        private MemberRenamerDelegate _renamer;

        /// <summary>
        /// Parses Scriban templates in each input document and outputs documents with rendered content.
        /// </summary>
        public RenderScriban()
        {
        }

        /// <summary>
        /// Parses Scriban templates in the metadata of each input document and outputs documents with metadata containing the rendered content.
        /// </summary>
        /// /// <param name="sourceKey">The metadata key of the Scriban template to process.</param>
        /// <param name="destinationKey">The metadata key to store the rendered content (if null, it gets placed back in the source metadata key).</param>
        public RenderScriban(string sourceKey, string destinationKey = null)
            : this()
        {
            _sourceKey = sourceKey;
            _destinationKey = destinationKey;
        }

        /// <summary>
        /// Specifies a model to use for each page based on the current input
        /// document and context.
        /// </summary>
        /// <param name="model">A delegate that returns the model.</param>
        /// <returns>The current module instance.</returns>
        public RenderScriban WithModel(Config<object> model)
        {
            _model = model;
            return this;
        }

        public RenderScriban WithMemberRenamer(MemberRenamerDelegate renamer)
        {
            _renamer = renamer;
            return this;
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            context.LogDebug(
                   "Processing Scriban {0} for {1}",
                   string.IsNullOrEmpty(_sourceKey) ? string.Empty : ("in" + _sourceKey),
                   input.ToSafeDisplayString());

            string content;
            if (string.IsNullOrEmpty(_sourceKey))
            {
                content = await input.GetContentStringAsync();
            }
            else if (input.ContainsKey(_sourceKey))
            {
                content = input.GetString(_sourceKey) ?? string.Empty;
            }
            else
            {
                // Don't do anything if the key doesn't exist
                return input.Yield();
            }

            _renamer ??= StandardMemberRenamer.Default;

            // TODO: Support Liquid
            // TODO: Expose ParserOptions and LexerOptions
            Template template = Template.Parse(content, input.Source.FullPath);

            IScriptObject scriptObject;

            if (_model is null)
            {
                scriptObject = new ScriptObject(input, _renamer);
            }
            else
            {
                object model = await _model.GetValueAsync(input, context);
                scriptObject = new global::Scriban.Runtime.ScriptObject();

                if (model != null)
                {
                    scriptObject.Import(model, filter: null, renamer: _renamer);
                }
            }

            // TODO: Expose member filter
            TemplateContext templateContext = new StatiqTemplateContext
            {
                TemplateLoader = new TemplateLoader(context.FileSystem),
                MemberRenamer = _renamer
            };
            templateContext.PushGlobal(scriptObject);

            string result = await template.RenderAsync(templateContext);

            return string.IsNullOrEmpty(_sourceKey)
                ? input.Clone(await context.GetContentProviderAsync(result, MediaTypes.Html)).Yield()
                : input
                    .Clone(new MetadataItems
                    {
                        { string.IsNullOrEmpty(_destinationKey) ? _sourceKey : _destinationKey, result }
                    })
                    .Yield();
        }
    }
}