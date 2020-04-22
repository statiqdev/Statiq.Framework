using System;
using System.IO;

namespace Statiq.Testing
{
    public static class StreamExtensions
    {
        public static string ReadToEnd(this Stream stream)
        {
            _ = stream ?? throw new ArgumentNullException(nameof(stream));
            stream.Seek(0, SeekOrigin.Begin);
            return new StreamReader(stream).ReadToEnd();
        }
    }
}
