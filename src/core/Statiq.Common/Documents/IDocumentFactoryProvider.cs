using System;
using System.Collections.Generic;
using System.Text;

namespace Statiq.Common.Documents
{
    public interface IDocumentFactoryProvider
    {
        DocumentFactory DocumentFactory { get; }
    }
}
