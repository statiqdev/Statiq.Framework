using System.Collections;
using System.Collections.Generic;
using Wyam.Common;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

namespace Wyam.Core.Execution
{
    internal class ReadOnlyPipeline : IReadOnlyPipeline
    {
        private readonly IPipeline _pipeline;

        public ReadOnlyPipeline(IPipeline pipeline)
        {
            _pipeline = pipeline;
        }

        public string Name => _pipeline.Name;

        public bool ProcessDocumentsOnce => _pipeline.ProcessDocumentsOnce;

        IEnumerator<IModule> IEnumerable<IModule>.GetEnumerator() => _pipeline.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<IModule> GetEnumerator() => _pipeline.GetEnumerator();

        public int Count => _pipeline.Count;

        public IModule this[int index] => _pipeline[index];
    }
}