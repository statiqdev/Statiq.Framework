using System.Threading;
using System.Threading.Tasks;
using Statiq.Core;

namespace Statiq.App
{
    public interface IEngineManager
    {
        Engine Engine { get; }

        Task<bool> ExecuteAsync(CancellationTokenSource cancellationTokenSource);
    }
}
