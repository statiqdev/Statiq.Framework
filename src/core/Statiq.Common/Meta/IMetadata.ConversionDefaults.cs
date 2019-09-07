using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// Extensions to make it easier to get typed information from metadata.
    /// </summary>
    public partial interface IMetadata
    {
        /// <summary>
        /// Gets the value for the specified key converted to a string. This method never throws an exception. It will return the specified
        /// default value if the key is not found.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a string.</param>
        /// <returns>The value for the specified key converted to a string or the specified default value.</returns>
        public string GetString(string key, string defaultValue = null) => Get(key, defaultValue);

        /// <summary>
        /// Formats a string value if it exists in the metadata, otherwise returns a default value.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="formatFunc">A formatting function to apply to the string value of the specified key.</param>
        /// <param name="defaultValue">The default value to use if the key is not found.</param>
        /// <returns>The formatted value of the specified key if it exists or the specified default value.</returns>
        public string GetString(string key, Func<string, string> formatFunc, string defaultValue = null) =>
            ContainsKey(key) ? formatFunc(GetString(key)) : defaultValue;

        /// <summary>
        /// Gets the value for the specified key converted to a bool. This method never throws an exception. It will return the specified
        /// default value if the key is not found.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a bool.</param>
        /// <returns>The value for the specified key converted to a bool or the specified default value.</returns>
        public bool GetBool(string key, bool defaultValue = false) => Get(key, defaultValue);

        /// <summary>
        /// Gets the value for the specified key converted to an int. This method never throws an exception. It will return the specified
        /// default value if the key is not found.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to an int.</param>
        /// <returns>The value for the specified key converted to an int or the specified default value.</returns>
        public int GetInt(string key, int defaultValue = 0) => Get(key, defaultValue);

        /// <summary>
        /// Gets the value for the specified key converted to a <see cref="GetDateTime"/>. This method never throws an exception. It will return the specified
        /// default value if the key is not found.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a <see cref="GetDateTime"/>.</param>
        /// <returns>The value for the specified key converted to a <see cref="GetDateTime"/> or the specified default value.</returns>
        public DateTime GetDateTime(string key, DateTime defaultValue = default(DateTime)) => Get(key, defaultValue);

        /// <summary>
        /// Gets the value for the specified key converted to a <see cref="GetFilePath"/>. This method never throws an exception. It will
        /// return the specified default value if the key is not found or if the string value can't be converted to a <see cref="GetFilePath"/>.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a <see cref="GetFilePath"/>.</param>
        /// <returns>The value for the specified key converted to a <see cref="GetFilePath"/> or the specified default value.</returns>
        public FilePath GetFilePath(string key, FilePath defaultValue = null) => Get(key, defaultValue);

        /// <summary>
        /// Gets the value for the specified key converted to a <see cref="GetDirectoryPath"/>. This method never throws an exception. It will
        /// return the specified default value if the key is not found or if the string value can't be converted to a <see cref="GetDirectoryPath"/>.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a <see cref="GetDirectoryPath"/>.</param>
        /// <returns>The value for the specified key converted to a <see cref="GetDirectoryPath"/> or the specified default value.</returns>
        public DirectoryPath GetDirectoryPath(string key, DirectoryPath defaultValue = null) => Get(key, defaultValue);

        /// <summary>
        /// Gets the value for the specified key converted to a <see cref="IReadOnlyList{T}"/>. This method never throws an exception. It will return the specified
        /// default value if the key is not found. Note that if the value is atomic, the conversion operation will succeed and return a list with one item.
        /// </summary>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a list.</param>
        /// <returns>The value for the specified key converted to a list or the specified default value.</returns>
        public IReadOnlyList<T> GetList<T>(string key, IReadOnlyList<T> defaultValue = null) => Get(key, defaultValue);

        /// <summary>
        /// Gets the value for the specified key converted to a <see cref="IDocument"/>. This method never throws an exception.
        /// It will return null if the key is not found.
        /// </summary>
        /// <param name="key">The key of the document to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a document.</param>
        /// <returns>The value for the specified key converted to a string or null.</returns>
        public IDocument GetDocument(string key, IDocument defaultValue = null) => Get(key, defaultValue);

        /// <summary>
        /// Gets the value for the specified key converted to a <c>IReadOnlyList&lt;IDocument&gt;</c>. This method never throws an exception.
        /// It will return null if the key is not found and an empty list if the key is found but contains no items that can be converted to <see cref="IDocument"/>.
        /// </summary>
        /// <param name="key">The key of the documents to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a document list.</param>
        /// <returns>The value for the specified key converted to a list or null.</returns>
        public IReadOnlyList<IDocument> GetDocumentList(string key, IReadOnlyList<IDocument> defaultValue = null) => Get(key, defaultValue);

        /// <summary>
        /// Gets the value associated with the specified key as a dynamic object. This is equivalent
        /// to calling <c>as dynamic</c> to cast the value.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if either the key is not found or the
        /// underlying value is null (since the dynamic runtime binder can't bind null values).</param>
        /// <returns>A dynamic value for the specific key or default value.</returns>
        public dynamic GetDynamic(string key, object defaultValue = null) => Get(key, defaultValue) ?? defaultValue;
    }
}
