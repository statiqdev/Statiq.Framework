using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Statiq.Razor
{
    /// <summary>
    /// Razor compiler should be shared so that pages are only compiled once.
    /// </summary>
    internal class RazorService
    {
        private readonly ConcurrentDictionary<CompilationParameters, RazorCompiler> _compilers
            = new ConcurrentDictionary<CompilationParameters, RazorCompiler>();

        private Guid _executionId = Guid.Empty;

        public async Task RenderAsync(RenderRequest request)
        {
            CompilationParameters parameters = new CompilationParameters
            {
                BasePageType = request.BaseType,
                Namespaces = new NamespaceCollection(request.Context.Namespaces)
            };

            RazorCompiler compiler = _compilers.GetOrAdd(parameters, _ => new RazorCompiler(parameters, request.Context));
            await compiler.RenderPageAsync(request);
        }

        /// <summary>
        /// Expires the change tokens if the execution ID is different than the last one
        /// </summary>
        public void ExpireChangeTokensOnNewExecution(Guid executionId)
        {
            if (_executionId != Guid.Empty && _executionId != executionId)
            {
                foreach (RazorCompiler compiler in _compilers.Values)
                {
                    compiler.ExpireChangeTokens();
                }
            }
            _executionId = executionId;
        }
    }
}