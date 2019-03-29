using System.IO;
using System.Threading.Tasks;
using dotless.Core.Input;
using Wyam.Common.IO;

namespace Wyam.Less
{
    internal class FileSystemReader : IFileReader
    {
        private readonly IReadOnlyFileSystem _fileSystem;

        public FileSystemReader(IReadOnlyFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public byte[] GetBinaryFileContents(string fileName)
        {
            IFile file = GetInputFileAsync(fileName).Result;
            using (Stream stream = file.OpenReadAsync().Result)
            {
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, (int)stream.Length);
                return buffer;
            }
        }

        public string GetFileContents(string fileName) => GetInputFileAsync(fileName).Result.ReadAllTextAsync().Result;

        public bool DoesFileExist(string fileName) => GetInputFileAsync(fileName).Result.GetExistsAsync().Result;

        public bool UseCacheDependencies => true;

        private async Task<IFile> GetInputFileAsync(FilePath filePath)
        {
            // Find the requested file
            // ...as specified
            IFile file = await _fileSystem.GetInputFileAsync(filePath);
            if (await file.GetExistsAsync())
            {
                return file;
            }

            // ...with extension (if not already)
            if (!filePath.HasExtension || filePath.Extension != ".less")
            {
                FilePath extensionPath = filePath.AppendExtension(".less");
                IFile extensionFile = await _fileSystem.GetInputFileAsync(extensionPath);
                if (await extensionFile.GetExistsAsync())
                {
                    return extensionFile;
                }

                // ...and with underscore prefix (if not already)
                if (!extensionPath.FileName.FullPath.StartsWith("_"))
                {
                    extensionPath = extensionPath.Directory.CombineFile("_" + extensionPath.FileName.FullPath);
                    extensionFile = await _fileSystem.GetInputFileAsync(extensionPath);
                    if (await extensionFile.GetExistsAsync())
                    {
                        return extensionFile;
                    }
                }
            }

            // ...with underscore prefix (if not already)
            if (!filePath.FileName.FullPath.StartsWith("_"))
            {
                filePath = filePath.Directory.CombineFile("_" + filePath.FileName.FullPath);
                IFile underscoreFile = await _fileSystem.GetInputFileAsync(filePath);
                if (await underscoreFile.GetExistsAsync())
                {
                    return underscoreFile;
                }
            }

            // Can't find it, default to the original
            return file;
        }
    }
}