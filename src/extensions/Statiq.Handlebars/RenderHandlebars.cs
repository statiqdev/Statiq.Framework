using System;
using System.Collections;
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
        private readonly IDictionary<string, Config<string>> _partials;
        private readonly IDictionary<string, Config<HandlebarsHelper>> _helpers;
        private readonly IDictionary<string, Config<HandlebarsBlockHelper>> _blockHelpers;

        private Config<object> _model;
        private Func<IExecutionContext, IDocument, HandlebarsConfiguration, Task<HandlebarsConfiguration>> _configure;

        public RenderHandlebars()
        {
            _partials = new Dictionary<string, Config<string>>(StringComparer.Ordinal);
            _helpers = new Dictionary<string, Config<HandlebarsHelper>>(StringComparer.Ordinal);
            _blockHelpers = new Dictionary<string, Config<HandlebarsBlockHelper>>(StringComparer.Ordinal);
        }

        public RenderHandlebars(string sourceKey, string destinationKey = null)
            : this()
        {
            _sourceKey = sourceKey;
            _destinationKey = destinationKey;
        }

        public RenderHandlebars WithModel(Config<object> model)
        {
            _model = model;
            return this;
        }

        public RenderHandlebars WithPartial(string name, Config<string> partial)
        {
            _partials[name] = partial;
            return this;
        }

        public RenderHandlebars WithHelper(string name, Config<HandlebarsHelper> helper)
        {
            _helpers[name] = helper;
            return this;
        }

        public RenderHandlebars WithBlockHelper(string name, Config<HandlebarsBlockHelper> blockHelper)
        {
            _blockHelpers[name] = blockHelper;
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
            foreach ((string name, Config<string> partial) in _partials)
            {
                handlebars.RegisterTemplate(name, await partial.GetValueAsync(input, context));
            }

            // Register helpers
            foreach ((string name, Config<HandlebarsHelper> helper) in _helpers)
            {
                handlebars.RegisterHelper(name, await helper.GetValueAsync(input, context));
            }

            // Register block helpers
            foreach ((string name, Config<HandlebarsBlockHelper> blockHelper) in _blockHelpers)
            {
                handlebars.RegisterHelper(name, await blockHelper.GetValueAsync(input, context));
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