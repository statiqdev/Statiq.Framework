using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Common
{
    public partial interface IReadOnlyFileSystem
    {
        /// <summary>
        /// Gets a file representing an input.
        /// </summary>
        /// <param name="path">
        /// The path of the input file. If this is an absolute path,
        /// then a file representing the specified path is returned.
        /// If it's a relative path, then operations will search all
        /// current input paths.
        /// </param>
        /// <returns>An input file.</returns>
        public IFile GetInputFile(FilePath path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));

            if (path.IsRelative)
            {
                IFile notFound = null;
                foreach (DirectoryPath inputPath in InputPaths.Reverse())
                {
                    IFile file = GetFile(RootPath.Combine(inputPath).CombineFile(path));
                    if (notFound == null)
                    {
                        notFound = file;
                    }
                    if (file.Exists)
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
            return GetFile(path);
        }

        /// <summary>
        /// Gets matching input files based on globbing patterns and/or absolute paths. If any absolute paths
        /// are provided, only those that actually exist are returned.
        /// </summary>
        /// <param name="patterns">The globbing patterns and/or absolute paths.</param>
        /// <returns>All input files that match the globbing patterns and/or absolute paths.</returns>
        public IEnumerable<IFile> GetInputFiles(params string[] patterns) => GetInputFiles((IEnumerable<string>)patterns);

        /// <summary>
        /// Gets matching input files based on globbing patterns and/or absolute paths. If any absolute paths
        /// are provided, only those that actually exist are returned.
        /// </summary>
        /// <param name="patterns">The globbing patterns and/or absolute paths.</param>
        /// <returns>All input files that match the globbing patterns and/or absolute paths.</returns>
        public IEnumerable<IFile> GetInputFiles(IEnumerable<string> patterns) => GetFiles(GetInputDirectory(), patterns);

        /// <summary>
        /// Gets all absolute input directories.
        /// </summary>
        /// <returns>The absolute input directories.</returns>
        public IEnumerable<IDirectory> GetInputDirectories() => InputPaths.Select(x => GetRootDirectory(x)).ToImmutableArray();

        /// <summary>
        /// Gets the absolute input path that contains the specified file or directory. If the provided
        /// file or directory path is absolute, this returns the input path that contains the specified
        /// path (note that the specified file or directory does not need to exist and this just returns
        /// the input path that would contain the file or directory based only on path information). If
        /// the provided path is relative, this checks all input paths for the existence of the file
        /// or directory and returns the first one where it exists.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>The input path that contains the specified file,
        /// or <c>null</c> if no input path does.</returns>
        public DirectoryPath GetContainingInputPath(NormalizedPath path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));

            if (path.IsAbsolute)
            {
                return GetContainingInputPathForAbsolutePath(path);
            }

            if (path is FilePath filePath)
            {
                IFile file = GetInputFile(filePath);
                return file.Exists ? GetContainingInputPath(file.Path) : null;
            }
            if (path is DirectoryPath directoryPath)
            {
                IEnumerable<(DirectoryPath x, IDirectory)> rootDirectories =
                    InputPaths
                        .Reverse()
                        .Select(x => (x, GetRootDirectory(x.Combine(directoryPath))));
                IEnumerable<(DirectoryPath x, IDirectory)> existingRootDirectories = rootDirectories.Where(x => x.Item2.Exists);
                return existingRootDirectories.Select(x => RootPath.Combine(x.Item1)).FirstOrDefault();
            }

            return null;
        }

        internal DirectoryPath GetContainingInputPathForAbsolutePath(NormalizedPath path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));

            if (!path.IsAbsolute)
            {
                throw new ArgumentException("Path must be absolute");
            }

            return InputPaths
                .Reverse()
                .Select(x => RootPath.Combine(x))
                .FirstOrDefault(x => path.Segments.StartsWith(x.Segments));
        }

        /// <summary>
        /// Gets an output file path by combining it with the root path and output path.
        /// </summary>
        /// <param name="path">The path to combine with the root path and output path.</param>
        /// <returns>The output file path.</returns>
        public FilePath GetOutputPath(FilePath path) =>
            RootPath.Combine(OutputPath).CombineFile(path ?? throw new ArgumentNullException(nameof(path)));

        /// <summary>
        /// Gets an output directory path by combining it with the root path and output path.
        /// </summary>
        /// <param name="path">The path to combine with the root path and output path.
        /// If this is <c>null</c>, returns the root path combined with the output path.</param>
        /// <returns>The output directory path.</returns>
        public DirectoryPath GetOutputPath(DirectoryPath path = null) =>
            path == null
                ? RootPath.Combine(OutputPath)
                : RootPath.Combine(OutputPath).Combine(path);

        /// <summary>
        /// Gets a file representing an output.
        /// </summary>
        /// <param name="path">
        /// The path of the output file. If this is an absolute path,
        /// then a file representing the specified path is returned.
        /// If it's a relative path, then it will be combined with the
        /// current output path.
        /// </param>
        /// <returns>An output file.</returns>
        public IFile GetOutputFile(FilePath path) => GetFile(GetOutputPath(path));

        /// <summary>
        /// Gets a directory representing an output.
        /// </summary>
        /// <param name="path">
        /// The path of the output directory. If this is an absolute path,
        /// then a directory representing the specified path is returned.
        /// If it's a relative path, then it will be combined with the
        /// current output path. If this is <c>null</c> then the base
        /// output directory is returned.
        /// </param>
        /// <returns>An output directory.</returns>
        public IDirectory GetOutputDirectory(DirectoryPath path = null) => GetDirectory(GetOutputPath(path));

        /// <summary>
        /// Gets a temp file path by combining it with the root path and temp path.
        /// </summary>
        /// <param name="path">The path to combine with the root path and temp path.</param>
        /// <returns>The temp file path.</returns>
        public FilePath GetTempPath(FilePath path) =>
            RootPath.Combine(TempPath).CombineFile(path ?? throw new ArgumentNullException(nameof(path)));

        /// <summary>
        /// Gets a temp directory path by combining it with the root path and temp path.
        /// </summary>
        /// <param name="path">The path to combine with the root path and temp path.
        /// If this is <c>null</c>, returns the root path combined with the temp path.</param>
        /// <returns>The temp directory path.</returns>
        public DirectoryPath GetTempPath(DirectoryPath path = null) =>
            path == null
                ? RootPath.Combine(TempPath)
                : RootPath.Combine(TempPath).Combine(path);

        /// <summary>
        /// Gets a file representing a temp file.
        /// </summary>
        /// <param name="path">
        /// If this is an absolute path,
        /// then a file representing the specified path is returned.
        /// If it's a relative path, then it will be combined with the
        /// current temp path.
        /// </param>
        /// <returns>A temp file.</returns>
        public IFile GetTempFile(FilePath path) => GetFile(GetTempPath(path));

        /// <summary>
        /// Gets a file representing a temp file with a random file name.
        /// </summary>
        /// <returns>A temp file.</returns>
        public IFile GetTempFile() => GetTempFile(Path.ChangeExtension(Path.GetRandomFileName(), "tmp"));

        /// <summary>
        /// Gets a directory representing temp files.
        /// </summary>
        /// <param name="path">
        /// The path of the temp directory. If this is an absolute path,
        /// then a directory representing the specified path is returned.
        /// If it's a relative path, then it will be combined with the
        /// current temp path. If this is <c>null</c> then the base
        /// temp directory is returned.
        /// </param>
        /// <returns>A temp directory.</returns>
        public IDirectory GetTempDirectory(DirectoryPath path = null) => GetDirectory(GetTempPath(path));

        /// <summary>
        /// Gets a file representing a root file.
        /// </summary>
        /// <param name="path">
        /// The path of the root file. If this is an absolute path,
        /// then a file representing the specified path is returned.
        /// If it's a relative path, then it will be combined with the
        /// current root path.
        /// </param>
        /// <returns>A root file.</returns>
        public IFile GetRootFile(FilePath path) =>
            GetFile(RootPath.CombineFile(path ?? throw new ArgumentNullException(nameof(path))));

        /// <summary>
        /// Gets a directory representing a root directory.
        /// </summary>
        /// <param name="path">
        /// The path of the root directory. If this is an absolute path,
        /// then a directory representing the specified path is returned.
        /// If it's a relative path, then it will be combined with the
        /// current root path. If this is <c>null</c> then the base
        /// root directory is returned.
        /// </param>
        /// <returns>A root directory.</returns>
        public IDirectory GetRootDirectory(DirectoryPath path = null) =>
            path == null
            ? GetDirectory(RootPath)
            : GetDirectory(RootPath.Combine(path));

        /// <summary>
        /// Gets an absolute file.
        /// </summary>
        /// <param name="path">
        /// The absolute path of the file.
        /// </param>
        /// <returns>A file.</returns>
        public IFile GetFile(FilePath path) =>
            FileProvider.GetFile(path ?? throw new ArgumentNullException(nameof(path)));

        /// <summary>
        /// Gets an absolute directory.
        /// </summary>
        /// <param name="path">
        /// The absolute path of the directory.
        /// </param>
        /// <returns>A directory.</returns>
        public IDirectory GetDirectory(DirectoryPath path) =>
            FileProvider.GetDirectory(path ?? throw new ArgumentNullException(nameof(path)));

        /// <summary>
        /// Gets a directory representing an input.
        /// </summary>
        /// <param name="path">
        /// The path of the input directory. If this is an absolute path,
        /// then a directory representing the specified path is returned.
        /// If it's a relative path, then the returned directory will
        /// be a virtual directory that aggregates all input
        /// paths. If this is <c>null</c> then a virtual
        /// directory aggregating all input paths is returned.
        /// </param>
        /// <returns>An input directory.</returns>
        public IDirectory GetInputDirectory(DirectoryPath path = null) =>
           path == null
               ? new VirtualInputDirectory(this, ".")
               : (path.IsRelative ? new VirtualInputDirectory(this, path) : GetDirectory(path));

        /// <summary>
        /// Gets matching files based on globbing patterns from the root path or absolute paths.
        /// </summary>
        /// <param name="patterns">The globbing patterns and/or absolute paths.</param>
        /// <returns>
        /// All files in the specified directory that match the globbing patterns and/or absolute paths.
        /// </returns>
        public IEnumerable<IFile> GetFiles(params string[] patterns) => GetFiles(GetRootDirectory(), patterns);

        /// <summary>
        /// Gets matching files based on globbing patterns from the root path or absolute paths.
        /// </summary>
        /// <param name="patterns">The globbing patterns and/or absolute paths.</param>
        /// <returns>
        /// All files in the specified directory that match the globbing patterns and/or absolute paths.
        /// </returns>
        public IEnumerable<IFile> GetFiles(IEnumerable<string> patterns) => GetFiles(GetRootDirectory(), patterns);

        /// <summary>
        /// Gets matching files based on globbing patterns and/or absolute paths. If any absolute paths
        /// are provided, only those that actually exist are returned.
        /// </summary>
        /// <param name="directory">The directory to search.</param>
        /// <param name="patterns">The globbing patterns and/or absolute paths.</param>
        /// <returns>
        /// All files in the specified directory that match the globbing patterns and/or absolute paths.
        /// </returns>
        public IEnumerable<IFile> GetFiles(IDirectory directory, params string[] patterns) =>
            GetFiles(directory, (IEnumerable<string>)patterns);

        /// <summary>
        /// Gets matching files based on globbing patterns and/or absolute paths. If any absolute paths
        /// are provided, only those that actually exist are returned.
        /// </summary>
        /// <param name="directory">The directory to search.</param>
        /// <param name="patterns">The globbing patterns and/or absolute paths.</param>
        /// <returns>
        /// All files in the specified directory that match the globbing patterns and/or absolute paths.
        /// </returns>
        public IEnumerable<IFile> GetFiles(IDirectory directory, IEnumerable<string> patterns)
        {
            _ = directory ?? throw new ArgumentNullException(nameof(directory));

            IEnumerable<Tuple<IDirectory, string>> directoryPatterns = patterns
                .Where(x => x != null)
                .Select(x =>
                {
                    bool negated = x[0] == '!';
                    FilePath filePath = negated ? new FilePath(x.Substring(1)) : new FilePath(x);
                    if (filePath.IsAbsolute)
                    {
                        // The globber doesn't support absolute paths, so get the root directory of this path
                        IDirectory rootDirectory = GetDirectory(filePath.Root);
                        FilePath relativeFilePath = filePath.RootRelative;
                        return Tuple.Create(
                            rootDirectory,
                            negated ? ('!' + relativeFilePath.FullPath) : relativeFilePath.FullPath);
                    }
                    return Tuple.Create(directory, x);
                });
            IEnumerable<IGrouping<IDirectory, string>> patternGroups = directoryPatterns
                .GroupBy(x => x.Item1, x => x.Item2, new DirectoryEqualityComparer());
            return patternGroups.SelectMany(x => Globber.GetFiles(x.Key, x));
        }
    }
}
