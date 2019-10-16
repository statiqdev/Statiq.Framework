using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Statiq.Common
{
    /// <summary>
    /// Wraps a <see cref="IConfiguration"/> as presents it as metadata.
    /// </summary>
    public class ConfigurationMetadata : IMetadata
    {
        public ConfigurationMetadata(IConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IConfiguration Configuration { get; protected set; }

        public bool ContainsKey(string key)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));
            IConfigurationSection section = Configuration.GetSection(key);
            return section.Exists() && section.Value != null;
        }

        public bool TryGetRaw(string key, out object value)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));
            IConfigurationSection section = Configuration.GetSection(key);
            if (section.Exists() && section.Value != null)
            {
                value = section.Value;
                return true;
            }
            value = default;
            return false;
        }

        public bool TryGetValue<TValue>(string key, out TValue value)
        {
            value = default;
            if (key != null && TryGetRaw(key, out object raw))
            {
                return TypeHelper.TryConvert(raw, out value);
            }
            return false;
        }

        public bool TryGetValue(string key, out object value) => TryGetValue<object>(key, out value);

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
                    throw new KeyNotFoundException("The key " + key + " was not found in metadata, use Get() to provide a default value.");
                }
                return value;
            }
        }

        public IEnumerable<string> Keys => this.Select(x => x.Key);

        public IEnumerable<object> Values => this.Select(x => x.Value);

        // The Select ensures LINQ optimizations won't turn this into a recursive call to Count
        public int Count => this.Select(_ => (object)null).Count();

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() =>
            Configuration.AsEnumerable().Select(x => new KeyValuePair<string, object>(x.Key, x.Value)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IMetadata GetMetadata(params string[] keys) =>
            new Metadata(this.Where(x => keys.Contains(x.Key, StringComparer.OrdinalIgnoreCase)));
    }
}
