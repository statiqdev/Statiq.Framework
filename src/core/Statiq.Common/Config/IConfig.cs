using System.Threading.Tasks;

namespace Statiq.Common
{
    public interface IConfig
    {
        bool RequiresDocument { get; }

        Task<object> GetValueAsync(IDocument document, IExecutionContext context);
    }
}
