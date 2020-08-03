using System;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Statiq.Common;

namespace Statiq.Images.Operations
{
    internal class ActionOperation : IImageOperation
    {
        private readonly Func<IImageProcessingContext<Rgba32>, IImageProcessingContext<Rgba32>> _operation;
        private readonly Func<NormalizedPath, NormalizedPath> _pathModifier;

        public ActionOperation(
            Func<IImageProcessingContext<Rgba32>, IImageProcessingContext<Rgba32>> operation,
            Func<NormalizedPath, NormalizedPath> pathModifier)
        {
            _operation = operation;
            _pathModifier = pathModifier;
        }

        public IImageProcessingContext<Rgba32> Apply(IImageProcessingContext<Rgba32> image) =>
            _operation is null ? image : _operation(image);

        public NormalizedPath GetPath(NormalizedPath path) =>
            _pathModifier is null ? path : _pathModifier(path);
    }
}
