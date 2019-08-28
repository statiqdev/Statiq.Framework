using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class Config
    {
        /// <summary>
        /// Creates a config value.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="value">The config value.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromValue<TValue>(TValue value) =>
            new Config<TValue>((_, __) => Task.FromResult(value), false);

        /// <summary>
        /// Creates a config value.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="value">The config value.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromValue<TValue>(Task<TValue> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            return new Config<TValue>((_, __) => value, false);
        }

        /// <summary>
        /// Creates an enumeration of config values.
        /// </summary>
        /// <typeparam name="TValue">The type of config value items.</typeparam>
        /// <param name="values">The config values.</param>
        /// <returns>A config object.</returns>
        public static Config<IEnumerable<TValue>> FromValues<TValue>(params TValue[] values) =>
            new Config<IEnumerable<TValue>>((_, __) => Task.FromResult<IEnumerable<TValue>>(values), false);

        /// <summary>
        /// Creates a config value from a delegate that uses the execution context.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="func">The delegate that produces the config value.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromContext<TValue>(Func<IExecutionContext, TValue> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }
            return new Config<TValue>((_, ctx) => Task.FromResult(func(ctx)), false);
        }

        /// <summary>
        /// Creates a config value from a delegate that uses the execution context.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="func">The delegate that produces the config value.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromContext<TValue>(Func<IExecutionContext, Task<TValue>> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }
            return new Config<TValue>((_, ctx) => func(ctx), false);
        }

        /// <summary>
        /// Creates a config value from an action that uses the execution context and returns
        /// the default value of <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromContext<TValue>(Action<IExecutionContext> action) =>
            new Config<TValue>((_, ctx) =>
            {
                if (action == null)
                {
                    throw new ArgumentNullException(nameof(action));
                }
                action(ctx);
                return Task.FromResult(default(TValue));
            });

        /// <summary>
        /// Creates a config value from an action that uses the execution context and returns
        /// the default value of <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromContext<TValue>(Func<IExecutionContext, Task> action) =>
            new Config<TValue>(async (_, ctx) =>
            {
                if (action == null)
                {
                    throw new ArgumentNullException(nameof(action));
                }
                await action(ctx);
                return default;
            });

        /// <summary>
        /// Creates a config value from an action that uses the execution context and returns null.
        /// </summary>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<object> FromContext(Action<IExecutionContext> action) =>
            FromContext<object>(action);

        /// <summary>
        /// Creates a config value from an action that uses the execution context and returns null.
        /// </summary>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<object> FromContext(Func<IExecutionContext, Task> action) =>
            FromContext<object>(action);

        /// <summary>
        /// Creates a config value by getting the metadata value from the execution context of a specified key.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="key">The metadata key to get the value from.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromSetting<TValue>(string key) =>
            new Config<TValue>((_, ctx) => Task.FromResult(ctx.Settings.Get<TValue>(key)), false);

        /// <summary>
        /// Creates a config value by getting the metadata value from the execution context of a specified key.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="key">The metadata key to get the value from.</param>
        /// <param name="defaultValue">The default value to use if the key cannot be found, is null, or cannot be converted to <typeparamref name="TValue"/>.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromSetting<TValue>(string key, TValue defaultValue) =>
            new Config<TValue>((_, ctx) => Task.FromResult(ctx.Settings.Get(key, defaultValue)), false);

        /// <summary>
        /// Creates a config value by getting the metadata value from the execution context of a specified key.
        /// </summary>
        /// <param name="key">The metadata key to get the value from.</param>
        /// <param name="defaultValue">The default value to use if the key cannot be found or is null.</param>
        /// <returns>A config object.</returns>
        public static Config<object> FromSetting(string key, object defaultValue = null) =>
            new Config<object>((_, ctx) => Task.FromResult(ctx.Settings.Get(key, defaultValue)), false);

        /// <summary>
        /// Creates a config value from a delegate that uses a document.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="func">The delegate that produces the config value.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromDocument<TValue>(Func<IDocument, TValue> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }
            return new Config<TValue>((doc, _) => Task.FromResult(func(doc)));
        }

        /// <summary>
        /// Creates a config value from a delegate that uses a document.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="func">The delegate that produces the config value.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromDocument<TValue>(Func<IDocument, Task<TValue>> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }
            return new Config<TValue>((doc, _) => func(doc));
        }

        /// <summary>
        /// Creates a config value from an action that uses a document and returns
        /// the default value of <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromDocument<TValue>(Action<IDocument> action) =>
            new Config<TValue>((doc, _) =>
            {
                if (action == null)
                {
                    throw new ArgumentNullException(nameof(action));
                }
                action(doc);
                return Task.FromResult(default(TValue));
            });

        /// <summary>
        /// Creates a config value from an action that uses a document and returns
        /// the default value of <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromDocument<TValue>(Func<IDocument, Task> action) =>
            new Config<TValue>(async (doc, _) =>
            {
                if (action == null)
                {
                    throw new ArgumentNullException(nameof(action));
                }
                await action(doc);
                return default;
            });

        /// <summary>
        /// Creates a config value from an action that uses a document and returns null.
        /// </summary>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<object> FromDocument(Action<IDocument> action) =>
            FromDocument<object>(action);

        /// <summary>
        /// Creates a config value from an action that uses a document and returns null.
        /// </summary>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<object> FromDocument(Func<IDocument, Task> action) =>
            FromDocument<object>(action);

        /// <summary>
        /// Creates a config value from a delegate that uses a document and the execution context.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="func">The delegate that produces the config value.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromDocument<TValue>(Func<IDocument, IExecutionContext, TValue> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }
            return new Config<TValue>((doc, ctx) => Task.FromResult(func(doc, ctx)));
        }

        /// <summary>
        /// Creates a config value from a delegate that uses a document and the execution context.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="func">The delegate that produces the config value.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromDocument<TValue>(Func<IDocument, IExecutionContext, Task<TValue>> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }
            return new Config<TValue>(func);
        }

        /// <summary>
        /// Creates a config value from an action that uses a document and the execution context and returns
        /// the default value of <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromDocument<TValue>(Action<IDocument, IExecutionContext> action) =>
            new Config<TValue>((doc, ctx) =>
            {
                if (action == null)
                {
                    throw new ArgumentNullException(nameof(action));
                }
                action(doc, ctx);
                return Task.FromResult(default(TValue));
            });

        /// <summary>
        /// Creates a config value from an action that uses a document and the execution context and returns
        /// the default value of <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromDocument<TValue>(Func<IDocument, IExecutionContext, Task> action) =>
            new Config<TValue>(async (doc, ctx) =>
            {
                if (action == null)
                {
                    throw new ArgumentNullException(nameof(action));
                }
                await action(doc, ctx);
                return default;
            });

        /// <summary>
        /// Creates a config value from an action that uses a document and the execution context and returns null.
        /// </summary>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<object> FromDocument(Action<IDocument, IExecutionContext> action) =>
            FromDocument<object>(action);

        /// <summary>
        /// Creates a config value from an action that uses a document and the execution context and returns null.
        /// </summary>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<object> FromDocument(Func<IDocument, IExecutionContext, Task> action) =>
            FromDocument<object>(action);

        /// <summary>
        /// Creates a config value by getting the metadata value of a specified key from a document.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="key">The metadata key to get the value from.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromDocument<TValue>(string key) => new Config<TValue>((doc, _) =>
        Task.FromResult(doc.Get<TValue>(key)));

        /// <summary>
        /// Creates a config value by getting the metadata value of a specified key from a document.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="key">The metadata key to get the value from.</param>
        /// <param name="defaultValue">The default value to use if the key cannot be found, is null, or cannot be converted to <typeparamref name="TValue"/>.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromDocument<TValue>(string key, TValue defaultValue) => new Config<TValue>((doc, _) =>
        Task.FromResult(doc.Get(key, defaultValue)));

        /// <summary>
        /// Creates a config value by getting the metadata value of a specified key from a document.
        /// </summary>
        /// <param name="key">The metadata key to get the value from.</param>
        /// <param name="defaultValue">The default value to use if the key cannot be found or is null.</param>
        /// <returns>A config object.</returns>
        public static Config<object> FromDocument(string key, object defaultValue = null) => new Config<object>((doc, _) =>
        Task.FromResult(doc.Get(key, defaultValue)));

        // This just adds a space to the front of error details so it'll format nicely
        // Used by the extensions that convert values from a DocumentConfig<object> or ContextConfig<object>
        internal static string GetErrorDetails(string errorDetails)
        {
            if (errorDetails?.StartsWith(" ") == false)
            {
                errorDetails = " " + errorDetails;
            }
            return errorDetails;
        }
    }
}
