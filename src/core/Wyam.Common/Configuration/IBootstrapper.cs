using System.Collections.Generic;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;

namespace Wyam.Common.Configuration
{
    public interface IBootstrapper
    {
        /// <summary>
        /// Holds configurators for specific configurable types as instances of
        /// <see cref="IConfigurator{T}"/>. Right now the following types are
        /// configurable:
        /// <list type="bullet">
        ///     <item>
        ///         <term><c>IBootstrapper</c></term>
        ///         <description>
        ///         These types of configurators are automatically discovered and added in all referenced libraries.
        ///         Use them to add other configurators that should always be run. Note that bootstrapper configurators
        ///         should not be used to manipulate pipelines or otherwise directly modify a site build.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><c>IEngine</c></term>
        ///         <description>
        ///         Used to add pipelines and otherwise configure the execution engine.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><c>IServiceCollection</c></term>
        ///         <description>
        ///         Can add services to the dependency injection container.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><c>Spectre.Cli.IConfigurator</c></term>
        ///         <description>
        ///         This can be used to configure command line options.
        ///         </description>
        ///     </item>
        /// </list>
        /// </summary>
        IConfiguratorCollection Configurators { get; }
    }
}
