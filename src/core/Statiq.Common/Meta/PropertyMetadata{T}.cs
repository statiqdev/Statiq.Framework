using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Statiq.Common;

namespace Statiq.Common
{
    /// <summary>
    /// Provides metadata for the properties of a given object.
    /// </summary>
    /// <typeparam name="T">The type of object to provide property metadata for.</typeparam>
    public static class PropertyMetadata<T>
    {
        private static readonly Dictionary<string, IPropertyCallAdapter> Properties = GetPropertyMetadata();

        private static Dictionary<string, IPropertyCallAdapter> GetPropertyMetadata()
        {
            Dictionary<string, IPropertyCallAdapter> propertyMetadata =
                new Dictionary<string, IPropertyCallAdapter>(StringComparer.OrdinalIgnoreCase);

            // Do a first pass for non-attribute properties
            // This ensures actual properties will get added first and take precedence over any attributes that define colliding names
            List<(PropertyMetadataAttribute, MethodInfo)> attributeProperties =
                new List<(PropertyMetadataAttribute Property, MethodInfo Getter)>();
            foreach ((PropertyInfo property, MethodInfo getter) in
                typeof(T)
                    .GetProperties()
                    .Select(x => (x, x.GetGetMethod()))
                    .Where(x => x.Item2 != null && x.Item2.GetParameters().Length == 0))
            {
                // If there's an attribute, do this in a second pass
                PropertyMetadataAttribute attribute = property.GetCustomAttribute<PropertyMetadataAttribute>();
                if (attribute != null)
                {
                    // Only add the property for later processing if the new name isn't null
                    if (!string.IsNullOrEmpty(attribute.Name))
                    {
                        attributeProperties.Add((attribute, getter));
                    }
                }
                else
                {
                    // No attribute, so add this property
                    if (!propertyMetadata.ContainsKey(property.Name))
                    {
                        propertyMetadata.Add(property.Name, GetPropertyCallAdapter(getter));
                    }
                }
            }

            // Now that all the actual property names have been added, add ones from the attribute
            foreach ((PropertyMetadataAttribute attribute, MethodInfo getter) in attributeProperties)
            {
                if (!propertyMetadata.ContainsKey(attribute.Name))
                {
                    propertyMetadata.Add(attribute.Name, GetPropertyCallAdapter(getter));
                }
            }

            return propertyMetadata;
        }

        private static IPropertyCallAdapter GetPropertyCallAdapter(MethodInfo getter)
        {
            Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), getter.ReturnType);
            Delegate getterDelegate = getter.CreateDelegate(delegateType);
            Type adapterType = typeof(PropertyCallAdapter<>).MakeGenericType(typeof(T), getter.ReturnType);
            return Activator.CreateInstance(adapterType, getterDelegate) as IPropertyCallAdapter;
        }

        private interface IPropertyCallAdapter
        {
            object GetValue(T instance);
        }

        private class PropertyCallAdapter<TResult> : IPropertyCallAdapter
        {
            private readonly Func<T, TResult> _getter;

            public PropertyCallAdapter(Func<T, TResult> getter)
            {
                _getter = getter;
            }

            public object GetValue(T instance) => _getter.Invoke(instance);
        }

        /// <summary>
        /// Gets an <see cref="IMetadata"/> for a given instance of the object type.
        /// </summary>
        /// <param name="instance">The object instance to present as metadata.</param>
        /// <returns>A metadata collection for the properties of the provided instance.</returns>
        public static IMetadata For(T instance) => new PropertyMetadataInstance(instance);

        private class PropertyMetadataInstance : IMetadata
        {
            private readonly T _instance;

            public PropertyMetadataInstance(T instance)
            {
                if (instance == null)
                {
                    throw new ArgumentNullException(nameof(instance));
                }
                _instance = instance;
            }

            public bool ContainsKey(string key) => Properties.ContainsKey(key);

            public object this[string key]
            {
                get
                {
                    if (key == null)
                    {
                        throw new ArgumentNullException(nameof(key));
                    }
                    if (!TryGetValue(key, out object value))
                    {
                        throw new KeyNotFoundException("The key " + key + " was not found, use Get() to provide a default value.");
                    }
                    return value;
                }
            }

            public IEnumerable<string> Keys => this.Select(x => x.Key);

            public IEnumerable<object> Values => this.Select(x => x.Value);

            public bool TryGetRaw(string key, out object value)
            {
                value = default;

                if (Properties.TryGetValue(key, out IPropertyCallAdapter adapter))
                {
                    value = adapter.GetValue(_instance);
                    return true;
                }

                return false;
            }

            public bool TryGetValue<TValue>(string key, out TValue value)
            {
                value = default;

                if (Properties.TryGetValue(key, out IPropertyCallAdapter adapter))
                {
                    object raw = adapter.GetValue(_instance);
                    return TypeHelper.TryConvert(raw, out value);
                }

                return false;
            }

            public bool TryGetValue(string key, out object value) => TryGetValue<object>(key, out value);

            public IMetadata GetMetadata(params string[] keys) =>
                new Metadata(this.Where(x => keys.Contains(x.Key, StringComparer.OrdinalIgnoreCase)));

#pragma warning disable RCS1077 // We want to count the enumerable items, not recursivly call this property
            public int Count => this.Count();
#pragma warning restore RCS1077

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                foreach (KeyValuePair<string, IPropertyCallAdapter> item in Properties)
                {
                    yield return new KeyValuePair<string, object>(item.Key, item.Value.GetValue(_instance));
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
