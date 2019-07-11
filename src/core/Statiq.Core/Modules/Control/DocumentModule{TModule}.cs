using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common.Configuration;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.Modules;

namespace Statiq.Core.Modules.Control
{
    public abstract class DocumentModule<TModule> : ContainerModule
        where TModule : DocumentModule<TModule>
    {
        private readonly DocumentConfig<IEnumerable<IDocument>> _documents;
        private bool _withInputs;

        /// <summary>
        /// Executes the specified modules.
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        protected DocumentModule(params IModule[] modules)
            : this((IEnumerable<IModule>)modules)
        {
        }

        /// <summary>
        /// Executes the specified modules.
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        protected DocumentModule(IEnumerable<IModule> modules)
            : base(modules)
        {
        }

        /// <summary>
        /// This outputs the documents from the specified pipeline(s).
        /// </summary>
        /// <param name="pipelines">The pipeline(s) to output documents from.</param>
        protected DocumentModule(params string[] pipelines)
            : this((IEnumerable<string>)pipelines)
        {
        }

        /// <summary>
        /// This outputs the documents from the specified pipeline(s).
        /// </summary>
        /// <param name="pipelines">The pipeline(s) to output documents from.</param>
        protected DocumentModule(IEnumerable<string> pipelines)
            : base(null)
        {
            _ = pipelines ?? throw new ArgumentNullException(nameof(pipelines));
            _documents = Config.FromContext(ctx => pipelines.SelectMany(x => ctx.Documents[x]));
        }

        /// <summary>
        /// This will get documents based on each input document. The output will be the
        /// aggregate of all returned documents for each input document. The return value
        /// is expected to be a <c>IEnumerable&lt;IDocument&gt;</c>.
        /// </summary>
        /// <param name="documents">A delegate that should return
        /// a <c>IEnumerable&lt;IDocument&gt;</c> containing the documents to
        /// output for each input document.</param>
        protected DocumentModule(DocumentConfig<IEnumerable<IDocument>> documents)
            : base(null)
        {
            _documents = documents ?? throw new ArgumentNullException(nameof(documents));
        }

        /// <summary>
        /// Controls whether the input documents are passed to the child modules.
        /// If child modules were not specified this has no effect.
        /// </summary>
        /// <param name="withInputDocuments"><c>true</c> to pass input documents to the child modules, <c>false</c> otherwise.</param>
        /// <returns>The current module instance.</returns>
        public TModule WithInputDocuments(bool withInputDocuments = true)
        {
            _withInputs = withInputDocuments;
            return (TModule)this;
        }

        public override async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            // Get documents from a delegate if requested
            if (_documents != null)
            {
                IEnumerable<IDocument> results = _documents.RequiresDocument
                    ? await inputs.SelectManyAsync(context, x => _documents.GetValueAsync(x, context))
                    : await _documents.GetValueAsync(null, context);
                return GetOutputDocuments(inputs, results);
            }

            // Otherwise get the documents from child modules
            if (Children.Count > 0)
            {
                IEnumerable<IDocument> results = await context.ExecuteAsync(Children, _withInputs ? inputs : null);
                return GetOutputDocuments(inputs, results);
            }

            return Array.Empty<IDocument>();
        }

        protected abstract IEnumerable<IDocument> GetOutputDocuments(IEnumerable<IDocument> inputs, IEnumerable<IDocument> results);
    }
}
