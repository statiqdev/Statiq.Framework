using System.Threading.Tasks;

namespace Statiq.Common
{
    public abstract class DocumentValidator : Validator
    {
        public sealed override async Task ValidateAsync(IValidationContext context) =>
            await context.Documents.ParallelForEachAsync(async doc => await ValidateAsync(doc, context), context.CancellationToken);

        protected abstract Task ValidateAsync(IDocument document, IValidationContext context);
    }
}
