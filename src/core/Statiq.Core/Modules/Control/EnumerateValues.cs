using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Clones a new document for each item in a given metadata value.
    /// </summary>
    /// <remarks>
    /// Enumerates values returned by a config and clones a document for each one.
    /// The current value for each cloned document is contained in <see cref="Keys.Current"/>.
    /// </remarks>
    /// <category>Control</category>
    public class EnumerateValues : ParallelSyncConfigModule<IEnumerable<object>>
    {
        private string _currentKey = Keys.Current;

        /// <summary>
        /// Enumerates the values returned by the config and clones a document for each one.
        /// If the config returns null for a given document, the original input document will
        /// be output. If the config returns an empty enumerable for a given document, no result
        /// documents will be output for that input document.
        /// </summary>
        /// <param name="values">A delegate that returns the values to enumerate.</param>
        public EnumerateValues(Config<IEnumerable<object>> values)
            : base(values, false)
        {
        }

        /// <summary>
        /// Enumerates the values stored in the given metadata key.
        /// </summary>
        /// <param name="key">The metadata key that contains the values to enumerate.</param>
        public EnumerateValues(string key = Keys.Enumerate)
            : this(Config.FromDocument(doc => doc.GetList<object>(key)))
        {
        }

        /// <summary>
        /// Sets the metadata key to use for storing the current enumerated value in cloned documents.
        /// This is <see cref="Keys.Current"/> by default.
        /// </summary>
        /// <param name="currentKey">The metadata key to set for the current value.</param>
        /// <returns>The current module instance.</returns>
        public EnumerateValues WithCurrentKey(string currentKey)
        {
            _currentKey = currentKey ?? throw new ArgumentNullException(nameof(currentKey));
            return this;
        }

        protected override IEnumerable<IDocument> ExecuteConfig(IDocument input, IExecutionContext context, IEnumerable<object> value)
        {
            // Return the input document(s) for empty values
            if (value == null)
            {
                if (input == null && !context.Inputs.IsEmpty)
                {
                    return context.Inputs;
                }
                return input.Yield();
            }

            // If the config value didn't require a document but we have documents, clone each one with the enumeration results
            if (input == null && !context.Inputs.IsEmpty)
            {
                return context.Inputs.SelectMany(i =>
                    value.Select(x => i.Clone(new MetadataItems
                    {
                        { _currentKey, x }
                    })));
            }
            return value.Select(x => context.CloneOrCreateDocument(input, new MetadataItems
            {
                { _currentKey, x }
            }));
        }
    }
}
