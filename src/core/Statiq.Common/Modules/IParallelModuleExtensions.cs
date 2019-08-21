using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class IParallelModuleExtensions
    {
        /// <summary>
        /// Indicates that the module should process documents in parallel.
        /// </summary>
        /// <typeparam name="TModule">The type of module.</typeparam>
        /// <param name="module">The module that should process documents in parallel.</param>
        /// <returns>The module.</returns>
        public static TModule WithParallelExecution<TModule>(this TModule module)
            where TModule : IParallelModule
        {
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module));
            }
            module.Parallel = true;
            return module;
        }

        /// <summary>
        /// Indicates that the module should process documents sequentially.
        /// </summary>
        /// <typeparam name="TModule">The type of module.</typeparam>
        /// <param name="module">The module that should process documents sequentially.</param>
        /// <returns>The module.</returns>
        public static TModule WithSequentialExecution<TModule>(this TModule module)
            where TModule : IParallelModule
        {
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module));
            }
            module.Parallel = false;
            return module;
        }
    }
}
