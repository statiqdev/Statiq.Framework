using System.Collections;
using System.Collections.Generic;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

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
