using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common.Util;

namespace Wyam.Common.Content
{
    /// <summary>
    /// Wraps another stream and releases a mutex when it's disposed.
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
            base.Dispose(disposing);
            _mutex?.Release();
        }
    }
}
