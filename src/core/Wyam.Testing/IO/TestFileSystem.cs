using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;
using Wyam.Common.Util;

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
        public async Task<IFile> GetInputFileAsync(FilePath path)
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
                    IFile file = await GetFileAsync(RootPath.Combine(inputPath).CombineFile(path));
                    if (notFound == null)
                    {
                        notFound = file;
                    }
                    if (await file.GetExistsAsync())
                    {
                        return file;
                    }
                }
                if (notFound == null)
                {
                    throw new InvalidOperationException("The input paths collection must have at least one path");
                }
                return notFound;
            }
            return await GetFileAsync(path);
        }

        /// <inheritdoc />
        public Task<IEnumerable<IFile>> GetInputFilesAsync(params string[] patterns) =>
            GetInputFilesAsync((IEnumerable<string>)patterns);

        /// <inheritdoc />
        public async Task<IEnumerable<IFile>> GetInputFilesAsync(IEnumerable<string> patterns) =>
            await GetFilesAsync(await GetInputDirectoryAsync(), patterns);

        /// <inheritdoc />
        public async Task<IDirectory> GetInputDirectoryAsync(DirectoryPath path = null) =>
            path == null
                ? new TestDirectory(FileProvider, ".")
                : (path.IsRelative ? new TestDirectory(FileProvider, path) : await GetDirectoryAsync(path));

        /// <inheritdoc />
        public async Task<IReadOnlyList<IDirectory>> GetInputDirectoriesAsync() =>
            (await InputPaths.SelectAsync(async x => await GetRootDirectoryAsync(x))).ToImmutableArray();

        /// <inheritdoc />
        public async Task<DirectoryPath> GetContainingInputPathAsync(NormalizedPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (path.IsAbsolute)
            {
                return InputPaths
                    .Reverse()
                    .Select(x => RootPath.Combine(x))
                    .FirstOrDefault(x => x.FileProvider == path.FileProvider
                        && (path.FullPath == x.Collapse().FullPath || path.FullPath.StartsWith(x.Collapse().FullPath + "/")));
            }
            if (path is FilePath filePath)
            {
                IFile file = await GetInputFileAsync(filePath);
                return await file.GetExistsAsync() ? await GetContainingInputPathAsync(file.Path) : null;
            }
            if (path is DirectoryPath directoryPath)
            {
                IEnumerable<KeyValuePair<DirectoryPath, IDirectory>> inputPaths = await InputPaths
                    .Reverse()
                    .SelectAsync(async x => new KeyValuePair<DirectoryPath, IDirectory>(x, await GetRootDirectoryAsync(x.Combine(directoryPath))));
                return (await inputPaths
                    .WhereAsync(async x => await x.Value.GetExistsAsync()))
                    .Select(x => RootPath.Combine(x.Key))
                    .FirstOrDefault();
            }
            return null;
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
        public async Task<IEnumerable<IFile>> GetFilesAsync(params string[] patterns) =>
            await GetFilesAsync(await GetRootDirectoryAsync(), patterns);

        /// <inheritdoc />
        public async Task<IEnumerable<IFile>> GetFilesAsync(IEnumerable<string> patterns) =>
            await GetFilesAsync(await GetRootDirectoryAsync(), patterns);

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
