using ConcurrentCollections;
using Statiq.Common.Modules;

namespace Statiq.Core.Modules
{
    public static class ModuleExtensions
    {
        internal static ConcurrentHashSet<IModule> AsNewDocumentModules { get; } = new ConcurrentHashSet<IModule>();

        public static TModule AsNewDocuments<TModule>(this TModule module)
            where TModule : IModule, IAsNewDocuments
        {
            AsNewDocumentModules.Add(module);
            return module;
        }
    }
}
