using System;
using System.Collections.Generic;
using System.Linq;
using Statiq.Common;

namespace Statiq.Common
{
    public static class ObjectDocumentExtensions
    {
        // ToDocument

        public static IDocument ToDocument<T>(
            this T obj,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            GetObjectDocument(obj, null, destination, new Metadata(items), contentProvider);

        public static IDocument ToDocument<T>(
            this T obj,
            NormalizedPath source,
            NormalizedPath destination,
            IContentProvider contentProvider = null) =>
            GetObjectDocument(obj, source, destination, null, contentProvider);

        public static IDocument ToDocument<T>(
            this T obj,
            NormalizedPath destination,
            IContentProvider contentProvider = null) =>
            GetObjectDocument(obj, null, destination, null, contentProvider);

        public static IDocument ToDocument<T>(
            this T obj,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            GetObjectDocument(obj, null, null, new Metadata(items), contentProvider);

        public static IDocument ToDocument<T>(
            this T obj,
            IContentProvider contentProvider = null) =>
            GetObjectDocument(obj, null, null, null, contentProvider);

        public static IDocument ToDocument<T>(
            this T obj,
            NormalizedPath source,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            GetObjectDocument(obj, source, destination, new Metadata(items), contentProvider);

        // ToDocuments

        public static IEnumerable<IDocument> ToDocuments<T>(
            this IEnumerable<T> objs,
            Func<T, NormalizedPath> destinationFunc,
            Func<T, IEnumerable<KeyValuePair<string, object>>> itemsFunc,
            Func<T, IContentProvider> contentProviderFunc = null) =>
            objs?.Select(x => x == null
                ? null
                : GetObjectDocument(
                    x,
                    null,
                    destinationFunc?.Invoke(x) ?? NormalizedPath.Null,
                    new Metadata(itemsFunc?.Invoke(x)),
                    contentProviderFunc?.Invoke(x)));

        public static IEnumerable<IDocument> ToDocuments<T>(
            this IEnumerable<T> objs,
            Func<T, NormalizedPath> sourceFunc,
            Func<T, NormalizedPath> destinationFunc,
            Func<T, IContentProvider> contentProviderFunc = null) =>
            objs?.Select(x => x == null
                ? null
                : GetObjectDocument(
                    x,
                    sourceFunc?.Invoke(x) ?? NormalizedPath.Null,
                    destinationFunc?.Invoke(x) ?? NormalizedPath.Null,
                    null,
                    contentProviderFunc?.Invoke(x)));

        public static IEnumerable<IDocument> ToDocuments<T>(
            this IEnumerable<T> objs,
            Func<T, NormalizedPath> destinationFunc,
            Func<T, IContentProvider> contentProviderFunc = null) =>
            objs?.Select(x => x == null
                ? null
                : GetObjectDocument(
                    x,
                    null,
                    destinationFunc?.Invoke(x) ?? NormalizedPath.Null,
                    null,
                    contentProviderFunc?.Invoke(x)));

        public static IEnumerable<IDocument> ToDocuments<T>(
            this IEnumerable<T> objs,
            Func<T, IEnumerable<KeyValuePair<string, object>>> itemsFunc,
            Func<T, IContentProvider> contentProviderFunc = null) =>
            objs?.Select(x => x == null
                ? null
                : GetObjectDocument(
                    x,
                    null,
                    null,
                    new Metadata(itemsFunc?.Invoke(x)),
                    contentProviderFunc?.Invoke(x)));

        public static IEnumerable<IDocument> ToDocuments<T>(
            this IEnumerable<T> objs,
            Func<T, IContentProvider> contentProviderFunc = null) =>
            objs?.Select(x => x == null
                ? null
                : GetObjectDocument(
                    x,
                    null,
                    null,
                    null,
                    contentProviderFunc?.Invoke(x)));

        public static IEnumerable<IDocument> ToDocuments<T>(
            this IEnumerable<T> objs,
            Func<T, NormalizedPath> sourceFunc,
            Func<T, NormalizedPath> destinationFunc,
            Func<T, IEnumerable<KeyValuePair<string, object>>> itemsFunc,
            Func<T, IContentProvider> contentProviderFunc = null) =>
            objs?.Select(x => x == null
                ? null
                : GetObjectDocument(
                    x,
                    sourceFunc?.Invoke(x) ?? NormalizedPath.Null,
                    destinationFunc?.Invoke(x) ?? NormalizedPath.Null,
                    new Metadata(itemsFunc?.Invoke(x)),
                    contentProviderFunc?.Invoke(x)));

        // Construct an ObjectDocument<T> from the actual type of the document
        private static IDocument GetObjectDocument<T>(
            T obj,
            NormalizedPath source,
            NormalizedPath destination,
            IMetadata metadata,
            IContentProvider contentProvider = null)
        {
            _ = obj ?? throw new ArgumentNullException();
            Type objType = obj.GetType();
            if (typeof(T).Equals(objType))
            {
                // Fast track if the type is the same
                return new ObjectDocument<T>(
                    obj,
                    source,
                    destination,
                    metadata,
                    contentProvider);
            }

            // The generic type isn't the same as the actual type so use reflection to get it right
            return (IDocument)Activator.CreateInstance(
                typeof(ObjectDocument<>).MakeGenericType(obj.GetType()),
                new object[]
                {
                    obj,
                    source,
                    destination,
                    metadata,
                    contentProvider
                });
        }
    }
}
