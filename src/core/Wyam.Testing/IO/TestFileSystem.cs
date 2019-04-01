using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;

namespace Wyam.Testing.IO
{
    /// <summary>
    /// A file system for testing that uses a single file provider.
    /// </summary>
    public class TestFileSystem : IFileSystem
    {
        /// <summary>
        /// The file provider to use for this file system.
        /// </summary>
        public TestFileProvider FileProvider { get; set; } = new TestFileProvider();

        /// <inheritdoc />
        public IFileProviderCollection FileProviders
        {
            get { throw new NotImplementedException(); }
        }

        IReadOnlyFileProviderCollection IReadOnlyFileSystem.FileProviders => FileProviders;

        /// <inheritdoc />
        public DirectoryPath RootPath { get; set; } = new DirectoryPath("/");

        /// <inheritdoc />
        public PathCollection<DirectoryPath> InputPaths { get; set; } = new PathCollection<DirectoryPath>(new[]
        {
            new DirectoryPath("theme"),
            new DirectoryPath("input")
        });

        IReadOnlyList<DirectoryPath> IReadOnlyFileSystem.InputPaths => InputPaths;

        /// <inheritdoc />
        public DirectoryPath OutputPath { get; set; } = "output";

        /// <inheritdoc />
        public DirectoryPath TempPath { get; set; } = "temp";

        /// <inheritdoc />
        public Task<IFile> GetInputFileAsync(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (path.IsRelative)
            {
                IFile notFound = null;
                foreach (DirectoryPath inputPath in InputPaths.Reverse())
                {
                    IFile file = GetFileAsync(RootPath.Combine(inputPath).CombineFile(path)).Result;
                    if (notFound == null)
                    {
                        notFound = file;
                    }
                    if (file.GetExistsAsync().Result)
                    {
                        return Task.FromResult(file);
                    }
                }
                if (notFound == null)
                {
                    throw new InvalidOperationException("The input paths collection must have at least one path");
                }
                return Task.FromResult(notFound);
            }
            return GetFileAsync(path);
        }

        /// <inheritdoc />
        public Task<IEnumerable<IFile>> GetInputFilesAsync(params string[] patterns) =>
            GetInputFilesAsync((IEnumerable<string>)patterns);

        /// <inheritdoc />
        public Task<IEnumerable<IFile>> GetInputFilesAsync(IEnumerable<string> patterns) =>
            GetFilesAsync(GetInputDirectoryAsync().Result, patterns);

        /// <inheritdoc />
        public Task<IDirectory> GetInputDirectoryAsync(DirectoryPath path = null) =>
            Task.FromResult(path == null
                ? new TestDirectory(FileProvider, ".")
                : (path.IsRelative ? new TestDirectory(FileProvider, path) : GetDirectoryAsync(path).Result));

        /// <inheritdoc />
        public Task<IReadOnlyList<IDirectory>> GetInputDirectoriesAsync() =>
            Task.FromResult<IReadOnlyList<IDirectory>>(InputPaths.Select(x => GetRootDirectoryAsync(x).Result).ToImmutableArray());

        /// <inheritdoc />
        public Task<DirectoryPath> GetContainingInputPathAsync(NormalizedPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (path.IsAbsolute)
            {
                return Task.FromResult(InputPaths
                    .Reverse()
                    .Select(x => RootPath.Combine(x))
                    .FirstOrDefault(x => x.FileProvider == path.FileProvider
                        && (path.FullPath == x.Collapse().FullPath || path.FullPath.StartsWith(x.Collapse().FullPath + "/"))));
            }
            if (path is FilePath filePath)
            {
                IFile file = GetInputFileAsync(filePath).Result;
                return Task.FromResult(file.GetExistsAsync().Result ? GetContainingInputPathAsync(file.Path).Result : null);
            }
            DirectoryPath directoryPath = path as DirectoryPath;
            if (directoryPath != null)
            {
                return Task.FromResult(InputPaths
                    .Reverse()
                    .Select(x => new KeyValuePair<DirectoryPath, IDirectory>(x, GetRootDirectoryAsync(x.Combine(directoryPath)).Result))
                    .Where(x => x.Value.GetExistsAsync().Result)
                    .Select(x => RootPath.Combine(x.Key))
                    .FirstOrDefault());
            }
            return Task.FromResult<DirectoryPath>(null);
        }

        /// <inheritdoc />
        public FilePath GetOutputPath(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return RootPath.Combine(OutputPath).CombineFile(path);
        }

        /// <inheritdoc />
        public DirectoryPath GetOutputPath(DirectoryPath path = null) =>
            path == null
                ? RootPath.Combine(OutputPath)
                : RootPath.Combine(OutputPath).Combine(path);

        /// <inheritdoc />
        public Task<IFile> GetOutputFileAsync(FilePath path) =>
            GetFileAsync(GetOutputPath(path));

        /// <inheritdoc />
        public Task<IDirectory> GetOutputDirectoryAsync(DirectoryPath path = null) =>
            GetDirectoryAsync(GetOutputPath(path));

        /// <inheritdoc />
        public FilePath GetTempPath(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return RootPath.Combine(TempPath).CombineFile(path);
        }

        /// <inheritdoc />
        public DirectoryPath GetTempPath(DirectoryPath path = null) =>
            path == null
                ? RootPath.Combine(TempPath)
                : RootPath.Combine(TempPath).Combine(path);

        /// <inheritdoc />
        public Task<IFile> GetTempFileAsync(FilePath path) =>
            GetFileAsync(GetTempPath(path));

        /// <inheritdoc />
        public Task<IFile> GetTempFileAsync() =>
            GetTempFileAsync(System.IO.Path.ChangeExtension(System.IO.Path.GetRandomFileName(), "tmp"));

        /// <inheritdoc />
        public Task<IDirectory> GetTempDirectoryAsync(DirectoryPath path = null) =>
            GetDirectoryAsync(GetTempPath(path));

        /// <inheritdoc />
        public Task<IFile> GetRootFileAsync(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return GetFileAsync(RootPath.CombineFile(path));
        }

        /// <inheritdoc />
        public Task<IDirectory> GetRootDirectoryAsync(DirectoryPath path = null) =>
            path == null
            ? GetDirectoryAsync(RootPath)
            : GetDirectoryAsync(RootPath.Combine(path));

        /// <inheritdoc />
        public Task<IFile> GetFileAsync(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return GetFileProvider(path).GetFileAsync(path);
        }

        /// <inheritdoc />
        public Task<IDirectory> GetDirectoryAsync(DirectoryPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return GetFileProvider(path).GetDirectoryAsync(path);
        }

        /// <inheritdoc />
        public Task<IEnumerable<IFile>> GetFilesAsync(params string[] patterns) =>
            GetFilesAsync(GetRootDirectoryAsync().Result, patterns);

        /// <inheritdoc />
        public Task<IEnumerable<IFile>> GetFilesAsync(IEnumerable<string> patterns) =>
            GetFilesAsync(GetRootDirectoryAsync().Result, patterns);

        /// <inheritdoc />
        public Task<IEnumerable<IFile>> GetFilesAsync(IDirectory directory, params string[] patterns) =>
            GetFilesAsync(directory, (IEnumerable<string>)patterns);

        /// <inheritdoc />
        public Task<IEnumerable<IFile>> GetFilesAsync(IDirectory directory, IEnumerable<string> patterns) =>
            Task.FromResult<IEnumerable<IFile>>(Array.Empty<IFile>());

        /// <inheritdoc />
        public IFileProvider GetFileProvider(NormalizedPath path) => FileProvider;
    }
}
