using System.IO;
using System.Text;
using Microsoft.IO;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Forwards calls to an underlying <see cref="RecyclableMemoryStreamManager"/>
    /// so that Statiq.Common doesn't have to maintain a reference to it.
    /// </summary>
    public class MemoryStreamFactory : IMemoryStreamFactory
    {
        private const int BlockSize = 16384;

        private readonly RecyclableMemoryStreamManager _manager =
#pragma warning disable SA1000
            new(new RecyclableMemoryStreamManager.Options(
#pragma warning restore SA1000
                BlockSize,
                RecyclableMemoryStreamManager.DefaultLargeBufferMultiple,
                RecyclableMemoryStreamManager.DefaultMaximumBufferSize,
                BlockSize * 32768L * 2, // 1 GB
                0));

        public virtual MemoryStream GetStream() => _manager.GetStream();

        public virtual MemoryStream GetStream(int requiredSize) => _manager.GetStream(null, requiredSize);

        public virtual MemoryStream GetStream(int requiredSize, bool asContiguousBuffer) =>
            _manager.GetStream(null, requiredSize, asContiguousBuffer);

        public virtual MemoryStream GetStream(byte[] buffer, int offset, int count) =>
            _manager.GetStream(null, buffer, offset, count);

        public virtual MemoryStream GetStream(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return GetStream();
            }
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            return GetStream(contentBytes, 0, contentBytes.Length);
        }
    }
}