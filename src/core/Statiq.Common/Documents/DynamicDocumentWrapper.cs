using System.Dynamic;

namespace Statiq.Common
{
    internal class DynamicDocumentWrapper : DynamicObject
    {
        private readonly IDocument _document;

        public DynamicDocumentWrapper(IDocument document)
        {
            _document = document;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) =>
            _document.TryGetValue(binder.Name, out result);
    }
}
