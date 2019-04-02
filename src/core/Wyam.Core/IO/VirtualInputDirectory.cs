using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;
using Wyam.Common.Util;

namespace Wyam.Core.IO
{
    internal class VirtualInputDirectory : IDirectory
    {
        private readonly FileSystem _fileSystem;

        public VirtualInputDirectory(FileSystem fileSystem, DirectoryPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (!path.IsRelative)
            {
                throw new ArgumentException("Virtual input paths should always be relative", nameof(path));
            }

            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            Path = path;
        }

        /// <inheritdoc/>
        public DirectoryPath Path { get; }

        NormalizedPath IFileSystemEntry.Path => Path;

        /// <inheritdoc/>
        public Task<IDirectory> GetParentAsync()
        {
            DirectoryPath parentPath = Path.Parent;
            if (parentPath == null)
            {
                return Task.FromResult<IDirectory>(null);
            }
            return Task.FromResult<IDirectory>(new VirtualInputDirectory(_fileSystem, parentPath));
        }

        /// <inheritdoc/>
        public Task CreateAsync()
        {
            throw new NotSupportedException("Can not create a virtual input directory");
        }

        /// <inheritdoc/>
        public Task DeleteAsync(bool recursive)
        {
            throw new NotSupportedException("Can not delete a virtual input directory");
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<IDirectory>> GetDirectoriesAsync(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            // For the root (".") virtual directory, this should just return the child name,
            // but for all others it should include the child directory name

            // Get all the relative child directories
            HashSet<DirectoryPath> directories = new HashSet<DirectoryPath>();
            foreach (IDirectory directory in await GetExistingDirectoriesAsync())
            {
                foreach (IDirectory childDirectory in await directory.GetDirectoriesAsync(searchOption))
                {
                    directories.Add(Path.Combine(directory.Path.GetRelativePath(childDirectory.Path)));
                }
            }

            // Return a new virtual directory for each one
            return directories.Select(x => new VirtualInputDirectory(_fileSystem, x));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<IFile>> GetFilesAsync(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            // Get all the files for each input directory, replacing earlier ones with later ones
            Dictionary<FilePath, IFile> files = new Dictionary<FilePath, IFile>();
            foreach (IDirectory directory in await GetExistingDirectoriesAsync())
            {
                foreach (IFile file in await directory.GetFilesAsync(searchOption))
                {
                    files[directory.Path.GetRelativePath(file.Path)] = file;
                }
            }
            return files.Values;
        }

        /// <inheritdoc/>
        public Task<IDirectory> GetDirectoryAsync(DirectoryPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (!path.IsRelative)
            {
                throw new ArgumentException("Path must be relative", nameof(path));
            }

            return Task.FromResult<IDirectory>(new VirtualInputDirectory(_fileSystem, Path.Combine(path)));
        }

        /// <inheritdoc/>
        public async Task<IFile> GetFileAsync(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (!path.IsRelative)
            {
                throw new ArgumentException("Path must be relative", nameof(path));
            }

            return await _fileSystem.GetInputFileAsync(Path.CombineFile(path));
        }

        /// <summary>
        /// Gets a value indicating whether any of the input paths contain this directory.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this directory exists at one of the input paths; otherwise, <c>false</c>.
        /// </returns>
        public async Task<bool> GetExistsAsync() => (await GetExistingDirectoriesAsync()).Any();

        /// <summary>
        /// Returns <c>true</c> if any of the input paths are case sensitive.
        /// </summary>
        /// <remarks>
        /// When dealing with virtual input directories that could be comprised of multiple
        /// file systems with different case sensitivity, it's safer to treat the
        /// virtual file system as case-sensitive if any of the underlying file systems
        /// are case-sensitive. Otherwise, if we treated it as case-insensitive when
        /// one of the file systems was actually case-sensitive we would get false-positive
        /// results when assuming if directories and files in that file system existed
        /// (for example, in the globber).
        /// </remarks>
        public bool IsCaseSensitive => GetExistingDirectoriesAsync().Result.Any(x => x.IsCaseSensitive);

        private async Task<IEnumerable<IDirectory>> GetExistingDirectoriesAsync()
        {
            IEnumerable<IDirectory> directories = await _fileSystem.InputPaths
                .SelectAsync(async x => await _fileSystem.GetRootDirectoryAsync(x.Combine(Path)));
            return await directories.WhereAsync(async x => await x.GetExistsAsync());
        }
    }
}
