using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Statiq.Common.Configuration;
using Statiq.Common.Content;
using Statiq.Common.Execution;
using Statiq.Common.IO;
using Statiq.Common.Meta;
using Statiq.Common.Util;

namespace Statiq.Common.Documents
{
    /// <summary>
    /// Indicates that a document property should have a different metadata name than the property name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DocumentMetadataAttribute : Attribute
    {
        public string Name { get; }

        /// <summary>
        /// Specifies an alternate name for this property in document metadata.
        /// </summary>
        /// <param name="name">The alternate metadata name, or <c>null</c> to exclude from metadata.</param>
        public DocumentMetadataAttribute(string name)
        {
            Name = name;
        }
    }
}
