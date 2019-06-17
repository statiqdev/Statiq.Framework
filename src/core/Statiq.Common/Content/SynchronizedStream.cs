using System.IO;
using System.Threading;
using Statiq.Common.Util;

namespace Statiq.Common.Content
{
    /// <summary>
    /// Wraps another stream and releases a mutex when it's disposed,
    /// but doesn't dispose the underlying stream.
    /// </summary>
    internal class SynchronizedStream : DelegatingStream
    {
        private readonly SemaphoreSlim _mutex;

        public SynchronizedStream(Stream stream, SemaphoreSlim mutex)
            : base(stream)
        {
            _mutex = mutex;
        }

        protected override void Dispose(bool disposing)
        {
            _mutex.Release();
        }
    }
}
