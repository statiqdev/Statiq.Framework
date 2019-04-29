using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Util;

namespace Wyam.Core.Execution
{
    internal class PipelineCollection : Dictionary<string, IPipeline>, IPipelineCollection
    {
        private IPipeline _previousPipeline;

        public IPipeline Add(string name)
        {
            IPipeline pipeline = new Pipeline();
            Add(name, pipeline);
            return pipeline;
        }

        // This has to be defined in PipelineCollection so that it can track the previous sequential pipeline added
        public IPipeline AddSequential(string name, IEnumerable<IModule> processModules)
        {
            IPipeline pipeline = new Pipeline();
            Add(name, pipeline);
            pipeline.Process.AddRange(processModules);
            if (_previousPipeline != null)
            {
                pipeline.Dependencies.Add(_previousPipeline);
            }
            _previousPipeline = pipeline;
            return pipeline;
        }
    }
}
