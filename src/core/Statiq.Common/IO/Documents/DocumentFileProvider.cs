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
        public DocumentFileProvider(IEnumerable<IDocument> documents)
        {
            if (documents != null)
            {
                foreach (IDocument document in documents)
                {
                    if (document.Source != null)
                    {
                        Files[document.Source] = document;
                        DirectoryPath directory = document.Source.Directory;
                        while (directory != null)
                        {
                            Directories.Add(directory);
                            directory = directory.Parent;
                        }
                    }
                }
            }
        }

        internal Dictionary<FilePath, IDocument> Files { get; } = new Dictionary<FilePath, IDocument>();

        internal HashSet<DirectoryPath> Directories { get; } = new HashSet<DirectoryPath>();

        public Task<IDirectory> GetDirectoryAsync(DirectoryPath path) =>
            Task.FromResult<IDirectory>(new DocumentDirectory(this, path));

        public Task<IFile> GetFileAsync(FilePath path) =>
            Task.FromResult<IFile>(new DocumentFile(this, path));

        public IDocument GetDocument(FilePath path) =>
            Files.TryGetValue(path, out IDocument document) ? document : throw new KeyNotFoundException();
    }
}
