using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Spectre.Cli;

namespace Statiq.App
{
    /// <summary>
    /// Extension methods for <see cref="IConfigurationRoot"/>.
    /// </summary>
    internal static class IConfigurationRootExtensions
    {
        /// <summary>
        /// Generates a human-readable view of the configuration showing where each value came from.
        /// </summary>
        /// <remarks>
        /// The original code is from <see cref="ConfigurationRootExtensions.GetDebugView(IConfigurationRoot)"/>
        /// but this version masks any values from environment variables.
        /// </remarks>
        /// <returns> The debug view. </returns>
        public static string GetSafeDebugView(this IConfigurationRoot root)
        {
            void RecurseChildren(
                StringBuilder stringBuilder,
                IEnumerable<IConfigurationSection> children,
                string indent)
            {
                foreach (IConfigurationSection child in children)
                {
                    (string Value, IConfigurationProvider Provider) valueAndProvider = GetValueAndProvider(root, child.Path);

                    if (valueAndProvider.Provider != null)
                    {
                        stringBuilder
                            .Append(indent)
                            .Append(child.Key)
                            .Append("=")
                            .Append((valueAndProvider.Provider is EnvironmentVariablesConfigurationProvider) ? "***" : valueAndProvider.Value)
                            .Append(" (")
                            .Append(valueAndProvider.Provider)
                            .AppendLine(")");
                    }
                    else
                    {
                        stringBuilder
                            .Append(indent)
                            .Append(child.Key)
                            .AppendLine(":");
                    }

                    RecurseChildren(stringBuilder, child.GetChildren(), indent + "  ");
                }
            }

            StringBuilder builder = new StringBuilder();

            RecurseChildren(builder, root.GetChildren(), string.Empty);

            return builder.ToString();
        }

        private static (string Value, IConfigurationProvider Provider) GetValueAndProvider(
            IConfigurationRoot root,
            string key)
        {
            foreach (IConfigurationProvider provider in root.Providers.Reverse())
            {
                if (provider.TryGet(key, out string value))
                {
                    return (value, provider);
                }
            }

            return (null, null);
        }
    }
}
