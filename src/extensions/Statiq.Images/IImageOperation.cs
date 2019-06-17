using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Statiq.Common.IO;

namespace Statiq.Images.Operations
{
    internal interface IImageOperation
    {
        IImageProcessingContext<Rgba32> Apply(IImageProcessingContext<Rgba32> image);
        FilePath GetPath(FilePath path);
    }
}
