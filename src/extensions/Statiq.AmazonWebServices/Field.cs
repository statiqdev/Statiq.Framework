using Statiq.Common.Configuration;

namespace Statiq.AmazonWebServices
{
    internal class Field
    {
        public DocumentConfig<string> FieldName { get; }
        public DocumentConfig<object> FieldValue { get; }

        public Field(DocumentConfig<string> fieldName, DocumentConfig<object> fieldValue)
        {
            FieldName = fieldName;
            FieldValue = fieldValue;
        }
    }
}