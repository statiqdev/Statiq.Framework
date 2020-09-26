using System.Threading.Tasks;

namespace Statiq.Common
{
    public abstract class SyncValidator : Validator
    {
        public sealed override Task ValidateAsync(IValidationContext context)
        {
            Validate(context);
            return Task.CompletedTask;
        }

        protected abstract void Validate(IValidationContext context);
    }
}
