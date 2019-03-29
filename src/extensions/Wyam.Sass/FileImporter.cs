using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Wyam.Common.Execution;
using Wyam.Common.IO;

namespace Wyam.Sass
{
    internal class FileImporter
    {
        // Maps each parent path to the containing path for use in nested imports
        // since the parent path may be relative in those cases
        private readonly ConcurrentDictionary<FilePath, FilePath> _parentAbsolutePaths
            = new ConcurrentDictionary<FilePath, FilePath>();

        private readonly IReadOnlyFileSystem _fileSystem;
        private readonly Func<string, string> _importPathFunc;

        public FileImporter(IReadOnlyFileSystem fileSystem, Func<string, string> importPathFunc)
        {
            _fileSystem = fileSystem;
            _importPathFunc = importPathFunc;
        }

        // This is a TryImportDelegate which unfortunately isn't async
        public bool TryImport(string requestedFile, string parentPath, out string scss, out string map)
        {
            scss = TryImportAsync(requestedFile, parentPath).Result;
            map = null;
            return scss != null;
        }

        public async Task<string> TryImportAsync(string requestedFile, string parentPath)
        {
            // Modify the requested file if we have an import path function
            string modifiedParentPath = null;
            if (_importPathFunc != null)
            {
                requestedFile = _importPathFunc(requestedFile);
                modifiedParentPath = _importPathFunc(parentPath);
            }
            if (string.IsNullOrWhiteSpace(requestedFile))
            {
                return null;
            }

            // Get the input relative path to the parent file
            // Make sure to try checking for a previously processed parent post-modification
            FilePath parentFilePath = new FilePath(parentPath);
            FilePath requestedFilePath = new FilePath(requestedFile);
            if (parentFilePath.IsRelative
                && !_parentAbsolutePaths.TryGetValue(parentFilePath, out parentFilePath)
                && (modifiedParentPath == null || !_parentAbsolutePaths.TryGetValue(modifiedParentPath, out parentFilePath)))
            {
                // Relative parent path and no available absolute path, try with the relative path
                parentFilePath = new FilePath(parentPath);
            }

            // Try to get the relative path to the parent file from inside the input virtual file system
            // But if the parent file isn't under an input path, just use it directly
            DirectoryPath containingInputPath = await _fileSystem.GetContainingInputPathAsync(parentFilePath);
            FilePath parentRelativePath = containingInputPath != null
                ? containingInputPath.GetRelativePath(parentFilePath)
                : parentFilePath;

            // Find the requested file by first combining with the parent
            FilePath filePath = parentRelativePath.Directory.CombineFile(requestedFilePath);
            string scss = await GetFileVariationsAsync(filePath, requestedFilePath);
            if (scss != null)
            {
                return scss;
            }

            // That didn't work so try it again as a relative path from the input folder
            scss = await GetFileVariationsAsync(requestedFilePath, requestedFilePath);
            if (!requestedFilePath.IsAbsolute && scss != null)
            {
                return scss;
            }

            return null;
        }

        private async Task<string> GetFileVariationsAsync(FilePath filePath, FilePath requestedFilePath)
        {
            // ...as specified
            string scss = await GetFileAsync(filePath, requestedFilePath);
            if (scss != null)
            {
                return scss;
            }

            // ...with extension (if not already)
            if (!filePath.HasExtension || filePath.Extension != ".scss")
            {
                FilePath extensionPath = filePath.AppendExtension(".scss");
                scss = await GetFileAsync(extensionPath, requestedFilePath);
                if (scss != null)
                {
                    return scss;
                }

                // ...and with underscore prefix (if not already)
                if (!extensionPath.FileName.FullPath.StartsWith("_"))
                {
                    extensionPath = extensionPath.Directory.CombineFile("_" + extensionPath.FileName.FullPath);
                    scss = await GetFileAsync(extensionPath, requestedFilePath);
                    if (scss != null)
                    {
                        return scss;
                    }
                }
            }

            // ...with underscore prefix (if not already)
            if (!filePath.FileName.FullPath.StartsWith("_"))
            {
                filePath = filePath.Directory.CombineFile("_" + filePath.FileName.FullPath);
                scss = await GetFileAsync(filePath, requestedFilePath);
                if (scss != null)
                {
                    return scss;
                }
            }

            return null;
        }

        private async Task<string> GetFileAsync(FilePath filePath, FilePath requestedFilePath)
        {
            string scss = null;
            IFile file = await _fileSystem.GetInputFileAsync(filePath);
            if (await file.GetExistsAsync())
            {
                if (requestedFilePath.IsRelative)
                {
                    _parentAbsolutePaths.AddOrUpdate(requestedFilePath, file.Path, (x, y) => file.Path);
                }
                scss = await file.ReadAllTextAsync();
            }
            return scss;
        }
    }
}