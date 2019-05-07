using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Util;

namespace Wyam.Common.Documents.Content
{
    /// <summary>
    /// A content provider for files.
    /// </summary>
    internal class StringFileContent : IContentProvider
    {
        private readonly IFile _file;

        public StringFileContent(IFile file)
        {
            _file = file ?? throw new ArgumentException();
        }

        public void Dispose()
        {
            if (_file.GetExistsAsync().Result)
            {
                _file.DeleteAsync().Wait();
            }
        }

        public Task<Stream> GetStreamAsync() => _file.OpenReadAsync();
    }
}
