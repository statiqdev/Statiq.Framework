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

        public void ExpireChangeTokens()
        {
            foreach (RazorCompiler compiler in _compilers.Values)
            {
                compiler.ExpireChangeTokens();
            }
        }
    }
}