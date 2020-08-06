using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JavaScriptEngineSwitcher.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Statiq.Common;

namespace Statiq.Core
{
    public static class IEngineExtensions
    {
        public static void LogAndCheckVersion(this IEngine engine, Assembly assembly, string name, string versionRangeKey)
        {
            if (!(Attribute.GetCustomAttribute(assembly, typeof(AssemblyInformationalVersionAttribute)) is AssemblyInformationalVersionAttribute versionAttribute))
            {
                throw new InvalidOperationException("Something went terribly wrong, could not determine Statiq version");
            }

            // Trim a "+" since intermediate builds add one
            string version = versionAttribute.InformationalVersion;
            int plusIndex = version.LastIndexOf('+');
            if (plusIndex > 0)
            {
                version = version.Substring(0, plusIndex);
            }

            engine.Logger.LogInformation($"{name} version {version}");
            if (!versionRangeKey.IsNullOrWhiteSpace())
            {
                string versionRange = engine.Settings.GetString(versionRangeKey);
                if (!versionRange.IsNullOrWhiteSpace())
                {
                    SemVer.Version semver = new SemVer.Version(version);
                    SemVer.Range range = new SemVer.Range(versionRange, true);
                    if (!range.IsSatisfied(semver))
                    {
                        throw new Exception($"{name} {version} does not meet the version requirement of {versionRange}");
                    }
                }
            }
        }
    }
}
