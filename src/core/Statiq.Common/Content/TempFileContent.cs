using System;
using System.IO;
using System.Threading.Tasks;
using Statiq.Common.IO;

namespace Statiq.Common.Content
{
    /// <summary>
    /// A content provider for temporary files that will delete the file when the content provider is disposed.
    /// </summary>
    public class TempFileContent : IContentProvider
    {
        private readonly IFile _file;

        public TempFileContent(IFile file)
        {
            _file = file ?? throw new ArgumentException();
        }

        public void Dispose()
        {
            Task.Run(async () =>
            {
                if (await _file.GetExistsAsync())
                {
                    await _file.DeleteAsync();
                }
            });
        }

        public Task<Stream> GetStreamAsync() => _file.OpenReadAsync();
    }
}
