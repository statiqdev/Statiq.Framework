using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using HandlebarsDotNet;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Handlebars
{
    public class RenderHandlebars : ParallelModule
    {
        private readonly string _sourceKey;
        private readonly string _destinationKey;
        private Config<object> _model;
        private Config<IEnumerable<KeyValuePair<string, string>>> _partials;
        private Func<IExecutionContext, IDocument, HandlebarsConfiguration, Task<HandlebarsConfiguration>> _configure;

        public RenderHandlebars()
        {
        }

        public RenderHandlebars(string sourceKey, string destinationKey = null)
        {
            _sourceKey = sourceKey;
            _destinationKey = destinationKey;
        }

        public RenderHandlebars WithModel(Config<object> model)
        {
            _model = model;
            return this;
        }

        public RenderHandlebars WithPartials(Config<IEnumerable<KeyValuePair<string, string>>> partials)
        {
            _partials = partials;
            return this;
        }

        public RenderHandlebars Configure(
            Func<IExecutionContext, IDocument, HandlebarsConfiguration, HandlebarsConfiguration> configure)
        {
            return Configure((context, document, configuration) => Task.FromResult(configure(context, document, configuration)));
        }

        public RenderHandlebars Configure(
            Func<IExecutionContext, IDocument, HandlebarsConfiguration, Task<HandlebarsConfiguration>> configure)
        {
            _configure = configure;
            return this;
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            context.LogDebug(
                   "Processing Handlebars {0} for {1}",
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

            // Configure
            HandlebarsConfiguration configuration = new HandlebarsConfiguration();
            if (_configure != null)
            {
                configuration = await _configure(context, input, configuration);
            }

            IHandlebars handlebars = HandlebarsDotNet.Handlebars.Create(configuration);

            // Register partials
            if (_partials != null)
            {
                foreach (KeyValuePair<string, string> partial in await _partials.GetValueAsync(input, context))
                {
                    handlebars.RegisterTemplate(partial.Key, partial.Value);
                }
            }

            string result = handlebars.Compile(content)(_model is null
                ? new DocumentWrapper(input)
                : await _model.GetValueAsync(input, context));

            return string.IsNullOrEmpty(_sourceKey)
                ? input.Clone(await context.GetContentProviderAsync(result, MediaTypes.Html)).Yield()
                : input
                    .Clone(new MetadataItems
                    {
                        { string.IsNullOrEmpty(_destinationKey) ? _sourceKey : _destinationKey, result }
                    })
                    .Yield();
        }

        private class DocumentWrapper : DynamicObject
        {
            private readonly IDocument _document;

            public DocumentWrapper(IDocument document)
            {
                _document = document;
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                return _document.TryGetValue(binder.Name, out result);
            }
        }
    }
}