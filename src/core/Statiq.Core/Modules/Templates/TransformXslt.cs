using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Transforms input documents using a supplied XSLT template.
    /// </summary>
    /// <remarks>
    /// This module uses <see cref="System.Xml.Xsl.XslCompiledTransform"/> with default settings. This means that the
    /// XSLT <c>document()</c> function and embedded scripts are disabled. For more information
    /// see the <a href="https://msdn.microsoft.com/en-us/library/system.xml.xsl.xslcompiledtransform.aspx">MSDN documentation</a>.
    /// </remarks>
    /// <category>Templates</category>
    public class TransformXslt : ParallelModule
    {
        private readonly Config<FilePath> _xsltPath;
        private readonly IModule[] _xsltGeneration;

        /// <summary>
        /// Transforms input documents using a specified XSLT file from the file system
        /// as provided by a delegate. This allows you to use different XSLT files depending
        /// on the input document.
        /// </summary>
        /// <param name="xsltPath">A delegate that should return a <see cref="FilePath"/> with the XSLT file to use.</param>
        public TransformXslt(Config<FilePath> xsltPath)
        {
            _xsltPath = xsltPath;
        }

        /// <summary>
        /// Transforms input documents using the output content from the specified modules. The modules are executed for each input
        /// document with the current document as the input to the specified modules.
        /// </summary>
        /// <param name="modules">Modules that should output a single document containing the XSLT template in it's content.</param>
        public TransformXslt(params IModule[] modules)
        {
            _xsltGeneration = modules;
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteAsync(IDocument input, IExecutionContext context)
        {
            XslCompiledTransform xslt = new XslCompiledTransform();

            if (_xsltPath != null)
            {
                FilePath path = await _xsltPath.GetValueAsync(input, context);
                if (path != null)
                {
                    IFile file = context.FileSystem.GetInputFile(path);
                    if (file.Exists)
                    {
                        using (Stream fileStream = file.OpenRead())
                        {
                            xslt.Load(XmlReader.Create(fileStream));
                        }
                    }
                }
            }
            else if (_xsltGeneration != null)
            {
                IDocument xsltDocument = (await context.ExecuteModulesAsync(_xsltGeneration, input.Yield())).Single();
                using (Stream stream = xsltDocument.GetStream())
                {
                    xslt.Load(XmlReader.Create(stream));
                }
            }
            using (Stream stream = input.GetStream())
            {
                StringWriter str = new StringWriter();
                using (XmlTextWriter writer = new XmlTextWriter(str))
                {
                    xslt.Transform(XmlReader.Create(stream), writer);
                }
                return input.Clone(await context.GetContentProviderAsync(str.ToString())).Yield();
            }
        }
    }
}
