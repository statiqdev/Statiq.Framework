using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Shortcodes;
using Wyam.Core.Shortcodes;

namespace Wyam.Core.Tests.Shortcodes
{
    public class TestShortcode : IShortcode
    {
        public Task<IShortcodeResult> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
