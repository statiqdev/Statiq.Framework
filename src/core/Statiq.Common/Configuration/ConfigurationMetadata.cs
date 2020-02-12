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
        protected internal ConfigurationMetadata(IConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IConfiguration Configuration { get; protected set; }

        public virtual bool ContainsKey(string key) =>
            Configuration.GetSection(key ?? throw new ArgumentNullException(nameof(key))).Exists();

        protected virtual object GetSectionMetadata(IConfigurationSection section) => new ConfigurationMetadata(section);

        public virtual bool TryGetRaw(string key, out object value)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));
            IConfigurationSection section = Configuration.GetSection(key);
            if (section.Exists())
            {
                value = section.Value ?? GetSectionMetadata(section);
                return true;
            }
            value = default;
            return false;
        }

        public bool TryGetValue<TValue>(string key, out TValue value)
        {
            if (key != null && TryGetRaw(key, out object raw))
            {
                return TypeHelper.TryExpandAndConvert(raw, this, out value);
            }
            value = default;
            return false;
        }

        public bool TryGetValue(string key, out object value) => TryGetValue<object>(key, out value);

        public object this[string key]
        {
            get
            {
                _ = key ?? throw new ArgumentNullException(nameof(key));
                if (!TryGetValue(key, out object value))
                {
                    throw new KeyNotFoundException("The key " + key + " was not found in metadata, use Get() to provide a default value.");
                }
                return value;
            }
        }

        // Enumerate the keys seperatly so we don't evaluate values
        public virtual IEnumerable<string> Keys => Configuration.AsEnumerable().Select(x => x.Key);

        public IEnumerable<object> Values => this.Select(x => x.Value);

        // The Select ensures LINQ optimizations won't turn this into a recursive call to Count
        public int Count => this.Select(_ => (object)null).Count();

        public virtual IEnumerator<KeyValuePair<string, object>> GetEnumerator() =>
            Configuration.AsEnumerable().Select(x => new KeyValuePair<string, object>(x.Key, x.Value)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
