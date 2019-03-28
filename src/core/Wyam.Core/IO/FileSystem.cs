using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common.IO;
using Wyam.Common.Util;
using Wyam.Core.IO.FileProviders.Local;
using Wyam.Core.IO.Globbing;

namespace Wyam.Core.IO
{
    // Initially based on code from Cake (http://cakebuild.net/)
    internal class FileSystem : IFileSystem
    {
        private DirectoryPath _rootPath = Directory.GetCurrentDirectory();
        private DirectoryPath _outputPath = "output";
        private DirectoryPath _tempPath = "temp";

        public FileSystem()
        {
            FileProviders = new FileProviderCollection(new LocalFileProvider());
            InputPaths = new PathCollection<DirectoryPath>(new[]
            {
                new DirectoryPath("theme"),
                new DirectoryPath("input")
            });
        }

        public IFileProviderCollection FileProviders { get; }

        IReadOnlyFileProviderCollection IReadOnlyFileSystem.FileProviders => FileProviders;

        public DirectoryPath RootPath
        {
            get => _rootPath;

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(RootPath));
                }
                if (value.IsRelative)
                {
                    throw new ArgumentException("The root path must not be relative");
                }
                _rootPath = value;
            }
        }

        public PathCollection<DirectoryPath> InputPaths { get; }

        IReadOnlyList<DirectoryPath> IReadOnlyFileSystem.InputPaths => InputPaths;

        public DirectoryPath OutputPath
        {
            get
            {
                return _outputPath;
            }

            set
            {
                _outputPath = value ?? throw new ArgumentNullException(nameof(OutputPath));
            }
        }

        public DirectoryPath TempPath
        {
            get
            {
                return _tempPath;
            }

            set
            {
                _tempPath = value ?? throw new ArgumentNullException(nameof(TempPath));
            }
        }

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

        public async Task<IEnumerable<IFile>> GetInputFilesAsync(params string[] patterns) =>
            await GetInputFilesAsync((IEnumerable<string>)patterns);

        public async Task<IEnumerable<IFile>> GetInputFilesAsync(IEnumerable<string> patterns) =>
            await GetFilesAsync(await GetInputDirectoryAsync(), patterns);

        public async Task<IDirectory> GetInputDirectoryAsync(DirectoryPath path = null) =>
            path == null
                ? new VirtualInputDirectory(this, ".")
                : (path.IsRelative ? new VirtualInputDirectory(this, path) : await GetDirectoryAsync(path));

        public async Task<IReadOnlyList<IDirectory>> GetInputDirectoriesAsync() =>
            (await InputPaths.SelectAsync(async x => await GetRootDirectoryAsync(x))).ToImmutableArray();

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
                return await InputPaths
                    .Reverse()
                    .SelectAsync(async x => (x, await GetRootDirectoryAsync(x.Combine(directoryPath))))
                    .WhereAsync(async x => await x.Item2.GetExistsAsync())
                    .SelectAsync(x => RootPath.Combine(x.Item1))
                    .FirstOrDefaultAsync();
            }
            return null;
        }

        public FilePath GetOutputPath(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return RootPath.Combine(OutputPath).CombineFile(path);
        }

        public DirectoryPath GetOutputPath(DirectoryPath path = null) =>
            path == null
                ? RootPath.Combine(OutputPath)
                : RootPath.Combine(OutputPath).Combine(path);

        public async Task<IFile> GetOutputFileAsync(FilePath path) =>
            await GetFileAsync(GetOutputPath(path));

        public async Task<IDirectory> GetOutputDirectoryAsync(DirectoryPath path = null) =>
            await GetDirectoryAsync(GetOutputPath(path));

        public FilePath GetTempPath(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return RootPath.Combine(TempPath).CombineFile(path);
        }

        public DirectoryPath GetTempPath(DirectoryPath path = null) =>
            path == null
                ? RootPath.Combine(TempPath)
                : RootPath.Combine(TempPath).Combine(path);

        public async Task<IFile> GetTempFileAsync(FilePath path) =>
            await GetFileAsync(GetTempPath(path));

        public async Task<IFile> GetTempFileAsync() =>
            await GetTempFileAsync(Path.ChangeExtension(Path.GetRandomFileName(), "tmp"));

        public async Task<IDirectory> GetTempDirectoryAsync(DirectoryPath path = null) =>
            await GetDirectoryAsync(GetTempPath(path));

        public async Task<IFile> GetRootFileAsync(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return await GetFileAsync(RootPath.CombineFile(path));
        }

        public async Task<IDirectory> GetRootDirectoryAsync(DirectoryPath path = null) =>
            path == null
            ? await GetDirectoryAsync(RootPath)
            : await GetDirectoryAsync(RootPath.Combine(path));

        public async Task<IFile> GetFileAsync(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return await GetFileProvider(path).GetFileAsync(path);
        }

        public async Task<IDirectory> GetDirectoryAsync(DirectoryPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return await GetFileProvider(path).GetDirectoryAsync(path);
        }

        public async Task<IEnumerable<IFile>> GetFilesAsync(params string[] patterns) =>
            await GetFilesAsync(await GetRootDirectoryAsync(), patterns);

        public async Task<IEnumerable<IFile>> GetFilesAsync(IEnumerable<string> patterns) =>
            await GetFilesAsync(await GetRootDirectoryAsync(), patterns);

        public async Task<IEnumerable<IFile>> GetFilesAsync(IDirectory directory, params string[] patterns) =>
            await GetFilesAsync(directory, (IEnumerable<string>)patterns);

        public async Task<IEnumerable<IFile>> GetFilesAsync(IDirectory directory, IEnumerable<string> patterns)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            IEnumerable<Tuple<IDirectory, string>> directoryPatterns = await patterns
                .Where(x => x != null)
                .SelectAsync(async x =>
                {
                    bool negated = x[0] == '!';
                    FilePath filePath = negated ? new FilePath(x.Substring(1)) : new FilePath(x);
                    if (filePath.IsAbsolute)
                    {
                        // The globber doesn't support absolute paths, so get the root directory of this path (including provider)
                        IDirectory rootDirectory = await GetDirectoryAsync(filePath.Root);
                        FilePath relativeFilePath = filePath.RootRelative.Collapse();
                        return Tuple.Create(
                            rootDirectory,
                            negated ? ('!' + relativeFilePath.FullPath) : relativeFilePath.FullPath);
                    }
                    return Tuple.Create(directory, x);
                });
            return directoryPatterns
                .GroupBy(x => x.Item1, x => x.Item2, new DirectoryEqualityComparer())
                .SelectMany(x => Globber.GetFiles(x.Key, x));
        }

        public IFileProvider GetFileProvider(NormalizedPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (path.IsRelative)
            {
                throw new ArgumentException("The path must be absolute");
            }
            if (path.FileProvider == null)
            {
                throw new ArgumentException("The path has no provider");
            }
            if (!FileProviders.TryGet(path.FileProvider.Scheme, out IFileProvider fileProvider))
            {
                throw new KeyNotFoundException($"A provider for the scheme {path.FileProvider} could not be found");
            }
            return fileProvider;
        }
    }
}
