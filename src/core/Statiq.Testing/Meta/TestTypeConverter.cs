using System;
using System.Collections.Generic;
using System.Text;
using Statiq.Common.Configuration;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.IO;
using Statiq.Common.Shortcodes;
using Statiq.Testing.Configuration;
using Statiq.Testing.IO;

namespace Statiq.Testing.Meta
{
    public class TestTypeConverter
    {
        // Includes some initial common conversions
        public Dictionary<(Type Value, Type Result), Func<object, object>> TypeConversions { get; } = new Dictionary<(Type Value, Type Result), Func<object, object>>(DefaultTypeConversions);

        public static Dictionary<(Type Value, Type Result), Func<object, object>> DefaultTypeConversions { get; } =
            new Dictionary<(Type Value, Type Result), Func<object, object>>
            {
                { (typeof(string), typeof(bool)), x => bool.Parse((string)x) },
                { (typeof(string), typeof(FilePath)), x => new FilePath((string)x) },
                { (typeof(FilePath), typeof(string)), x => ((FilePath)x).FullPath },
                { (typeof(string), typeof(DirectoryPath)), x => new DirectoryPath((string)x) },
                { (typeof(DirectoryPath), typeof(string)), x => ((DirectoryPath)x).FullPath },
                { (typeof(string), typeof(Uri)), x => new Uri((string)x) },
                { (typeof(Uri), typeof(string)), x => ((Uri)x).ToString() }
            };

        public void AddTypeConversion<T, TResult>(Func<T, TResult> typeConversion) => TypeConversions.Add((typeof(T), typeof(TResult)), x => typeConversion((T)x));

        /// <inheritdoc/>
        public bool TryConvert<T>(object value, out T result)
        {
            // Check if there's a test-specific conversion
            if (TypeConversions.TryGetValue((value?.GetType() ?? typeof(object), typeof(T)), out Func<object, object> typeConversion))
            {
                result = (T)typeConversion(value);
                return true;
            }

            // Default conversion is just to cast
            if (value is T)
            {
                result = (T)value;
                return true;
            }

            result = default;
            return value == null;
        }
    }
}
