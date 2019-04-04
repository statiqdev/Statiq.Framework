using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Configuration
{
    public static class Config
    {
        public static ContextConfig FromValue(object value) => new ContextConfig(_ => Task.FromResult(value));

        public static ContextConfig FromContext(Func<IExecutionContext, Task<object>> func) => new ContextConfig(func);

        public static ContextConfig<T> FromContext<T>(Func<IExecutionContext, Task<T>> func) => new ContextConfig<T>(func);

        public static DocumentConfig FromDocument(Func<IDocument, IExecutionContext, Task<object>> func) => new DocumentConfig(func);

        public static DocumentConfig<T> FromDocument<T>(Func<IDocument, IExecutionContext, Task<T>> func) => new DocumentConfig<T>(func);

        public static DocumentConfig FromDocument(Func<IDocument, Task<object>> func) => new DocumentConfig((doc, _) => func(doc));

        public static DocumentConfig<T> FromDocument<T>(Func<IDocument, Task<T>> func) => new DocumentConfig<T>((doc, _) => func(doc));

        public static ContextPredicate IfContext(Func<IExecutionContext, Task<bool>> func) => new ContextPredicate(func);

        public static DocumentPredicate IfDocument(Func<IDocument, IExecutionContext, Task<bool>> func) => new DocumentPredicate(func);

        public static DocumentPredicate IfDocument(Func<IDocument, Task<bool>> func) => new DocumentPredicate((doc, _) => func(doc));

        // This just adds a space to the front of error details so it'll format nicely
        internal static string GetErrorDetails(string errorDetails)
        {
            if (errorDetails?.StartsWith(" ") == false)
            {
                errorDetails = " " + errorDetails;
            }
            return errorDetails;
        }
    }
}
