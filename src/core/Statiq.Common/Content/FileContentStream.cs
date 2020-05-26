using System;
using System.IO;
using Statiq.Common;

namespace Statiq.Common
{
    internal class FileContentStream : ContentStream
    {
        private readonly IFile _file;

        public FileContentStream(IFile file)
            : base(file.Open())
        {
            _file = file;
        }

        protected override void Dispose(bool disposing) => Stream.Dispose();

        public override IContentProvider GetContentProvider(string mediaType) => new FileContent(_file, mediaType);
    }
}
