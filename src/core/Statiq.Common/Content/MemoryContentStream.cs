using System;
using System.IO;
using Statiq.Common;

namespace Statiq.Common
{
    internal class MemoryContentStream : ContentStream
    {
        private MemoryStream _memoryStream;
        private byte[] _buffer;

        public MemoryContentStream(MemoryStream memoryStream)
            : base(memoryStream)
        {
            _memoryStream = memoryStream;
        }

        protected override void Dispose(bool disposing)
        {
            if (_memoryStream == null)
            {
                throw new ObjectDisposedException(nameof(MemoryContentStream));
            }

            // Copy the buffer out of the MemoryStream before disposing it
            _buffer = _memoryStream.ToArray();
            _memoryStream.Dispose();
            _memoryStream = null;
        }

        public override IContentProvider GetContentProvider(string mediaType) =>
            _memoryStream == null
                ? new MemoryContent(_buffer, mediaType)
                : new MemoryContent(_memoryStream.ToArray(), mediaType);
    }
}
