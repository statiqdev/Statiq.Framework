using System.Threading.Tasks;

namespace Statiq.Common
{
    public interface IConfig : IMetadataValue
    {
        bool RequiresDocument { get; }

        Task<object> GetValueAsync(IDocument document, IExecutionContext context);

        // IConfig values can be used as metadata
        object IMetadataValue.Get(IMetadata metadata)
        {
            IExecutionContext context = IExecutionContext.Current;
            if (RequiresDocument)
            {
                if (metadata is IDocument document)
                {
                    return GetValueAsync(document, context).GetAwaiter().GetResult();
                }

                // The source metadata isn't a document but we need one so go ahead and create a dummy one that wraps the metadata
                return GetValueAsync(context.CreateDocument(metadata), context).GetAwaiter().GetResult();
            }
            return GetValueAsync(null, context).GetAwaiter().GetResult();
        }
    }
}
