using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class IFileExtensions
    {
        public static async ValueTask<TValue> DeserializeJsonAsync<TValue>(
            this IFile file,
            JsonSerializerOptions options = null,
            CancellationToken cancellationToken = default)
        {
            file.ThrowIfNull(nameof(file));
            using (Stream stream = file.OpenRead())
            {
                return await JsonSerializer.DeserializeAsync<TValue>(stream, options, cancellationToken);
            }
        }

        public static async ValueTask<object> DeserializeJsonAsync(
            this IFile file,
            Type returnType,
            JsonSerializerOptions options = null,
            CancellationToken cancellationToken = default)
        {
            file.ThrowIfNull(nameof(file));
            using (Stream stream = file.OpenRead())
            {
                return await JsonSerializer.DeserializeAsync(stream, returnType, options, cancellationToken);
            }
        }

        public static async Task SerializeJsonAsync<TValue>(
            this IFile file,
            TValue value,
            bool createDirectory,
            JsonSerializerOptions options = null,
            CancellationToken cancellationToken = default)
        {
            file.ThrowIfNull(nameof(file));
            using (Stream stream = file.OpenWrite(createDirectory))
            {
                await JsonSerializer.SerializeAsync(stream, value, options, cancellationToken);
            }
        }

        public static async Task SerializeJsonAsync<TValue>(
            this IFile file,
            TValue value,
            JsonSerializerOptions options = null,
            CancellationToken cancellationToken = default) =>
            await file.SerializeJsonAsync(value, true, options, cancellationToken);

        public static async Task SerializeJsonAsync(
            this IFile file,
            object value,
            Type inputType,
            bool createDirectory,
            JsonSerializerOptions options = null,
            CancellationToken cancellationToken = default)
        {
            file.ThrowIfNull(nameof(file));
            using (Stream stream = file.OpenWrite(createDirectory))
            {
                await JsonSerializer.SerializeAsync(stream, value, inputType, options, cancellationToken);
            }
        }

        public static async Task SerializeJsonAsync(
            this IFile file,
            object value,
            Type inputType,
            JsonSerializerOptions options = null,
            CancellationToken cancellationToken = default) =>
            await file.SerializeJsonAsync(value, inputType, true, options, cancellationToken);
    }
}
