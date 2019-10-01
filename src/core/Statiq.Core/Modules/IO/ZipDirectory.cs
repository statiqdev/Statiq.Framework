using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Zips the contents of a given directory.
    /// </summary>
    /// <category>Input/Output</category>
    public class ZipDirectory : SyncConfigModule<DirectoryPath>
    {
        /// <summary>
        /// Zips all files in the given directory.
        /// </summary>
        /// <param name="directory">The path to zip (absolute or relative to the root directory).</param>
        public ZipDirectory(Config<DirectoryPath> directory)
            : base(directory, false)
        {
        }

        protected override IEnumerable<IDocument> ExecuteConfig(IDocument input, IExecutionContext context, DirectoryPath value) =>
            context.CloneOrCreateDocument(input, ZipFileHelper.CreateZipFile(context, value).GetContentProvider()).Yield();
    }
}
