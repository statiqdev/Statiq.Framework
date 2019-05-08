using ConcurrentCollections;
using Wyam.Common.Modules;

namespace Wyam.Core.Modules
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
