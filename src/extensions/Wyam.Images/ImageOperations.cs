using System.Collections.Generic;
using Wyam.Images.Operations;

namespace Wyam.Images
{
    internal class ImageOperations
    {
        public Queue<IImageOperation> Operations { get; } = new Queue<IImageOperation>();
        public List<OutputAction> OutputActions { get; } = new List<OutputAction>();

        public void Enqueue(IImageOperation operation) => Operations.Enqueue(operation);
    }
}
