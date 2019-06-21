using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Statiq.Common.Configuration;
using Statiq.Common.Content;
using Statiq.Common.Execution;
using Statiq.Common.IO;
using Statiq.Common.Meta;
using Statiq.Common.Tracing;
using Statiq.Common.Util;

namespace Statiq.Common.Documents
{
    public abstract class FactoryDocument
    {
        internal abstract IDocument Initialize(
            IMetadata defaultMetadata,
            FilePath source,
            FilePath destination,
            IMetadata metadata,
            IContentProvider contentProvider);
    }
}
