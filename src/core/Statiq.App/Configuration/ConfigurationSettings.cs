using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Statiq.Common;

namespace Statiq.App
{
    /// <summary>
    /// Contains a <see cref="IConfigurationRoot" /> with a dictionary of settings
    /// that override the configuration values. Used by the <see cref="Bootstrapper" />
    /// to add settings on top of configuration and passed to the
    /// <see cref="Statiq.Core.Engine" /> as it's initial settings.
    /// </summary>
    /// <remarks>
    /// This will be disposed after passing to the engine as a sanity check, it shouldn't
    /// be used after that.
    /// </remarks>
    internal class ConfigurationSettings : ConfigurationMetadata, IConfigurationSettings, IDisposable
    {
        private bool _disposed;

        public ConfigurationSettings(IConfigurationRoot configuration)
            : base(configuration)
        {
        }

        public IDictionary<string, object> Settings { get; private set; } =
            new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public void Dispose()
        {
            _disposed = true;
            Settings = null;
            Configuration = null;
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ConfigurationSettings));
            }
        }

        public override bool ContainsKey(string key)
        {
            CheckDisposed();
            _ = key ?? throw new ArgumentNullException(nameof(key));
            return Settings.ContainsKey(key) || base.ContainsKey(key);
        }

        public override bool TryGetRaw(string key, out object value)
        {
            CheckDisposed();
            _ = key ?? throw new ArgumentNullException(nameof(key));
            return Settings.TryGetValue(key, out value) || base.TryGetRaw(key, out value);
        }

        // Enumerate the keys separately so we don't evaluate values
        public override IEnumerable<string> Keys
        {
            get
            {
                CheckDisposed();
                foreach (string key in Settings.Keys)
                {
                    yield return key;
                }
                foreach (string previousKey in base.Keys)
                {
                    if (!Settings.ContainsKey(previousKey))
                    {
                        yield return previousKey;
                    }
                }
            }
        }

        public override IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            CheckDisposed();
            foreach (KeyValuePair<string, object> item in Settings)
            {
                yield return TypeHelper.ExpandKeyValuePair(item, this);
            }
            IEnumerator<KeyValuePair<string, object>> baseEnumerator = base.GetEnumerator();
            while (baseEnumerator.MoveNext())
            {
                if (!Settings.ContainsKey(baseEnumerator.Current.Key))
                {
                    yield return baseEnumerator.Current;
                }
            }
        }

        public override IEnumerator<KeyValuePair<string, object>> GetRawEnumerator()
        {
            CheckDisposed();
            foreach (KeyValuePair<string, object> item in Settings)
            {
                yield return item;
            }
            IEnumerator<KeyValuePair<string, object>> baseEnumerator = base.GetEnumerator();
            while (baseEnumerator.MoveNext())
            {
                if (!Settings.ContainsKey(baseEnumerator.Current.Key))
                {
                    yield return baseEnumerator.Current;
                }
            }
        }

        object IDictionary<string, object>.this[string key]
        {
            get
            {
                CheckDisposed();
                return this[key];
            }

            set
            {
                CheckDisposed();
                Settings[key] = value;
            }
        }

        public bool IsReadOnly => false;

        ICollection<string> IDictionary<string, object>.Keys => Keys.ToArray();

        ICollection<object> IDictionary<string, object>.Values => Values.ToArray();

        ICollection<string> IConfigurationSettings.Keys => Keys.ToArray();

        ICollection<object> IConfigurationSettings.Values => Values.ToArray();

        object IConfigurationSettings.this[string key]
        {
            get
            {
                CheckDisposed();
                return this[key];
            }

            set
            {
                CheckDisposed();
                Settings[key] = value;
            }
        }

        public void Add(string key, object value)
        {
            CheckDisposed();
            Settings[key] = value;
        }

        public void Add(KeyValuePair<string, object> item)
        {
            CheckDisposed();
            Settings.Add(item);
        }

        // Not supported because the value of the item is raw vs. the expanded values presented by the dictionary
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => throw new NotSupportedException();

        // Not supported because the value of the item is raw vs. the expanded values presented by the dictionary
        public bool Contains(KeyValuePair<string, object> item) => throw new NotSupportedException();

        public void Clear() => throw new NotSupportedException();

        public bool Remove(string key) => throw new NotSupportedException();

        public bool Remove(KeyValuePair<string, object> item) => throw new NotSupportedException();
    }
}
