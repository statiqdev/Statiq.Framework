using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    public class ProcessLauncherResult
    {
        internal ProcessLauncherResult(int exitCode, string errorData)
        {
            ExitCode = exitCode;
            ErrorData = errorData;
        }

        public int ExitCode { get; }
        public string ErrorData { get; }
    }
}