using Statiq.Common.Configuration;
using Statiq.Common.Modules;

namespace Statiq.Core.Modules.Control
{
    public class IfCondition : ModuleList
    {
        public DocumentConfig<bool> Predicate { get; set; }

        internal IfCondition(DocumentConfig<bool> predicate, IModule[] modules)
            : base(modules)
        {
            Predicate = predicate;
        }

        internal IfCondition(IModule[] modules)
            : base(modules)
        {
        }
    }
}
