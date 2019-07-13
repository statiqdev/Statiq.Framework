using System;
using System.Collections.Generic;
using System.Linq;
using Statiq.Common;

namespace Statiq.Common
{
    public static class ObjectDocumentExtensions
    {
        // ToDocument

        public static ObjectDocument<T> ToDocument<T>(
            this T obj,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            new ObjectDocument<T>(obj, null, destination, items, contentProvider);

        public static ObjectDocument<T> ToDocument<T>(
            this T obj,
            FilePath source,
            FilePath destination,
            IContentProvider contentProvider = null) =>
            new ObjectDocument<T>(obj, source, destination, null, contentProvider);

        public static ObjectDocument<T> ToDocument<T>(
            this T obj,
            FilePath destination,
            IContentProvider contentProvider = null) =>
            new ObjectDocument<T>(obj, null, destination, null, contentProvider);

        public static ObjectDocument<T> ToDocument<T>(
            this T obj,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            new ObjectDocument<T>(obj, null, null, items, contentProvider);

        public static ObjectDocument<T> ToDocument<T>(
            this T obj,
            IContentProvider contentProvider = null) =>
            new ObjectDocument<T>(obj, null, null, null, contentProvider);

        public static ObjectDocument<T> ToDocument<T>(
            this T obj,
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            new ObjectDocument<T>(obj, source, destination, items, contentProvider);

        // ToDocuments

        public static IEnumerable<ObjectDocument<T>> ToDocuments<T>(
            this IEnumerable<T> objs,
            Func<T, FilePath> destinationFunc,
            Func<T, IEnumerable<KeyValuePair<string, object>>> itemsFunc,
            Func<T, IContentProvider> contentProviderFunc = null) =>
            objs?.Select(x => x == null
                ? null
                : new ObjectDocument<T>(
                    x,
                    null,
                    destinationFunc?.Invoke(x),
                    itemsFunc?.Invoke(x),
                    contentProviderFunc?.Invoke(x)));

        public static IEnumerable<ObjectDocument<T>> ToDocuments<T>(
            this IEnumerable<T> objs,
            Func<T, FilePath> sourceFunc,
            Func<T, FilePath> destinationFunc,
            Func<T, IContentProvider> contentProviderFunc = null) =>
            objs?.Select(x => x == null
                ? null
                : new ObjectDocument<T>(
                    x,
                    sourceFunc?.Invoke(x),
                    destinationFunc?.Invoke(x),
                    null,
                    contentProviderFunc?.Invoke(x)));

        public static IEnumerable<ObjectDocument<T>> ToDocuments<T>(
            this IEnumerable<T> objs,
            Func<T, FilePath> destinationFunc,
            Func<T, IContentProvider> contentProviderFunc = null) =>
            objs?.Select(x => x == null
                ? null
                : new ObjectDocument<T>(
                    x,
                    null,
                    destinationFunc?.Invoke(x),
                    null,
                    contentProviderFunc?.Invoke(x)));

        public static IEnumerable<ObjectDocument<T>> ToDocuments<T>(
            this IEnumerable<T> objs,
            Func<T, IEnumerable<KeyValuePair<string, object>>> itemsFunc,
            Func<T, IContentProvider> contentProviderFunc = null) =>
            objs?.Select(x => x == null
                ? null
                : new ObjectDocument<T>(
                    x,
                    null,
                    null,
                    itemsFunc?.Invoke(x),
                    contentProviderFunc?.Invoke(x)));

        public static IEnumerable<ObjectDocument<T>> ToDocuments<T>(
            this IEnumerable<T> objs,
            Func<T, IContentProvider> contentProviderFunc = null) =>
            objs?.Select(x => x == null
                ? null
                : new ObjectDocument<T>(
                    x,
                    null,
                    null,
                    null,
                    contentProviderFunc?.Invoke(x)));

        public static IEnumerable<ObjectDocument<T>> ToDocuments<T>(
            this IEnumerable<T> objs,
            Func<T, FilePath> sourceFunc,
            Func<T, FilePath> destinationFunc,
            Func<T, IEnumerable<KeyValuePair<string, object>>> itemsFunc,
            Func<T, IContentProvider> contentProviderFunc = null) =>
            objs?.Select(x => x == null
                ? null
                : new ObjectDocument<T>(
                    x,
                    sourceFunc?.Invoke(x),
                    destinationFunc?.Invoke(x),
                    itemsFunc?.Invoke(x),
                    contentProviderFunc?.Invoke(x)));
    }
}
