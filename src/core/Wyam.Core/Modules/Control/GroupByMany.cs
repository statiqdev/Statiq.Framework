using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using Wyam.Core.Util;

namespace Wyam.Core.Modules.Control
{
    /// <summary>
    /// Splits a sequence of documents into groups based on a specified function or metadata key
    /// that returns or contains a sequence of group keys.
    /// </summary>
    /// <remarks>
    /// This module forms groups from the output documents of the specified modules.
    /// If the function or metadata key returns or contains an enumerable, each item in the enumerable
    /// will become one of the grouping keys. If a document contains multiple group keys, it will
    /// be included in multiple groups. A good example is a tag engine where each document can contain
    /// any number of tags and you want to make groups for each tag including all the documents with that tag.
    /// Each input document is cloned for each group and metadata related
    /// to the groups, including the sequence of documents for each group,
    /// is added to each clone. For example, if you have 2 input documents
    /// and the result of grouping is 3 groups, this module will output 6 documents.
    /// </remarks>
    /// <metadata cref="Keys.GroupDocuments" usage="Output" />
    /// <metadata cref="Keys.GroupKey" usage="Output" />
    /// <category>Control</category>
    public class GroupByMany : ContainerModule
    {
        private readonly DocumentConfig<IEnumerable<object>> _key;
        private DocumentConfig<bool> _predicate;
        private IEqualityComparer<object> _comparer;
        private bool _emptyOutputIfNoGroups;

        /// <summary>
        /// Partitions the result of the specified modules into groups with matching keys
        /// based on the key delegate.
        /// The input documents to GroupBy are used as
        /// the initial input documents to the specified modules.
        /// </summary>
        /// <param name="key">A delegate that returns the group keys.</param>
        /// <param name="modules">Modules to execute on the input documents prior to grouping.</param>
        public GroupByMany(DocumentConfig<IEnumerable<object>> key, params IModule[] modules)
            : this(key, (IEnumerable<IModule>)modules)
        {
        }

        /// <summary>
        /// Partitions the result of the specified modules into groups with matching keys
        /// based on the key delegate.
        /// The input documents to GroupBy are used as
        /// the initial input documents to the specified modules.
        /// </summary>
        /// <param name="key">A delegate that returns the group keys.</param>
        /// <param name="modules">Modules to execute on the input documents prior to grouping.</param>
        public GroupByMany(DocumentConfig<IEnumerable<object>> key, IEnumerable<IModule> modules)
            : base(modules)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
        }

        /// <summary>
        /// Partitions the result of the specified modules into groups with matching keys
        /// based on the value at the specified metadata key.
        /// If a document to group does not contain the specified metadata key, it is not included in any output groups.
        /// The input documents to GroupBy are used as
        /// the initial input documents to the specified modules.
        /// </summary>
        /// <param name="keyMetadataKey">The key metadata key.</param>
        /// <param name="modules">Modules to execute on the input documents prior to grouping.</param>
        public GroupByMany(string keyMetadataKey, params IModule[] modules)
            : this(keyMetadataKey, (IEnumerable<IModule>)modules)
        {
        }

        /// <summary>
        /// Partitions the result of the specified modules into groups with matching keys
        /// based on the value at the specified metadata key.
        /// If a document to group does not contain the specified metadata key, it is not included in any output groups.
        /// The input documents to GroupBy are used as
        /// the initial input documents to the specified modules.
        /// </summary>
        /// <param name="keyMetadataKey">The key metadata key.</param>
        /// <param name="modules">Modules to execute on the input documents prior to grouping.</param>
        public GroupByMany(string keyMetadataKey, IEnumerable<IModule> modules)
            : base(modules)
        {
            if (keyMetadataKey == null)
            {
                throw new ArgumentNullException(nameof(keyMetadataKey));
            }

            _key = Config.FromDocument(doc => doc.Get<IEnumerable<object>>(keyMetadataKey));
            _predicate = Config.FromDocument(doc => doc.ContainsKey(keyMetadataKey));
        }

        /// <summary>
        /// Limits the documents to be grouped to those that satisfy the supplied predicate.
        /// </summary>
        /// <param name="predicate">A delegate that should return a <c>bool</c>.</param>
        /// <returns>The current module instance.</returns>
        public GroupByMany Where(DocumentConfig<bool> predicate)
        {
            _predicate = _predicate.CombineWith(predicate);
            return this;
        }

        /// <summary>
        /// Specifies an equality comparer to use for the grouping.
        /// </summary>
        /// <param name="comparer">The equality comparer to use.</param>
        /// <returns>The current module instance.</returns>
        public GroupByMany WithComparer(IEqualityComparer<object> comparer)
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
        public GroupByMany WithComparer<TValue>(IEqualityComparer<TValue> comparer)
        {
            _comparer = comparer == null ? null : new ConvertingEqualityComparer<TValue>(comparer);
            return this;
        }

        /// <summary>
        /// Specifies that no documents should be output if there are no groups. This is in contrast to the
        /// default behavior of outputting the unmodified input documents if no groups were found.
        /// </summary>
        /// <param name="emptyOutput"><c>true</c> to not output documents when no groups are found.</param>
        /// <returns>The current module instance.</returns>
        public GroupByMany WithEmptyOutputIfNoGroups(bool emptyOutput = true)
        {
            _emptyOutputIfNoGroups = emptyOutput;
            return this;
        }

        /// <inheritdoc />
        public override async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            IEnumerable<IDocument> docs = await (await context.ExecuteAsync(this, inputs)).FilterAsync(_predicate, context);
            IEnumerable<(IDocument x, IEnumerable<object>)> keys = await docs.SelectAsync(async x => (x, await _key.GetValueAsync(x, context)));
            ImmutableArray<IGrouping<object, IDocument>> groupings = keys
                .GroupByMany(x => x.Item2, x => x.Item1, _comparer)
                .ToImmutableArray();
            if (groupings.Length == 0)
            {
                return _emptyOutputIfNoGroups ? Array.Empty<IDocument>() : inputs;
            }

            if (inputs.Count > 0)
            {
                return await inputs.SelectManyAsync(context, async input =>
                {
                    return await groupings.SelectAsync(async x => await context.NewGetDocumentAsync(
                        input,
                        metadata: new MetadataItems
                        {
                            { Keys.GroupDocuments, x.ToImmutableArray() },
                            { Keys.GroupKey, x.Key }
                        }));
                });
            }
            return await groupings.SelectAsync(async x => await context.NewGetDocumentAsync(
                metadata: new MetadataItems
                {
                    { Keys.GroupDocuments, x.ToImmutableArray() },
                    { Keys.GroupKey, x.Key }
                }));
        }
    }
}
