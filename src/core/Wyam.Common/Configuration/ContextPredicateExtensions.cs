using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Configuration
{
    public static class ContextPredicateExtensions
    {
        public static ContextPredicate CombineWith(this ContextPredicate first, ContextPredicate second)
        {
            if (first == null)
            {
                return second;
            }
            if (second == null)
            {
                return first;
            }
            return new ContextPredicate(async ctx => await first.GetValueAsync(ctx) && await second.GetValueAsync(ctx));
        }
    }
}
