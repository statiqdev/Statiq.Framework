using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public abstract class SyncDocumentValidator : DocumentValidator
    {
        protected sealed override Task ValidateAsync(IDocument document, IValidationContext context)
        {
            Validate(document, context);
            return Task.CompletedTask;
        }

        protected abstract void Validate(IDocument document, IValidationContext context);
    }
}
