using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Razor.Language;

namespace Wyam.Razor
{
    /// <summary>
    /// A RazorProjectFileSystem that lets us use the Wyam file provider while
    /// allowing replacement of the stream with document content.
    /// </summary>
    internal class WyamRazorProjectFileSystem : FileProviderRazorProjectFileSystem
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public WyamRazorProjectFileSystem(IRazorViewEngineFileProviderAccessor accessor, IHostingEnvironment hostingEnviroment)
            : base(accessor, hostingEnviroment)
        {
            _hostingEnvironment = hostingEnviroment;
        }

        public RazorProjectItem GetItem(string path, Stream stream)
        {
            FileProviderRazorProjectItem projectItem = (FileProviderRazorProjectItem)GetItem(path);
            return new FileProviderRazorProjectItem(
                new StreamFileInfo(projectItem.FileInfo, stream),
                projectItem.BasePath,
                projectItem.FilePath,
                _hostingEnvironment.ContentRootPath);
        }
    }
}