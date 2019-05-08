using Wyam.Common.Configuration;
using Wyam.Common.Modules;

namespace Wyam.Core.Modules.Control
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
