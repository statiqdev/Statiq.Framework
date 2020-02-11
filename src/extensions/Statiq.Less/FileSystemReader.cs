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
            IFile file = GetInputFile(fileName);
            using (Stream stream = file.OpenRead())
            {
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, (int)stream.Length);
                return buffer;
            }
        }

        public string GetFileContents(string fileName) => GetInputFile(fileName).ReadAllTextAsync().GetAwaiter().GetResult();

        public bool DoesFileExist(string fileName) => GetInputFile(fileName).Exists;

        public bool UseCacheDependencies => true;

        private IFile GetInputFile(FilePath filePath)
        {
            // Find the requested file
            // ...as specified
            IFile file = _fileSystem.GetInputFile(filePath);
            if (file.Exists)
            {
                return file;
            }

            // ...with extension (if not already)
            if (!filePath.HasExtension || filePath.Extension != ".less")
            {
                FilePath extensionPath = filePath.AppendExtension(".less");
                IFile extensionFile = _fileSystem.GetInputFile(extensionPath);
                if (extensionFile.Exists)
                {
                    return extensionFile;
                }

                // ...and with underscore prefix (if not already)
                if (!extensionPath.FileName.FullPath.StartsWith("_"))
                {
                    extensionPath = extensionPath.ChangeFileName("_" + extensionPath.FileName.FullPath);
                    extensionFile = _fileSystem.GetInputFile(extensionPath);
                    if (extensionFile.Exists)
                    {
                        return extensionFile;
                    }
                }
            }

            // ...with underscore prefix (if not already)
            if (!filePath.FileName.FullPath.StartsWith("_"))
            {
                filePath = filePath.ChangeFileName("_" + filePath.FileName.FullPath);
                IFile underscoreFile = _fileSystem.GetInputFile(filePath);
                if (underscoreFile.Exists)
                {
                    return underscoreFile;
                }
            }

            // Can't find it, default to the original
            return file;
        }
    }
}