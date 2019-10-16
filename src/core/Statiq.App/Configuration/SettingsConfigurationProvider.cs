using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Spectre.Cli;

namespace Statiq.App
{
    internal class SettingsConfigurationProvider : ConfigurationProvider, IConfigurationSource, IDictionary<string, string>
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder) => this;

        public string this[string key] { get => Data[key]; set => Data[key] = value; }

        public ICollection<string> Keys => Data.Keys;

        public ICollection<string> Values => Data.Values;

        public int Count => Data.Count;

        public bool IsReadOnly => Data.IsReadOnly;

        public void Add(string key, string value) => Data.Add(key, value);

        public void Add(KeyValuePair<string, string> item) => Data.Add(item);

        public void Clear() => Data.Clear();

        public bool Contains(KeyValuePair<string, string> item) => Data.Contains(item);

        public bool ContainsKey(string key) => Data.ContainsKey(key);

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) => Data.CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => Data.GetEnumerator();

        public bool Remove(string key) => Data.Remove(key);

        public bool Remove(KeyValuePair<string, string> item) => Data.Remove(item);

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value) => Data.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => Data.GetEnumerator();
    }
}
