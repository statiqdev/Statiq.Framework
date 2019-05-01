using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO.Globbing;
using Wyam.Common.Util;

namespace Wyam.Common.IO
{
    public static class IReadOnlyFileSystemExtensions
    {
        /// <summary>
        /// Gets a file representing an input.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">
        /// The path of the input file. If this is an absolute path,
        /// then a file representing the specified path is returned.
        /// If it's a relative path, then operations will search all
        /// current input paths.
        /// </param>
        /// <returns>An input file.</returns>
        public static async Task<IFile> GetInputFileAsync(this IReadOnlyFileSystem fileSystem, FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (path.IsRelative)
            {
                IFile notFound = null;
                foreach (DirectoryPath inputPath in fileSystem.InputPaths.Reverse())
                {
                    IFile file = await fileSystem.GetFileAsync(fileSystem.RootPath.Combine(inputPath).CombineFile(path));
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
            return await fileSystem.GetFileAsync(path);
        }

        /// <summary>
        /// Gets matching input files based on globbing patterns and/or absolute paths. If any absolute paths
        /// are provided, only those that actually exist are returned.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="patterns">The globbing patterns and/or absolute paths.</param>
        /// <returns>All input files that match the globbing patterns and/or absolute paths.</returns>
        public static async Task<IEnumerable<IFile>> GetInputFilesAsync(this IReadOnlyFileSystem fileSystem, params string[] patterns) =>
            await fileSystem.GetInputFilesAsync((IEnumerable<string>)patterns);

        /// <summary>
        /// Gets matching input files based on globbing patterns and/or absolute paths. If any absolute paths
        /// are provided, only those that actually exist are returned.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="patterns">The globbing patterns and/or absolute paths.</param>
        /// <returns>All input files that match the globbing patterns and/or absolute paths.</returns>
        public static async Task<IEnumerable<IFile>> GetInputFilesAsync(this IReadOnlyFileSystem fileSystem, IEnumerable<string> patterns) =>
            await fileSystem.GetFilesAsync(await fileSystem.GetInputDirectoryAsync(), patterns);

        /// <summary>
        /// Gets all absolute input directories.
        /// </summary>
        /// <returns>The absolute input directories.</returns>
        public static async Task<IReadOnlyList<IDirectory>> GetInputDirectoriesAsync(this IReadOnlyFileSystem fileSystem) =>
            (await fileSystem.InputPaths.SelectAsync(async x => await fileSystem.GetRootDirectoryAsync(x))).ToImmutableArray();

        /// <summary>
        /// Gets the absolute input path that contains the specified file or directory. If the provided
        /// file or directory path is absolute, this returns the input path that contains the specified
        /// path (note that the specified file or directory does not need to exist and this just returns
        /// the input path that would contain the file or directory based only on path information). If
        /// the provided path is relative, this checks all input paths for the existence of the file
        /// or directory and returns the first one where it exists.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The file path.</param>
        /// <returns>The input path that contains the specified file,
        /// or <c>null</c> if no input path does.</returns>
        public static async Task<DirectoryPath> GetContainingInputPathAsync(this IReadOnlyFileSystem fileSystem, NormalizedPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (path.IsAbsolute)
            {
                return fileSystem.InputPaths
                    .Reverse()
                    .Select(x => fileSystem.RootPath.Combine(x))
                    .FirstOrDefault(x => x.FileProvider == path.FileProvider
                        && (path.FullPath == x.Collapse().FullPath || path.FullPath.StartsWith(x.Collapse().FullPath + "/")));
            }
            if (path is FilePath filePath)
            {
                IFile file = await fileSystem.GetInputFileAsync(filePath);
                return await file.GetExistsAsync() ? await fileSystem.GetContainingInputPathAsync(file.Path) : null;
            }
            if (path is DirectoryPath directoryPath)
            {
                IEnumerable<(DirectoryPath x, IDirectory)> rootDirectories =
                    await fileSystem.InputPaths
                        .Reverse()
                        .SelectAsync(async x => (x, await fileSystem.GetRootDirectoryAsync(x.Combine(directoryPath))));
                IEnumerable<(DirectoryPath x, IDirectory)> existingRootDirectories =
                    await rootDirectories
                        .WhereAsync(async x => await x.Item2.GetExistsAsync());
                return existingRootDirectories.Select(x => fileSystem.RootPath.Combine(x.Item1)).FirstOrDefault();
            }
            return null;
        }

        /// <summary>
        /// Gets an output file path by combining it with the root path and output path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path to combine with the root path and output path.</param>
        /// <returns>The output file path.</returns>
        public static FilePath GetOutputPath(this IReadOnlyFileSystem fileSystem, FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return fileSystem.RootPath.Combine(fileSystem.OutputPath).CombineFile(path);
        }

        /// <summary>
        /// Gets an output directory path by combining it with the root path and output path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path to combine with the root path and output path.
        /// If this is <c>null</c>, returns the root path combined with the output path.</param>
        /// <returns>The output directory path.</returns>
        public static DirectoryPath GetOutputPath(this IReadOnlyFileSystem fileSystem, DirectoryPath path = null) =>
            path == null
                ? fileSystem.RootPath.Combine(fileSystem.OutputPath)
                : fileSystem.RootPath.Combine(fileSystem.OutputPath).Combine(path);

        /// <summary>
        /// Gets a file representing an output.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">
        /// The path of the output file. If this is an absolute path,
        /// then a file representing the specified path is returned.
        /// If it's a relative path, then it will be combined with the
        /// current output path.
        /// </param>
        /// <returns>An output file.</returns>
        public static async Task<IFile> GetOutputFileAsync(this IReadOnlyFileSystem fileSystem, FilePath path) =>
            await fileSystem.GetFileAsync(fileSystem.GetOutputPath(path));

        /// <summary>
        /// Gets a directory representing an output.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">
        /// The path of the output directory. If this is an absolute path,
        /// then a directory representing the specified path is returned.
        /// If it's a relative path, then it will be combined with the
        /// current output path. If this is <c>null</c> then the base
        /// output directory is returned.
        /// </param>
        /// <returns>An output directory.</returns>
        public static async Task<IDirectory> GetOutputDirectoryAsync(this IReadOnlyFileSystem fileSystem, DirectoryPath path = null) =>
            await fileSystem.GetDirectoryAsync(fileSystem.GetOutputPath(path));

        /// <summary>
        /// Gets a temp file path by combining it with the root path and temp path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path to combine with the root path and temp path.</param>
        /// <returns>The temp file path.</returns>
        public static FilePath GetTempPath(this IReadOnlyFileSystem fileSystem, FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return fileSystem.RootPath.Combine(fileSystem.TempPath).CombineFile(path);
        }

        /// <summary>
        /// Gets a temp directory path by combining it with the root path and temp path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path to combine with the root path and temp path.
        /// If this is <c>null</c>, returns the root path combined with the temp path.</param>
        /// <returns>The temp directory path.</returns>
        public static DirectoryPath GetTempPath(this IReadOnlyFileSystem fileSystem, DirectoryPath path = null) =>
            path == null
                ? fileSystem.RootPath.Combine(fileSystem.TempPath)
                : fileSystem.RootPath.Combine(fileSystem.TempPath).Combine(path);

        /// <summary>
        /// Gets a file representing a temp file.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">
        /// If this is an absolute path,
        /// then a file representing the specified path is returned.
        /// If it's a relative path, then it will be combined with the
        /// current temp path.
        /// </param>
        /// <returns>A temp file.</returns>
        public static async Task<IFile> GetTempFileAsync(this IReadOnlyFileSystem fileSystem, FilePath path) =>
            await fileSystem.GetFileAsync(fileSystem.GetTempPath(path));

        /// <summary>
        /// Gets a file representing a temp file with a random file name.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <returns>A temp file.</returns>
        public static async Task<IFile> GetTempFileAsync(this IReadOnlyFileSystem fileSystem) =>
            await fileSystem.GetTempFileAsync(Path.ChangeExtension(Path.GetRandomFileName(), "tmp"));

        /// <summary>
        /// Gets a directory representing temp files.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">
        /// The path of the temp directory. If this is an absolute path,
        /// then a directory representing the specified path is returned.
        /// If it's a relative path, then it will be combined with the
        /// current temp path. If this is <c>null</c> then the base
        /// temp directory is returned.
        /// </param>
        /// <returns>A temp directory.</returns>
        public static async Task<IDirectory> GetTempDirectoryAsync(this IReadOnlyFileSystem fileSystem, DirectoryPath path = null) =>
            await fileSystem.GetDirectoryAsync(fileSystem.GetTempPath(path));

        /// <summary>
        /// Gets a file representing a root file.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">
        /// The path of the root file. If this is an absolute path,
        /// then a file representing the specified path is returned.
        /// If it's a relative path, then it will be combined with the
        /// current root path.
        /// </param>
        /// <returns>A root file.</returns>
        public static async Task<IFile> GetRootFileAsync(this IReadOnlyFileSystem fileSystem, FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return await fileSystem.GetFileAsync(fileSystem.RootPath.CombineFile(path));
        }

        /// <summary>
        /// Gets a directory representing a root directory.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">
        /// The path of the root directory. If this is an absolute path,
        /// then a directory representing the specified path is returned.
        /// If it's a relative path, then it will be combined with the
        /// current root path. If this is <c>null</c> then the base
        /// root directory is returned.
        /// </param>
        /// <returns>A root directory.</returns>
        public static async Task<IDirectory> GetRootDirectoryAsync(this IReadOnlyFileSystem fileSystem, DirectoryPath path = null) =>
            path == null
            ? await fileSystem.GetDirectoryAsync(fileSystem.RootPath)
            : await fileSystem.GetDirectoryAsync(fileSystem.RootPath.Combine(path));

        /// <summary>
        /// Gets an absolute file.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">
        /// The absolute path of the file.
        /// </param>
        /// <returns>A file.</returns>
        public static async Task<IFile> GetFileAsync(this IReadOnlyFileSystem fileSystem, FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return await fileSystem.GetFileProvider(path).GetFileAsync(path);
        }

        /// <summary>
        /// Gets an absolute directory.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">
        /// The absolute path of the directory.
        /// </param>
        /// <returns>A directory.</returns>
        public static async Task<IDirectory> GetDirectoryAsync(this IReadOnlyFileSystem fileSystem, DirectoryPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return await fileSystem.GetFileProvider(path).GetDirectoryAsync(path);
        }

        /// <summary>
        /// Gets a directory representing an input.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">
        /// The path of the input directory. If this is an absolute path,
        /// then a directory representing the specified path is returned.
        /// If it's a relative path, then the returned directory will
        /// be a virtual directory that aggregates all input
        /// paths. If this is <c>null</c> then a virtual
        /// directory aggregating all input paths is returned.
        /// </param>
        /// <returns>An input directory.</returns>
        public static async Task<IDirectory> GetInputDirectoryAsync(this IReadOnlyFileSystem fileSystem, DirectoryPath path = null) =>
           path == null
               ? new VirtualInputDirectory(fileSystem, ".")
               : (path.IsRelative ? new VirtualInputDirectory(fileSystem, path) : await fileSystem.GetDirectoryAsync(path));

        /// <summary>
        /// Gets matching files based on globbing patterns from the root path or absolute paths.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="patterns">The globbing patterns and/or absolute paths.</param>
        /// <returns>
        /// All files in the specified directory that match the globbing patterns and/or absolute paths.
        /// </returns>
        public static async Task<IEnumerable<IFile>> GetFilesAsync(this IReadOnlyFileSystem fileSystem, params string[] patterns) =>
            await fileSystem.GetFilesAsync(await fileSystem.GetRootDirectoryAsync(), patterns);

        /// <summary>
        /// Gets matching files based on globbing patterns from the root path or absolute paths.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="patterns">The globbing patterns and/or absolute paths.</param>
        /// <returns>
        /// All files in the specified directory that match the globbing patterns and/or absolute paths.
        /// </returns>
        public static async Task<IEnumerable<IFile>> GetFilesAsync(this IReadOnlyFileSystem fileSystem, IEnumerable<string> patterns) =>
            await fileSystem.GetFilesAsync(await fileSystem.GetRootDirectoryAsync(), patterns);

        /// <summary>
        /// Gets matching files based on globbing patterns and/or absolute paths. If any absolute paths
        /// are provided, only those that actually exist are returned.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="directory">The directory to search.</param>
        /// <param name="patterns">The globbing patterns and/or absolute paths.</param>
        /// <returns>
        /// All files in the specified directory that match the globbing patterns and/or absolute paths.
        /// </returns>
        public static async Task<IEnumerable<IFile>> GetFilesAsync(this IReadOnlyFileSystem fileSystem, IDirectory directory, params string[] patterns) =>
            await fileSystem.GetFilesAsync(directory, (IEnumerable<string>)patterns);

        /// <summary>
        /// Gets matching files based on globbing patterns and/or absolute paths. If any absolute paths
        /// are provided, only those that actually exist are returned.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="directory">The directory to search.</param>
        /// <param name="patterns">The globbing patterns and/or absolute paths.</param>
        /// <returns>
        /// All files in the specified directory that match the globbing patterns and/or absolute paths.
        /// </returns>
        public static async Task<IEnumerable<IFile>> GetFilesAsync(this IReadOnlyFileSystem fileSystem, IDirectory directory, IEnumerable<string> patterns)
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
                        IDirectory rootDirectory = await fileSystem.GetDirectoryAsync(filePath.Root);
                        FilePath relativeFilePath = filePath.RootRelative.Collapse();
                        return Tuple.Create(
                            rootDirectory,
                            negated ? ('!' + relativeFilePath.FullPath) : relativeFilePath.FullPath);
                    }
                    return Tuple.Create(directory, x);
                });
            IEnumerable<IGrouping<IDirectory, string>> patternGroups = directoryPatterns
                .GroupBy(x => x.Item1, x => x.Item2, new DirectoryEqualityComparer());
            return await patternGroups.SelectManyAsync(async x => await Globber.GetFilesAsync(x.Key, x));
        }
    }
}
