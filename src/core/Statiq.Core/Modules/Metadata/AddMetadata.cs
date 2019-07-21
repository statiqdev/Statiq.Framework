using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Adds the specified metadata to each input document.
    /// </summary>
    /// <category>Metadata</category>
    public class AddMetadata : IModule
    {
        private readonly string _key;
        private readonly Config<object> _value;
        private readonly IModule[] _modules;
        private bool _ignoreNull;
        private bool _onlyIfNonExisting;

        /// <summary>
        /// Uses a delegate (or value) to determine an object to be added as metadata for each document.
        /// This allows you to specify different metadata for each document depending on the input.
        /// </summary>
        /// <param name="key">The metadata key to set.</param>
        /// <param name="value">A delegate that returns the object to add as metadata.</param>
        public AddMetadata(string key, Config<object> value)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// The specified modules are executed against an empty initial document and all metadata that exist in all of the result documents
        /// are added as metadata to each input document.
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        public AddMetadata(params IModule[] modules)
        {
            _modules = modules;
        }

        /// <summary>
        /// Ignores null values and does not add a metadata item for them.
        /// </summary>
        /// <param name="ignoreNull"><c>true</c> to ignore null values.</param>
        /// <returns>The current module instance.</returns>
        public AddMetadata IgnoreNull(bool ignoreNull = true)
        {
            _ignoreNull = ignoreNull;
            return this;
        }

        /// <summary>
        /// Only sets the new metadata value if a value doesn't already exist.
        /// The default behavior is to set the new value regardless.
        /// </summary>
        /// <param name="onlyIfNonExisting"><c>true</c> if the new value should only be set if it doesn't already exist.</param>
        /// <returns>The current module instance.</returns>
        public AddMetadata OnlyIfNonExisting(bool onlyIfNonExisting = true)
        {
            _onlyIfNonExisting = onlyIfNonExisting;
            return this;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            if (_modules != null)
            {
                Dictionary<string, object> metadata = new Dictionary<string, object>();

                // Execute the modules once and apply to each input document
                foreach (IDocument result in await context.ExecuteAsync(_modules))
                {
                    foreach (KeyValuePair<string, object> kvp in result)
                    {
                        if (kvp.Value != null || !_ignoreNull)
                        {
                            metadata[kvp.Key] = kvp.Value;
                        }
                    }
                }
                return inputs.Select(context, input => input.Clone(_onlyIfNonExisting ? metadata.Where(x => !input.ContainsKey(x.Key)) : metadata));
            }

            return await inputs.SelectAsync(context, async doc => _onlyIfNonExisting && doc.ContainsKey(_key)
                ? doc
                : doc.Clone(
                    new[]
                    {
                        new KeyValuePair<string, object>(_key, await _value.GetValueAsync(doc, context))
                    }));
        }
    }
}
