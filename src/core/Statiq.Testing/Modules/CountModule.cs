using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Testing.Modules
{
    public class CountModule : IModule
    {
        public int AdditionalOutputs { get; set; } // Controls how many additional outputs are spawned
        public string ValueKey { get; set; } // This is the key used for storing the value in the metadata
        public int Value { get; set; } // This is incremented on every call and output and added to the metadata
        public int ExecuteCount { get; set; }
        public int InputCount { get; set; }
        public int OutputCount { get; set; }
        public bool CloneSource { get; set; } // Indicates whether the clone call should output a source
        public bool EnsureInputDocument { get; set; }

        public CountModule(string valueKey)
        {
            ValueKey = valueKey;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            int sourceCount = 0;
            ExecuteCount++;
            List<IDocument> results = new List<IDocument>();

            // Add an initial document if there isn't already one
            if (inputs.Count == 0 && EnsureInputDocument)
            {
                inputs = new[] { context.CreateDocument() };
            }

            foreach (IDocument input in inputs)
            {
                string inputContent = await input.GetStringAsync();
                InputCount++;
                for (int c = 0; c < AdditionalOutputs + 1; c++)
                {
                    OutputCount++;
                    Value++;
                    if (CloneSource)
                    {
                        results.Add(
                            input.Clone(
                                new FilePath(ValueKey + sourceCount++, PathKind.Absolute),
                                null,
                                new Dictionary<string, object> { { ValueKey, Value } },
                                await context.GetContentProviderAsync(inputContent == null ? Value.ToString() : inputContent + Value)));
                    }
                    else
                    {
                        results.Add(
                            input.Clone(
                                new Dictionary<string, object> { { ValueKey, Value } },
                                await context.GetContentProviderAsync(inputContent == null ? Value.ToString() : inputContent + Value)));
                    }
                }
            }
            return results;
        }
    }
}
