using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Statiq.Common;

namespace Statiq.Common
{
    public static class TypeHelper
    {
        /// <summary>
        /// Registers a type converter at runtime.
        /// </summary>
        /// <param name="type">The type the converter applies to.</param>
        /// <param name="typeConverterType">The type of the type converter (should be a <see cref="TypeConverter"/>).</param>
        public static void RegisterTypeConverter(Type type, Type typeConverterType) =>
            TypeDescriptor.AddAttributes(type, new TypeConverterAttribute(typeConverterType));

        /// <summary>
        /// Registers a type converter at runtime.
        /// </summary>
        /// <param name="type">The type the converter applies to.</param>
        /// <param name="typeConverterTypeName">The type name of the type converter (should be a <see cref="TypeConverter"/>).</param>
        public static void RegisterTypeConverter(Type type, string typeConverterTypeName) =>
            TypeDescriptor.AddAttributes(type, new TypeConverterAttribute(typeConverterTypeName));

        /// <summary>
        /// Registers a type converter at runtime.
        /// </summary>
        /// <typeparam name="TType">The type the converter applies to.</typeparam>
        /// <param name="typeConverterType">The type of the type converter (should be a <see cref="TypeConverter"/>).</param>
        public static void RegisterTypeConverter<TType>(Type typeConverterType) =>
            TypeDescriptor.AddAttributes(typeof(TType), new TypeConverterAttribute(typeConverterType));

        /// <summary>
        /// Registers a type converter at runtime.
        /// </summary>
        /// <typeparam name="TType">The type the converter applies to.</typeparam>
        /// <param name="typeConverterTypeName">The type name of the type converter (should be a <see cref="TypeConverter"/>).</param>
        public static void RegisterTypeConverter<TType>(string typeConverterTypeName) =>
            TypeDescriptor.AddAttributes(typeof(TType), new TypeConverterAttribute(typeConverterTypeName));

        /// <summary>
        /// Registers a type converter at runtime.
        /// </summary>
        /// <typeparam name="TType">The type the converter applies to.</typeparam>
        /// <typeparam name="TTypeConverter">The type of the type converter (should be a <see cref="TypeConverter"/>).</typeparam>
        public static void RegisterTypeConverter<TType, TTypeConverter>()
            where TTypeConverter : TypeConverter =>
            TypeDescriptor.AddAttributes(typeof(TType), new TypeConverterAttribute(typeof(TTypeConverter)));

        /// <summary>
        /// Converts the provided value to the specified type. This method never throws an exception.
        /// It will return default(T) if the value cannot be converted to T.
        /// </summary>
        /// <typeparam name="T">The desired return type.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <returns>The value converted to type T or default(T) if the value cannot be converted to type T.</returns>
        public static T Convert<T>(object value) => Convert<T>(value, null);

        /// <summary>
        /// Converts the provided value to the specified type. This method never throws an exception.
        /// It will return the specified default value if the value cannot be converted to T.
        /// </summary>
        /// <typeparam name="T">The desired return type.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <param name="defaultValueFactory">A factory to get a default value if the value cannot be converted to type T.</param>
        /// <returns>The value converted to type T or the specified default value if the value cannot be converted to type T.</returns>
        public static T Convert<T>(object value, Func<T> defaultValueFactory) =>
            TryConvert(value, out T result) ? result : (defaultValueFactory == null ? default : defaultValueFactory());

        /// <summary>
        /// Trys to convert the provided value to the specified type while recursivly expanding <see cref="IMetadataValue"/>.
        /// </summary>
        /// <typeparam name="T">The desired return type.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <param name="metadata">The current metadata instance.</param>
        /// <param name="result">The result of conversion.</param>
        /// <returns><c>true</c> if the value could be converted to the desired type, <c>false</c> otherwise.</returns>
        public static bool TryExpandAndConvert<T>(object value, IMetadata metadata, out T result) => TryConvert(ExpandValue(value, metadata), out result);

        /// <summary>
        /// This resolves the value by recursively expanding <see cref="IMetadataValue"/>.
        /// </summary>
        public static object ExpandValue(object value, IMetadata metadata)
        {
            // Perform special expansions of IConfig and IMetadataValue
            switch (value)
            {
                case IConfig config:
                    IExecutionContext context = IExecutionContext.Current;
                    if (context == null)
                    {
                        throw new InvalidOperationException("Cannot expand metadata config value that requires an execution context, no execution context is available");
                    }
                    if (config.RequiresDocument)
                    {
                        if (metadata is IDocument document)
                        {
                            return config.GetValueAsync(document, context).GetAwaiter().GetResult();
                        }

                        // The source metadata isn't a document but we need one so go ahead and create a dummy one that wraps the metadata
                        return config.GetValueAsync(context.CreateDocument(metadata), context).GetAwaiter().GetResult();
                    }
                    return config.GetValueAsync(null, context).GetAwaiter().GetResult();
                case IMetadataValue metadataValue:
                    return ExpandValue(metadataValue.Get(metadata), metadata);
            }

            return value;
        }

        /// <summary>
        /// This resolves the value by recursively expanding <see cref="IMetadataValue"/>.
        /// </summary>
        public static KeyValuePair<TKey, object> ExpandKeyValuePair<TKey>(KeyValuePair<TKey, object> item, IMetadata metadata) =>
            item.Value is IMetadataValue
                ? new KeyValuePair<TKey, object>(item.Key, ExpandValue(item.Value, metadata))
                : item;

        /// <summary>
        /// Trys to convert the provided value to the specified type.
        /// </summary>
        /// <typeparam name="T">The desired return type.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <param name="result">The result of conversion.</param>
        /// <returns><c>true</c> if the value could be converted to the desired type, <c>false</c> otherwise.</returns>
        public static bool TryConvert<T>(object value, out T result)
        {
            // Check for null
            if (value == null)
            {
                result = default;
                return !typeof(T).IsValueType
                    || (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>));
            }

            // Just return if they're the same type
            if (typeof(T) == value.GetType())
            {
                result = (T)value;
                return true;
            }

            // Special case if value is an enumerable that hasn't overridden .ToString() and T is a string
            // Otherwise we'd end up doing a .ToString() on the enumerable
            IEnumerable enumerableValue = value is string ? null : value as IEnumerable;
            if (typeof(T) == typeof(string) && enumerableValue != null
                && value.GetType().GetMethod("ToString").DeclaringType == typeof(object))
            {
                if (TryGetFirstConvertibleItem(enumerableValue, out result))
                {
                    return true;
                }
                enumerableValue = null;  // Don't try getting the first item again for the more general case below
            }

            // Check a normal conversion (in case it's a special type that implements a cast, IConvertible, or something)
            if (MetadataTypeConverter<T>.TryConvert(value, out result))
            {
                return true;
            }

            // If value is an enumerable but the result type is not, return the first convertible item
            if (enumerableValue != null && !typeof(IEnumerable).IsAssignableFrom(typeof(T)))
            {
                return TryGetFirstConvertibleItem(enumerableValue, out result);
            }

            // IReadOnlyList<>
            if (typeof(T).IsConstructedGenericType && typeof(T).GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
            {
                return TryConvertEnumerable(value, x => x.GetGenericArguments()[0], (x, y) => x.ToReadOnlyList(y), out result);
            }

            // IList<>
            if (typeof(T).IsConstructedGenericType
                && (typeof(T).GetGenericTypeDefinition() == typeof(IList<>)
                    || typeof(T).GetGenericTypeDefinition() == typeof(List<>)))
            {
                return TryConvertEnumerable(value, x => x.GetGenericArguments()[0], (x, y) => x.ToList(y), out result);
            }

            // Array
            if (typeof(Array).IsAssignableFrom(typeof(T))
                || (typeof(T).IsArray && typeof(T).GetArrayRank() == 1))
            {
                return TryConvertEnumerable(value, x => x.GetElementType() ?? typeof(object), (x, y) => x.ToArray(y), out result);
            }

            // IEnumerable<>
            if (typeof(T).IsConstructedGenericType && typeof(T).GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return TryConvertEnumerable(value, x => x.GetGenericArguments()[0], (x, y) => x.ToEnumerable(y), out result);
            }

            return false;
        }

        private static bool TryGetFirstConvertibleItem<T>(IEnumerable value, out T result)
        {
            MetadataTypeConverter<T> converter = new MetadataTypeConverter<T>();
            bool gotResult = true;
            result = ((IEnumerable<T>)converter.ToEnumerable(value))
                .Select(x =>
                {
                    gotResult = true;
                    return x;
                }).FirstOrDefault();
            return gotResult;
        }

        private static bool TryConvertEnumerable<T>(
            object value,
            Func<Type, Type> elementTypeFunc,
            Func<MetadataTypeConverter,
            IEnumerable, IEnumerable> conversionFunc,
            out T result)
        {
            Type elementType = elementTypeFunc(typeof(T));
            IEnumerable enumerable = value is string ? null : value as IEnumerable;
            if (enumerable == null || (elementType.IsInstanceOfType(value) && elementType != typeof(object)))
            {
                enumerable = new[] { value };
            }
            Type adapterType = typeof(MetadataTypeConverter<>).MakeGenericType(elementType);
            MetadataTypeConverter converter = (MetadataTypeConverter)Activator.CreateInstance(adapterType);
            result = (T)conversionFunc(converter, enumerable);
            return true;
        }
    }
}
