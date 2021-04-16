using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Wraps a <see cref="FileStream"/> and logs that it was written to on disposal,
    /// but only if it was actually written to.
    /// </summary>
    internal class WrittenFileStream : DelegatingStream
    {
        private readonly LocalFileProvider _fileProvider;
        private readonly LocalFile _file;

        private bool _wrote;

        public WrittenFileStream(FileStream fileStream, LocalFileProvider fileProvider, LocalFile file)
            : base(fileStream)
        {
            _fileProvider = fileProvider.ThrowIfNull(nameof(fileProvider));
            _file = file.ThrowIfNull(nameof(file));
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            _wrote = true;
            return base.BeginWrite(buffer, offset, count, callback, state);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            _wrote = true;
            return base.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _wrote = true;
            base.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            _wrote = true;
            base.WriteByte(value);
        }

        protected override void Dispose(bool disposing)
        {
            Stream.Dispose();
            if (_wrote)
            {
                _fileProvider.WrittenFiles[_file.Path] = _file.GetCacheHashCode();
            }
        }

        public override async ValueTask DisposeAsync()
        {
            await Stream.DisposeAsync();
            if (_wrote)
            {
                _fileProvider.WrittenFiles[_file.Path] = _file.GetCacheHashCode();
            }
        }
    }
}
