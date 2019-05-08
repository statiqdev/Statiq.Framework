using System.IO;
using System.Text;
using Microsoft.IO;
using Wyam.Common.Execution;

namespace Wyam.Core.Execution
{
    /// <summary>
    /// Forwards calls to an underlying <see cref="RecyclableMemoryStreamManager"/>
    /// so that Wyam.Common doesn't have to maintain a reference to it.
    /// </summary>
    internal class MemoryStreamFactory : IMemoryStreamFactory
    {
        private const int BlockSize = 16384;

        private readonly RecyclableMemoryStreamManager _manager =
            new RecyclableMemoryStreamManager(
                BlockSize,
                RecyclableMemoryStreamManager.DefaultLargeBufferMultiple,
                RecyclableMemoryStreamManager.DefaultMaximumBufferSize)
            {
                MaximumFreeSmallPoolBytes = BlockSize * 32768L * 2, // 1 GB
            };

        public MemoryStream GetStream() => _manager.GetStream();

        public MemoryStream GetStream(int requiredSize) => _manager.GetStream(null, requiredSize);

        public MemoryStream GetStream(int requiredSize, bool asContiguousBuffer) =>
            _manager.GetStream(null, requiredSize, asContiguousBuffer);

        public MemoryStream GetStream(byte[] buffer, int offset, int count) =>
            _manager.GetStream(null, buffer, offset, count);

        public MemoryStream GetStream(string content)
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
