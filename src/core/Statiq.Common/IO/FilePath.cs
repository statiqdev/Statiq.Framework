using System;

namespace Statiq.Common
{
    /// <summary>
    /// Represents a file path.
    /// </summary>
    public sealed class FilePath : NormalizedPath
    {
        // Initially based on code from Cake (http://cakebuild.net/)

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePath"/> class.
        /// The path will be considered absolute if the underlying OS file system
        /// considers it absolute.
        /// </summary>
        /// <param name="path">The path.</param>
        public FilePath(string path)
            : base(path, PathKind.RelativeOrAbsolute)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePath"/> class..
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="pathKind">Specifies whether the path is relative, absolute, or indeterminate.</param>
        public FilePath(string path, PathKind pathKind)
            : base(path, pathKind)
        {
        }

        /// <summary>
        /// Gets a value indicating whether this path has a file extension.
        /// </summary>
        /// <value>
        /// <c>true</c> if this file path has a file extension; otherwise, <c>false</c>.
        /// </value>
        public bool HasExtension => System.IO.Path.HasExtension(FullPath);

        /// <summary>
        /// Gets the directory part of the path.
        /// </summary>
        /// <value>The directory part of the path.</value>
        public DirectoryPath Directory
        {
            get
            {
                string directory = System.IO.Path.GetDirectoryName(FullPath);
                if (string.IsNullOrWhiteSpace(directory))
                {
                    directory = Dot;
                }
                return new DirectoryPath(directory);
            }
        }

        /// <summary>
        /// Gets the file path relative to it's root path.
        /// </summary>
        public FilePath RootRelative
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
                    : new FilePath(FullPath.Substring(root.FullPath.Length), PathKind.Relative);
            }
        }

        /// <summary>
        /// Gets the filename.
        /// </summary>
        /// <value>The filename.</value>
        public FilePath FileName =>
            new FilePath(System.IO.Path.GetFileName(FullPath));

        /// <summary>
        /// Gets the filename without it's extension.
        /// </summary>
        /// <value>The filename without it's extension, or <c>null</c> if the file has no name.</value>
        public FilePath FileNameWithoutExtension
        {
            get
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(FullPath);
                return string.IsNullOrEmpty(fileName) ? null : new FilePath(System.IO.Path.GetFileNameWithoutExtension(FullPath));
            }
        }

        /// <summary>
        /// Gets the file extension (including the preceding ".").
        /// </summary>
        /// <value>The file extension (including the preceding ".").</value>
        public string Extension
        {
            get
            {
                string extension = System.IO.Path.GetExtension(FullPath);
                return string.IsNullOrWhiteSpace(extension) ? null : extension;
            }
        }

        /// <summary>
        /// Changes the file extension of the path.
        /// </summary>
        /// <param name="extension">The new extension.</param>
        /// <returns>A new <see cref="FilePath"/> with a new extension.</returns>
        public FilePath ChangeExtension(string extension) =>
            new FilePath(System.IO.Path.ChangeExtension(FullPath, extension));

        /// <summary>
        /// Changes the file name of the path by combining the specified path with the <see cref="Directory"/>.
        /// </summary>
        /// <param name="filePath">The path to combine with the <see cref="Directory"/>.</param>
        /// <returns>A new path with the specified path replacing the current file name.</returns>
        public FilePath ChangeFileName(FilePath filePath) => Directory.CombineFile(filePath);

        /// <summary>
        /// Appends a file extension to the path.
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <returns>A new <see cref="FilePath"/> with an appended extension.</returns>
        public FilePath AppendExtension(string extension)
        {
            _ = extension ?? throw new ArgumentNullException(nameof(extension));

            if (!extension.StartsWith(Dot, StringComparison.OrdinalIgnoreCase))
            {
                extension = string.Concat(Dot, extension);
            }
            return new FilePath(string.Concat(FullPath, extension));
        }

        /// <summary>
        /// Inserts a suffix into the file name before the extension.
        /// </summary>
        /// <param name="suffix">The suffix to insert.</param>
        /// <returns>A new <see cref="FilePath"/> with the specified suffix.</returns>
        public FilePath InsertSuffix(string suffix)
        {
            _ = suffix ?? throw new ArgumentNullException(nameof(suffix));

            int extensionIndex = FullPath.LastIndexOf(Dot);
            return extensionIndex == -1
                ? new FilePath(string.Concat(FullPath, suffix))
                : new FilePath(string.Concat(FullPath.Substring(0, extensionIndex), suffix, FullPath.Substring(extensionIndex)));
        }

        /// <summary>
        /// Inserts a prefix into the file name.
        /// </summary>
        /// <param name="prefix">The prefix to insert.</param>
        /// <returns>A new <see cref="FilePath"/> with the specified prefix.</returns>
        public FilePath InsertPrefix(string prefix)
        {
            _ = prefix ?? throw new ArgumentNullException(nameof(prefix));

            int nameIndex = FullPath.LastIndexOf(Slash);
            if (nameIndex == -1)
            {
                return new FilePath(string.Concat(prefix, FullPath));
            }
            return new FilePath(string.Concat(FullPath.Substring(0, nameIndex + 1), prefix, FullPath.Substring(nameIndex + 1)));
        }

        /// <summary>
        /// Gets path to this file relative to it's containing input directory in the current file system.
        /// If no input directories contain this file, then the file name is returned.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <returns>A path to this file relative to it's containing input directory in the current file system.</returns>
        public FilePath GetRelativeInputPath(IExecutionContext context)
        {
            if (IsRelative)
            {
                return this;
            }

            return context.FileSystem.GetContainingInputPathForAbsolutePath(this)?.GetRelativePath(this) ?? FileName;
        }

        /// <summary>
        /// Gets path to this file relative to the output directory in the current file system.
        /// If this path is not relative to the output directory, then the file name is returned.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <returns>A path to this file relative to it's containing input directory in the current file system.</returns>
        public FilePath GetRelativeOutputPath(IExecutionContext context)
        {
            if (IsRelative)
            {
                return this;
            }

            return Directory.Segments.StartsWith(context.FileSystem.OutputPath.Segments)
                ? context.FileSystem.OutputPath.GetRelativePath(this)
                : FileName;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="string"/> to <see cref="FilePath"/>.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A <see cref="FilePath"/>.</returns>
        public static implicit operator FilePath(string path) => path == null ? null : new FilePath(path);
    }
}
