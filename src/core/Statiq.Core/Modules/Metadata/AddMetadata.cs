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
    public class AddMetadata : ParallelConfigModule<object>
    {
        private readonly string _key;
        private bool _ignoreNull;
        private bool _onlyIfNonExisting;

        /// <summary>
        /// Uses a delegate (or value) to determine an object to be added as metadata for each document.
        /// This allows you to specify different metadata for each document depending on the input.
        /// </summary>
        /// <param name="key">The metadata key to set.</param>
        /// <param name="value">A delegate that returns the object to add as metadata.</param>
        public AddMetadata(string key, Config<object> value)
            : base(value, true)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
        }

        /// <summary>
        /// Ignores null values and does not add a metadata item for them.
        /// </summary>
        /// <remarks>
        /// The default behavior is not to ignore null values and add them to the metadata regardless.
        /// </remarks>
        /// <param name="ignoreNull"><c>true</c> to ignore null values.</param>
        /// <returns>The current module instance.</returns>
        public AddMetadata IgnoreNull(bool ignoreNull = true)
        {
            _ignoreNull = ignoreNull;
            return this;
        }

        /// <summary>
        /// Only sets the new metadata value if a value doesn't already exist.
        /// </summary>
        /// <remarks>
        /// The default behavior is to set the new value regardless.
        /// </remarks>
        /// <param name="onlyIfNonExisting"><c>true</c> if the new value should only be set if it doesn't already exist.</param>
        /// <returns>The current module instance.</returns>
        public AddMetadata OnlyIfNonExisting(bool onlyIfNonExisting = true)
        {
            _onlyIfNonExisting = onlyIfNonExisting;
            return this;
        }

        protected override Task<IEnumerable<IDocument>> ExecuteAsync(IDocument input, IExecutionContext context, object value) =>
            Task.FromResult(
                (_onlyIfNonExisting && input.ContainsKey(_key)) || (_ignoreNull && value == null)
                    ? input.Yield()
                    : input.Clone(new MetadataItems { { _key, value } }).Yield());
    }
}
