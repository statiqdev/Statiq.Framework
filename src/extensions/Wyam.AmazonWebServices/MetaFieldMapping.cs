using System;
using Wyam.Common.Configuration;

namespace Wyam.AmazonWebServices
{
    internal class MetaFieldMapping
    {
        public DocumentConfig<string> FieldName { get; }
        public DocumentConfig<string> MetaKey { get; }

        public Func<object, object> Transformer { get; }

        public MetaFieldMapping(DocumentConfig<string> fieldName, DocumentConfig<string> metaKey, Func<object, object> transformer = null)
        {
            FieldName = fieldName;
            MetaKey = metaKey;
            Transformer = transformer;
        }
    }
}