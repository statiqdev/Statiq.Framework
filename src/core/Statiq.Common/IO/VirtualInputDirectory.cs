using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Statiq.Common
{
    internal class VirtualInputDirectory : IDirectory
    {
        private readonly IReadOnlyFileSystem _fileSystem;

        public VirtualInputDirectory(IReadOnlyFileSystem fileSystem, in NormalizedPath path)
        {
            _fileSystem = fileSystem.ThrowIfNull(nameof(fileSystem));
            Path = path.ThrowIfNull(nameof(path));

            if (!path.IsRelative)
            {
                throw new ArgumentException("Virtual input paths should always be relative", nameof(path));
            }
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

        /// <inheritdoc/>
        public DateTime LastWriteTime => throw new NotSupportedException();

        /// <inheritdoc/>
        public DateTime CreationTime => throw new NotSupportedException();

        /// <inheritdoc/>
        public void Create() => throw new NotSupportedException("Can not create a virtual input directory");

        /// <inheritdoc/>
        public void Delete(bool recursive) => throw new NotSupportedException("Can not delete a virtual input directory");

        /// <inheritdoc/>
        public IEnumerable<IDirectory> GetDirectories(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            // Get all the relative child directories for each existing input directory (I.e. "select many")
            Dictionary<NormalizedPath, VirtualInputDirectory> directories = new Dictionary<NormalizedPath, VirtualInputDirectory>();
            foreach (IDirectory existing in GetExistingInputDirectories())
            {
                foreach (IDirectory childDirectory in existing.GetDirectories(searchOption))
                {
                    // Get the relative path starting from the current directory path to use as a key so we don't end up with duplicates
                    NormalizedPath relativePath = existing.Path.GetRelativePath(childDirectory.Path);
                    directories[relativePath] = new VirtualInputDirectory(_fileSystem, Path.Combine(relativePath));
                }
            }
            return directories.Values;
        }

        /// <inheritdoc/>
        public IEnumerable<IFile> GetFiles(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            // Get all the files for each input directory, replacing earlier ones with later ones
            Dictionary<NormalizedPath, VirtualInputFile> files = new Dictionary<NormalizedPath, VirtualInputFile>();
            foreach (IDirectory existing in GetExistingInputDirectories())
            {
                foreach (IFile file in existing.GetFiles(searchOption))
                {
                    // Get the relative path starting from the current directory path to use as a key so we don't end up with duplicates
                    NormalizedPath relativePath = existing.Path.GetRelativePath(file.Path);
                    files[relativePath] = new VirtualInputFile(file, this);
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
        public bool Exists => GetExistingInputDirectories().Any();

        /// <summary>
        /// Gets this path under each input directory and returns the ones that exist.
        /// </summary>
        // Internal for testing
        internal IEnumerable<IDirectory> GetExistingInputDirectories() =>
            _fileSystem.InputPaths
                .Select(x =>
                {
                    // Is this input path mapped?
                    if (_fileSystem.InputPathMappings.TryGetValue(x, out NormalizedPath mappedInputPath))
                    {
                        return mappedInputPath.ContainsDescendantOrSelf(Path)
                            ? _fileSystem.GetRootDirectory(x.Combine(mappedInputPath.GetRelativePath(Path)))
                            : null;
                    }

                    return _fileSystem.GetRootDirectory(x.Combine(Path));
                })
                .Where(x => x is object && x.Exists);

        public override string ToString() => Path.ToString();

        /// <inheritdoc/>
        public string ToDisplayString() => Path.ToDisplayString();
    }
}
