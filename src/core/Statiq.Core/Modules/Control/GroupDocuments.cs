using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Splits a sequence of documents into groups based on a specified function or metadata key
    /// that returns or contains a sequence of group keys.
    /// </summary>
    /// <remarks>
    /// This module forms groups from the input documents.
    /// If the function or metadata key returns or contains an enumerable, each item in the enumerable
    /// will become one of the grouping keys. If a document contains multiple group keys, it will
    /// be included in multiple groups. A good example is a tag engine where each document can contain
    /// any number of tags and you want to make groups for each tag including all the documents with that tag.
    /// This module outputs a new document with the documents and key for each group.
    /// </remarks>
    /// <metadata cref="Keys.GroupDocuments" usage="Output" />
    /// <metadata cref="Keys.GroupKey" usage="Output" />
    /// <category>Control</category>
    public class GroupDocuments : IModule
    {
        private readonly DocumentConfig<IEnumerable<object>> _key;
        private IEqualityComparer<object> _comparer;

        /// <summary>
        /// Partitions the input documents into groups with matching keys
        /// based on the key delegate.
        /// </summary>
        /// <param name="groupKeys">A delegate that returns the group key(s).</param>
        public GroupDocuments(DocumentConfig<IEnumerable<object>> groupKeys)
        {
            _key = groupKeys ?? throw new ArgumentNullException(nameof(groupKeys));
        }

        /// <summary>
        /// Partitions the result of the input documents into groups with matching keys
        /// based on the value(s) at the specified metadata key.
        /// If a document to group does not contain the specified metadata key, it is not included in any output groups.
        /// </summary>
        /// <param name="metadataKey">The key metadata key.</param>
        public GroupDocuments(string metadataKey)
        {
            if (metadataKey == null)
            {
                throw new ArgumentNullException(nameof(metadataKey));
            }

            _key = Config.FromDocument(doc => doc.Get<IEnumerable<object>>(metadataKey));
        }

        /// <summary>
        /// Specifies an equality comparer to use for the grouping.
        /// </summary>
        /// <param name="comparer">The equality comparer to use.</param>
        /// <returns>The current module instance.</returns>
        public GroupDocuments WithComparer(IEqualityComparer<object> comparer)
        {
            _comparer = comparer;
            return this;
        }

        /// <summary>
        /// Specifies a typed equality comparer to use for the grouping. A conversion to the
        /// comparer type will be attempted for all metadata values. If the conversion fails,
        /// the value will not be considered equal. Note that this will also have the effect
        /// of treating different convertible types as being of the same type. For example,
        /// if you have two group keys, 1 and "1" (in that order), and use a string-based comparison, you will
        /// only end up with a single group for those documents with a group key of 1 (since the <c>int</c> key came first).
        /// </summary>
        /// <param name="comparer">The typed equality comparer to use.</param>
        /// <returns>The current module instance.</returns>
        public GroupDocuments WithComparer<TValue>(IEqualityComparer<TValue> comparer)
        {
            _comparer = comparer == null ? null : new ConvertingEqualityComparer<TValue>(comparer);
            return this;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context) =>
            (await inputs.SelectAsync(context, async x => (Document: x, Keys: await _key.GetValueAsync(x, context))))
                .GroupByMany(x => x.Keys, x => x.Document, _comparer)
                .Select(x => context.CreateDocument(
                    new MetadataItems
                    {
                        { Keys.GroupDocuments, x.ToImmutableArray() },
                        { Keys.GroupKey, x.Key }
                    }));
    }
}
