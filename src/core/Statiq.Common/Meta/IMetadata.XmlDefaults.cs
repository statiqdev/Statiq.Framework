using System;
using System.Xml.Linq;

namespace Statiq.Common
{
    public partial interface IMetadata
    {
        /// <summary>
        /// Gets an XML attribute for the given metadata key.
        /// The name of the attribute will be the lower-case key name.
        /// </summary>
        /// <param name="key">The key containing the attribute value.</param>
        /// <returns>The attribute if the key was found, <c>null</c> if not.</returns>
        public XAttribute XAttribute(string key) => XAttribute(key, x => x);

        /// <summary>
        /// Gets an XML attribute for the given metadata key.
        /// </summary>
        /// <param name="name">The name of the XML attribute.</param>
        /// <param name="key">The key containing the attribute value.</param>
        /// <returns>The attribute if the key was found, <c>null</c> if not.</returns>
        public XAttribute XAttribute(string name, string key) => XAttribute(name, key, x => x);

        public XAttribute XAttribute(string key, Func<string, string> valueFunc) =>
            XAttribute(key.ToLower(), key, valueFunc);

        public XAttribute XAttribute(string name, string key, Func<string, string> valueFunc) =>
            XAttribute(key, x => new XAttribute(name, valueFunc(x)));

        public XAttribute XAttribute(string key, Func<string, XAttribute> attributeFunc) =>
            TryGetValue(key, out string value) ? attributeFunc(value) : null;

        public XElement XElement(string key, Func<string, object[]> contentFunc) =>
            XElement(key.ToLower(), key, contentFunc);

        public XElement XElement(string name, string key, Func<string, object[]> contentFunc) =>
            XElement(key, x => new XElement(name, contentFunc(x)));

        public XElement XElement(string key, Func<string, XElement> elementFunc) =>
            TryGetValue(key, out string value) ? elementFunc(value) : null;
    }
}
