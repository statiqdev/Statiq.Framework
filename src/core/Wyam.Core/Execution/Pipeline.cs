using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using ConcurrentCollections;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Tracing;
using Wyam.Common.Util;
using Wyam.Core.Caching;
using Wyam.Core.Documents;
using Wyam.Core.Meta;

namespace Wyam.Core.Execution
{
    public class Pipeline : IPipeline
    {
        public IModuleList Read { get; } = new ModuleList();

        public IModuleList Process { get; } = new ModuleList();

        public IModuleList Render { get; } = new ModuleList();

        public IModuleList Write { get; } = new ModuleList();

        public HashSet<string> Dependencies { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public bool Isolated { get; set; }

        public bool AlwaysProcess { get; set; }
    }
}
