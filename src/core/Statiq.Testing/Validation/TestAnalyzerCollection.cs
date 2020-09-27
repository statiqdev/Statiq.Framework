using System;
using System.Collections.Generic;
using System.Text;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestAnalyzerCollection : IAnalyzerCollection
    {
        public List<IAnalyzer> Analyzers { get; } = new List<IAnalyzer>();

        public void Add(IAnalyzer analyzer) => Analyzers.Add(analyzer);
    }
}
