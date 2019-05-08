using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Wyam.Razor
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
                DynamicAssemblies = new DynamicAssemblyCollection(request.Context.DynamicAssemblies),
                Namespaces = new NamespaceCollection(request.Context.Namespaces),
                FileSystem = request.Context.FileSystem
            };

            RazorCompiler compiler = _compilers.GetOrAdd(parameters, _ => new RazorCompiler(parameters));
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