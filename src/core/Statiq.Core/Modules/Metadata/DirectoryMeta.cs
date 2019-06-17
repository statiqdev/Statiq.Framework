using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;
using Statiq.Common.Configuration;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.IO;
using Statiq.Common.Meta;
using Statiq.Common.Modules;
using Statiq.Core.Modules.IO;

namespace Statiq.Core.Modules.Metadata
{
    /// <summary>
    /// Applies metadata from specified input documents to all input documents based on a directory hierarchy.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This module allows you to specify certain documents that contain common metadata for all other
    /// documents in the same directory (and optionally nested directories). It assumes that all input documents
    /// are generated from the file system (for example, from the <see cref="ReadFiles"/> module). In other words,
    /// both the documents that contain the common metadata and the documents to which the common metadata should
    /// be applied should be passed as inputs to this module.
    /// </para>
    /// <para>
    /// Documents that contain the common metadata are specified by file name using the <c>WithMetadataFile</c> method.
    /// You can specify more than one metadata file and/or metadata files at different levels in the directory
    /// hierarchy. If the same metadata key exists across multiple common metadata documents, the following can be
    /// used to determine which metadata value will get set in the target output documents:
    /// <list type="bullet">
    /// <item><description>
    /// Pre-existing metadata in the target document (common metadata will
    /// not overwrite existing metadata unless the <c>replace</c> flag is set).
    /// </description></item>
    /// <item><description>
    /// Common metadata documents in the same directory as the target document
    /// (those registered first have a higher priority).
    /// </description></item>
    /// <item><description>
    /// Common metadata documents in parent directories of the target document (but only if the <c>inherited</c> flag
    /// is set and those closer to the target document have a higher priority).
    /// </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// By default, documents that are identified as containing common metadata will be filtered and won't be
    /// contained in the sequence of output documents. <c>PreserveMetadataFiles</c> can be used to change this behavior.
    /// </para>
    /// </remarks>
    /// <category>Metadata</category>
    public class DirectoryMeta : IModule
    {
        private readonly List<MetaFileEntry> _metadataFiles = new List<MetaFileEntry>();
        private bool _preserveMetadataFiles;

        /// <summary>
        /// Preserves the files that hold the common metadata and ensures they are included in the module output. Without this option, theses documents will
        /// be consumed by this module and will not be present in the module output.
        /// </summary>
        /// <returns>The current module instance.</returns>
        public DirectoryMeta WithPreserveMetadataFiles()
        {
            _preserveMetadataFiles = true;
            return this;
        }

        /// <summary>
        /// Specifies a file name to use as common metadata using a delegate so that the common metadata document can be specific to the input document.
        /// </summary>
        /// <param name="metadataFileName">A delegate that returns a <c>bool</c> indicating if the current document contains the metadata you want to use.</param>
        /// <param name="inherited">If set to <c>true</c>, metadata from documents with this file name will be inherited by documents in nested directories.</param>
        /// <param name="replace">If set to <c>true</c>, metadata from this document will replace any existing metadata on the target document.</param>
        /// <returns>The current module instance.</returns>
        public DirectoryMeta WithMetadataFile(DocumentConfig<bool> metadataFileName, bool inherited = false, bool replace = false)
        {
            _metadataFiles.Add(new MetaFileEntry(metadataFileName, inherited, replace));
            return this;
        }

        /// <summary>
        /// Specifies a file name to use as common metadata.
        /// </summary>
        /// <param name="metadataFileName">Name of the metadata file.</param>
        /// <param name="inherited">If set to <c>true</c>, metadata from documents with this file name will be inherited by documents in nested directories.</param>
        /// <param name="replace">If set to <c>true</c>, metadata from this document will replace any existing metadata on the target document.</param>
        /// <returns>The current module instance.</returns>
        public DirectoryMeta WithMetadataFile(FilePath metadataFileName, bool inherited = false, bool replace = false)
        {
            return WithMetadataFile(Config.FromDocument(doc => doc.Source?.FileName.Equals(metadataFileName) == true), inherited, replace);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            // Find metadata files
            IEnumerable<MetaInfo> metaInfos = await inputs
                .Where(input => input.Source != null)
                .SelectAsync(context, GetMetaInfo);
            Dictionary<DirectoryPath, MetaInfo[]> metadataDictionary = metaInfos
                .Where(x => x != null)
                .ToLookup(x => x.Path)
                .ToDictionary(x => x.Key, x => x.OrderBy(y => y.Priority).ToArray());

            // Ignore files that define Metadata if not preserved
            IEnumerable<IDocument> filteredInputs =
                await inputs.WhereAsync(async x => x.Source != null && (_preserveMetadataFiles || !await _metadataFiles.AnyAsync(isMetadata => isMetadata.MetadataFileName.GetValueAsync(x, context))));

            // Apply Metadata
            return await filteredInputs.SelectAsync(context, ApplyMetadata);

            async Task<MetaInfo> GetMetaInfo(IDocument input)
            {
                for (int c = 0; c < _metadataFiles.Count; c++)
                {
                    if (await _metadataFiles[c].MetadataFileName.GetValueAsync(input, context))
                    {
                        return new MetaInfo
                        {
                            Priority = c,
                            Path = input.Source.Directory,
                            MetadataFileEntry = _metadataFiles[c],
                            Metadata = input.Metadata
                        };
                    }
                }
                return null;
            }

            async Task<IDocument> ApplyMetadata(IDocument input)
            {
                // First add the inherited metadata to the temp dictionary
                List<DirectoryPath> sourcePaths = new List<DirectoryPath>();
                DirectoryPath inputPath = await context.FileSystem.GetContainingInputPathAsync(input.Source);
                if (inputPath != null)
                {
                    DirectoryPath dir = input.Source.Directory;
                    while (dir?.FullPath.StartsWith(inputPath.FullPath) == true)
                    {
                        sourcePaths.Add(dir);
                        dir = dir.Parent;
                    }
                }

                HashSet<string> overriddenKeys = new HashSet<string>(); // we need to know which keys we may override if they are overridden.
                List<KeyValuePair<string, object>> newMetadata = new List<KeyValuePair<string, object>>();

                bool firstLevel = true;
                foreach (DirectoryPath path in sourcePaths)
                {
                    if (metadataDictionary.ContainsKey(path))
                    {
                        foreach (MetaInfo metadataEntry in metadataDictionary[path])
                        {
                            if (!firstLevel && !metadataEntry.MetadataFileEntry.Inherited)
                            {
                                continue; // If we are not in the same directory and inherited isn't activated
                            }

                            foreach (KeyValuePair<string, object> keyValuePair in metadataEntry.Metadata)
                            {
                                if (overriddenKeys.Contains(keyValuePair.Key))
                                {
                                    continue; // The value was already written.
                                }

                                if (input.Metadata.ContainsKey(keyValuePair.Key)
                                    && !metadataEntry.MetadataFileEntry.Replace)
                                {
                                    continue; // The value already exists and this MetadataFile has no override
                                }

                                // We can add the value.
                                overriddenKeys.Add(keyValuePair.Key); // no other MetadataFile may override it.

                                newMetadata.Add(keyValuePair);
                            }
                        }
                    }
                    firstLevel = false;
                }

                return newMetadata.Count > 0 ? context.GetDocument(input, newMetadata) : input;
            }
        }

        private class MetaInfo
        {
            public int Priority { get; set; }
            public DirectoryPath Path { get; set; }
            public MetaFileEntry MetadataFileEntry { get; set; }
            public IMetadata Metadata { get; set; }
        }

        private class MetaFileEntry
        {
            public bool Inherited { get; }
            public DocumentConfig<bool> MetadataFileName { get; }
            public bool Replace { get; }

            public MetaFileEntry(DocumentConfig<bool> metadataFileName, bool inherited, bool replace)
            {
                MetadataFileName = metadataFileName;
                Inherited = inherited;
                Replace = replace;
            }
        }
    }
}
