using System.IO;
using System.Threading.Tasks;
using dotless.Core.Input;
using Statiq.Common;

namespace Statiq.Less
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
            using (Stream stream = file.OpenRead().Result)
            {
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, (int)stream.Length);
                return buffer;
            }
        }

        public string GetFileContents(string fileName) => GetInputFileAsync(fileName).Result.ReadAllTextAsync().Result;

        public bool DoesFileExist(string fileName) => GetInputFileAsync(fileName).Result.GetExists().Result;

        public bool UseCacheDependencies => true;

        private async Task<IFile> GetInputFileAsync(FilePath filePath)
        {
            // Find the requested file
            // ...as specified
            IFile file = await _fileSystem.GetInputFile(filePath);
            if (await file.GetExists())
            {
                return file;
            }

            // ...with extension (if not already)
            if (!filePath.HasExtension || filePath.Extension != ".less")
            {
                FilePath extensionPath = filePath.AppendExtension(".less");
                IFile extensionFile = await _fileSystem.GetInputFile(extensionPath);
                if (await extensionFile.GetExists())
                {
                    return extensionFile;
                }

                // ...and with underscore prefix (if not already)
                if (!extensionPath.FileName.FullPath.StartsWith("_"))
                {
                    extensionPath = extensionPath.ChangeFileName("_" + extensionPath.FileName.FullPath);
                    extensionFile = await _fileSystem.GetInputFile(extensionPath);
                    if (await extensionFile.GetExists())
                    {
                        return extensionFile;
                    }
                }
            }

            // ...with underscore prefix (if not already)
            if (!filePath.FileName.FullPath.StartsWith("_"))
            {
                filePath = filePath.ChangeFileName("_" + filePath.FileName.FullPath);
                IFile underscoreFile = await _fileSystem.GetInputFile(filePath);
                if (await underscoreFile.GetExists())
                {
                    return underscoreFile;
                }
            }

            // Can't find it, default to the original
            return file;
        }
    }
}