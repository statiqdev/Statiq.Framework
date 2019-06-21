using System;
using System.Collections.Generic;
using System.Text;
using Statiq.Common.Configuration;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.IO;
using Statiq.Common.Shortcodes;
using Statiq.Testing.Configuration;
using Statiq.Testing.IO;
using Statiq.Testing.Meta;

namespace Statiq.Testing.Execution
{
    public class TestEngine : IEngine
    {
        public TestEngine()
        {
            DocumentFactory = new DocumentFactory(_settings);
        }

        private readonly TestSettings _settings = new TestSettings();

        public ISettings Settings => _settings;

        public IFileSystem FileSystem { get; set; } = new TestFileSystem();

        public IMemoryStreamFactory MemoryStreamFactory { get; set; } = new TestMemoryStreamFactory();

        public string ApplicationInput { get; set; }

        public IPipelineCollection Pipelines => throw new NotImplementedException();

        public IShortcodeCollection Shortcodes => throw new NotImplementedException();

        public INamespacesCollection Namespaces => throw new NotImplementedException();

        public IRawAssemblyCollection DynamicAssemblies => throw new NotImplementedException();

        public DocumentFactory DocumentFactory { get; }
    }
}
