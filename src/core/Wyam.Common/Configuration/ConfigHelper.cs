using System;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Configuration
{
    /// <summary>
    /// This class satisfies a common use case for modules where you need to get some configuration value
    /// either directly, from a delegate at the module level, or from a delegate at a per-document level
    /// and the user should be able to specify any of these possibilities (typically via module constructor
    /// overloads).
    /// </summary>
    /// <typeparam name="T">The type of the value you want to eventually convert to.</typeparam>
    public class ConfigHelper<T>
    {
        private readonly AsyncContextConfig _contextConfig;
        private readonly AsyncDocumentConfig _documentConfig;
        private readonly T _defaultValue;
        private T _value;
        private bool _gotValue;

        /// <summary>
        /// Creates a new helper with the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public ConfigHelper(T value)
        {
            _contextConfig = _ => Task.FromResult<object>(value);
        }

        /// <summary>
        /// Creates a new helper with the specified delegate.
        /// </summary>
        /// <param name="config">The delegate.</param>
        /// <param name="defaultValue">A default value to use if the delegate is null.</param>
        public ConfigHelper(AsyncContextConfig config, T defaultValue = default(T))
        {
            _contextConfig = config;
            _defaultValue = defaultValue;
        }

        /// <summary>
        /// Creates a new helper with the specified delegate.
        /// </summary>
        /// <param name="config">The delegate.</param>
        /// <param name="defaultValue">A default value to use if the delegate is null.</param>
        public ConfigHelper(AsyncDocumentConfig config, T defaultValue = default(T))
        {
            _documentConfig = config;
            _defaultValue = defaultValue;
        }

        /// <summary>
        /// Call this each time you need the value, passing in a post-processing function if required.
        /// If no document delegate is specified, then this will get and cache the value on first request.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="postProcessing">An optional post-processing function.</param>
        /// <returns>The result value.</returns>
        public async Task<T> GetValueAsync(IDocument document, IExecutionContext context, Func<T, T> postProcessing = null)
        {
            if (_documentConfig == null)
            {
                if (_gotValue)
                {
                    return _value;
                }
                _value = _contextConfig == null ? _defaultValue : await _contextConfig.InvokeAsync<T>(context);
                if (postProcessing != null)
                {
                    _value = postProcessing(_value);
                }
                _gotValue = true;
                return _value;
            }

            T value = await _documentConfig.InvokeAsync<T>(document, context);
            if (postProcessing != null)
            {
                value = postProcessing(value);
            }
            return value;
        }
    }
}
