using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Represents a sequence of documents as a file system (I.e., for use in the globber).
    /// </summary>
    public class DocumentFileProvider : IFileProvider
    {
        /// <summary>
        /// Creates a file provider for a sequence of documents.
        /// </summary>
        /// <param name="documents">The documents to provide virtual directories and files for.</param>
        /// <param name="source">
        /// <c>true</c> to use <see cref="IDocument.Source"/> as the basis for paths,
        /// <c>false</c> to use <see cref="IDocument.Destination"/>.
        /// </param>
        public DocumentFileProvider(IEnumerable<IDocument> documents, bool source)
        {
            if (documents != null)
            {
                foreach (IDocument document in documents)
                {
                    NormalizedPath path = source ? document.Source : NormalizedPath.Root.Combine(document.Destination);
                    if (path != null)
                    {
                        Files[path] = document;
                        NormalizedPath directory = path.Parent;
                        while (directory != null)
                        {
                            Directories.Add(directory);
                            directory = directory.Parent;
                        }
                    }
                }
            }
        }

        internal Dictionary<NormalizedPath, IDocument> Files { get; } = new Dictionary<NormalizedPath, IDocument>();

        internal HashSet<NormalizedPath> Directories { get; } = new HashSet<NormalizedPath>();

        public IDirectory GetDirectory(NormalizedPath path) => new DocumentDirectory(this, path);

        public IFile GetFile(NormalizedPath path) => new DocumentFile(this, path);

        public IDocument GetDocument(NormalizedPath path) =>
            Files.TryGetValue(path, out IDocument document) ? document : throw new KeyNotFoundException();
    }
}
