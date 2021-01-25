using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    internal class VirtualInputDirectory : IDirectory
    {
        private readonly IReadOnlyFileSystem _fileSystem;

        public VirtualInputDirectory(IReadOnlyFileSystem fileSystem, in NormalizedPath path)
        {
            path.ThrowIfNull(nameof(path));

            if (!path.IsRelative)
            {
                throw new ArgumentException("Virtual input paths should always be relative", nameof(path));
            }

            _fileSystem = fileSystem.ThrowIfNull(nameof(fileSystem));
            Path = path;
        }

        /// <inheritdoc/>
        public NormalizedPath Path { get; }

        NormalizedPath IFileSystemEntry.Path => Path;

        /// <inheritdoc/>
        public IDirectory Parent
        {
            get
            {
                NormalizedPath parentPath = Path.Parent;
                return parentPath.IsNull ? null : new VirtualInputDirectory(_fileSystem, parentPath);
            }
        }

        public DateTime LastWriteTime => throw new NotSupportedException();

        public DateTime CreationTime => throw new NotSupportedException();

        /// <inheritdoc/>
        public void Create() => throw new NotSupportedException("Can not create a virtual input directory");

        /// <inheritdoc/>
        public void Delete(bool recursive) => throw new NotSupportedException("Can not delete a virtual input directory");

        /// <inheritdoc/>
        public IEnumerable<IDirectory> GetDirectories(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            // For the root (".") virtual directory, this should just return the child name,
            // but for all others it should include the child directory name

            // Get all the relative child directories
            HashSet<NormalizedPath> directories = new HashSet<NormalizedPath>();
            foreach (IDirectory directory in GetExistingDirectories())
            {
                foreach (IDirectory childDirectory in directory.GetDirectories(searchOption))
                {
                    directories.Add(Path.Combine(directory.Path.GetRelativePath(childDirectory.Path)));
                }
            }

            // Return a new virtual directory for each one
            return directories.Select(x => new VirtualInputDirectory(_fileSystem, x));
        }

        /// <inheritdoc/>
        public IEnumerable<IFile> GetFiles(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            // Get all the files for each input directory, replacing earlier ones with later ones
            Dictionary<NormalizedPath, VirtualInputFile> files = new Dictionary<NormalizedPath, VirtualInputFile>();
            foreach (IDirectory directory in GetExistingDirectories())
            {
                foreach (IFile file in directory.GetFiles(searchOption))
                {
                    files[directory.Path.GetRelativePath(file.Path)] = new VirtualInputFile(file, this);
                }
            }
            return files.Values;
        }

        /// <inheritdoc/>
        public IDirectory GetDirectory(NormalizedPath path)
        {
            path.ThrowIfNull(nameof(path));

            if (!path.IsRelative)
            {
                throw new ArgumentException("Path must be relative", nameof(path));
            }

            return new VirtualInputDirectory(_fileSystem, Path.Combine(path));
        }

        /// <inheritdoc/>
        public IFile GetFile(NormalizedPath path)
        {
            path.ThrowIfNull(nameof(path));

            if (!path.IsRelative)
            {
                throw new ArgumentException("Path must be relative", nameof(path));
            }

            return new VirtualInputFile(_fileSystem.GetInputFile(Path.Combine(path)), this);
        }

        /// <summary>
        /// Gets a value indicating whether any of the input paths contain this directory.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this directory exists at one of the input paths; otherwise, <c>false</c>.
        /// </returns>
        public bool Exists => GetExistingDirectories().Any();

        private IEnumerable<IDirectory> GetExistingDirectories() =>
            _fileSystem.InputPaths
                .Select(x => _fileSystem.GetRootDirectory(x.Combine(Path)))
                .Where(x => x.Exists);

        public override string ToString() => Path.ToString();

        public string ToDisplayString() => Path.ToDisplayString();
    }
}
