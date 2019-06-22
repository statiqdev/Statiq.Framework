using System;
using System.Linq;

namespace Statiq.Common.IO
{
    /// <summary>
    /// Represents a directory path.
    /// </summary>
    public sealed class DirectoryPath : NormalizedPath
    {
        // Initially based on code from Cake (http://cakebuild.net/)

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryPath"/> class.
        /// The path will be considered absolute if the underlying OS file system
        /// considers it absolute.
        /// </summary>
        /// <param name="path">The path.</param>
        public DirectoryPath(string path)
            : base(path, PathKind.RelativeOrAbsolute)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryPath"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="pathKind">Specifies whether the path is relative, absolute, or indeterminate.</param>
        public DirectoryPath(string path, PathKind pathKind)
            : base(path, pathKind)
        {
        }

        /// <summary>
        /// Gets the name of the directory.
        /// </summary>
        /// <value>The directory name.</value>
        /// <remarks>
        /// If this is passed a file path, it will return the file name.
        /// This is by-and-large equivalent to how DirectoryInfo handles this scenario.
        /// If we wanted to return the *actual* directory name, we'd need to pull in IFileSystem,
        /// and do various checks to make sure things exists.
        /// </remarks>
        public string Name => Segments.Length == 0 ? FullPath : Segments.Last().ToString();

        /// <summary>
        /// Gets the parent path or <c>null</c> if this is a root path.
        /// </summary>
        /// <value>
        /// The parent path or <c>null</c> if this is a root path.
        /// </value>
        public DirectoryPath Parent
        {
            get
            {
                string directory = System.IO.Path.GetDirectoryName(FullPath);
                if (string.IsNullOrWhiteSpace(directory))
                {
                    return null;
                }
                return new DirectoryPath(directory);
            }
        }

        /// <summary>
        /// Gets current path relative to it's root. If this is already a relative
        /// path or there is no root path, this just returns the current path.
        /// </summary>
        /// <value>
        /// The current path relative to it's root.
        /// </value>
        public DirectoryPath RootRelative
        {
            get
            {
                if (!IsAbsolute)
                {
                    return this;
                }
                DirectoryPath root = Root;
                return root.FullPath == Dot
                    ? this
                    : new DirectoryPath(FullPath.Substring(root.FullPath.Length), PathKind.Relative);
            }
        }

        /// <summary>
        /// Combines the current path with the file name of a <see cref="FilePath"/>.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A combination of the current path and the file name of the provided <see cref="FilePath"/>.</returns>
        public FilePath GetFilePath(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            return new FilePath(System.IO.Path.Combine(FullPath, path.FileName.FullPath));
        }

        /// <summary>
        /// Get the relative path to another directory. If this path and the target path
        /// do not share the same file provider, the target path is returned.
        /// </summary>
        /// <param name="target">The target directory path.</param>
        /// <returns>A <see cref="DirectoryPath"/>.</returns>
        public DirectoryPath GetRelativePath(DirectoryPath target) => RelativePathResolver.Resolve(this, target);

        /// <summary>
        /// Get the relative path to another file.
        /// </summary>
        /// <param name="target">The target file path.</param>
        /// <returns>A <see cref="FilePath"/>.</returns>
        public FilePath GetRelativePath(FilePath target) => RelativePathResolver.Resolve(this, target);

        /// <summary>
        /// Combines the current path with a <see cref="FilePath"/>.
        /// If the provided <see cref="FilePath"/> is not relative, then it is returned.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A combination of the current path and the provided <see cref="FilePath"/>, unless
        /// the provided <see cref="FilePath"/> is absolute in which case it is returned.</returns>
        public FilePath CombineFile(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            return !path.IsRelative ? path : new FilePath(System.IO.Path.Combine(FullPath, path.FullPath));
        }

        /// <summary>
        /// Combines the current path with another <see cref="DirectoryPath"/>.
        /// If the provided <see cref="DirectoryPath"/> is not relative, then it is returned.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A combination of the current path and the provided <see cref="DirectoryPath"/>, unless
        /// the provided <see cref="DirectoryPath"/> is absolute in which case it is returned.</returns>
        public DirectoryPath Combine(DirectoryPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            return !path.IsRelative ? path : new DirectoryPath(System.IO.Path.Combine(FullPath, path.FullPath));
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="string"/> to <see cref="DirectoryPath"/>.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A <see cref="DirectoryPath"/>.</returns>
        public static implicit operator DirectoryPath(string path) => path == null ? null : new DirectoryPath(path);
    }
}
