using System.Collections.Generic;

namespace Statiq.App
{
    internal class EngineCommandData
    {
        public EngineCommandData(string[] pipelines, bool defaultPipelines, IEnumerable<KeyValuePair<string, string>> settings)
        {
            Pipelines = pipelines;
            DefaultPipelines = defaultPipelines;
            Settings = settings;
        }

        public string[] Pipelines { get; }

        public bool DefaultPipelines { get; }

        public IEnumerable<KeyValuePair<string, string>> Settings { get; }
    }
}
