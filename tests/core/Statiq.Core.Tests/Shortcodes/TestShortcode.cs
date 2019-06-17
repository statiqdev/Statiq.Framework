using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.Shortcodes;

namespace Statiq.Core.Tests.Shortcodes
{
    public class TestShortcode : IShortcode
    {
        public Task<IDocument> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
