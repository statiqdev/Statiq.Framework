using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;
using Wyam.Common.IO;

namespace Wyam.Common.Documents.Content
{
    /// <summary>
    /// A content provider for files.
    /// </summary>
    internal class FileContent : IContentProvider
    {
        private readonly IFile _file;

        public FileContent(IFile file)
        {
            _file = file ?? throw new ArgumentException();
        }

        public void Dispose()
        {
            // Nothing to do
        }

        public Task<Stream> GetStreamAsync() => _file.OpenReadAsync();
    }
}
