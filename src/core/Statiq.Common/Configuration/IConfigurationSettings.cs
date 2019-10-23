using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Statiq.Common
{
    public interface IConfigurationSettings : IReadOnlyConfigurationSettings, IDictionary<string, string>
    {
        // Redefine overlap between IReadOnlyDictionary<string, object> and IDictionary<string, string> to avoid ambiguity errors

        new string this[string key] { get; set; }

        new ICollection<string> Keys { get; }

        new ICollection<string> Values { get; }

        new int Count { get; }

        new bool ContainsKey(string key);
    }
}
