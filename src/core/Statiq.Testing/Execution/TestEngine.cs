using System;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestEngine : IEngine
    {
        public TestEngine()
        {
            DocumentFactory = new DocumentFactory(_settings);
            DocumentFactory.SetDefaultDocumentType<TestDocument>();
        }

        private readonly TestSettings _settings = new TestSettings();

        public ISettings Settings => _settings;

        public IFileSystem FileSystem { get; set; } = new TestFileSystem();

        public IMemoryStreamFactory MemoryStreamFactory { get; set; } = new TestMemoryStreamFactory();

        public string ApplicationInput { get; set; }

        public IPipelineCollection Pipelines => throw new NotImplementedException();

        public IShortcodeCollection Shortcodes => new TestShortcodeCollection();

        public INamespacesCollection Namespaces => throw new NotImplementedException();

        public IRawAssemblyCollection DynamicAssemblies => throw new NotImplementedException();

        public DocumentFactory DocumentFactory { get; }
    }
}
